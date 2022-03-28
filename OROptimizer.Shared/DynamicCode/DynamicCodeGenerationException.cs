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
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace OROptimizer.DynamicCode
{
    /// <summary>
    /// Exception thrown when dynamic code generation fails.
    /// </summary>
    public class DynamicCodeGenerationException : Exception
    {
        /// <summary>
        /// Constructor.
        /// Use this constructor to generate an error message based on details in EmitResult.Diagnostics.
        /// </summary>
        public DynamicCodeGenerationException([NotNull] string assemblyPath, [CanBeNull] EmitResult emitResult) : 
            base(GenerateErrorMessage(assemblyPath, emitResult))
        {
            AssemblyPath = assemblyPath;
            EmitResult = emitResult;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DynamicCodeGenerationException([NotNull] string assemblyPath, [NotNull] string errorMessage, EmitResult emitResult) : base(errorMessage)
        {
            AssemblyPath = assemblyPath;
            EmitResult = emitResult;
        }

        private static string GenerateErrorMessage([NotNull] String assemblyPath, [CanBeNull] EmitResult emitResult)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"Failed to build assembly '{assemblyPath}'.");

            if (emitResult != null)
            {
                var diagnostics = emitResult.Diagnostics.Where(x =>
                    x.IsWarningAsError ||
                    x.Severity == DiagnosticSeverity.Error).ToList();

                if (diagnostics.Count > 0)
                {
                    errorMessage.AppendLine("\tError details:");
                    foreach (var diagnostic in diagnostics)
                        errorMessage.AppendLine($"\t{nameof(Diagnostic.Id)}:{diagnostic.Id}, {nameof(Diagnostic.Location)}:{diagnostic.Location}, Details: '{diagnostic.GetMessage()}', {nameof(Diagnostic.Severity)}:{diagnostic.Severity}, {nameof(Diagnostic.DefaultSeverity)}:{diagnostic.DefaultSeverity}, {nameof(Diagnostic.WarningLevel)}:{diagnostic.WarningLevel}, {nameof(Diagnostic.IsSuppressed)}:{diagnostic.IsSuppressed}.");

                }
            }

            return errorMessage.ToString();
        }

        /// <summary>
        /// Generated assembly path.
        /// </summary>
        [NotNull]
        public string AssemblyPath { get; }

        /// <summary>
        /// Dynamic code compilation result.
        /// </summary>
        public EmitResult EmitResult { get; }
    }
}