namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Defines priority levels for resolving custom constructor parameters.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to specify the priority of custom constructor parameter resolvers
    /// within an object creation pipeline. A resolver with a higher priority may be favored over
    /// those with lower priorities when determining parameter values during object construction.
    /// </remarks>
    public enum CustomConstructorParameterResolverPriority
    {
        Low,
        Medium,
        High
    }
}