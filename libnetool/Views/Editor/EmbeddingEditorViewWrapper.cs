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
        public IEditorView View { get { if (view == null) view = factory(); return view; } }

        private EditorViewFactory factory;

        public EmbeddingEditorViewWrapper(EditorViewFactory factory, string id)
        {
            ID = id;
            this.factory = factory;
        }

        public void Clear()
        {
            View.Clear();
        }

        public Network.DataFormats.IDataStream GetValue()
        {
            return View.GetValue();
        }

        public void SetValue(Network.DataFormats.IDataStream s)
        {
            View.SetValue(s);
        }

        public System.Windows.Forms.Form GetForm()
        {
            return View.GetForm();
        }
    }
}