namespace Assistant.Net.Unions
{
    /// <summary>
    ///    Base type of maybe monad implementation.
    /// </summary>
    public record Option<T>
    {
        /// <summary>
        ///    Whether current option has some value.
        /// </summary>
        public bool IsSome => this is Some<T>;

        /// <summary>
        ///    Whether current option has none value.
        /// </summary>
        public bool IsNone => !IsSome;

        public static implicit operator bool(Option<T> option) => option.IsSome;

        public static implicit operator Option<T>(None _) => new None<T>();
    }

    /// <summary>
    ///    List of options for maybe monad implementation.
    /// </summary>
    public static class Option
    {
        public static Option<T> Some<T>(T value) => new Some<T>(value);

        public static None None { get; } = null!;
    }
}