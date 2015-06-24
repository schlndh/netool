﻿using Netool.Controllers;
using Netool.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Netool.Views.Instance
{
    public partial class DefaultServerView : Form, IServerView
    {
        public interface IDataRowFactory
        {
            /// <summary>
            /// Fill given Row with information about given channel
            /// </summary>
            /// <param name="row">row</param>
            /// <param name="c">channel</param>
            void FillRow(DataRow row, IServerChannel c);
        }

        public class DefaultDataRowFactory : IDataRowFactory
        {
            public void FillRow(DataRow row, IServerChannel c)
            {
                row["id"] = c.ID;
                row["name"] = c.Name;
                if(c.Driver != null)
                {
                    row["driver"] = c.Driver.ID;
                }
            }
        }

        public static DataTable GetDefaultDataTable()
        {
            var ret = new DataTable();
            ret.Columns.Add("id", typeof(int));
            ret.Columns.Add("driver", typeof(string));
            ret.Columns.Add("name", typeof(string));
            return ret;
        }

        private IServerController controller;
        private IDataRowFactory rowFactory;
        private DataTable table;
        private List<int> rowIndexToID = new List<int>();

        /// <summary>
        /// Default view settings with default DataTable schema and corresponding row factory
        /// </summary>
        public DefaultServerView()
            : this(DefaultServerView.GetDefaultDataTable(), new DefaultDataRowFactory())
        { }

        public DefaultServerView(DataTable table, IDataRowFactory rowFactory)
        {
            InitializeComponent();
            this.table = table;
            this.rowFactory = rowFactory;
            this.channels.DataSource = table;
        }

        public void SetController(IServerController c)
        {
            controller = c;
        }

        public void SetServer(IServer s)
        {
            start.Enabled = s.IsStarted;
            stop.Enabled = !start.Enabled;
        }

        public void AddChannel(IServerChannel c)
        {
            channels.Invoke(new Action(() =>
            {
                var row = table.NewRow();
                rowFactory.FillRow(row, c);
                table.Rows.Add(row);
            }));
        }

        public Form GetForm()
        {
            return this;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            controller.Stop();
            start.Enabled = true;
            stop.Enabled = false;
        }

        private void start_Click(object sender, EventArgs e)
        {
            controller.Start();
            start.Enabled = false;
            stop.Enabled = true;
        }

        private void channels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int id = (int) this.channels.Rows[e.RowIndex].Cells["id"].Value;
            controller.ShowDetail(id);
        }
    }
}