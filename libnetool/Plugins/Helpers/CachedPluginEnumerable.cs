using System.Collections.Generic;

namespace Netool.Plugins.Helpers
{
    /// <summary>
    /// Helper struct to simplify using PluginLoader.GetPluginsByType while avoiding unneccessary Dictionary access.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// <code language="C#">
    /// <![CDATA[
    /// private CachedPluginEnumerable<IEventViewPlugin> eventViews = new CachedPluginEnumerable<IEventViewPlugin>();
    ///
    /// void IExtensiblePlugin.AfterLoad(PluginLoader loader)
    /// {
    ///     eventViews.Loader = loader;
    /// }
    ///
    /// private void SomeVeryUsefulMethod()
    /// {
    ///     foreach(var pl in eventViews)
    ///     {
    ///         // do some magic stuff here
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public struct CachedPluginEnumerable<T> : IEnumerable<T> where T : IPlugin
    {
        private IEnumerable<T> plugins;
        private PluginLoader loader;

        /// <summary>
        /// Get or Set the PluginLoader
        /// </summary>
        /// <remarks>
        /// Don't forget to set the PluginLoader.
        /// </remarks>
        public PluginLoader Loader
        {
            get
            {
                return loader;
            }
            set
            {
                if (loader != null)
                {
                    loader.PluginLoaded -= loader_PluginLoaded;
                    plugins = null;
                }
                loader = value;
                loader.PluginLoaded += loader_PluginLoaded;
            }
        }

        /// <summary>
        /// Gets cached plugins or empty list if loader isn't set.
        /// </summary>
        public IEnumerable<T> Plugins
        {
            get
            {
                if (plugins != null) return plugins;
                if (loader != null) return plugins = loader.GetPluginsByType<T>();
                return new List<T>();
            }
        }

        private void loader_PluginLoaded(object sender, IPlugin e)
        {
            plugins = null;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }
    }
}