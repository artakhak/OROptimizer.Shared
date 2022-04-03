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
using System.Linq;

namespace OROptimizer.Utilities
{
    /// <summary>
    /// File/folder utility methods.
    /// </summary>
    public static class FilePathHelpers
    {
        /// <summary>
        /// Returns an absolute path in relation to folder specified in <paramref name="directoryPath"/> given a relative file or folder path
        /// specified in <paramref name="fileOrFolderRelativePath"/> (path relative to <paramref name="fileOrFolderRelativePath"/>)
        /// </summary>
        /// <returns>Returns a tuple of three value.
        /// The first value (i.e., isSuccess), is a boolean value for success or failure.
        /// The second value (i.e., absoluteFilePath) is the generated absolute file path. This value is null only if the first value is false.
        /// The third value (i.e., errorMessage) is the error message if the first value is false. Otherwise, if the first value (i.e., isSuccess)
        /// is true, the third value is null.
        /// </returns>
        public static (bool isSuccess, string absoluteFilePath,  String errorMessage) TryGetAbsoluteFilePath(string directoryPath, string fileOrFolderRelativePath)
        {
            fileOrFolderRelativePath = fileOrFolderRelativePath.Trim();

            if (fileOrFolderRelativePath.Length == 0)
                return (false, null, "No file specified.");

            if (File.Exists(fileOrFolderRelativePath))            
                return (true, fileOrFolderRelativePath, null);

            if (fileOrFolderRelativePath.StartsWith(@"..\"))
            {
                var folderPathComponents = directoryPath.Split('\\');
                var includedTemplateRelativePathComponents = fileOrFolderRelativePath.Split('\\');

                var numberOfParentFolderReferences = 0;

                for (var i = 0; i < includedTemplateRelativePathComponents.Length; ++i)
                {
                    if (includedTemplateRelativePathComponents[i].Trim() == "..")
                    {
                        ++numberOfParentFolderReferences;

                        if (numberOfParentFolderReferences >= folderPathComponents.Length ||
                            numberOfParentFolderReferences == includedTemplateRelativePathComponents.Length)
                            return (false, null, "Too many references to parent folder in relative file path '{fileOrFolderRelativePath}'.");
                    }
                    else
                    {
                        break;
                    }
                }

                var filePathItems = new List<string>(folderPathComponents.Take(folderPathComponents.Length - numberOfParentFolderReferences));

                // Uncomment this line and remove the next one, once the package uses latest C#. 
                //filePathItems.AddRange(includedTemplateRelativePathComponents.TakeLast(includedTemplateRelativePathComponents.Length - numberOfParentFolderReferences));
                for (var i = numberOfParentFolderReferences; i < includedTemplateRelativePathComponents.Length; ++i)
                    filePathItems.Add(includedTemplateRelativePathComponents[i]);

                return(true, Path.Combine(filePathItems.ToArray()), null);
            }

            return (true, Path.Combine(directoryPath, fileOrFolderRelativePath), null);
        }
    }
}