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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;

namespace OROptimizer
{
    /// <summary>
    ///     An assembly resolver class that resolves assemblies based on probing paths passed as a constructor parameter.
    ///     The method <see cref="System.IDisposable.Dispose()"/> should be called to unregister assembly resolution.
    /// </summary>
    public class AssemblyResolver : IDisposable
    {

        [NotNull]
        private readonly IEnumerable<string> _probingPaths;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyResolver" /> class. Creating an instance of this class using
        ///     this constructor will
        ///     automatically resolve assemblies using the probing paths passed as a constructor parameter.
        ///     Note, the assemblies will be re-solved if they are not already loaded into app domain, and are not resolved
        ///     somehwre else.
        /// </summary>
        /// <param name="probingPaths">The probing paths.</param>
        public AssemblyResolver([NotNull] IEnumerable<string> probingPaths)
        {
            _probingPaths = probingPaths;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyNameItems = args.Name.Split(',');

            if (assemblyNameItems == null || assemblyNameItems.Length == 0)
                return null;

            var assemblyFileName = $"{assemblyNameItems[0]}.dll";

            foreach (var probingPath in _probingPaths)
                if (new DirectoryInfo(probingPath).GetFiles().Any(fileInfo => fileInfo.Name.Equals(assemblyFileName, StringComparison.OrdinalIgnoreCase)))
                    return GlobalsCoreAmbientContext.Context.LoadAssembly(Path.Combine(probingPath, assemblyFileName));

            LogHelper.Context.Log.Error($"Failed to resolve assembly '{args.Name}'.");
            return null;
        }
    }
}