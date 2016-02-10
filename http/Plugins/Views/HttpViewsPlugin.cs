using Netool.Views;
using System;
using System.Collections.Generic;
using Netool.Plugins.Http;
using Netool.Views.Event;
using Netool.Views.Editor;

namespace Netool.Plugins.Views
{
    public class HttpViewsPlugin : IEditorViewPlugin, IEventViewPlugin, IExtensiblePlugin
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

        private List<IEditorViewPlugin> editors = new List<IEditorViewPlugin>();
        private List<IEventViewPlugin> eventViews = new List<IEventViewPlugin>();
        private Dictionary<string, IStreamDecoderPlugin> streamDecoders = new Dictionary<string, IStreamDecoderPlugin>();

        /// <inheritdoc/>
        public IEnumerable<IEditorView> CreateEditorViews()
        {
            return new IEditorView[]
            {
                new EmbeddingEditorViewWrapper(() => (new HttpDataView(editors)), HttpDataView.StaticID),
                new EmbeddingEditorViewWrapper(() => (new WebSocketMessageView(editors)), WebSocketMessageView.StaticID),
            };
        }

        /// <inheritdoc/>
        public IEnumerable<IEventView> CreateEventViews()
        {
            return new IEventView[]
            {
                new EmbeddingEventViewWrapper(() => new HttpDataView(eventViews, streamDecoders), HttpDataView.StaticID),
                new EmbeddingEventViewWrapper(() => new WebSocketMessageView(eventViews), WebSocketMessageView.StaticID),
            };
        }

        public void AfterLoad(PluginLoader loader)
        {
            foreach(var p in loader.Plugins)
            {
                loader_PluginLoaded(null, p);
            }
            loader.PluginLoaded += loader_PluginLoaded;
        }

        private void loader_PluginLoaded(object sender, IPlugin e)
        {
            if (e == this || e == null || e is HttpViewsPlugin) return;
            var editor = e as IEditorViewPlugin;
            var eventView = e as IEventViewPlugin;
            var decoder = e as IStreamDecoderPlugin;
            if (editor != null) editors.Add(editor);
            if (eventView != null) eventViews.Add(eventView);
            if (decoder != null) streamDecoders[decoder.EncodingName.ToLower()] = decoder;
        }
    }
}