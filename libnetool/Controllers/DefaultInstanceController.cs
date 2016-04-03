using Netool.ChannelDrivers;
using Netool.Logging;
using Netool.Network;
using Netool.Plugins;
using Netool.Plugins.Helpers;
using Netool.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Netool.Controllers
{
    public class DefaultInstanceController : IInstanceController
    {
        public interface IChannelViewFactory
        {
            IChannelView CreateChannelView(ChannelLogger info, bool active);
        }

        public class DefaultChannelViewFactory : IChannelViewFactory
        {
            public delegate IChannelView ChannelViewCallback(Views.Channel.DefaultChannelView v);
            private ChannelViewCallback callback;
            private CachedPluginEnumerable<IEditorViewPlugin> editorViewPlugins = new CachedPluginEnumerable<IEditorViewPlugin>();
            private CachedPluginEnumerable<IEventViewPlugin> eventViewPlugins = new CachedPluginEnumerable<IEventViewPlugin>();
            private CachedPluginEnumerable<IMessageTemplatePlugin> templatePlugins = new CachedPluginEnumerable<IMessageTemplatePlugin>();

            public DefaultChannelViewFactory(PluginLoader loader)
            {
                editorViewPlugins.Loader = loader;
                eventViewPlugins.Loader = loader;
                templatePlugins.Loader = loader;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="loader"></param>
            /// <param name="c">callback that will be called before returning channel view from factory</param>
            public DefaultChannelViewFactory(PluginLoader loader, ChannelViewCallback c) : this(loader)
            {
                callback = c;
            }

            /// <inheritdoc/>
            public IChannelView CreateChannelView(ChannelLogger logger, bool active)
            {
                var v = new Views.Channel.DefaultChannelView(logger);
                foreach(var pl in eventViewPlugins)
                {
                    foreach(var ev in pl.CreateEventViews())
                    {
                        v.AddEventView(ev);
                    }
                }

                if (active && logger.channel != null && logger.channel.Driver != null && logger.channel.Driver.AllowManualControl)
                {
                    var masterEd = new Views.Editor.EditorMasterView();
                    foreach (var pl in editorViewPlugins)
                    {
                        foreach (var ev in pl.CreateEditorViews())
                        {
                            masterEd.AddEditorView(ev);
                        }
                    }
                    v.AllowManualControl(masterEd);
                    v.AddMessageTemplates(templatePlugins);
                }
                if (callback == null) return v;
                return callback(v);
            }
        }

        private IInstanceView view;
        private IInstance instance;
        public IInstance Instance { get { return instance; } }
        private SortedList<int, IChannelDriver> drivers = new SortedList<int, IChannelDriver>();
        private List<IChannelView> channelViews = new List<IChannelView>();
        private InstanceLogger logger;
        public InstanceLogger Logger { get { return logger; } }

        /// <summary>
        /// False if instance was opened from log file, true otherwise.
        /// </summary>
        public bool Active { get; private set; }

        private IChannelViewFactory detailFactory;
        private RejectDriver rejectDriver = new RejectDriver();

        public DefaultInstanceController(IInstanceView view, IInstance instance, InstanceLogger logger, PluginLoader loader)
            : this(view, instance, logger, new DefaultChannelViewFactory(loader))
        {
        }

        public DefaultInstanceController(IInstanceView view, IInstance instance, InstanceLogger logger, IChannelViewFactory detailFactory)
        {
            this.view = view;
            this.instance = instance;
            view.SetInstance(instance);

            if(this.instance is IClient)
            {
                ((IClient)this.instance).ChannelCreated += handleConnectionCreated;
            }
            else if (this.instance is IServer)
            {
                ((IServer)this.instance).ChannelCreated += handleConnectionCreated;
            }
            else if (this.instance is IProxy)
            {
                ((IProxy)this.instance).ChannelCreated += handleConnectionCreated;
            }

            this.logger = logger;
            this.view.SetLogger(logger);
            this.detailFactory = detailFactory;
            this.Active = true;
        }

        /// <summary>
        /// This constructor is used for loading communication from file
        /// </summary>
        /// <param name="view"></param>
        /// <param name="logger"></param>
        /// <param name="loader"></param>
        public DefaultInstanceController(IInstanceView view, InstanceLogger logger, PluginLoader loader)
            : this(view, logger, new DefaultChannelViewFactory(loader))
        {
        }

        /// <summary>
        /// This constructor is used for loading communication from file
        /// </summary>
        /// <param name="view"></param>
        /// <param name="logger"></param>
        /// <param name="detailFactory"></param>
        public DefaultInstanceController(IInstanceView view, InstanceLogger logger, IChannelViewFactory detailFactory)
        {
            this.view = view;
            instance = logger.ReadInstanceData();
            view.SetInstance(instance);
            view.SetLogger(logger);
            this.logger = logger;
            this.detailFactory = detailFactory;
            this.Active = false;
        }

        public void Start()
        {
            if(Active)
            {
                logger.Open();
                var t = new Thread(delegate () {
                    instance.Start();
                });
                t.Start();
            }
        }

        public void Stop()
        {
            if(Active)
            {
                Debug.WriteLine("DefaultInstanceController - stopping instance(type: {0})", instance.GetType(), 1);
                instance.Stop();
                Debug.WriteLine("DefaultInstanceController - instance(type: {0}) stopped", instance.GetType(), 1);
            }
        }

        public void Close()
        {
            Debug.WriteLine("DefaultInstanceController - closing instance(type: {0})", instance.GetType(), 1);
            view.Close();
            // copy the channel views
            // because closing the form removes the view from channelViews
            var views = new List<IChannelView>(channelViews);
            foreach(var v in views)
            {
                v.Close();
            }
            channelViews.Clear();
            views.Clear();
            if(Active)
            {
                logger.WriteInstanceData(instance);
            }
            logger.Close();
            Debug.WriteLine("DefaultInstanceController - instance(type: {0}) closed", instance.GetType(), 1);
        }

        /// <summary>
        /// Adds a driver to driver queue
        /// </summary>
        /// <param name="d">driver</param>
        /// <param name="order">lower number = higher priority</param>
        public void AddDriver(IChannelDriver d, int order)
        {
            drivers.Add(order, d);
        }

        public void ShowDetail(int id)
        {
            var v = detailFactory.CreateChannelView(logger.GetChannelLogger(id), Active);
            channelViews.Add(v);
            var form = v.GetForm();
            form.FormClosed += delegate (object sender, System.Windows.Forms.FormClosedEventArgs args) { channelViews.Remove(v); };
            form.Show();
        }

        public InstanceType GetInstanceType()
        {
            return instance.GetInstanceType();
        }

        private void handleConnectionCreated(object sender, IChannel c)
        {
            // must be registered before the driver, so that events are logged in proper order
            logger.AddChannel(c);
            bool handled = false;
            foreach (var d in drivers.Values)
            {
                if (d.CanAccept(c))
                {
                    d.Handle(c);
                    c.Driver = d;
                    handled = true;
                    break;
                }
            }
            // no driver available -> reject
            if(!handled)
            {
                rejectDriver.Handle(c);
                c.Driver = rejectDriver;
            }
        }
    }
}