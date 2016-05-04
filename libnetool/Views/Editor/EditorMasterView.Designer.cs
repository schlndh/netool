namespace Netool.Views.Editor
{
    partial class EditorMasterView
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.sendButton = new System.Windows.Forms.Button();
            this.proxyFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.proxyRadioClient = new System.Windows.Forms.RadioButton();
            this.proxyRadioServer = new System.Windows.Forms.RadioButton();
            this.innerEditors = new Netool.Views.Components.DataViewSelection();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.proxyFlowPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.proxyFlowPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.innerEditors, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(711, 479);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Controls.Add(this.clearButton);
            this.flowLayoutPanel1.Controls.Add(this.sendButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 434);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(703, 41);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(599, 4);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(491, 4);
            this.clearButton.Margin = new System.Windows.Forms.Padding(4);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(100, 28);
            this.clearButton.TabIndex = 1;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(383, 4);
            this.sendButton.Margin = new System.Windows.Forms.Padding(4);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(100, 28);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // proxyFlowPanel
            // 
            this.proxyFlowPanel.Controls.Add(this.proxyRadioClient);
            this.proxyFlowPanel.Controls.Add(this.proxyRadioServer);
            this.proxyFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proxyFlowPanel.Location = new System.Drawing.Point(4, 4);
            this.proxyFlowPanel.Margin = new System.Windows.Forms.Padding(4);
            this.proxyFlowPanel.Name = "proxyFlowPanel";
            this.proxyFlowPanel.Size = new System.Drawing.Size(703, 41);
            this.proxyFlowPanel.TabIndex = 2;
            // 
            // proxyRadioClient
            // 
            this.proxyRadioClient.AutoSize = true;
            this.proxyRadioClient.Location = new System.Drawing.Point(4, 4);
            this.proxyRadioClient.Margin = new System.Windows.Forms.Padding(4);
            this.proxyRadioClient.Name = "proxyRadioClient";
            this.proxyRadioClient.Size = new System.Drawing.Size(83, 21);
            this.proxyRadioClient.TabIndex = 0;
            this.proxyRadioClient.Text = "To client";
            this.proxyRadioClient.UseVisualStyleBackColor = true;
            // 
            // proxyRadioServer
            // 
            this.proxyRadioServer.AutoSize = true;
            this.proxyRadioServer.Checked = true;
            this.proxyRadioServer.Location = new System.Drawing.Point(95, 4);
            this.proxyRadioServer.Margin = new System.Windows.Forms.Padding(4);
            this.proxyRadioServer.Name = "proxyRadioServer";
            this.proxyRadioServer.Size = new System.Drawing.Size(90, 21);
            this.proxyRadioServer.TabIndex = 1;
            this.proxyRadioServer.TabStop = true;
            this.proxyRadioServer.Text = "To server";
            this.proxyRadioServer.UseVisualStyleBackColor = true;
            // 
            // innerEditors
            // 
            this.innerEditors.AutoSize = true;
            this.innerEditors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerEditors.IsEditor = true;
            this.innerEditors.Label = "Data view type:";
            this.innerEditors.Location = new System.Drawing.Point(4, 53);
            this.innerEditors.Margin = new System.Windows.Forms.Padding(4);
            this.innerEditors.Name = "innerEditors";
            this.innerEditors.SelectedIndex = -1;
            this.innerEditors.SelectedItem = null;
            this.innerEditors.Size = new System.Drawing.Size(703, 373);
            this.innerEditors.Stream = null;
            this.innerEditors.TabIndex = 3;
            this.innerEditors.MinimumSizeChanged += new System.EventHandler(this.InnerEditors_MinimumSizeChanged);
            // 
            // EditorMasterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(711, 479);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "EditorMasterView";
            this.Text = "EditorMasterView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.proxyFlowPanel.ResumeLayout(false);
            this.proxyFlowPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.FlowLayoutPanel proxyFlowPanel;
        private System.Windows.Forms.RadioButton proxyRadioClient;
        private System.Windows.Forms.RadioButton proxyRadioServer;
        private Components.DataViewSelection innerEditors;
    }
}