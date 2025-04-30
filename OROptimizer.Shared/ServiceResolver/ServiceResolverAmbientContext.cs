// This software is part of the OROptimizer library
// Copyright © 2018 OROptimizer Contributors
// http://oroptimizer.com

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace OROptimizer.ServiceResolver
{
    /// <summary>
    /// Ambient context for accessing service resolver globally, when constructor or property dependency injection
    /// does not work (for example in UWP or Windows forms applications when a control class should have a parameter-less constructor). 
    /// Example of setting context is:
    /// <see cref="ServiceResolverAmbientContext"/>.Context = new IoC.Configuration.Ninject.NinjectDiContainer(); where
    /// "IoC.Configuration.DiContainer.IDiContainer" is in IoC.Configuration.Ninject Nuget package
    /// </summary>
    public class ServiceResolverAmbientContext : AmbientContext<IServiceResolver, NullServiceResolver>
    {

    }
}