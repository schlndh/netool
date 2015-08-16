using System.Windows.Forms;

namespace Netool
{
    public static class Helpers
    {
        /// <summary>
        /// Embed form in the control
        /// </summary>
        /// <param name="c"></param>
        /// <param name="frm"></param>
        public static void Embed(this Control c, Form frm)
        {
            frm.TopLevel = false;
            frm.Visible = true;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            c.Controls.Add(frm);
        }
    }
}