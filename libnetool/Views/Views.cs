using Netool.Logging;
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


    [System.Serializable]
    public class UnsupportedDataStreamException : System.Exception
    {
        public UnsupportedDataStreamException() { }
        public UnsupportedDataStreamException(string message) : base(message) { }
        public UnsupportedDataStreamException(string message, System.Exception inner) : base(message, inner) { }
        protected UnsupportedDataStreamException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    public interface IEventView : IFormView
    {
        string ID { get; }

        /// <summary>
        /// Change content based on given data stream
        /// </summary>
        /// <param name="s"></param>
        /// <exception cref="UnsupportedDataStreamException">given stream is not supported by this view</exception>
        void Show(IDataStream s);
    }

    [System.Serializable]
    public class EditorException : System.Exception
    {
        public EditorException() { }
        public EditorException(string message) : base(message) { }
        public EditorException(string message, System.Exception inner) : base(message, inner) { }
        protected EditorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [System.Serializable]
    public class ValidationException : EditorException
    {
        public ValidationException() { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, System.Exception inner) : base(message, inner) { }
        protected ValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public interface IEditorView : IFormView
    {
        string ID { get; }

        /// <summary>
        /// Reset fields to empty/default values
        /// </summary>
        void Clear();

        /// <summary>
        /// Get value from editor
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ValidationException">validation failed</exception>
        IDataStream GetValue();

        /// <summary>
        /// Fills editor view with given data
        /// </summary>
        /// <param name="s"></param>
        /// <exception cref="UnsupportedDataStreamException">given stream is not supported by this view</exception>
        void SetValue(IDataStream s);
    }
}