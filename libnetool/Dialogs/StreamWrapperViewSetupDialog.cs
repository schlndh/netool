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
                    usedWrappers.Insert(moved, tmp);
                    wrappersListView.Items.RemoveAt(i);
                    wrappersListView.Items.Insert(moved, tmpItem);
                    ++moved;
                }
                wrappersListView.SelectedIndices.Clear();
                for (int i = 0; i < selected.Count; ++i) wrappersListView.SelectedIndices.Add(i);
            }
        }

        private void upBtn_Click(object sender, EventArgs e)
        {
            if (wrappersListView.SelectedIndices.Count > 0)
            {
                var selected = getSelectedIndices();
                selected.Sort();
                int lastSelected = -1;
                foreach (var i in selected)
                {
                    if(lastSelected != i - 1)
                    {
                        var tmp = usedWrappers[i];
                        var tmpItem = wrappersListView.Items[i];
                        usedWrappers[i] = usedWrappers[i - 1];
                        usedWrappers[i - 1] = tmp;
                        wrappersListView.Items[i] = (ListViewItem)wrappersListView.Items[i - 1].Clone();
                        wrappersListView.Items[i - 1] = tmpItem;
                        wrappersListView.SelectedIndices.Remove(i);
                        wrappersListView.SelectedIndices.Add(i - 1);
                    }
                    else
                    {
                        lastSelected = i;
                    }
                }
            }
        }

        private void downBtn_Click(object sender, EventArgs e)
        {
            if (wrappersListView.SelectedIndices.Count > 0)
            {
                var selected = getSelectedIndices();
                selected.Sort();
                selected.Reverse();
                int lastSelected = wrappersListView.Items.Count;
                foreach (var i in selected)
                {
                    if (lastSelected != i + 1)
                    {
                        var tmp = usedWrappers[i];
                        var tmpItem = wrappersListView.Items[i];
                        usedWrappers[i] = usedWrappers[i + 1];
                        usedWrappers[i + 1] = tmp;
                        wrappersListView.Items[i] = (ListViewItem)wrappersListView.Items[i + 1].Clone();
                        wrappersListView.Items[i + 1] = tmpItem;
                        wrappersListView.SelectedIndices.Remove(i);
                        wrappersListView.SelectedIndices.Add(i + 1);
                    }
                    else
                    {
                        lastSelected = i;
                    }
                }
            }
        }

        private void bottomBtn_Click(object sender, EventArgs e)
        {
            if (wrappersListView.SelectedIndices.Count > 0)
            {
                var selected = getSelectedIndices();
                selected.Sort();
                int moved = 0;
                foreach (var i in selected)
                {
                    var tmp = usedWrappers[i - moved];
                    var tmpItem = wrappersListView.Items[i - moved];
                    usedWrappers.RemoveAt(i - moved);
                    usedWrappers.Add(tmp);
                    wrappersListView.Items.RemoveAt(i - moved);
                    wrappersListView.Items.Add(tmpItem);
                    ++moved;
                }
                wrappersListView.SelectedIndices.Clear();
                for (int i = 0; i < selected.Count; ++i) wrappersListView.SelectedIndices.Add(wrappersListView.Items.Count - i - 1);
            }
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