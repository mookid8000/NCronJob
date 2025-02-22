namespace LinkDotNet.NCronJob;

/// <summary>
/// Represents a registry for instant jobs.
/// </summary>
public interface IInstantJobRegistry
{
    /// <summary>
    /// Adds an instant job to the registry, which gets directly executed. The instance is retrieved from the container.
    /// <param name="parameter">An optional parameter that is passed down as the <see cref="JobExecutionContext"/> to the job.</param>
    /// </summary>
    /// <remarks>
    /// The contents of <paramref name="parameter" /> are not serialized and deserialized. It is the reference to the passed in object.
    /// </remarks>
    void AddInstantJob<TJob>(object? parameter = null) where TJob : IJob;
}
