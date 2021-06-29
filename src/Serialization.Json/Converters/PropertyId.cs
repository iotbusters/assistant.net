using System;

namespace Assistant.Net.Serialization.Converters
{
    internal class PropertyId
    {
        public PropertyId(string name, Type returnType)
        {
            Name = name.ToLowerInvariant();
            ReturnType = returnType;
        }

        public string Name { get; }

        public Type ReturnType { get; }

        public override int GetHashCode() => HashCode.Combine(Name, ReturnType);

        public override bool Equals(object? obj)
        {
            if (obj is not PropertyId other) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other);
        }

        private bool Equals(PropertyId other) =>
            Name == other.Name
            && ReturnType == other.ReturnType;
    }
}