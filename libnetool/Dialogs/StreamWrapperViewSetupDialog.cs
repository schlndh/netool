using Netool.Network.DataFormats;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Dialogs
{
    public partial class StreamWrapperViewSetupDialog : Form
    {
        private List<IStreamWrapper> usedWrappers = new List<IStreamWrapper>();
        public IEnumerable<IStreamWrapper> UsedWrappers { get { return usedWrappers; } }

        public StreamWrapperViewSetupDialog(IEnumerable<IStreamWrapperPlugin> plugins, IEnumerable<IStreamWrapper> currentWrappers)
        {
            InitializeComponent();
            foreach (var p in plugins)
            {
                wrapperSelect.Items.Add(p);
                wrapperSelect.SelectedIndex = 0;
            }

            if (currentWrappers != null) usedWrappers.AddRange(currentWrappers);
            foreach (var w in usedWrappers)
            {
                wrappersListView.Items.Add(w.Name + "(" + w.Params + ")");
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            var plugin = wrapperSelect.SelectedItem as IStreamWrapperPlugin;
            if (plugin != null)
            {
                try
                {
                    var w = plugin.CreateWrapper();
                    if (w != null)
                    {
                        usedWrappers.Add(w);
                        wrappersListView.Items.Add(w.Name + "(" + w.Params + ")");
                    }
                }
                catch { }
            }
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            if (wrappersListView.SelectedIndices.Count > 0)
            {
                var selected = getSelectedIndices();
                selected.Sort();
                int removed = 0;
                foreach (var i in selected)
                {
                    usedWrappers.RemoveAt(i - removed);
                    wrappersListView.Items.RemoveAt(i - removed);
                    ++removed;
                }
            }
        }

        private void topBtn_Click(object sender, EventArgs e)
        {
            if (wrappersListView.SelectedIndices.Count > 0)
            {
                var selected = getSelectedIndices();
                selected.Sort();
                int moved = 0;
                foreach (var i in selected)
                {
                    var tmp = usedWrappers[i];
                    var tmpItem = wrappersListView.Items[i];
                    usedWrappers.RemoveAt(i);
                    usedWrappers.Insert(i + moved, tmp);
                    wrappersListView.Items.RemoveAt(i);
                    wrappersListView.Items.Insert(i + moved, tmpItem);
                    ++moved;
                }
            }
        }

        private void upBtn_Click(object sender, EventArgs e)
        {
            // TODO: implement this
        }

        private void downBtn_Click(object sender, EventArgs e)
        {
            // TODO: implement this
        }

        private void bottomBtn_Click(object sender, EventArgs e)
        {
            // TODO: implement this
        }

        private List<int> getSelectedIndices()
        {
            var selected = new List<int>(wrappersListView.SelectedIndices.Count);
            foreach (int i in wrappersListView.SelectedIndices)
            {
                selected.Add(i);
            }
            return selected;
        }
    }
}