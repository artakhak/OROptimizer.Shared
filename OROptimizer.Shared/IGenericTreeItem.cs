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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace OROptimizer
{
    public interface IGenericTreeItem<TTreeItem> where TTreeItem : class
    {
        [NotNull]
        TTreeItem TreeItemValue { get; }

        [CanBeNull]
        IGenericTreeItem<TTreeItem> ParentTreeItem { get; }

        [NotNull, ItemNotNull]
        List<IGenericTreeItem<TTreeItem>> ChildTreeItems { get; }
    }

    public class GenericTreeItem<TTreeItem> : IGenericTreeItem<TTreeItem> where TTreeItem : class
    {
        public GenericTreeItem([NotNull] TTreeItem treeItemValue, [CanBeNull] IGenericTreeItem<TTreeItem> parentTreeItem = null)
        {
            TreeItemValue = treeItemValue;
            ParentTreeItem = parentTreeItem;
        }

        public TTreeItem TreeItemValue { get; }

        public IGenericTreeItem<TTreeItem> ParentTreeItem { get; }

        public List<IGenericTreeItem<TTreeItem>> ChildTreeItems { get; } = new List<IGenericTreeItem<TTreeItem>>();
    }
}
