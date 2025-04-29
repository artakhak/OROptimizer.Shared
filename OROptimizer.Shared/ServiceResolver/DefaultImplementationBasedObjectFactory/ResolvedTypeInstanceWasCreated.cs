using System;
using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    public class ResolvedTypeInstanceWasCreated : EventArgs
    {
        public ResolvedTypeInstanceWasCreated([NotNull] Type resolvedType, [NotNull] object createdInstance)
        {
            ResolvedType = resolvedType;
            CreatedInstance = createdInstance;
        }

        [NotNull]
        public Type ResolvedType { get; }

        [NotNull]
        public object CreatedInstance { get; }
    }
}