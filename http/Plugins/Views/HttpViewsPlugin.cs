using Netool.Views;
using System;
using System.Collections.Generic;
using Netool.Plugins.Helpers;
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

        private CachedPluginEnumerable<IEditorViewPlugin> editors = new CachedPluginEnumerable<IEditorViewPlugin>();
        private CachedPluginEnumerable<IEventViewPlugin> eventViews = new CachedPluginEnumerable<IEventViewPlugin>();

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

        void IExtensiblePlugin.AfterLoad(PluginLoader loader)
        {
            editors.Loader = loader;
            eventViews.Loader = loader;
            foreach(var p in loader.Plugins)
            {
                loader_PluginLoaded(null, p);
            }
            loader.PluginLoaded += loader_PluginLoaded;
        }

        private void loader_PluginLoaded(object sender, IPlugin e)
        {
            if (e == this || e == null || e is HttpViewsPlugin) return;
            var decoder = e as IStreamDecoderPlugin;
            if (decoder != null) streamDecoders[decoder.EncodingName.ToLower()] = decoder;
        }
    }
}