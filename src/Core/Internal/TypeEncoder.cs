using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Assistant.Net.Internal;

/// <summary>
///     Default implementation of type encoder with short type name based behavior.
/// </summary>
internal sealed class TypeEncoder : ITypeEncoder, IDisposable
{
    private const string GenericTypeArgumentsSeparator = ",";

    private readonly ILogger<TypeEncoder> logger;
    private readonly IOptionsMonitor<TypeEncoderOptions> optionsMonitor;
    private readonly IDisposable optionsChangeOnSubscription;
    private readonly List<(string name, Type type)> duplicatedTypes = new();
    private readonly Dictionary<string, Type> decodeTypes = new();
    private readonly Dictionary<Type, string> encodeTypes = new();
    private readonly Regex arrayRegex = new("^(\\w+)(\\[(,*)\\])?$", RegexOptions.Compiled);
    private readonly Regex genericRegex = new("^(\\w+`\\d)(\\[([\\[\\]`,\\w]+)*\\])?$", RegexOptions.Compiled);

    public TypeEncoder(ILogger<TypeEncoder> logger, IOptionsMonitor<TypeEncoderOptions> optionsMonitor)
    {
        this.logger = logger;
        this.optionsMonitor = optionsMonitor;
        this.optionsChangeOnSubscription = optionsMonitor.OnChange(Configure);

        Configure(optionsMonitor.CurrentValue);
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    public IEnumerable<(string name, Type type)> DuplicatedTypes => duplicatedTypes;

    void IDisposable.Dispose()
    {
        optionsChangeOnSubscription.Dispose();
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
    }

    public Type? Decode(string encodedType)
    {
        if (decodeTypes.TryGetValue(encodedType, out var type))
            return type;

        var arrayMatch = arrayRegex.Match(encodedType);
        if (arrayMatch.Success)
        {
            var arrayType = arrayMatch.Groups[1].Value;
            var arrayRanksDefinition = arrayMatch.Groups[3].Value;

            if (!decodeTypes.TryGetValue(arrayType, out var typeDefinition))
                return null; // unknown array type

            if (arrayRanksDefinition.Length == 0)
                return typeDefinition.MakeArrayType();

            return typeDefinition.MakeArrayType(arrayRanksDefinition.Length + 1);
        }

        var genericMatch = genericRegex.Match(encodedType);
        if (genericMatch.Success)
        {
            var typeDefinitionName = genericMatch.Groups[1].Value;
            var argumentTypeNames = genericMatch.Groups[3].Value.Split(GenericTypeArgumentsSeparator);

            if (!decodeTypes.TryGetValue(typeDefinitionName, out var typeDefinition))
                return null; // unknown definition type

            var argumentTypes = argumentTypeNames.Select(Decode).ToArray();
            if (argumentTypes.Any(x => x == null))
                return null; // unknown argument type

            return typeDefinition.MakeGenericType(argumentTypes!);
        }

        return null; // unknown type
    }

    /// <exception cref="ArgumentException"/>
    public string? Encode(Type type)
    {
        if (encodeTypes.TryGetValue(type, out var name))
            return name;

        if (type.IsArray)
            return GetName(type);

        if (type.IsGenericTypeDefinition)
            return null; // excluded

        if (!type.IsGenericType || !encodeTypes.TryGetValue(type.GetGenericTypeDefinition(), out name))
            return null; // unknown type

        var argumentTypeNames = type.GetGenericArguments().Select(Encode);
        var commaSeparatedArgumentTypeNames = string.Join(GenericTypeArgumentsSeparator, argumentTypeNames);
        return $"{GetName(type)}[{commaSeparatedArgumentTypeNames}]";
    }

    private void Configure(TypeEncoderOptions o)
    {
        logger.LogInformation("Configure known types on options change.");

        duplicatedTypes.Clear();
        decodeTypes.Clear();
        encodeTypes.Clear();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            AddTypes(o, assembly);

        logger.LogInformation("Configured types: {TypeCount} and duplicates: {DuplicatedTypeCount}.",
            decodeTypes.Count, duplicatedTypes.Count);
    }

    private void AddTypes(TypeEncoderOptions options, Assembly assembly)
    {
        if (options.ExcludedAssembly.Contains(assembly))
            return;

        var types = assembly.GetTypes()
            .Where(x => x.IsPublic && x.Namespace != null)
            .Where(x => !x.IsAbstract || !x.IsSealed) // not static
            .Where(x => x.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
            .Where(x => !options.ExcludedNamespaces.Any(y => x.Namespace!.StartsWith(y)))
            .ToArray();
        foreach (var type in types.Except(encodeTypes.Keys).Except(options.ExcludedTypes))
        {
            var name = GetName(type);
            if (!decodeTypes.TryGetValue(name, out var knownType))
            {
                encodeTypes.Add(type, name);
                decodeTypes.Add(name, type);
            }
            else if (type != knownType)
            {
                duplicatedTypes.Add((name, knownType));
                logger.LogWarning("Duplicate {Type} was ignored.", knownType);
            }
        }
    }

    private static string GetName(Type type) => type.IsNested
        ? type.FullName![type.Namespace!.Length..]
        : type.Name;

    private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args) =>
        AddTypes(optionsMonitor.CurrentValue, args.LoadedAssembly);
}
