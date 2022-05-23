namespace Assistant.Net.Unions;

/// <summary>
///    Represents none value for maybe monad implementation.
/// </summary>
public record None<T> : Option<T>
{
    /// <inheritdoc/>
    public override bool IsSome => false;
}

/// <summary>
///    Represents none value for maybe monad implementation.
/// </summary>
public sealed record None
{
    private None() { }

    internal static None Instance { get; } = new();

    /// <summary/>
    public static implicit operator bool(None _) => false;
}
