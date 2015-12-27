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
        public IEventView View { get { if (view == null) view = factory(); return view; } }

        private EventViewFactory factory;

        public EmbeddingEventViewWrapper(EventViewFactory factory, string id)
        {
            ID = id;
            this.factory = factory;
        }

        /// <inheritdoc/>
        public void Show(Network.DataFormats.IDataStream s)
        {
            View.Show(s);
        }

        /// <inheritdoc/>
        public System.Windows.Forms.Form GetForm()
        {
            return View.GetForm();
        }
    }
}