using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Netool.Views
{
    public class BaseForm : Form
    {
        // it's necessary to override MinimumSize, because apparently there is a maximum Height
        // for MinimumSize in Windows Forms.
        private Size minSize;

        /// <inheritdoc/>
        public override Size MinimumSize
        {
            get
            {
                return minSize;
            }

            set
            {
                minSize = value;
                OnMinimumSizeChanged(EventArgs.Empty);
            }
        }
    }
}
