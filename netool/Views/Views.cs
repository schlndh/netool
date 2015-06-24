using Netool.Network;
using System.Windows.Forms;

namespace Netool.Views
{
    /**
     * Important note: Most methods on views will be called from non-gui threads!
     */

    public interface IServerView
    {
        /// <summary>
        /// Sets view's server, for view to get additional information about the server
        /// </summary>
        /// <param name="s">server</param>
        void SetServer(IServer s);

        void AddChannel(IServerChannel c);

        /// <summary>
        /// Get Form to display
        /// </summary>
        /// <returns></returns>
        Form GetForm();
    }

    public interface IChannelView
    {
        void AddEventView(IEventView v);

        /// <summary>
        /// Allow user to manually control the channel (note: control will be shared with channel's driver)
        /// </summary>
        /// <param name="v"></param>
        void AllowManualControl(Editor.EditorMasterView v);

        /// <summary>
        /// Get Form to display
        /// </summary>
        /// <returns></returns>
        Form GetForm();
    }

    public interface IEventView
    {
        string ID { get; }

        /// <summary>
        /// Change content based on given event
        /// </summary>
        /// <param name="e"></param>
        void Show(Netool.Event e);

        /// <summary>
        /// Get Form to display
        /// </summary>
        /// <returns></returns>
        Form GetForm();
    }

    public interface IEditorView
    {
        string ID { get; }

        /// <summary>
        /// Reset fields to empty/default values
        /// </summary>
        void Clear();

        IByteArrayConvertible GetValue();

        void SetValue(IByteArrayConvertible v);

        /// <summary>
        /// Get Form to display
        /// </summary>
        /// <returns></returns>
        Form GetForm();
    }
}