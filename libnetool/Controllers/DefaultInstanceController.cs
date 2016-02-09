using Netool.ChannelDrivers;
using Netool.Logging;
using Netool.Network;
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
            IChannelView CreateChannelView(ChannelLogger info, IMainController mainCont);
        }

        public class DefaultChannelViewFactory : IChannelViewFactory
        {
            public delegate IChannelView ChannelViewCallback(Views.Channel.DefaultChannelView v);
            private ChannelViewCallback callback;

            public DefaultChannelViewFactory() { }

            /// <summary>
            ///
            /// </summary>
            /// <param name="c">callback that will be called before returning channel view from factory</param>
            public DefaultChannelViewFactory(ChannelViewCallback c)
            {
                callback = c;
            }

            /// <inheritdoc/>
            public IChannelView CreateChannelView(ChannelLogger logger, IMainController mainCont)
            {
                var v = new Views.Channel.DefaultChannelView(logger);
                foreach(var ev in mainCont.CreateEventViews())
                {
                    v.AddEventView(ev);
                }

                if (logger.channel != null && logger.channel.Driver != null && logger.channel.Driver.AllowManualControl)
                {
                    var masterEd = new Views.Editor.EditorMasterView();
                    foreach (var ev in mainCont.CreateEditorViews())
                    {
                        masterEd.AddEditorView(ev);
                    }
                    v.AllowManualControl(masterEd);
                }
                if(callback == null) return v;
                return callback(v);
            }
        }

        private IInstanceView view;
        private IInstance instance;
        public IInstance Instance { get { return instance; } }
        private SortedList<int, IChannelDriver> drivers = new SortedList<int, IChannelDriver>();
        private InstanceLogger logger;
        public InstanceLogger Logger { get { return logger; } }
        private IChannelViewFactory detailFactory;
        private RejectDriver rejectDriver = new RejectDriver();
        private IMainController mainCont;

        public DefaultInstanceController(IInstanceView view, IInstance instance, InstanceLogger logger)
            : this(view, instance, logger, new DefaultChannelViewFactory())
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
        }

        public void Start()
        {
            logger.Open();
            var t = new Thread(delegate() {
                instance.Start();
            });
            t.Start();
        }

        public void Stop()
        {
            Debug.WriteLine("DefaultInstanceController - stopping instance(type: {0})", instance.GetType(), 1);
            instance.Stop();
            Debug.WriteLine("DefaultInstanceController - instance(type: {0}) stopped", instance.GetType(), 1);
        }

        public void Close()
        {
            Debug.WriteLine("DefaultInstanceController - closing instance(type: {0})", instance.GetType(), 1);
            logger.WriteInstanceData(instance);
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
            detailFactory.CreateChannelView(logger.GetChannelLogger(id), mainCont).GetForm().Show();
        }

        public InstanceType GetInstanceType()
        {
            return instance.GetInstanceType();
        }

        public void SetMainController(IMainController c)
        {
            this.mainCont = c;
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