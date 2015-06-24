namespace Netool.Views.Editor
{
    /// <summary>
    /// Creates EditorMasterView with all available editor types
    /// </summary>
    public class DefaultEditorMasterViewFactory
    {
        public EditorMasterView Create()
        {
            var e = new EditorMasterView();
            e.AddEditor(new HexView());
            return e;
        }
    }
}