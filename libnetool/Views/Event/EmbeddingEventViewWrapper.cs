namespace Netool.Views.Event
{
    /// <summary>
    /// Use this class as a wrapper for event views that embed other event views,
    /// to prevent infinite embedding.
    /// </summary>
    public class EmbeddingEventViewWrapper : IEventView
    {
        public delegate IEventView EventViewFactory();

        /// <inheritdoc/>
        public string ID { get; private set; }

        private IEventView view;

        private EventViewFactory factory;

        public EmbeddingEventViewWrapper(EventViewFactory factory, string id)
        {
            ID = id;
            this.factory = factory;
        }

        /// <inheritdoc/>
        public void Show(Network.DataFormats.IDataStream s)
        {
            if (view == null)
            {
                view = factory();
            }
            view.Show(s);
        }

        /// <inheritdoc/>
        public System.Windows.Forms.Form GetForm()
        {
            if (view == null)
            {
                view = factory();
            }
            return view.GetForm();
        }
    }
}