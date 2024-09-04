using System;
using System.Collections.Generic;

namespace OROptimizer.Diagnostics.Log
{
    public sealed class AddedContextProperties : IDisposable
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _contextProperties;
        private readonly ILog _logger;

        public AddedContextProperties(IEnumerable<KeyValuePair<string, string>> contextProperties, ILog logger)
        {
            _contextProperties = contextProperties;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var contextProperty in this._contextProperties)
            {
                _logger.RemoveContextProperty(contextProperty.Key);
            }
        }
    }
}