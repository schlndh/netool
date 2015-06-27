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
            this.editorViewPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.editorViewSelect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.proxyFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.proxyRadioClient = new System.Windows.Forms.RadioButton();
            this.proxyRadioServer = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.proxyFlowPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.editorViewPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(533, 389);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Controls.Add(this.clearButton);
            this.flowLayoutPanel1.Controls.Add(this.sendButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 352);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(527, 34);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(449, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(368, 3);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 1;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(287, 3);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // editorViewPanel
            // 
            this.editorViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorViewPanel.Location = new System.Drawing.Point(3, 43);
            this.editorViewPanel.Name = "editorViewPanel";
            this.editorViewPanel.Size = new System.Drawing.Size(527, 303);
            this.editorViewPanel.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.editorViewSelect);
            this.flowLayoutPanel2.Controls.Add(this.proxyFlowPanel);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(527, 34);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // editorViewSelect
            // 
            this.editorViewSelect.DisplayMember = "ID";
            this.editorViewSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.editorViewSelect.Location = new System.Drawing.Point(69, 3);
            this.editorViewSelect.Name = "editorViewSelect";
            this.editorViewSelect.Size = new System.Drawing.Size(121, 21);
            this.editorViewSelect.TabIndex = 1;
            this.editorViewSelect.SelectedIndexChanged += new System.EventHandler(this.editorViewSelect_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Editor type:";
            // 
            // proxyFlowPanel
            // 
            this.proxyFlowPanel.Controls.Add(this.proxyRadioClient);
            this.proxyFlowPanel.Controls.Add(this.proxyRadioServer);
            this.proxyFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proxyFlowPanel.Location = new System.Drawing.Point(196, 3);
            this.proxyFlowPanel.Name = "proxyFlowPanel";
            this.proxyFlowPanel.Size = new System.Drawing.Size(200, 21);
            this.proxyFlowPanel.TabIndex = 3;
            // 
            // proxyRadioClient
            // 
            this.proxyRadioClient.AutoSize = true;
            this.proxyRadioClient.Location = new System.Drawing.Point(3, 3);
            this.proxyRadioClient.Name = "proxyRadioClient";
            this.proxyRadioClient.Size = new System.Drawing.Size(66, 17);
            this.proxyRadioClient.TabIndex = 0;
            this.proxyRadioClient.Text = "To client";
            this.proxyRadioClient.UseVisualStyleBackColor = true;
            // 
            // proxyRadioServer
            // 
            this.proxyRadioServer.AutoSize = true;
            this.proxyRadioServer.Checked = true;
            this.proxyRadioServer.Location = new System.Drawing.Point(75, 3);
            this.proxyRadioServer.Name = "proxyRadioServer";
            this.proxyRadioServer.Size = new System.Drawing.Size(70, 17);
            this.proxyRadioServer.TabIndex = 1;
            this.proxyRadioServer.TabStop = true;
            this.proxyRadioServer.Text = "To server";
            this.proxyRadioServer.UseVisualStyleBackColor = true;
            // 
            // EditorMasterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 389);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EditorMasterView";
            this.Text = "EditorMasterView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.proxyFlowPanel.ResumeLayout(false);
            this.proxyFlowPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Panel editorViewPanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox editorViewSelect;
        private System.Windows.Forms.FlowLayoutPanel proxyFlowPanel;
        private System.Windows.Forms.RadioButton proxyRadioClient;
        private System.Windows.Forms.RadioButton proxyRadioServer;
    }
}