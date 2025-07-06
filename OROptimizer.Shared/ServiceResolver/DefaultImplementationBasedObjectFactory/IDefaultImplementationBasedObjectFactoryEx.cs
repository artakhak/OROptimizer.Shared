using System;
using System.Collections.Generic;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Extension of <see cref="IDefaultImplementationBasedObjectFactory"/> that allows registering custom parameter resolvers.
    /// </summary>
    public interface IDefaultImplementationBasedObjectFactoryEx : IDefaultImplementationBasedObjectFactory
    {
        /// <summary>
        /// Retrieves the collection of custom constructor parameter resolvers that are currently registered.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="ICustomConstructorParameterResolver"/> instances representing the currently registered
        /// custom constructor parameter resolvers.
        /// </returns>
        IEnumerable<ICustomConstructorParameterResolver> GetCustomConstructorParameterResolvers();

        /// <summary>
        /// Removes all currently registered custom constructor parameter resolvers.
        /// </summary>
        void RemoveAllCustomConstructorParameterResolvers();

        /// <summary>
        /// Registers a custom constructor parameter resolver to be used during the object instantiation process.
        /// </summary>
        /// <param name="customConstructorParameterResolver">
        /// The instance of <see cref="ICustomConstructorParameterResolver"/> to be registered.
        /// </param>
        void RegisterCustomConstructorParameterResolvers(ICustomConstructorParameterResolver customConstructorParameterResolver);

        /// <summary>
        /// Unregisters a custom constructor parameter resolver using its unique identifier.
        /// </summary>
        /// <param name="customConstructorParameterResolverIdentifier">
        /// The unique identifier of the <see cref="ICustomConstructorParameterResolver"/> to be unregistered.
        /// </param>
        void UnregisterCustomConstructorParameterResolver(Guid customConstructorParameterResolverIdentifier);
    }
}