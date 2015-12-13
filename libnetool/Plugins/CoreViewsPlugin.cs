using Netool.Views;
using System;
using System.Collections.Generic;

namespace Netool.Plugins
{
    public class CoreViewsPlugin : IEditorViewPlugin, IEventViewPlugin
    {
        /// <inheritdoc/>
        public long ID { get { return 3001; } }
        /// <inheritdoc/>
        public string Name { get { return "CoreViewsPlugin"; } }
        /// <inheritdoc/>
        public string Description { get { return "Plugin for built-in views."; } }
        /// <inheritdoc/>
        public Version Version { get { return new Version(0, 1); } }
        /// <inheritdoc/>
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc/>
        public IEnumerable<IEditorView> CreateEditorViews()
        {
            return new IEditorView[] {new Views.Editor.HexView()};
        }

        /// <inheritdoc/>
        public IEnumerable<IEventView> CreateEventViews()
        {
            return new IEventView[] { new Views.Event.HexView() };
        }
    }
}