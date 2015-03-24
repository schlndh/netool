using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Netool
{
    public partial class MainView : Form
    {
        private MainController controller;
        public MainView()
        {
            InitializeComponent();
        }
        public void SetController(MainController c)
        {
            controller = c;
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            controller.Load();
        }
    }
}
