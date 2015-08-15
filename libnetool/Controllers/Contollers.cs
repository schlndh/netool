﻿using Netool.ChannelDrivers;
using Netool.Network;
using System.Collections.Generic;

namespace Netool.Controllers
{
    public interface IMainController
    {
        List<Views.IEditorView> CreateEditorViews();

        List<Views.IEventView> CreateEventViews();
    }

    public interface IInstanceController
    {
        IInstance Instance { get; }
        /// <summary>
        /// Start the associated instance.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the associated instance.
        /// </summary>
        void Stop();
        /// <summary>
        /// Called when instance tab is about to be closed.
        /// </summary>
        void Close();

        /// <summary>
        /// Show channel detail view
        /// </summary>
        /// <param name="id">channel ID</param>
        void ShowDetail(int id);
        /// <summary>
        /// Adds a driver to driver queue
        /// </summary>
        /// <param name="d">driver</param>
        /// <param name="order">lower number = higher priority</param>
        void AddDriver(IChannelDriver d, int order);

        void SetMainController(IMainController c);
    }
}