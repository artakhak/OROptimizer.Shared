﻿using System;
using JetBrains.Annotations;

namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Tries to resolve constructor parameter value. 
    /// </summary>
    /// <param name="constructedObjectType">Constructed object type. This is the type into which the value resolved by the delegate will be injected, and
    /// not the type of parameter itself. Parameter type can be found in <paramref name="parameterInfo"></paramref>.
    /// </param>
    /// <param name="parameterInfo">Constructor parameter info.</param>
    /// <returns>Returns a tuple. The first value indicates if value was resolved.
    /// If the first value is true, the second value is the resolved value.
    /// Otherwise, the second value is null.
    /// </returns>
    public delegate (bool parameterValueWasResolved, object resolvedValue) TryResolveConstructorParameterValueDelegate([NotNull] Type constructedObjectType, [NotNull] System.Reflection.ParameterInfo parameterInfo);
}