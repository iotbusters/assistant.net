using Assistant.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Assistant.Net.Internal
{

    /// <summary>
    ///     Default implementation of type encoder
    ///     with short type name based behavior.
    /// </summary>
    internal class TypeEncoder : ITypeEncoder, IDisposable
    {
        private const string GenericTypeArgumentsSeparator = ",";

        private readonly Dictionary<string, Type> knownTypes = new();
        private readonly Regex arrayRegex = new("^(\\w+)(\\[(,*)\\])?$", RegexOptions.Compiled);
        private readonly Regex genericRegex = new("^(\\w+`\\d)(\\[([\\[\\]`,\\w]+)*\\])?$", RegexOptions.Compiled);

        public TypeEncoder()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                AddTypes(assembly);
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        void IDisposable.Dispose() => AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

        public Type? Decode(string encodedType)
        {
            if (knownTypes.TryGetValue(encodedType, out var type))
                return type;

            var arrayMatch = arrayRegex.Match(encodedType);
            if (arrayMatch.Success)
            {
                var arrayType = arrayMatch.Groups[1].Value;
                var arrayRanksDefinition = arrayMatch.Groups[3].Value;

                if (!knownTypes.TryGetValue(arrayType, out var typeDefinition))
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

                if (!knownTypes.TryGetValue(typeDefinitionName, out var typeDefinition))
                    return null; // unknown definition type

                var argumentTypes = argumentTypeNames.Select(Decode).ToArray();
                if (argumentTypes.Any(x => x == null))
                    return null; // unknown argument type

                return typeDefinition.MakeGenericType(argumentTypes!);
            }

            return null; // not supported
        }

        /// <exception cref="ArgumentException"/>
        public string? Encode(Type type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return null;// not supported

            if (!type.IsGenericType || type.IsGenericTypeDefinition)
                return type.Name;

            var argumentTypeNames = type.GetGenericArguments().Select(Encode);
            var commaSeparatedArgumentTypeNames = string.Join(GenericTypeArgumentsSeparator, argumentTypeNames);
            return $"{type.Name}[{commaSeparatedArgumentTypeNames}]";
        }

        private void AddTypes(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(x => !string.IsNullOrEmpty(x.Namespace));
            foreach (var type in types)
            {
                var typeName = type.Name;
                if (!knownTypes.ContainsKey(typeName))
                    knownTypes.Add(typeName, type);
            }
        }

        private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args) => AddTypes(args.LoadedAssembly);
    }
}
