﻿using Netool.Logging;
using Netool.Network;
using Netool.Network.DataFormats;
using System.Windows.Forms;

namespace Netool.Views
{
    /**
     * Important note: Most methods on views will be called from non-gui threads!
     */

    public interface IFormView
    {
        /// <summary>
        /// Get Form to display
        /// </summary>
        /// <returns></returns>
        Form GetForm();
    }

    public interface IInstanceView : IFormView
    {
        /// <summary>
        /// Sets view's instance, for view to get additional information about the instance
        /// </summary>
        /// <param name="i">instance</param>
        void SetInstance(IInstance i);

        void SetLogger(InstanceLogger l);
    }

    public interface IChannelView : IFormView
    {
        void AddEventView(IEventView v);

        /// <summary>
        /// Allow user to manually control the channel (note: control will be shared with channel's driver)
        /// </summary>
        /// <param name="v"></param>
        void AllowManualControl(Editor.EditorMasterView v);
    }

    public interface IEventView : IFormView
    {
        string ID { get; }

        /// <summary>
        /// Change content based on given event
        /// </summary>
        /// <param name="e"></param>
        void Show(Netool.Logging.Event e);
    }

    public interface IEditorView : IFormView
    {
        string ID { get; }

        /// <summary>
        /// Reset fields to empty/default values
        /// </summary>
        void Clear();

        IInMemoryData GetValue();

        void SetValue(Netool.Logging.Event v);
    }
}