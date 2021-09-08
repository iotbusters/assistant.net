using Assistant.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly Regex regex = new("^(\\w+`\\d)(\\[(\\w+(,\\w+)*)\\])?$", RegexOptions.Compiled);

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

            var match = regex.Match(encodedType);
            if (!match.Success)
                return null;

            var typeDefinitionName = match.Groups[1].Value;
            var argumentTypeNames = match.Groups[3].Value.Split(GenericTypeArgumentsSeparator);

            if (!knownTypes.TryGetValue(typeDefinitionName, out var typeDefinition))
                return null;

            var argumentTypes = argumentTypeNames.Select(Decode).ToArray();
            if (argumentTypes.Any(x => x == null))
                return null;

            return typeDefinition.MakeGenericType(argumentTypes!);
        }

        /// <exception cref="ArgumentException"/>
        public string Encode(Type type)
        {
            if (type.IsGenericTypeDefinition)
                throw new ArgumentException($"Argument cannot be generic type definition but '{type}' is.", nameof(type));

            if (!type.IsGenericType)
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