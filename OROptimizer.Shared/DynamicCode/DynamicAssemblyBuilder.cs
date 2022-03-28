// This software is part of the IoC.Configuration library
// Copyright © 2018 IoC.Configuration Contributors
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
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OROptimizer.Diagnostics.Log;
using static OROptimizer.Delegates;

namespace OROptimizer.DynamicCode
{
    /// <summary>
    ///     Dynamic assembly generator for C# files.
    ///     Example:
    ///     <para />
    ///     using(var assemblyBuilder = new DynamicAssemblyBuilder("c:\Assembly1.dll", null))
    ///     {
    ///     assemblyBuilder.AddReferencedAssembly("c:\Assembly1.dll");
    ///     var cSharpFileContents = "C# file contents go here";
    ///     assemblyBuilder.AddCSharpFile(cSharpFileContents);
    ///     }
    /// </summary>
    /// <seealso cref="OROptimizer.DynamicCode.IDynamicAssemblyBuilder" />
    public class DynamicAssemblyBuilder : IDynamicAssemblyBuilder
    {
        [NotNull] 
        private readonly DynamicAssemblyBuilderParameters _dynamicAssemblyBuilderParameters;

        [NotNull]
        private readonly Dictionary<string, IDynamicallyGeneratedClass> _classFullNameToDynamicallyGeneratedClass = new Dictionary<string, IDynamicallyGeneratedClass>(StringComparer.Ordinal);

        [NotNull, ItemNotNull]
        private readonly List<string> _csharpFiles = new List<string>();

        [NotNull]
        private readonly Dictionary<string, string> _referencedAssemblyNameToPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private bool _isDisposed;

