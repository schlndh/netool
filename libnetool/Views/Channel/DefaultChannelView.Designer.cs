namespace Netool.Views.Channel
{
    partial class DefaultChannelView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.events = new System.Windows.Forms.ListView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.idLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.typeLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timeLabel = new System.Windows.Forms.Label();
            this.dataView = new Netool.Views.Components.DataViewSelection();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.channelMenu = new System.Windows.Forms.MenuStrip();
            this.templatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewsTabControl = new System.Windows.Forms.TabControl();
            this.eventTabPage = new System.Windows.Forms.TabPage();
            this.eventScrollPanel = new System.Windows.Forms.Panel();
            this.editorTabPage = new System.Windows.Forms.TabPage();
            this.eventsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.eventsEditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.channelMenu.SuspendLayout();
            this.viewsTabControl.SuspendLayout();
            this.eventTabPage.SuspendLayout();
            this.eventScrollPanel.SuspendLayout();
            this.eventsContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // events
            // 
            this.events.Dock = System.Windows.Forms.DockStyle.Fill;
            this.events.FullRowSelect = true;
            this.events.GridLines = true;
            this.events.Location = new System.Drawing.Point(0, 28);
            this.events.Margin = new System.Windows.Forms.Padding(4);
            this.events.MultiSelect = false;
            this.events.Name = "events";
            this.events.ShowGroups = false;
            this.events.Size = new System.Drawing.Size(895, 139);
            this.events.TabIndex = 0;
            this.events.UseCompatibleStateImageBehavior = false;
            this.events.View = System.Windows.Forms.View.Details;
            this.events.VirtualMode = true;
            this.events.CacheVirtualItems += new System.Windows.Forms.CacheVirtualItemsEventHandler(this.events_CacheVirtualItems);
            this.events.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.events_RetrieveVirtualItem);
            this.events.SelectedIndexChanged += new System.EventHandler(this.events_SelectedIndexChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.dataView, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(887, 309);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.idLabel);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.typeLabel);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Controls.Add(this.timeLabel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 4);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(879, 41);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 6);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "ID:";
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(37, 6);
            this.idLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(13, 17);
            this.idLabel.TabIndex = 7;
            this.idLabel.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Type:";
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(110, 6);
            this.typeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(13, 17);
            this.typeLabel.TabIndex = 9;
            this.typeLabel.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(131, 6);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Time:";
            // 
            // timeLabel
            // 
            this.timeLabel.AutoSize = true;
            this.timeLabel.Location = new System.Drawing.Point(182, 6);
            this.timeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(13, 17);
            this.timeLabel.TabIndex = 11;
            this.timeLabel.Text = "-";
            // 
            // dataView
            // 
            this.dataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataView.IsEditor = false;
            this.dataView.Label = "Data view type:";
            this.dataView.Location = new System.Drawing.Point(5, 54);
            this.dataView.Margin = new System.Windows.Forms.Padding(5);
            this.dataView.Name = "dataView";
            this.dataView.SelectedIndex = -1;
            this.dataView.Size = new System.Drawing.Size(877, 250);
            this.dataView.Stream = null;
            this.dataView.TabIndex = 4;
            this.dataView.MinimumSizeChanged += new System.EventHandler(this.dataView_MinimumSizeChanged);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(4);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.events);
            this.mainSplitContainer.Panel1.Controls.Add(this.channelMenu);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.viewsTabControl);
            this.mainSplitContainer.Size = new System.Drawing.Size(899, 521);
            this.mainSplitContainer.SplitterDistance = 171;
            this.mainSplitContainer.SplitterWidth = 5;
            this.mainSplitContainer.TabIndex = 1;
            // 
            // channelMenu
            // 
            this.channelMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.channelMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.templatesToolStripMenuItem});
            this.channelMenu.Location = new System.Drawing.Point(0, 0);
            this.channelMenu.Name = "channelMenu";
            this.channelMenu.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.channelMenu.Size = new System.Drawing.Size(895, 28);
            this.channelMenu.TabIndex = 1;
            // 
            // templatesToolStripMenuItem
            // 
            this.templatesToolStripMenuItem.Name = "templatesToolStripMenuItem";
            this.templatesToolStripMenuItem.Size = new System.Drawing.Size(89, 24);
            this.templatesToolStripMenuItem.Text = "Templates";
            this.templatesToolStripMenuItem.Click += new System.EventHandler(this.templatesToolStripMenuItem_Click);
            // 
            // viewsTabControl
            // 
            this.viewsTabControl.Controls.Add(this.eventTabPage);
            this.viewsTabControl.Controls.Add(this.editorTabPage);
            this.viewsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewsTabControl.Location = new System.Drawing.Point(0, 0);
            this.viewsTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.viewsTabControl.Name = "viewsTabControl";
            this.viewsTabControl.SelectedIndex = 0;
            this.viewsTabControl.Size = new System.Drawing.Size(895, 341);
            this.viewsTabControl.TabIndex = 2;
            // 
            // eventTabPage
            // 
            this.eventTabPage.AutoScroll = true;
            this.eventTabPage.Controls.Add(this.eventScrollPanel);
            this.eventTabPage.Location = new System.Drawing.Point(4, 25);
            this.eventTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.eventTabPage.Name = "eventTabPage";
            this.eventTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.eventTabPage.Size = new System.Drawing.Size(887, 312);
            this.eventTabPage.TabIndex = 0;
            this.eventTabPage.Text = "Event";
            this.eventTabPage.UseVisualStyleBackColor = true;
            // 
            // eventScrollPanel
            // 
            this.eventScrollPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventScrollPanel.AutoScroll = true;
            this.eventScrollPanel.Controls.Add(this.tableLayoutPanel2);
            this.eventScrollPanel.Location = new System.Drawing.Point(0, 0);
            this.eventScrollPanel.Name = "eventScrollPanel";
            this.eventScrollPanel.Size = new System.Drawing.Size(887, 309);
            this.eventScrollPanel.TabIndex = 2;
            // 
            // editorTabPage
            // 
            this.editorTabPage.Location = new System.Drawing.Point(4, 25);
            this.editorTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.editorTabPage.Name = "editorTabPage";
            this.editorTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.editorTabPage.Size = new System.Drawing.Size(887, 312);
            this.editorTabPage.TabIndex = 1;
            this.editorTabPage.Text = "Editor";
            this.editorTabPage.UseVisualStyleBackColor = true;
            // 
            // eventsContextMenu
            // 
            this.eventsContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.eventsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eventsEditMenuItem});
            this.eventsContextMenu.Name = "eventsContextMenu";
            this.eventsContextMenu.Size = new System.Drawing.Size(111, 30);
            // 
            // eventsEditMenuItem
            // 
            this.eventsEditMenuItem.Name = "eventsEditMenuItem";
            this.eventsEditMenuItem.Size = new System.Drawing.Size(110, 26);
            this.eventsEditMenuItem.Text = "Edit";
            this.eventsEditMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // DefaultChannelView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(899, 521);
            this.Controls.Add(this.mainSplitContainer);
            this.MainMenuStrip = this.channelMenu;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DefaultChannelView";
            this.Text = "DefaultServerChannelView";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DefaultServerChannelView_FormClosed);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel1.PerformLayout();
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.channelMenu.ResumeLayout(false);
            this.channelMenu.PerformLayout();
            this.viewsTabControl.ResumeLayout(false);
            this.eventTabPage.ResumeLayout(false);
            this.eventScrollPanel.ResumeLayout(false);
            this.eventsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView events;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.ContextMenuStrip eventsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem eventsEditMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl viewsTabControl;
        private System.Windows.Forms.TabPage eventTabPage;
        private System.Windows.Forms.TabPage editorTabPage;
        private Components.DataViewSelection dataView;
        private System.Windows.Forms.MenuStrip channelMenu;
        private System.Windows.Forms.ToolStripMenuItem templatesToolStripMenuItem;
        private System.Windows.Forms.Panel eventScrollPanel;
    }
}