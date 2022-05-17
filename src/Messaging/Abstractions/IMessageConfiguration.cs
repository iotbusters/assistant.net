namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Common messaging configuration abstraction required for grouping configurations by purpose and
///     resolving code duplication issues and improving code readability.
/// </summary>
/// <typeparam name="TBuilder">Specific messaging client builder implementation type.</typeparam>
public interface IMessageConfiguration<in TBuilder> where TBuilder : IMessagingClientBuilder
{
    /// <summary>
    ///     Configures <see cref="IMessagingClient"/>.
    /// </summary>
    void Configure(TBuilder builder);
}