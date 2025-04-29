using System;
using OROptimizer.Diagnostics.Log;

namespace OROptimizer
{
    internal static class AmbientContextHelpers
    {
        internal static TContext CreateDefaultImplementation<TContext, TContextDefaultImplementation>() where TContext : class
            where TContextDefaultImplementation : class, new()
        {
            var interfaceType = typeof(TContext);
            var implementationType = typeof(TContextDefaultImplementation);

            TContext defaultContext = null;
            if (interfaceType.IsAssignableFrom(implementationType))
            {
                var constructorInfo = implementationType.GetConstructor(new Type[] { });

                if (constructorInfo != null && constructorInfo.IsPublic)
                {
                    try
                    {
                        defaultContext = (TContext)constructorInfo.Invoke(new object[] { });
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"Failed to construct an object of type '{implementationType.FullName}' using the default constructor.", e);
                    }
                }
            }

            if (defaultContext == null)
            {
                LogHelper.Context.Log.Error($"Type '{implementationType.FullName}' should be an implementation of type '{interfaceType.FullName}' and should have a public parameterless constructor.");
                throw new ApplicationException($"Invalid types specified: '{interfaceType.FullName}', '{implementationType.FullName}'.");
            }

            return defaultContext;
        }
    }
}