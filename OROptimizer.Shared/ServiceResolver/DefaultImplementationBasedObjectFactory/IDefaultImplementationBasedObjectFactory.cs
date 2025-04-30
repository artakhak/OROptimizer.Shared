using System;
using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// A factory that creates an instance of a type using the default implementations, or custom type resolution provided
    /// by <see cref="TryResolveConstructorParameterValueDelegate"/>.
    /// </summary>
    public interface IDefaultImplementationBasedObjectFactory
    {
        /// <summary>
        /// Creates an instance of a type in parameter <paramref name="typeToResolve"></paramref> and dependencies using the default implementations, <br/>
        /// or custom type resolution provided by a mechanism in implementation<br/>
        /// If custom (non-default) implementation is provided it is used to resolve the parameter values<br/>
        /// of type <paramref name="typeToResolve"></paramref> (or constructor parameters of its dependencies in a transitive manner).<br/>
        /// Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly where the type is (or if the resolved type is not an interface, an instance of type will be created via reflection).
        /// </summary>
        /// <param name="typeToResolve">Type to resolve.</param>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        object CreateInstance([NotNull] Type typeToResolve);

        /// <summary>
        /// If type <paramref name="typeToResolve"/> was created and cached in the past, returns the cached instance.
        /// Otherwise, creates an instance of a type in parameter <paramref name="typeToResolve"></paramref> and dependencies using the default implementations, <br/>
        /// or custom type resolution provided by a mechanism in implementation<br/>
        /// If custom (non-default) implementation is provided it is used to resolve the parameter values<br/>
        /// of type <paramref name="typeToResolve"></paramref> (or constructor parameters of its dependencies in a transitive manner).<br/>
        /// Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly where the type is (or if the resolved type is not an interface, an instance of type will be created via reflection).
        /// </summary>
        /// <param name="typeToResolve">Type to resolve.</param>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        object GetOrCreateInstance([NotNull] Type typeToResolve);
    }

    /// <summary>
    /// Extensions for <see cref="IDefaultImplementationBasedObjectFactory"/>
    /// </summary>
    public static class DefaultImplementationBasedObjectFactoryExtensions
    {
        /// <summary>
        /// Creates an instance of a type <typeparamref name="T"/> and dependencies using the default implementations, <br/>
        /// or custom type resolution provided by a mechanism in implementation<br/>
        /// If custom (non-default) implementation is provided it is used to resolve the parameter values<br/>
        /// of type <typeparamref name="T"/> (or constructor parameters of its dependencies in a transitive manner).<br/>
        /// Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly where the type is (or if the resolved type is not an interface, an instance of type will be created via reflection).
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        public static T CreateInstance<T>([NotNull] this IDefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory) where T: class
        {
            if (defaultImplementationBasedObjectFactory.CreateInstance(typeof(T)) is T createdInstanceOfExpectedType)
                return createdInstanceOfExpectedType;

            throw new ArgumentException($"Failed to create an instance of type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// If type <typeparamref name="T"/> was created and cached in the past, returns the cached instance.
        /// Otherwise, creates an instance of a type <typeparamref name="T"/> and dependencies using the default implementations, <br/>
        /// or custom type resolution provided by a mechanism in implementation<br/>
        /// If custom (non-default) implementation is provided it is used to resolve the parameter values<br/>
        /// of type <typeparamref name="T"/> (or constructor parameters of its dependencies in a transitive manner).<br/>
        /// Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly where the type is (or if the resolved type is not an interface, an instance of type will be created via reflection).
        /// </summary>
        /// <exception cref="Exception">Throws an exception if type could not be created.</exception>
        public static T GetOrCreateInstance<T>([NotNull] this IDefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory) where T : class
        {
            if (defaultImplementationBasedObjectFactory.GetOrCreateInstance(typeof(T)) is T cachedOrCreatedInstanceOfExpectedType)
                return cachedOrCreatedInstanceOfExpectedType;

            throw new ArgumentException($"Failed to create an instance of type '{typeof(T).FullName}'.");
        }
    }
}
