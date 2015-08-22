using Netool.Views;
using System;
using System.Collections.Generic;

namespace Netool.Plugins.Views
{
    public class HttpViewsPlugin : IEditorViewPlugin, IEventViewPlugin
    {
        /// <inheritdoc/>
        public long ID { get { return 3002; } }
        /// <inheritdoc/>
        public string Name { get { return "HttpViewsPlugin"; } }
        /// <inheritdoc/>
        public string Description { get { return "Plugin for http views."; } }
        /// <inheritdoc/>
        public Version Version { get { return new Version(0, 1); } }
        /// <inheritdoc/>
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc/>
        public List<IEditorView> CreateEditorViews()
        {
            return new List<IEditorView> { new HttpDataView(true) };
        }

        /// <inheritdoc/>
        public List<IEventView> CreateEventViews()
        {
            return new List<IEventView> { new HttpDataView(false) };
        }
    }
}