        [NotNull]
        private readonly object _lockObject = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicAssemblyBuilder" /> class.
        /// </summary>
        /// <param name="dynamicAssemblyPath">The dynamic assembly path.</param>
        /// <param name="onDynamicAssemblyEmitComplete">The on dynamic assembly emit complete.</param>
        public DynamicAssemblyBuilder([NotNull] string dynamicAssemblyPath,
                                      [CanBeNull] OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete) : 
            this(new DynamicAssemblyBuilderParameters(dynamicAssemblyPath)
            {
                OnDynamicAssemblyEmitComplete = onDynamicAssemblyEmitComplete
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAssemblyBuilder" /> class using instance of <see cref="DynamicAssemblyBuilderParameters"/> as a constructor parameter.
        /// </summary>
        /// <param name="dynamicAssemblyBuilderParameters"></param>
        public DynamicAssemblyBuilder([NotNull] DynamicAssemblyBuilderParameters dynamicAssemblyBuilderParameters)
        {
            _dynamicAssemblyBuilderParameters = dynamicAssemblyBuilderParameters;
            LogHelper.Context.Log.InfoFormat("Started compiling dynamic assembly '{0}'.", _dynamicAssemblyBuilderParameters.DynamicAssemblyPath);

            AddReferencedAssembly(typeof(object));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicAssemblyBuilder" /> class.
        /// </summary>
        /// <param name="dynamicAssemblyPath">The dynamic assembly path.</param>
        /// <param name="onDynamicAssemblyEmitComplete">The on dynamic assembly emit complete.</param>
        /// <param name="defaultNameSpace">The default name space.</param>
        public DynamicAssemblyBuilder([NotNull] string dynamicAssemblyPath,
                                      [CanBeNull] OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete,
                                      [NotNull] string defaultNameSpace) : this(dynamicAssemblyPath, onDynamicAssemblyEmitComplete)
        {
            DefaultNamespace = defaultNameSpace;
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedClass StartDynamicallyGeneratedClass(string className, string classNamespace)
        {
            return StartDynamicallyGeneratedClass(className, new string[0], classNamespace);
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedClass StartDynamicallyGeneratedClass(string className, IEnumerable<string> baseClassesAndInterfaces, string classNamespace = null)
        {
            if (classNamespace == null)
                classNamespace = DefaultNamespace;

            var classFullName = GetClassFullName(className, classNamespace);

            if (_classFullNameToDynamicallyGeneratedClass.TryGetValue(classFullName, out var dynamicallyGeneratedClass))
            {
                LogHelper.Context.Log.WarnFormat("Class '{0}' was already started.", classFullName);
                return dynamicallyGeneratedClass;
            }

            dynamicallyGeneratedClass = new DynamicallyGeneratedClass(className, classNamespace, baseClassesAndInterfaces);
            _classFullNameToDynamicallyGeneratedClass[classFullName] = dynamicallyGeneratedClass;

            return dynamicallyGeneratedClass;
        }

        /// <inheritdoc />
        public void AddCSharpFile(string cSharpFileContents)
        {
            _csharpFiles.Add(cSharpFileContents);
        }

        /// <inheritdoc />
        public void AddReferencedAssembly(Type type)
        {
            AddReferencedAssembly(type.Assembly.GetName().Name, type.Assembly.Location);
        }

        /// <inheritdoc />
        public void AddReferencedAssembly(string assemblyPath)
        {
            AddReferencedAssembly(Path.GetFileNameWithoutExtension(assemblyPath), assemblyPath);
        }

        /// <inheritdoc />
        public AssemblyBuildStatus BuildStatus { get; private set; } = AssemblyBuildStatus.Started;

        /// <inheritdoc />
        public string DefaultNamespace { get; } = $"DynamicImplementations_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";

        private IReadOnlyList<string> GetAllCSharpFiles()
        {
            List<string> cSharpFiles = new List<string>(_csharpFiles.Count + 100);
            cSharpFiles.AddRange(_csharpFiles);

            foreach (var dynamicallyGeneratedClass in _classFullNameToDynamicallyGeneratedClass.Values)
                cSharpFiles.Add(dynamicallyGeneratedClass.GenerateCSharpFile());

#if DEBUG
#if DYNAMIC_CODE_DIAGNOSTICS_TYPE_1
            foreach (var cSharpFileContents in cSharpFiles)
            {
                try
                {
                    // Save the file in debug mode for testing purposes

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var cSharpFilesFolder = Path.Combine(Path.GetDirectoryName(_dynamicAssemblyBuilderParameters.DynamicAssemblyPath), "CSharpFiles");

                    if (!Directory.Exists(cSharpFilesFolder))
                        Directory.CreateDirectory(cSharpFilesFolder);

                    using (var streamWriter = new StreamWriter(Path.Combine(cSharpFilesFolder, $"DynamicFile_{cSharpFiles.Count}.cs")))
                    {
                        streamWriter.Write(cSharpFileContents);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Context.Log.Error("Failed to save the generated C# file in diagnostics mode.", e);
                }
            }
#endif
#endif
            return cSharpFiles;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            lock (_lockObject)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
            }

            if (!isDisposing)
                return;

            // Dispose of managed resources here.
            
            EmitResult emitResult = null;
            try
            {
                
                if (BuildStatus == AssemblyBuildStatus.Started)
                {
                    var referencedAssembliesMetadata = new List<MetadataReference>();

                    foreach (var assemblyPath in _referencedAssemblyNameToPathMap.Values)
                        referencedAssembliesMetadata.Add(MetadataReference.CreateFromFile(assemblyPath));

                    var syntaxTrees = new List<SyntaxTree>();

                    var cSharpFiles = GetAllCSharpFiles();

                    foreach (var cSharpFile in cSharpFiles)
                        syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(cSharpFile));

                    var csharpCompilation = CSharpCompilation.Create(Path.GetFileName(_dynamicAssemblyBuilderParameters.DynamicAssemblyPath),
                        syntaxTrees, referencedAssembliesMetadata, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                    string pdbPath = null;

#if DEBUG
                    //pdbPath = Path.Combine(Path.GetDirectoryName(_dynamicAssemblyPath), $"{Path.GetFileNameWithoutExtension(_dynamicAssemblyPath)}.pdb");
#endif
                    emitResult = csharpCompilation.Emit(_dynamicAssemblyBuilderParameters.DynamicAssemblyPath, pdbPath);
                }
            }
            catch (Exception e)
            {
                LogHelper.Context.Log.Error($"Failed to build an assembly {_dynamicAssemblyBuilderParameters.DynamicAssemblyPath}.", e);
            }
            finally
            {
                if (emitResult != null && emitResult.Success)
                {
                    BuildStatus = AssemblyBuildStatus.Succeeded;
                    LogHelper.Context.Log.InfoFormat("Successfully compiled dynamic assembly '{0}'.", _dynamicAssemblyBuilderParameters.DynamicAssemblyPath);
                }
                else
                {
                    LogHelper.Context.Log.Error($"Failed to build an assembly {_dynamicAssemblyBuilderParameters.DynamicAssemblyPath}.");
                }

                _dynamicAssemblyBuilderParameters.OnDynamicAssemblyEmitComplete?.Invoke(_dynamicAssemblyBuilderParameters.DynamicAssemblyPath, emitResult?.Success??false, emitResult);
            }
        }

        /// <summary>
        ///     Call this method to finalize the assembly generation.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        [Obsolete]
        public void FinalizeDynamicallyGeneratedClass(string className, string classNamespace)
        {
            LogHelper.Context.Log.ErrorFormat("Method {0} was deprecated.", nameof(FinalizeDynamicallyGeneratedClass));
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedClass GetDynamicallyGeneratedClass(string className, string classNamespace = null)
        {
            return _classFullNameToDynamicallyGeneratedClass.TryGetValue(
                GetClassFullName(className, classNamespace), out var dynamicallyGeneratedClass) ? dynamicallyGeneratedClass : null;
        }

        /// <inheritdoc />
        public void SetIsAborted()
        {
            BuildStatus = AssemblyBuildStatus.Aborted;
        }

        private void AddReferencedAssembly(string assemblyName, string assemblyPath)
        {
            if (_referencedAssemblyNameToPathMap.TryGetValue(assemblyName, out var previouslyAddedAssemblyPath))
            {
                if (!string.Equals(previouslyAddedAssemblyPath, assemblyPath, StringComparison.OrdinalIgnoreCase))
                    LogHelper.Context.Log.WarnFormat("Assembly '{0}' will not be added as a reference, since it was already added as a reference to assembly '{1}' from '{2}'.",
                        assemblyPath, _dynamicAssemblyBuilderParameters.DynamicAssemblyPath, previouslyAddedAssemblyPath);
                
                return;
            }

            _referencedAssemblyNameToPathMap[assemblyName] = assemblyPath;
        }

        private string GetClassFullName(string className, string classNamespace)
        {
            if (classNamespace == null)
                classNamespace = DefaultNamespace;

            return $"{classNamespace}.{className}";
        }
    }

    /// <summary>
    /// Parameters for <see cref="DynamicAssemblyBuilder"/>.
    /// </summary>
    public sealed class DynamicAssemblyBuilderParameters
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dynamicAssemblyPath">The file path of generated assembly.</param>
        public DynamicAssemblyBuilderParameters([NotNull] string dynamicAssemblyPath)
        {
            DynamicAssemblyPath = dynamicAssemblyPath;
        }

        /// <summary>
        /// The file path of generated assembly.
        /// </summary>
        [NotNull]
        public string DynamicAssemblyPath { get; }

        /// <summary>
        /// A delegate that will be executed when the assembly generation is complete.
        /// Use this method to log compilation error details in <see cref="EmitResult.Diagnostics"/> if necessary.
        /// </summary>
        [CanBeNull]
        public OnDynamicAssemblyEmitComplete OnDynamicAssemblyEmitComplete { get; set; }
    }
}