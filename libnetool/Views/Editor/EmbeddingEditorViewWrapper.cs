namespace Netool.Views.Editor
{
    /// <summary>
    /// Use this class as a wrapper for editor views that embed other editor views,
    /// to prevent infinite embedding.
    /// </summary>
    public class EmbeddingEditorViewWrapper : IEditorView
    {
        public delegate IEditorView EditorViewFactory();

        /// <inheritdoc/>
        public string ID { get; private set; }

        private IEditorView view;

        private EditorViewFactory factory;

        public EmbeddingEditorViewWrapper(EditorViewFactory factory, string id)
        {
            ID = id;
            this.factory = factory;
        }

        public void Clear()
        {
            if (view == null)
            {
                view = factory();
            }
            view.Clear();
        }

        public Network.DataFormats.IDataStream GetValue()
        {
            if (view == null)
            {
                view = factory();
            }
            return view.GetValue();
        }

        public void SetValue(Network.DataFormats.IDataStream s)
        {
            if (view == null)
            {
                view = factory();
            }
            view.SetValue(s);
        }

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