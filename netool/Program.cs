using Netool.Controllers;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Netool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Debug.Listeners.Add(new TextWriterTraceListener("debug.log"));
            Debug.AutoFlush = true;
            Debug.WriteLine("---------------- Netool started ----------------");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var view = new MainView();
            var controller = new MainController(view);
            Application.Run(view);
            Debug.WriteLine("---------------- Netool ended ----------------");
        }
    }
}