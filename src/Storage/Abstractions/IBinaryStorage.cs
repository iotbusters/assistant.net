namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    String key and binary value based common-purpose storage.
    /// </summary>
    public interface IBinaryStorage : IStorage<string, byte[]> { }
}