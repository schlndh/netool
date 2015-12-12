namespace Netool.Views.Components
{
    partial class DataViewSelection
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.innerViewSelectLabel = new System.Windows.Forms.Label();
            this.innerViewSelect = new System.Windows.Forms.ComboBox();
            this.exportBtn = new System.Windows.Forms.Button();
            this.innerViewPanel = new System.Windows.Forms.Panel();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.exportProgressBar = new System.Windows.Forms.ProgressBar();
            this.exportBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.innerViewPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(534, 214);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.innerViewSelectLabel);
            this.flowLayoutPanel1.Controls.Add(this.innerViewSelect);
            this.flowLayoutPanel1.Controls.Add(this.exportBtn);
            this.flowLayoutPanel1.Controls.Add(this.exportProgressBar);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(528, 34);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // innerViewSelectLabel
            // 
            this.innerViewSelectLabel.AutoSize = true;
            this.innerViewSelectLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerViewSelectLabel.Location = new System.Drawing.Point(3, 0);
            this.innerViewSelectLabel.Name = "innerViewSelectLabel";
            this.innerViewSelectLabel.Size = new System.Drawing.Size(81, 29);
            this.innerViewSelectLabel.TabIndex = 2;
            this.innerViewSelectLabel.Text = "Data view type:";
            this.innerViewSelectLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // innerViewSelect
            // 
            this.innerViewSelect.DisplayMember = "ID";
            this.innerViewSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.innerViewSelect.Location = new System.Drawing.Point(90, 3);
            this.innerViewSelect.Name = "innerViewSelect";
            this.innerViewSelect.Size = new System.Drawing.Size(121, 21);
            this.innerViewSelect.TabIndex = 1;
            this.innerViewSelect.ValueMember = "Value";
            this.innerViewSelect.SelectedIndexChanged += new System.EventHandler(this.innerViewSelect_SelectedIndexChanged);
            // 
            // exportBtn
            // 
            this.exportBtn.Location = new System.Drawing.Point(217, 3);
            this.exportBtn.Name = "exportBtn";
            this.exportBtn.Size = new System.Drawing.Size(75, 23);
            this.exportBtn.TabIndex = 3;
            this.exportBtn.Text = "Export to file";
            this.exportBtn.UseVisualStyleBackColor = true;
            this.exportBtn.Click += new System.EventHandler(this.exportBtn_Click);
            // 
            // innerViewPanel
            // 
            this.innerViewPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.innerViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerViewPanel.Location = new System.Drawing.Point(3, 43);
            this.innerViewPanel.Name = "innerViewPanel";
            this.innerViewPanel.Size = new System.Drawing.Size(528, 168);
            this.innerViewPanel.TabIndex = 1;
            // 
            // exportProgressBar
            // 
            this.exportProgressBar.Location = new System.Drawing.Point(298, 3);
            this.exportProgressBar.Name = "exportProgressBar";
            this.exportProgressBar.Size = new System.Drawing.Size(100, 23);
            this.exportProgressBar.TabIndex = 4;
            // 
            // exportBackgroundWorker
            // 
            this.exportBackgroundWorker.WorkerReportsProgress = true;
            this.exportBackgroundWorker.WorkerSupportsCancellation = true;
            this.exportBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.exportBackgroundWorker_DoWork);
            this.exportBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.exportBackgroundWorker_ProgressChanged);
            this.exportBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.exportBackgroundWorker_RunWorkerCompleted);
            // 
            // DataViewSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DataViewSelection";
            this.Size = new System.Drawing.Size(534, 214);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label innerViewSelectLabel;
        private System.Windows.Forms.ComboBox innerViewSelect;
        private System.Windows.Forms.Button exportBtn;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        private System.Windows.Forms.Panel innerViewPanel;
        private System.Windows.Forms.ProgressBar exportProgressBar;
        private System.ComponentModel.BackgroundWorker exportBackgroundWorker;


    }
}
