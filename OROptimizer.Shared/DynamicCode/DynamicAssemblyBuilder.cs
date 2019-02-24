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
    ///     using(var assemblyGenerator = new DynamicAssemblyBuilder("c:\Assembly1.dll", null))
    ///     {
    ///     assemblyGenerator.AddReferencedAssembly("c:\Assembly1.dll");
    ///     var cSharpFileContents = "C# file contents go here";
    ///     assemblyGenerator.AddCSharpFile(cSharpFileContents);
    ///     }
    /// </summary>
    /// <seealso cref="OROptimizer.DynamicCode.IDynamicAssemblyBuilder" />
    public class DynamicAssemblyBuilder : IDynamicAssemblyBuilder
    {
        #region Member Variables

        private readonly Dictionary<string, IDynamicallyGeneratedClass> _classFullNameToDynamicallyGeneratedClass = new Dictionary<string, IDynamicallyGeneratedClass>(StringComparer.Ordinal);

        [NotNull]
        private readonly List<string> _csharpFiles = new List<string>();


        [NotNull]
        private readonly string _dynamicAssemblyPath;

        [NotNull]
        private static object _lockObject = new object();

        [CanBeNull]
        private readonly OnDynamicAssemblyEmitComplete _onDynamicAssemblyEmitComplete;

        [NotNull]
        private readonly Dictionary<string, string> _referencedAssemblyNameToPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region  Constructors

        //static DynamicAssemblyBuilder()
        //{
        //    //var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        //    var settingValue = ConfigurationSettings.AppSettings.Get(nameof(SaveCSharpFiles));

        //    if (settingValue != null && bool.TryParse(settingValue, out SaveCSharpFiles) && SaveCSharpFiles)
        //    {
        //        SavedCSharpFilesFolder = ConfigurationSettings.AppSettings.Get(nameof(SavedCSharpFilesFolder));

        //        if (SavedCSharpFilesFolder != null)
        //        {
        //            if (!Directory.Exists(SavedCSharpFilesFolder))
        //            {
        //                LogHelper.Context.Log.WarnFormat("Folder '{0}' specified in settin '{1}' was not found.",
        //                    SavedCSharpFilesFolder, nameof(SavedCSharpFilesFolder));

        //                SavedCSharpFilesFolder = null;
        //            }
        //        }
        //    }
        //}  

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicAssemblyBuilder" /> class.
        /// </summary>
        /// <param name="dynamicAssemblyPath">The dynamic assembly path.</param>
        /// <param name="onDynamicAssemblyEmitComplete">The on dynamic assembly emit complete.</param>
        public DynamicAssemblyBuilder([NotNull] string dynamicAssemblyPath,
                                      [CanBeNull] OnDynamicAssemblyEmitComplete onDynamicAssemblyEmitComplete)
        {
            LogHelper.Context.Log.InfoFormat("Started compiling dynamic assembly '{0}'.", dynamicAssemblyPath);

            _dynamicAssemblyPath = dynamicAssemblyPath;
            _onDynamicAssemblyEmitComplete = onDynamicAssemblyEmitComplete;

            // Add common .Net assemblies
            var dotNetDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            AddReferencedAssembly(typeof(object));
            AddReferencedAssembly(Path.Combine(dotNetDirectory, "System.Runtime.dll"));
            AddReferencedAssembly(Path.Combine(dotNetDirectory, "netstandard.dll"));
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

        #endregion

        #region IDynamicAssemblyBuilder Interface Implementation

        /// <summary>
        ///     Adds the c# sharp file to assembly generator.
        /// </summary>
        /// <param name="cSharpFileContents">The c sharp file contents.</param>
        public void AddCSharpFile(string cSharpFileContents)
        {
#if DEBUG
#if DYNAMIC_CODE_DIAGNOSTICS_TYPE_1
            try
            {
                // Save the file in debug mode for testing purposes

                var cSharpFilesFolder = Path.Combine(Path.GetDirectoryName(_dynamicAssemblyPath), "CSharpFiles");

                if (!Directory.Exists(cSharpFilesFolder))
                    Directory.CreateDirectory(cSharpFilesFolder);

                using (var streamWriter = new StreamWriter(Path.Combine(cSharpFilesFolder, $"DynamicFile_{_csharpFiles.Count}.cs")))
                {
                    streamWriter.Write(cSharpFileContents);
                }
            }
            catch (Exception e)
            {
                LogHelper.Context.Log.Error("Failed to save the generated C# file in diagnostics mode.", e);
            }
#endif
#endif
            _csharpFiles.Add(cSharpFileContents);
        }

        /// <summary>
        ///     Adds a reference to assembly <paramref name="type" />.Assembly in the generated dynamic assembly.
        /// </summary>
        /// <param name="type">The type.</param>
        public void AddReferencedAssembly(Type type)
        {
            AddReferencedAssembly(type.Assembly.GetName().Name, type.Assembly.Location);
        }

        /// <summary>
        ///     Adds a reference to assembly <paramref name="assemblyPath" /> in the generated dynamic assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        public void AddReferencedAssembly(string assemblyPath)
        {
            AddReferencedAssembly(Path.GetFileNameWithoutExtension(assemblyPath), assemblyPath);
        }

        /// <summary>
        ///     Gets the build status.
        /// </summary>
        /// <value>
        ///     The build status.
        /// </value>
        public AssemblyBuildStatus BuildStatus { get; private set; } = AssemblyBuildStatus.Started;

        [NotNull]
        public string DefaultNamespace { get; } = $"DynamicImplementations_{GlobalsCoreAmbientContext.Context.GenerateUniqueId()}";

        /// <summary>
        ///     Call this method to finalize the assembly generation.
        /// </summary>
        public void Dispose()
        {
            EmitResult compilationResult = null;
            if (BuildStatus == AssemblyBuildStatus.Started)
            {
                var referencedAssembliesMetadata = new List<MetadataReference>();

                foreach (var assemblyPath in _referencedAssemblyNameToPathMap.Values)
                    referencedAssembliesMetadata.Add(MetadataReference.CreateFromFile(assemblyPath));

                var syntaxTrees = new List<SyntaxTree>();

                foreach (var cSharpFile in _csharpFiles)
                    syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(cSharpFile));

                var csharpCompilation = CSharpCompilation.Create(Path.GetFileName(_dynamicAssemblyPath),
                    syntaxTrees, referencedAssembliesMetadata, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                string pdbPath = null;

#if DEBUG
                pdbPath = Path.Combine(Path.GetDirectoryName(_dynamicAssemblyPath), $"{Path.GetFileNameWithoutExtension(_dynamicAssemblyPath)}.pdb");
#endif
                compilationResult = csharpCompilation.Emit(_dynamicAssemblyPath, pdbPath);
            }

            if (compilationResult != null && compilationResult.Success)
            {
                BuildStatus = AssemblyBuildStatus.Succeeded;
                LogHelper.Context.Log.InfoFormat("Successfully compiled dynamic assembly '{0}'.", _dynamicAssemblyPath);
            }
            else
            {
                BuildStatus = AssemblyBuildStatus.Failed;
                LogHelper.Context.Log.ErrorFormat("Compilation of dynamic assembly '{0}' failed.", _dynamicAssemblyPath);

                if (compilationResult != null)
                {
                    var diagnosticsCount = Math.Min(compilationResult.Diagnostics.Length, 100);
                    for (var i = 0; i < diagnosticsCount; ++i)
                    {
                        var diagnostic = compilationResult.Diagnostics[i];
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                            LogHelper.Context.Log.Error(diagnostic.ToString());
                    }
                }
            }

            _onDynamicAssemblyEmitComplete?.Invoke(_dynamicAssemblyPath, compilationResult?.Success ?? false, compilationResult);
        }

        /// <inheritdoc />
        public void FinalizeDynamicallyGeneratedClass(string className, string classNamespace)
        {
            var dynamicallyGeneratedClass = GetDynamicallyGeneratedClass(className, classNamespace);

            if (dynamicallyGeneratedClass == null)
            {
                LogHelper.Context.Log.Error($"Class '{GetClassFullName(className, classNamespace)}' was never started.");
                return;
            }

            if (dynamicallyGeneratedClass.IsFinalized)
            {
                LogHelper.Context.Log.Error($"Class '{GetClassFullName(className, classNamespace)}' was already finalized.");
                return;
            }

            dynamicallyGeneratedClass.FinalizeAndAddToAssembly();
        }


        /// <inheritdoc />
        public IDynamicallyGeneratedClass GetDynamicallyGeneratedClass(string className, string classNamespace = null)
        {
            return _classFullNameToDynamicallyGeneratedClass.TryGetValue(
                GetClassFullName(className, classNamespace), out var dynamicallyGeneratedClass)
                ? dynamicallyGeneratedClass
                : null;
        }

        /// <summary>
        ///     Call this method if the assembly generation should be aborted.
        /// </summary>
        public void SetIsAborted()
        {
            BuildStatus = AssemblyBuildStatus.Aborted;
        }

        /// <inheritdoc />
        public IDynamicallyGeneratedClass StartDynamicallyGeneratedClass(string className, string classNamespace)
        {
            if (classNamespace == null)
                classNamespace = DefaultNamespace;

            var classFullName = GetClassFullName(className, classNamespace);

            if (_classFullNameToDynamicallyGeneratedClass.TryGetValue(classFullName, out var dynamicallyGeneratedClass))
            {
                LogHelper.Context.Log.WarnFormat("Class '{0}' was already started.", classFullName);
                return dynamicallyGeneratedClass;
            }

            dynamicallyGeneratedClass = new DynamicallyGeneratedClass(this, className, classNamespace);
            _classFullNameToDynamicallyGeneratedClass[classFullName] = dynamicallyGeneratedClass;

            return dynamicallyGeneratedClass;
        }

        #endregion

        #region Member Functions

        private void AddReferencedAssembly(string assemblyName, string assemblyPath)
        {
            if (_referencedAssemblyNameToPathMap.ContainsKey(assemblyName))
                return;

            _referencedAssemblyNameToPathMap[assemblyName] = assemblyPath;
        }

        private string GetClassFullName(string className, string classNamespace)
        {
            if (classNamespace == null)
                classNamespace = DefaultNamespace;

            return $"{classNamespace}.{className}";
        }

        #endregion
    }
}