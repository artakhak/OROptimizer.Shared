using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <inheritdoc />
    public class DefaultImplementationBasedObjectFactory: IDefaultImplementationBasedObjectFactory
    {
        private readonly NoServiceProviderDiBasedObjectFactory _noServiceProviderDiBasedObjectFactory;
        private readonly DefaultImplementationBasedObjectFactoryServiceResolver _serviceResolver;

        private readonly object _lockObject = new object();

        /// <summary>
        /// Constructor.
        /// This class creates an instance of a type specified in method <see cref="CreateInstance"/> and dependencies using the default implementations,<br/>
        /// or custom type resolution provided  by <see cref="TryResolveConstructorParameterValueDelegate"/> in parameter<br/>
        /// <paramref name="tryResolveConstructorParameterValueDelegate"></paramref>.<br/>
        /// If <paramref name="tryResolveConstructorParameterValueDelegate"></paramref>.<see cref="TryResolveConstructorParameterValueDelegate"/> resolves<br/>
        /// the type, it will be used. Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly  where the type is (or if the resolved type is not an interface, an instance of a type will be created via reflection).
        /// </summary>
        /// <param name="resolvedTypeInstanceWasCreatedHandler">Event handler for <see cref="ResolvedTypeInstanceWasCreated"/> event.</param>
        /// <param name="tryResolveConstructorParameterValueDelegate">
        /// A service provider.
        /// </param>
        /// <param name="resolvedTypeInstanceCanBeCached">If the function returns true, the resolved type instance will be cached and<br/>
        /// re-used when injecting the type in other (or same) class constructor.<br/>
        /// Otherwise, a new instance of the type will be created every time it is injected.<br/>
        /// NOTE: the resolved type passed to <paramref name="resolvedTypeInstanceCanBeCached"></paramref> is the type being resolved (normally an interface),
        /// and not the implementation.
        /// </param>
        /// <param name="logger">Logger. If the value is null <see cref="LogToConsole"/> will be used.</param>
        public DefaultImplementationBasedObjectFactory(Action<ResolvedTypeInstanceWasCreated> resolvedTypeInstanceWasCreatedHandler,
            TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValueDelegate,
            Func<Type, bool> resolvedTypeInstanceCanBeCached,
            ILog logger = null)
        {
            
            LocalLoggerAmbientContext.Context = logger ?? new LogToConsole(LogLevel.Debug);

            ServiceResolutionContextData.TypeResolutionContext = new TypeResolutionContext(resolvedTypeInstanceCanBeCached, tryResolveConstructorParameterValueDelegate);
           
            _noServiceProviderDiBasedObjectFactory = new NoServiceProviderDiBasedObjectFactory(this);
            _noServiceProviderDiBasedObjectFactory.ResolvedTypeInstanceWasCreatedEvent += (sender, e) =>
            {
                resolvedTypeInstanceWasCreatedHandler(e);
            };

            _serviceResolver = new DefaultImplementationBasedObjectFactoryServiceResolver(_noServiceProviderDiBasedObjectFactory);
            _noServiceProviderDiBasedObjectFactory.ServiceResolver = _serviceResolver;
        }

        /// <inheritdoc />
        public object CreateInstance(Type typeToResolve)
        {
            lock (_lockObject)
            {
                try
                {
                    ServiceResolutionContextData.TypeResolutionContext.MainResolvedType = typeToResolve;

                    return _noServiceProviderDiBasedObjectFactory.CreateInstance(typeToResolve, _serviceResolver,
                        ServiceResolutionContextData.TypeResolutionContext.TryResolveConstructorParameterValueDelegate,
                        LocalLoggerAmbientContext.Context);
                }
                finally
                {
                    ServiceResolutionContextData.TypeResolutionContext.MainResolvedType = null;
                }
            }
        }

        /// <inheritdoc />
        public object GetOrCreateInstance(Type typeToResolve)
        {
            if (ServiceResolutionContextData.TypeResolutionContext.TypeToResolvedInstance.TryGetValue(typeToResolve, out var resolvedValue))
                return resolvedValue;

            return CreateInstance(typeToResolve);
        }

        /// <inheritdoc />
        private class DefaultImplementationBasedObjectFactoryServiceResolver : IServiceResolver
        {
            private readonly NoServiceProviderDiBasedObjectFactory _noServiceProviderDiBasedObjectFactory;

            public DefaultImplementationBasedObjectFactoryServiceResolver(NoServiceProviderDiBasedObjectFactory noServiceProviderDiBasedObjectFactory)
            {
                _noServiceProviderDiBasedObjectFactory = noServiceProviderDiBasedObjectFactory;
            }
           
            /// <inheritdoc />
            public T Resolve<T>() where T : class
            {
                return (T)Resolve(typeof(T));
            }

            /// <inheritdoc />
            public object Resolve(Type serviceType)
            {
                LocalLoggerAmbientContext.Context.ErrorFormat("The call to [{0}.{1}({2})] should have never happened.",
                    this.GetType().FullName, nameof(Resolve), nameof(serviceType));
                
                var typeResolutionContext = ServiceResolutionContextData.TypeResolutionContext;
              
                if (typeResolutionContext.TypeToResolvedInstance.TryGetValue(serviceType, out var resolvedValue))
                    return resolvedValue;

                var resolvedInstance = _noServiceProviderDiBasedObjectFactory.CreateInstance(serviceType, this,
                    typeResolutionContext.TryResolveConstructorParameterValueDelegate, LocalLoggerAmbientContext.Context);
               
                if (typeResolutionContext.ResolvedTypeInstanceCanBeCached(serviceType))
                    typeResolutionContext.TypeToResolvedInstance[serviceType] = resolvedInstance;

                return resolvedInstance;
            }
        }

        private class NoServiceProviderDiBasedObjectFactory : DiBasedObjectFactory
        {
            private readonly DefaultImplementationBasedObjectFactory _defaultImplementationBasedObjectFactory;

            /// <summary>
            /// Event handler for events raised when instance of a type is created.
            /// </summary>
            public EventHandler<ResolvedTypeInstanceWasCreated> ResolvedTypeInstanceWasCreatedEvent;

            public NoServiceProviderDiBasedObjectFactory(DefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory)
            {
                _defaultImplementationBasedObjectFactory = defaultImplementationBasedObjectFactory;
            }

            public DefaultImplementationBasedObjectFactoryServiceResolver ServiceResolver { get; set; }

            protected override object CreateInstance(Type resolvedType, ConstructorInfo constructorInfo, object[] constructorParameterValues)
            {
                var typeResolutionContext = ServiceResolutionContextData.TypeResolutionContext;
                
                if (resolvedType != typeResolutionContext.MainResolvedType && typeResolutionContext.TypeToResolvedInstance.TryGetValue(resolvedType, out var resolvedInstance))
                    return resolvedInstance;

                var createdInstance = base.CreateInstance(resolvedType, constructorInfo, constructorParameterValues);

                LocalLoggerAmbientContext.Context.DebugFormat("Resolved type [{0}] to [{1}].", resolvedType.FullName, createdInstance.GetType().FullName);

                if (typeResolutionContext.ResolvedTypeInstanceCanBeCached(resolvedType) &&
                    !typeResolutionContext.TypeToResolvedInstance.ContainsKey(resolvedType))
                    typeResolutionContext.TypeToResolvedInstance[resolvedType] = createdInstance;

                ResolvedTypeInstanceWasCreatedEvent?.Invoke(this, new ResolvedTypeInstanceWasCreated(resolvedType, createdInstance));
                return createdInstance;
            }

            protected override object ResolveConstructorParameterValue(ConstructorInfo injectedIntoConstructorInfo, System.Reflection.ParameterInfo parameterInfo)
            {
                var typeResolutionContext = ServiceResolutionContextData.TypeResolutionContext;
               
                if (typeResolutionContext.TypeToResolvedInstance.TryGetValue(parameterInfo.ParameterType, out var resolvedInstance))
                    return resolvedInstance;

                var parameterValue = this.CreateInstance(parameterInfo.ParameterType,
                    this.ServiceResolver ?? throw new InvalidOperationException($"The value of [{nameof(ServiceResolver)}] was not set."),
                    typeResolutionContext.TryResolveConstructorParameterValueDelegate,
                    LocalLoggerAmbientContext.Context);

                if (typeResolutionContext.ResolvedTypeInstanceCanBeCached(parameterInfo.ParameterType))
                    typeResolutionContext.TypeToResolvedInstance[parameterInfo.ParameterType] = parameterValue;

                return parameterValue;
            }
        }

        private class LocalLoggerAmbientContext : AmbientContext<ILog, NullLog>
        {

        }

        private static class ServiceResolutionContextData
        {
            [NotNull] public static TypeResolutionContext TypeResolutionContext { get; set; } = null;
        }

        private class TypeResolutionContext
        {
            public TypeResolutionContext([NotNull] Func<Type, bool> resolvedTypeInstanceCanBeCached, [NotNull] TryResolveConstructorParameterValueDelegate tryResolveConstructorParameterValueDelegate)
            {
                ResolvedTypeInstanceCanBeCached = resolvedTypeInstanceCanBeCached;
                TryResolveConstructorParameterValueDelegate = tryResolveConstructorParameterValueDelegate;
            }

            [NotNull]
            public Func<Type, bool> ResolvedTypeInstanceCanBeCached { get; }

            [NotNull]
            public TryResolveConstructorParameterValueDelegate TryResolveConstructorParameterValueDelegate { get; }

            [CanBeNull] 
            public Type MainResolvedType { get; set; }

            public Dictionary<Type, object> TypeToResolvedInstance { get; } = new Dictionary<Type, object>();
        }
    }
}