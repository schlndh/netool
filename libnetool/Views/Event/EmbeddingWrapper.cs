namespace Netool.Views.Event
{
    /// <summary>
    /// Use this class as a wrapper for event views that embed other event views,
    /// to prevent infinite embedding.
    /// </summary>
    public class EmbeddingWrapper<T> : IEventView where T : IEventView
    {
        public delegate T EventViewFactory();

        /// <inheritdoc/>
        public string ID { get; private set; }

        private T view;
        public T View { get { if (view == null) view = factory(); return view; } }

        private EventViewFactory factory;

        public EmbeddingWrapper(EventViewFactory factory, string id)
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