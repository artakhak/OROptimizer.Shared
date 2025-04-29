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
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace OROptimizer.Utilities
{
    /// <summary>
    /// A delegate that processes a tree item of type <typeparamref name="TTreeItem"/>.
    /// </summary>
    /// <typeparam name="TTreeItem">Type of items to process.</typeparam>
    /// <param name="treeItem">A tree item being processed.</param>
    /// <returns>Returns a tuple with two items.
    /// If the first value is true, then processing of siblings and children of <paramref name="treeItem"/> will stop.
    /// If the second value is true, then processing of children of <paramref name="treeItem"/> will stop, however siblings of <paramref name="treeItem"/> will continue.
    /// Otherwise, if both values are false, processing of both siblings and children of <paramref name="treeItem"/> will continue.
    /// </returns>
    public delegate Task<(bool stopProcessing, bool stopProcessingChildren)> ProcessTreeItemAsyncDelegate<in TTreeItem>([NotNull] TTreeItem treeItem) where TTreeItem : class;

    /// <summary>
    /// A delegate that processes a tree item of type <typeparamref name="TTreeItem"/>.
    /// </summary>
    /// <typeparam name="TTreeItem">Type of items to process.</typeparam>
    /// <param name="treeItem">A tree item being processed.</param>
    /// <returns>Returns true, if processing should continue. Returns false, if processing should stop.</returns>
    public delegate Task<bool> ProcessTreeItemAfterChildrenProcessedAsyncDelegate<in TTreeItem>([NotNull] TTreeItem treeItem) where TTreeItem : class;

    /// <summary>
    /// A delegate that processes a tree item of type <typeparamref name="TTreeItem"/>.
    /// </summary>
    /// <typeparam name="TTreeItem">Type of items to process.</typeparam>
    /// <param name="treeItem">A tree item being processed.</param>
    /// <returns>Returns a tuple with two items.
    /// If the first value is true, then processing of siblings and children of <paramref name="treeItem"/> will stop.
    /// If the second value is true, then processing of children of <paramref name="treeItem"/> will stop, however siblings of <paramref name="treeItem"/> will continue.
    /// Otherwise, if both values are false, processing of both siblings and children of <paramref name="treeItem"/> will continue.
    /// </returns>
    public delegate (bool stopProcessing, bool stopProcessingChildren) ProcessTreeItemDelegate<in TTreeItem>([NotNull] TTreeItem treeItem) where TTreeItem : class;

    /// <summary>
    /// A delegate that processes a tree item of type <typeparamref name="TTreeItem"/>.
    /// </summary>
    /// <typeparam name="TTreeItem">Type of items to process.</typeparam>
    /// <param name="treeItem">A tree item being processed.</param>
    /// <returns>Returns true, if processing should continue. Returns false, if processing should stop.</returns>
    public delegate bool ProcessTreeItemAfterChildrenProcessedDelegate<in TTreeItem>([NotNull] TTreeItem treeItem) where TTreeItem : class;

    /// <summary>
    /// Helper class for recursively processing tree items.
    /// </summary>
    public static class TreeProcessor
    {
        /// <summary>
        /// Recursively processes tree item in parameter <paramref name="treeItem"/>.
        /// </summary>
        /// <typeparam name="TTreeItem"></typeparam>
        /// <param name="treeItem"></param>
        /// <param name="processTreeItemBeforeChildrenProcessed">A delegate to execute before children of a tree item is processed.</param>
        /// <param name="processTreeItemAfterChildrenProcessed">A delegate to execute after children of a tree item is processed.</param>
        /// <param name="getChildren">A delegate that returns children of a tree item of type <typeparamref name="TTreeItem"/>.</param>
        /// <returns>Returns a task.</returns>
        [NotNull]
        public static async Task ProcessTreeItemAndChildrenAsync<TTreeItem>([NotNull] TTreeItem treeItem,
            [NotNull] ProcessTreeItemAsyncDelegate<TTreeItem> processTreeItemBeforeChildrenProcessed,
            [NotNull] ProcessTreeItemAfterChildrenProcessedAsyncDelegate<TTreeItem> processTreeItemAfterChildrenProcessed,
            [NotNull] Func<TTreeItem, IEnumerable<TTreeItem>> getChildren) where TTreeItem : class
        {
            await ProcessTreeItemAndChildrenLocalAsync(treeItem, processTreeItemBeforeChildrenProcessed, processTreeItemAfterChildrenProcessed, getChildren)
                .ConfigureAwait(false);
        }

        private static async Task<bool> ProcessTreeItemAndChildrenLocalAsync<TTreeItem>([NotNull] TTreeItem treeItem,
            [NotNull] ProcessTreeItemAsyncDelegate<TTreeItem> processTreeItemBeforeChildrenProcessed,
            [NotNull] ProcessTreeItemAfterChildrenProcessedAsyncDelegate<TTreeItem> processTreeItemAfterChildrenProcessed,
            [NotNull] Func<TTreeItem, IEnumerable<TTreeItem>> getChildren) where TTreeItem : class
        {
            var result = await processTreeItemBeforeChildrenProcessed(treeItem).ConfigureAwait(false);

            if (result.stopProcessing)
                return false;

            if (!result.stopProcessingChildren)
            {
                foreach (var treeItemChild in getChildren(treeItem))
                {
                    if (!await ProcessTreeItemAndChildrenLocalAsync(treeItemChild,
                        processTreeItemBeforeChildrenProcessed, processTreeItemAfterChildrenProcessed, getChildren).ConfigureAwait(false))
                        return false;
                }
            }

            return await processTreeItemAfterChildrenProcessed(treeItem).ConfigureAwait(false);
        }

        /// <summary>
        /// Recursively processes tree item in parameter <paramref name="treeItem"/>.
        /// </summary>
        /// <typeparam name="TTreeItem"></typeparam>
        /// <param name="treeItem"></param>
        /// <param name="processTreeItemBeforeChildrenProcessed">A delegate to execute before children of a tree item is processed.</param>
        /// <param name="processTreeItemAfterChildrenProcessed">A delegate to execute after children of a tree item is processed.</param>
        /// <param name="getChildren">A delegate that returns children of a tree item of type <typeparamref name="TTreeItem"/>.</param>
        public static void ProcessTreeItemAndChildren<TTreeItem>([NotNull] TTreeItem treeItem,
            [NotNull] ProcessTreeItemDelegate<TTreeItem> processTreeItemBeforeChildrenProcessed,
            [NotNull] ProcessTreeItemAfterChildrenProcessedDelegate<TTreeItem> processTreeItemAfterChildrenProcessed,
            [NotNull] Func<TTreeItem, IEnumerable<TTreeItem>> getChildren) where TTreeItem : class
        {
            ProcessTreeItemAndChildrenLocal(treeItem, processTreeItemBeforeChildrenProcessed, processTreeItemAfterChildrenProcessed, getChildren);
        }

        private static bool ProcessTreeItemAndChildrenLocal<TTreeItem>([NotNull] TTreeItem treeItem,
            [NotNull] ProcessTreeItemDelegate<TTreeItem> processTreeItemBeforeChildrenProcessed,
            [NotNull] ProcessTreeItemAfterChildrenProcessedDelegate<TTreeItem> processTreeItemAfterChildrenProcessed,
            [NotNull] Func<TTreeItem, IEnumerable<TTreeItem>> getChildren) where TTreeItem : class
        {
            var result = processTreeItemBeforeChildrenProcessed(treeItem);

            if (result.stopProcessing)
                return false;

            if (!result.stopProcessingChildren)
            {
                foreach (var treeItemChild in getChildren(treeItem))
                {
                    if (!ProcessTreeItemAndChildrenLocal(treeItemChild,
                        processTreeItemBeforeChildrenProcessed, processTreeItemAfterChildrenProcessed, getChildren))
                        return false;
                }
            }

            return processTreeItemAfterChildrenProcessed(treeItem);
        }
    }
}