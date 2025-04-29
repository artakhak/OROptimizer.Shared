using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Resolved type information.
    /// </summary>
    public class ResolvedTypeInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resolvedValue">Resolved value.</param>
        /// <param name="resolvedValueCanBeCached">If the value is true, the value will be cached and re-used.</param>
        public ResolvedTypeInfo([NotNull] object resolvedValue, bool resolvedValueCanBeCached)
        {
            ResolvedValueCanBeCached = resolvedValueCanBeCached;
            ResolvedValue = resolvedValue;
        }

        /// <summary>
        /// Resolved value.
        /// </summary>
        [NotNull]
        public object ResolvedValue { get; }

        /// <summary>
        /// If the value is true, the value will be cached and re-used.
        /// </summary>
        public bool ResolvedValueCanBeCached { get; } = false;
    }
}