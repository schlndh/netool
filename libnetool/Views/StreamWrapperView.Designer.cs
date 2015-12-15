namespace Netool.Views
{
    partial class StreamWrapperView
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.wrapperEditBtn = new System.Windows.Forms.Button();
            this.selectedWrapper = new System.Windows.Forms.Label();
            this.dataViewSelection = new Netool.Views.Components.DataViewSelection();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dataViewSelection, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(456, 307);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.Controls.Add(this.wrapperEditBtn, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.selectedWrapper, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(450, 34);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // wrapperEditBtn
            // 
            this.wrapperEditBtn.Location = new System.Drawing.Point(413, 3);
            this.wrapperEditBtn.Name = "wrapperEditBtn";
            this.wrapperEditBtn.Size = new System.Drawing.Size(34, 23);
            this.wrapperEditBtn.TabIndex = 0;
            this.wrapperEditBtn.Text = "...";
            this.wrapperEditBtn.UseVisualStyleBackColor = true;
            this.wrapperEditBtn.Click += new System.EventHandler(this.wrapperEditBtn_Click);
            // 
            // selectedWrapper
            // 
            this.selectedWrapper.AutoSize = true;
            this.selectedWrapper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedWrapper.Location = new System.Drawing.Point(3, 0);
            this.selectedWrapper.Name = "selectedWrapper";
            this.selectedWrapper.Size = new System.Drawing.Size(404, 34);
            this.selectedWrapper.TabIndex = 1;
            this.selectedWrapper.Text = "DATA";
            this.selectedWrapper.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dataViewSelection
            // 
            this.dataViewSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataViewSelection.IsEditor = false;
            this.dataViewSelection.Label = "Data view type:";
            this.dataViewSelection.Location = new System.Drawing.Point(3, 43);
            this.dataViewSelection.Name = "dataViewSelection";
            this.dataViewSelection.SelectedIndex = -1;
            this.dataViewSelection.Size = new System.Drawing.Size(450, 261);
            this.dataViewSelection.Stream = null;
            this.dataViewSelection.TabIndex = 0;
            // 
            // StreamWrapperView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 307);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "StreamWrapperView";
            this.Text = "StreamWrapperView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Components.DataViewSelection dataViewSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button wrapperEditBtn;
        private System.Windows.Forms.Label selectedWrapper;
    }
}