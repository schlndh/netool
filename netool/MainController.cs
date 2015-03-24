using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netool
{
    public class MainController
    {
        private MainView view;
        private MainModel model;
        public MainController(MainView view, MainModel model)
        {
            this.view = view;
            this.view.SetController(this);
            this.model = model;
        }
        /// <summary>
        /// Load settings, open previously open instances, etc
        /// </summary>
        public void Load()
        { 
            
        }
    }
}
