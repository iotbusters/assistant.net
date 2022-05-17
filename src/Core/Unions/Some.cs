namespace Assistant.Net.Unions;

/// <summary>
///    Represents some value for maybe monad implementation.
/// </summary>
/// <param name="Value">A value.</param>
public record Some<T>(T Value) : Option<T>;