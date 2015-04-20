using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Netool.Controllers;
namespace Netool.Views
{
    public partial class ServerView : Form
    {
        private ServerController controller;
        public ServerView()
        {
            InitializeComponent();
        }
        public void SetController(ServerController c)
        {
            controller = c;
        }
        public void LogMessage(string message)
        {
            log.Invoke(new Action(() => log.AppendText(message)));
        }
        public void AddClient(string id)
        {
            clients.Invoke(new Action(() => clients.Items.Add(id)));
        }
        public void RemoveClient(string id)
        {
            clients.Invoke(new Action(() => clients.Items.Remove(id)));
        }

        private void send_Click(object sender, EventArgs e)
        {
            if(clients.SelectedIndex != -1)
            {
                controller.Send((string) clients.SelectedItem, sendData.Text);
            }
            
        }

        private void close_Click(object sender, EventArgs e)
        {
            if(clients.SelectedIndex != -1)
            {
                controller.Close((string)clients.SelectedItem);
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            controller.Stop();
        }

        private void start_Click(object sender, EventArgs e)
        {
            controller.Start();
        }
    }
}
