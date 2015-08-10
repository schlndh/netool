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
            IChannelView CreateChannelView(ChannelLogger info);
        }

        public class DefaultChannelViewFactory : IChannelViewFactory
        {
            public IChannelView CreateChannelView(ChannelLogger logger)
            {
                var v = new Views.Channel.DefaultChannelView(logger);
                v.AddEventView(new Views.Event.HexView());
                if (logger.channel != null && logger.channel.Driver != null && logger.channel.Driver.AllowManualControl)
                {
                    v.AllowManualControl(new Views.Editor.DefaultEditorMasterViewFactory().Create());
                }
                return v;
            }
        }

        private IInstanceView view;
        private IInstance instance;
        public IInstance Instance { get { return instance; } }
        private SortedList<int, IChannelDriver> drivers = new SortedList<int, IChannelDriver>();
        private InstanceLogger logger;
        private IChannelViewFactory detailFactory;
        private RejectDriver rejectDriver = new RejectDriver();

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
            detailFactory.CreateChannelView(logger.GetChannelLogger(id)).GetForm().Show();
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