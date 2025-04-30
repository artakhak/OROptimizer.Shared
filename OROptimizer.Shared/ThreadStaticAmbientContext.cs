using System;
using JetBrains.Annotations;

namespace OROptimizer
{
    /// <summary>
    ///     A generic ambient context for to replace thread static methods (the context value set is static per thread).
    ///     Can be used for cross-cutting concerns such as logging in methods that execute in single thread.
    ///     Example of use case is using for logging in a library package that has methods that execute in single
    ///     thread (do not start threads, do not call methods that returns Task, etc).      
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TContextDefaultImplementation">The type of the context default implementation.</typeparam>
    /// <seealso cref="GlobalsCoreAmbientContext" />
    public class ThreadStaticAmbientContext<TContext, TContextDefaultImplementation>
        where TContext : class
        where TContextDefaultImplementation : class, new()
    {
        [ThreadStatic]
        private static TContext _context;
        private static readonly TContext _defaultContext;

        static ThreadStaticAmbientContext()
        {
            _defaultContext = AmbientContextHelpers.CreateDefaultImplementation<TContext, TContextDefaultImplementation>();
            SetDefaultContext();
        }

        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        /// <value>
        ///     The context.
        /// </value>
        [NotNull]
        public static TContext Context
        {
            get => _context;
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value == null)
                    SetDefaultContext();
                else
                    _context = value;
            }
        }

        /// <summary>
        ///     Sets the default context.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void SetDefaultContext()
        {
            _context = _defaultContext;
        }
    }
}