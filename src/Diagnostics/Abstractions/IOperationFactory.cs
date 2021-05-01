namespace Assistant.Net.Diagnostics.Abstractions
{
    public interface IOperationFactory
    {
        /// <summary>
        ///     Starts new operation within current operation context.
        /// </summary>
        IOperation Start(string name);
    }
}