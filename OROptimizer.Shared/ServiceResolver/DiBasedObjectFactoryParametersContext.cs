namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Represents a context for managing parameters specific to dependency injection (DI)-based object factories.
    /// </summary>
    /// <remarks>
    /// This class provides an ambient context for handling parameters used within DI-based object factories, ensuring that they can
    /// be accessed or overridden during object creation processes.
    /// It inherits from <see cref="AmbientContext{TContext, TContextDefaultImplementation}"/>, where
    /// <c>TContext</c> is <see cref="IDiBasedObjectFactoryParameters"/> and <c>TContextDefaultImplementation</c> is <see cref="DiBasedObjectFactoryParameters"/>.
    /// </remarks>
    public class DiBasedObjectFactoryParametersContext : AmbientContext<IDiBasedObjectFactoryParameters, DiBasedObjectFactoryParameters>
    {

    }
}