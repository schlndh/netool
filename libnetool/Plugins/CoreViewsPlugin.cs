using Netool.Views;
using System;
using System.Collections.Generic;
using Netool.Plugins.Helpers;

namespace Netool.Plugins
{
    public class CoreViewsPlugin : IEditorViewPlugin, IEventViewPlugin, IExtensiblePlugin
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

        private CachedPluginEnumerable<IStreamWrapperPlugin> streamWrapperPlugins = new CachedPluginEnumerable<IStreamWrapperPlugin>();
        private CachedPluginEnumerable<IEditorViewPlugin> editorViewPlugins = new CachedPluginEnumerable<IEditorViewPlugin>();
        private CachedPluginEnumerable<IEventViewPlugin> eventViewPlugins = new CachedPluginEnumerable<IEventViewPlugin>();

        /// <inheritdoc/>
        public IEnumerable<IEditorView> CreateEditorViews()
        {
            return new IEditorView[]
            {
                new Views.Editor.HexView(),
                new Views.Editor.Utf8TextEditor(),
                new Views.Editor.EmbeddingEditorViewWrapper(createEditorViewStreamWrapper, "StreamWrapper"),
                new Views.Editor.FileEditor(),
            };
        }

        /// <inheritdoc/>
        public IEnumerable<IEventView> CreateEventViews()
        {
            return new IEventView[]
            {
                new Views.Event.HexView(),
                new Views.Event.Utf8TextView(),
                new Views.Event.EmbeddingEventViewWrapper(createEventViewStreamWrapper, "StreamWrapper"),
            };
        }

        public void AfterLoad(PluginLoader loader)
        {
            streamWrapperPlugins.Loader = editorViewPlugins.Loader = eventViewPlugins.Loader = loader;
        }

        private IEventView createEventViewStreamWrapper()
        {
            var inner = new List<IEventView>();
            foreach(var p in eventViewPlugins)
            {
                inner.AddRange(p.CreateEventViews());
            }
            return new StreamWrapperView(streamWrapperPlugins, inner);
        }

        private IEditorView createEditorViewStreamWrapper()
        {
            var inner = new List<IEditorView>();
            foreach (var p in editorViewPlugins)
            {
                inner.AddRange(p.CreateEditorViews());
            }
            return new StreamWrapperView(streamWrapperPlugins, inner);
        }
    }
}