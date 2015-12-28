using Netool.Views;
using System;
using System.Collections.Generic;

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

        private List<IStreamWrapperPlugin> streamWrapperPlugins = new List<IStreamWrapperPlugin>();
        private List<IEventViewPlugin> eventViewPlugins = new List<IEventViewPlugin>();
        private List<IEditorViewPlugin> editorViewPlugins = new List<IEditorViewPlugin>();

        /// <inheritdoc/>
        public IEnumerable<IEditorView> CreateEditorViews()
        {
            return new IEditorView[] { new Views.Editor.HexView(), new Views.Editor.EmbeddingEditorViewWrapper(createEditorViewStreamWrapper, "StreamWrapper"), new Views.Editor.FileEditor() };
        }

        /// <inheritdoc/>
        public IEnumerable<IEventView> CreateEventViews()
        {
            return new IEventView[] { new Views.Event.HexView(), new Views.Event.EmbeddingEventViewWrapper(createEventViewStreamWrapper, "StreamWrapper") };
        }

        public void AfterLoad(PluginLoader loader)
        {
            foreach(var pl in loader.Plugins)
            {
                loadPlugin(loader, pl);
            }
            loader.PluginLoaded += loadPlugin;
        }

        void loadPlugin(object sender, IPlugin e)
        {
            var pl = e as IStreamWrapperPlugin;
            var eventView = e as IEventViewPlugin;
            var editor = e as IEditorViewPlugin;
            if(pl != null)
            {
                streamWrapperPlugins.Add(pl);
            }
            if(eventView != null)
            {
                eventViewPlugins.Add(eventView);
            }
            if(editor != null)
            {
                editorViewPlugins.Add(editor);
            }
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