using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OROptimizer.ServiceResolver.DefaultImplementationBasedObjectFactory
{
    /// <summary>
    /// Default implementation of <see cref="IDefaultImplementationBasedObjectFactoryEx"/>.
    /// </summary>
    public class DefaultImplementationBasedObjectFactory : IDefaultImplementationBasedObjectFactoryEx, IDisposable
    {
        private readonly Func<Type, bool> _resolvedTypeInstanceCanBeCached;
        private readonly object _lockObject = new object();

        [NotNull, ItemNotNull]
        private readonly LinkedList<ICustomConstructorParameterResolver> _customConstructorParameterResolvers = new LinkedList<ICustomConstructorParameterResolver>();

        private readonly Dictionary<Type, object> _typeToCachedInstances = new Dictionary<Type, object>();

        private readonly Stack<Type> _currentlyResolvedTypeStack = new Stack<Type>();

        private readonly ILog _logger;
        private readonly DiBasedObjectFactoryEx _diBasedObjectFactoryEx;
        private readonly ServiceResolver _serviceResolver;
        private bool _isDisposed = false;

        /// <summary>
        /// Constructor.
        /// This class creates an instance of a type specified in method <see cref="CreateInstance"/> and dependencies using the default implementations,<br/>
        /// or custom type resolution provided  by <see cref="ICustomConstructorParameterResolver"/> registered by method <see cref="IDefaultImplementationBasedObjectFactoryEx.RegisterCustomConstructorParameterResolvers"/>.<br/>
        /// If <see cref="ICustomConstructorParameterResolver"/> resolves<br/>
        /// the type, it will be used. Otherwise, any type in dependencies will transitively be resolved using default implementations of interfaces found<br/>
        /// in assembly  where the type is (or if the resolved type is not an interface, an instance of a type will be created via reflection).
        /// </summary>
        /// <param name="resolvedTypeInstanceCanBeCached">If the function returns true, the resolved type instance will be cached and<br/>
        /// re-used when injecting the type in other (or same) class constructor.<br/>
        /// Otherwise, a new instance of the type will be created every time it is injected.<br/>
        /// NOTE: the resolved type passed to <paramref name="resolvedTypeInstanceCanBeCached"></paramref> is the type being resolved (normally an interface),
        /// and not the implementation.
        /// </param>
        /// <param name="logger">Logger. If the value is null <see cref="LogToConsole"/> will be used.</param>
        public DefaultImplementationBasedObjectFactory(
            Func<Type, bool> resolvedTypeInstanceCanBeCached,
            ILog logger = null)
        {
            _resolvedTypeInstanceCanBeCached = resolvedTypeInstanceCanBeCached;
            _logger = logger ?? new LogToConsole(LogLevel.Debug);

            _diBasedObjectFactoryEx = new DiBasedObjectFactoryEx(this);
            _serviceResolver = new ServiceResolver(this);
        }

        /// <inheritdoc />
        public object CreateInstance(Type typeToResolve)
        {
            LocalLoggerAmbientContext.Context = this._logger;

            lock (_lockObject)
            {
                try
                {
                    _currentlyResolvedTypeStack.Push(typeToResolve);

                    return _diBasedObjectFactoryEx.CreateInstance(typeToResolve, _serviceResolver, TryResolveConstructorParameterValue,
                        _logger);
                }
                finally
                {

                    if (_currentlyResolvedTypeStack.Count > 0)
                    {
                        var currentlyResolvedType = _currentlyResolvedTypeStack.Peek();

                        if (currentlyResolvedType == typeToResolve)
                        {
                            _currentlyResolvedTypeStack.Pop();
                        }
                        else
                        {
                            this._logger.ErrorFormat("Invalid state reached. The top type in stack [{0}] should be [{1}].",
                                nameof(_currentlyResolvedTypeStack), typeToResolve);
                        }
                    }
                    else
                    {
                        this._logger.ErrorFormat("Invalid state reached. The stack [{0}] is empty after the type [{1}] was resolved.",
                            nameof(_currentlyResolvedTypeStack), typeToResolve);

                    }
                }
            }
        }

        /// <inheritdoc />
        public object GetOrCreateInstance(Type typeToResolve)
        {
            LocalLoggerAmbientContext.Context = this._logger;

            lock (_lockObject)
            {
                if (!_typeToCachedInstances.TryGetValue(typeToResolve, out var resolvedValue))
                {
                    resolvedValue = CreateInstance(typeToResolve);
                    _typeToCachedInstances[typeToResolve] = resolvedValue;
                }

                return resolvedValue;
            }
        }

        private (bool parameterValueWasResolved, object resolvedValue) TryResolveConstructorParameterValue([NotNull] Type constructedObjectType, [NotNull] System.Reflection.ParameterInfo parameterInfo)
        {
            if (_typeToCachedInstances.TryGetValue(parameterInfo.ParameterType, out var cachedValue))
                return (true, cachedValue);

            foreach (var customConstructorParameterResolver in _customConstructorParameterResolvers)
            {
                var result = customConstructorParameterResolver.CreateConstructorParameterValue(
                    this, constructedObjectType, parameterInfo);

                if (result.parameterValueWasResolved)
                {
                    if (_resolvedTypeInstanceCanBeCached(parameterInfo.ParameterType) && result.resolvedValue != null)
                        _typeToCachedInstances[parameterInfo.ParameterType] = result.resolvedValue;

                    
                    if (result.resolvedValue != null && DiBasedObjectFactoryParametersContext.Context.LogDiagnosticsData)
                        LocalLoggerAmbientContext.Context.InfoFormat("Created an instance of type [{0}] for type [{1}] using custom constructor parameter resolver [{2}].",
                            result.resolvedValue.GetType().FullName ?? String.Empty,
                            parameterInfo.ParameterType.FullName,
                            customConstructorParameterResolver.Identifier);

                    return result;
                }
            }

            return (false, null);
        }

        public event EventHandler<ResolvedTypeInstanceWasCreated> ResolvedTypeInstanceWasCreated;

        #region IDefaultImplementationBasedObjectFactoryEx
        /// <inheritdoc />
        public IEnumerable<ICustomConstructorParameterResolver> GetCustomConstructorParameterResolvers()
        {
            lock (_lockObject)
            {
                return _customConstructorParameterResolvers;
            }
        }

        /// <inheritdoc />
        public void RemoveAllCustomConstructorParameterResolvers()
        {
            lock (_lockObject)
            {
                _customConstructorParameterResolvers.Clear();
            }
        }

        /// <inheritdoc />
        public void RegisterCustomConstructorParameterResolvers(ICustomConstructorParameterResolver customConstructorParameterResolver)
        {
            lock (_lockObject)
            {
                var currentResolverNode = this._customConstructorParameterResolvers.First;

                while (currentResolverNode != null)
                {
                    // If new resolver has higher or similar priority than the current one, lets insert it before the current one.
                    // We want the resolvers registered most recently to take priority over resolvers registered earlier (even if both have the same priority)
                    if (customConstructorParameterResolver.Priority >= currentResolverNode.Value.Priority)
                    {
                        _customConstructorParameterResolvers.AddBefore(currentResolverNode, customConstructorParameterResolver);
                        return;
                    }

                    currentResolverNode = currentResolverNode.Next;
                }

                this._customConstructorParameterResolvers.AddLast(customConstructorParameterResolver);
            }
        }

        /// <inheritdoc />
        public void UnregisterCustomConstructorParameterResolver(Guid customConstructorParameterResolverIdentifier)
        {
            lock (_lockObject)
            {
                var currentResolverNode = this._customConstructorParameterResolvers.First;

                while (currentResolverNode != null)
                {
                    if (currentResolverNode.Value.Identifier == customConstructorParameterResolverIdentifier)
                    {
                        _customConstructorParameterResolvers.Remove(currentResolverNode);
                        return;
                    }

                    currentResolverNode = currentResolverNode.Next;
                }
            }
        }
        #endregion

        private class LocalLoggerAmbientContext : ThreadStaticAmbientContext<ILog, NullLog>
        {

        }

        private class ServiceResolver : IServiceResolver
        {
            private readonly DefaultImplementationBasedObjectFactory _defaultImplementationBasedObjectFactory;

            public ServiceResolver(DefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory)
            {
                _defaultImplementationBasedObjectFactory = defaultImplementationBasedObjectFactory;
            }

            public T Resolve<T>() where T : class
            {
                return (T)Resolve(typeof(T));
            }

            public object Resolve(Type type)
            {
                return _defaultImplementationBasedObjectFactory._diBasedObjectFactoryEx.CreateInstance(type, this,
                    this._defaultImplementationBasedObjectFactory.TryResolveConstructorParameterValue, this._defaultImplementationBasedObjectFactory._logger);
            }
        }

        private class DiBasedObjectFactoryEx : DiBasedObjectFactory
        {
            private readonly DefaultImplementationBasedObjectFactory _defaultImplementationBasedObjectFactory;

            public DiBasedObjectFactoryEx(DefaultImplementationBasedObjectFactory defaultImplementationBasedObjectFactory)
            {
                _defaultImplementationBasedObjectFactory = defaultImplementationBasedObjectFactory;
            }

            protected override object CreateInstance(Type resolvedType, ConstructorInfo constructorInfo, object[] constructorParameterValues)
            {
                if ((_defaultImplementationBasedObjectFactory._currentlyResolvedTypeStack.Count == 0 ||
                    _defaultImplementationBasedObjectFactory._currentlyResolvedTypeStack.Peek() != resolvedType) &&
                    _defaultImplementationBasedObjectFactory._typeToCachedInstances.TryGetValue(resolvedType, out var resolvedInstance))
                    return resolvedInstance;

                var createdInstance = base.CreateInstance(resolvedType, constructorInfo, constructorParameterValues);

               

                if (DiBasedObjectFactoryParametersContext.Context.LogDiagnosticsData)
                    LocalLoggerAmbientContext.Context.InfoFormat("Created an instance of type [{0}] for type [{1}].",
                        createdInstance.GetType().FullName,
                        resolvedType.FullName);

                if (_defaultImplementationBasedObjectFactory._resolvedTypeInstanceCanBeCached(resolvedType))
                {
                    _defaultImplementationBasedObjectFactory._typeToCachedInstances[resolvedType] = createdInstance;
                }

                _defaultImplementationBasedObjectFactory.ResolvedTypeInstanceWasCreated?.Invoke(_defaultImplementationBasedObjectFactory,
                    new ResolvedTypeInstanceWasCreated(resolvedType, createdInstance));

                return createdInstance;
            }
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
            }

            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                foreach (var cachedValue in _typeToCachedInstances.Values)
                {
                    if (cachedValue is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }
    }
}