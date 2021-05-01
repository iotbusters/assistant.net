namespace Assistant.Net.Diagnostics.Abstractions
{
    public interface IOperation
    {
        void Complete(string? message = null);
        void Fail(string? message = null);
    }
}