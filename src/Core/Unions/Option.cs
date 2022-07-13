namespace Assistant.Net.Unions;

/// <summary>
///    Base type of maybe monad implementation.
/// </summary>
public record Option<T>
{
    /// <summary/>
    protected Option() { }

    /// <summary>
    ///    Whether current option has some value.
    /// </summary>
    public virtual bool IsSome { get; } = false;

    /// <summary>
    ///    Whether current option has none value.
    /// </summary>
    public bool IsNone => !IsSome;

    /// <summary/>
    public static implicit operator bool(Option<T> option) => option.IsSome;

    /// <summary/>
    public static implicit operator Option<T>(None _) => new None<T>();
}

/// <summary>
///    List of options for maybe monad implementation.
/// </summary>
public static class Option
{
    /// <summary>
    ///     Creates some value of maybe monad.
    /// </summary>
    public static Option<T> Some<T>(T value) => new Some<T>(value);

    /// <summary>
    ///     Creates none value of maybe monad.
    /// </summary>
    public static None None => None.Instance;
}
