using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Views;
using System.Collections.Generic;
using System.Threading;

namespace Netool.Controllers
{
    public class DefaultInstanceController : IInstanceController
    {
        public interface IChannelViewFactory
        {
            IChannelView CreateChannelView(ChannelInfo info);
        }

        public class DefaultChannelViewFactory : IChannelViewFactory
        {
            public IChannelView CreateChannelView(ChannelInfo info)
            {
                var v = new Views.Channel.DefaultServerChannelView(info);
                v.AddEventView(new Views.Event.HexView());
                if (info.channel != null && info.channel.Driver != null && info.channel.Driver.AllowManualControl)
                {
                    v.AllowManualControl(new Views.Editor.DefaultEditorMasterViewFactory().Create());
                }
                return v;
            }
        }

        private IInstanceView view;
        private IInstance instance;
        private SortedList<int, IChannelDriver> drivers = new SortedList<int, IChannelDriver>();
        private EventLogger logger;
        private IChannelViewFactory detailFactory;
        private RejectDriver rejectDriver = new RejectDriver();

        public DefaultInstanceController(IInstanceView view, IInstance server)
            : this(view, server, new DefaultChannelViewFactory(), new EventLogger())
        {
        }

        public DefaultInstanceController(IInstanceView view, IInstance server, IChannelViewFactory detailFactory)
            : this(view, server, detailFactory, new EventLogger())
        {
        }

        public DefaultInstanceController(IInstanceView view, IInstance server, IChannelViewFactory detailFactory, EventLogger logger)
        {
            this.view = view;
            this.instance = server;
            view.SetInstance(server);

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
            this.detailFactory = detailFactory;
        }

        public void Start()
        {
            var t = new Thread(delegate() {
                instance.Start();
            });
            t.Start();
        }

        public void Stop()
        {
            instance.Stop();
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
            detailFactory.CreateChannelView(logger.GetChannelInfo(id)).GetForm().Show();
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
            view.AddChannel(c);
        }
    }
}