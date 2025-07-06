using System;
using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Tries to create a constructor parameter value for <paramref name="parameterInfo"/>
    /// </summary>
    /// <param name="defaultImplementationBasedObjectFactory">Default-implementation-based object factory.</param>
    /// <param name="constructedObjectType">Constructed object type. This is the type into which the value resolved by the delegate will be injected, and
    /// not the type of the parameter itself. Parameter type can be found in <paramref name="parameterInfo"></paramref>.
    /// </param>
    /// <param name="parameterInfo">Constructor parameter info.</param>
    /// <returns>Returns a tuple. The first value indicates if the value was resolved.
    /// If the first value is true, the second value is the resolved value.
    /// Otherwise, the second value is null.
    /// </returns>
    /// <exception cref="ApplicationException">Throws an exception parameter value cannot be created.</exception>
    [CanBeNull]
    public delegate (bool parameterValueWasResolved, object resolvedValue) CreateConstructorParameterValueDelegate(
        IDefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory,
        [NotNull] Type constructedObjectType,
        [NotNull] System.Reflection.ParameterInfo parameterInfo);
  
    /// <summary>
    /// Custom resolver for constructor parameter values.
    /// </summary>
    public interface ICustomConstructorParameterResolver
    {
        /// <summary>
        /// Unique identifier for the custom constructor parameter resolver.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        /// Tries to create a constructor parameter value for <paramref name="parameterInfo"/>
        /// </summary>
        /// <param name="defaultImplementationBasedObjectFactory">Default-implementation-based object factory.</param>
        /// <returns>Returns the created instance. The value can be null, which indicates that the delegate does not
        /// take responsibility for the parameter value creation.</returns>
        /// <param name="constructedObjectType">Constructed object type. This is the type into which the value resolved by the delegate will be injected, and
        /// not the type of the parameter itself. Parameter type can be found in <paramref name="parameterInfo"></paramref>.
        /// </param>
        /// <param name="parameterInfo">Constructor parameter info.</param>
        /// <returns>Returns a tuple. The first value indicates if the value was resolved.
        /// If the first value is true, the second value is the resolved value.
        /// Otherwise, the second value is null.
        /// </returns>
        /// <exception cref="ApplicationException">Throws an exception parameter value cannot be created.</exception>
        (bool parameterValueWasResolved, object resolvedValue) CreateConstructorParameterValue(
            [NotNull]
            IDefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory,
            [NotNull] Type constructedObjectType,
            [NotNull] System.Reflection.ParameterInfo parameterInfo);
    
        /**
         * Priority.
         */
        CustomConstructorParameterResolverPriority Priority { get; }
    }

    /// <inheritdoc />
    public class CustomConstructorParameterResolver : ICustomConstructorParameterResolver
    {
        private readonly CreateConstructorParameterValueDelegate _createConstructorParameterValue;

        public CustomConstructorParameterResolver(
            Guid identifier, 
            CreateConstructorParameterValueDelegate createConstructorParameterValue,
            CustomConstructorParameterResolverPriority priority = CustomConstructorParameterResolverPriority.Medium)
        {
            Identifier = identifier;
            _createConstructorParameterValue = createConstructorParameterValue;
            Priority = priority;
        }

        /// <inheritdoc />
        public Guid Identifier { get; }

        /// <inheritdoc />
        public (bool parameterValueWasResolved, object resolvedValue) CreateConstructorParameterValue(
            IDefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory,
            [NotNull] Type constructedObjectType,
            System.Reflection.ParameterInfo parameterInfo)
        {
            return _createConstructorParameterValue(defaultImplementationBasedObjectFactory, constructedObjectType, parameterInfo);
        }

        /// <inheritdoc />
        public CustomConstructorParameterResolverPriority Priority { get; }
    }
}