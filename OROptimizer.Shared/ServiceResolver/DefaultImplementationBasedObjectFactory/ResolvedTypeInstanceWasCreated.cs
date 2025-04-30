using System;
using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Represents an event that is triggered when an instance of a resolved type is created.
    /// This event provides information about the resolved type and the created instance.
    /// </summary>
    public class ResolvedTypeInstanceWasCreated : EventArgs
    {
        /// <summary>
        /// Represents an event that is triggered when an instance of a resolved type is created.
        /// This event provides information about the resolved type and the created instance.
        /// </summary>
        /// <param name="resolvedType">The type of the resolved instance that was created.</param>
        /// <param name="createdInstance">The instance that was created for the resolved type.</param>
        public ResolvedTypeInstanceWasCreated([NotNull] Type resolvedType, [NotNull] object createdInstance)
        {
            ResolvedType = resolvedType;
            CreatedInstance = createdInstance;
        }

        /// <summary>
        /// Gets the type that was resolved and for which an instance was created.
        /// </summary>
        /// <remarks>
        /// This property holds a reference to the type that has been resolved by the service resolver.
        /// It is used to provide information about the resolved type in events or logs when an instance
        /// of the type is created.
        /// </remarks>
        [NotNull]
        public Type ResolvedType { get; }

        /// <summary>
        /// Gets the instance of the object that was created for the resolved type.
        /// </summary>
        /// <remarks>
        /// This property holds the reference to the actual object instance that was created by the service resolver.
        /// It is useful for accessing or inspecting the created instance in events or logs after the resolution process.
        /// </remarks>
        [NotNull]
        public object CreatedInstance { get; }
    }
}