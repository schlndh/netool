namespace Netool.Dialogs
{
    partial class SaveTempLogFileDialog
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
            this.cancelBtn = new System.Windows.Forms.Button();
            this.noToAllBtn = new System.Windows.Forms.Button();
            this.noBtn = new System.Windows.Forms.Button();
            this.yesToAllBtn = new System.Windows.Forms.Button();
            this.yesBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.messageLabel = new System.Windows.Forms.Label();
            this.iconPictureBox = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(411, 144);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cancelBtn);
            this.flowLayoutPanel1.Controls.Add(this.noToAllBtn);
            this.flowLayoutPanel1.Controls.Add(this.noBtn);
            this.flowLayoutPanel1.Controls.Add(this.yesToAllBtn);
            this.flowLayoutPanel1.Controls.Add(this.yesBtn);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 107);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flowLayoutPanel1.Size = new System.Drawing.Size(405, 34);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(327, 3);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // noToAllBtn
            // 
            this.noToAllBtn.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noToAllBtn.Location = new System.Drawing.Point(246, 3);
            this.noToAllBtn.Name = "noToAllBtn";
            this.noToAllBtn.Size = new System.Drawing.Size(75, 23);
            this.noToAllBtn.TabIndex = 3;
            this.noToAllBtn.Text = "NoToAll";
            this.noToAllBtn.UseVisualStyleBackColor = true;
            this.noToAllBtn.Click += new System.EventHandler(this.noToAllBtn_Click);
            // 
            // noBtn
            // 
            this.noBtn.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noBtn.Location = new System.Drawing.Point(165, 3);
            this.noBtn.Name = "noBtn";
            this.noBtn.Size = new System.Drawing.Size(75, 23);
            this.noBtn.TabIndex = 2;
            this.noBtn.Text = "No";
            this.noBtn.UseVisualStyleBackColor = true;
            this.noBtn.Click += new System.EventHandler(this.noBtn_Click);
            // 
            // yesToAllBtn
            // 
            this.yesToAllBtn.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesToAllBtn.Location = new System.Drawing.Point(84, 3);
            this.yesToAllBtn.Name = "yesToAllBtn";
            this.yesToAllBtn.Size = new System.Drawing.Size(75, 23);
            this.yesToAllBtn.TabIndex = 1;
            this.yesToAllBtn.Text = "Yes to All";
            this.yesToAllBtn.UseVisualStyleBackColor = true;
            this.yesToAllBtn.Click += new System.EventHandler(this.yesToAllBtn_Click);
            // 
            // yesBtn
            // 
            this.yesBtn.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesBtn.Location = new System.Drawing.Point(3, 3);
            this.yesBtn.Name = "yesBtn";
            this.yesBtn.Size = new System.Drawing.Size(75, 23);
            this.yesBtn.TabIndex = 0;
            this.yesBtn.Text = "Yes";
            this.yesBtn.UseVisualStyleBackColor = true;
            this.yesBtn.Click += new System.EventHandler(this.yesBtn_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.messageLabel, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.iconPictureBox, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(405, 98);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // messageLabel
            // 
            this.messageLabel.AutoSize = true;
            this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.messageLabel.Location = new System.Drawing.Point(83, 0);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Padding = new System.Windows.Forms.Padding(20, 20, 0, 0);
            this.messageLabel.Size = new System.Drawing.Size(319, 98);
            this.messageLabel.TabIndex = 2;
            this.messageLabel.Text = "Instance log for ______ is in temporary file. Do you wish to save it?";
            // 
            // iconPictureBox
            // 
            this.iconPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconPictureBox.Location = new System.Drawing.Point(3, 3);
            this.iconPictureBox.Name = "iconPictureBox";
            this.iconPictureBox.Padding = new System.Windows.Forms.Padding(20, 15, 0, 0);
            this.iconPictureBox.Size = new System.Drawing.Size(74, 92);
            this.iconPictureBox.TabIndex = 3;
            this.iconPictureBox.TabStop = false;
            // 
            // SaveTempLogFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 144);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SaveTempLogFileDialog";
            this.Text = "Save instance log?";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button noToAllBtn;
        private System.Windows.Forms.Button noBtn;
        private System.Windows.Forms.Button yesToAllBtn;
        private System.Windows.Forms.Button yesBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.PictureBox iconPictureBox;
    }
}