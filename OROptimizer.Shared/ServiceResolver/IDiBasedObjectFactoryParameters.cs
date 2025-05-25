namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Defines parameters used for dependency injection (DI)-based object factories.
    /// </summary>
    /// <remarks>
    /// This interface specifies settings that can be utilized to configure and control behavior
    /// related to the generation of objects in a DI-based object factory implementation.
    /// </remarks>
    public interface IDiBasedObjectFactoryParameters
    {
        bool LogDiagnosticsData { get; }
    }

    /// <inheritdoc />
    public class DiBasedObjectFactoryParameters : IDiBasedObjectFactoryParameters
    {
        /// <inheritdoc />
        public bool LogDiagnosticsData { get; set; }
    }
}