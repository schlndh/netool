namespace Netool.Dialogs
{
    partial class TcpServerDialog
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
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.endPoint = new System.Windows.Forms.TextBox();
            this.endPointChange = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.maxConnections = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.CausesValidation = false;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(127, 98);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 5;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // ok
            // 
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(46, 98);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 4;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            // 
            // endPoint
            // 
            this.endPoint.Location = new System.Drawing.Point(46, 33);
            this.endPoint.Name = "endPoint";
            this.endPoint.ReadOnly = true;
            this.endPoint.Size = new System.Drawing.Size(144, 20);
            this.endPoint.TabIndex = 6;
            this.endPoint.Validating += new System.ComponentModel.CancelEventHandler(this.endPoint_Validating);
            // 
            // endPointChange
            // 
            this.endPointChange.CausesValidation = false;
            this.endPointChange.Location = new System.Drawing.Point(196, 33);
            this.endPointChange.Name = "endPointChange";
            this.endPointChange.Size = new System.Drawing.Size(26, 20);
            this.endPointChange.TabIndex = 7;
            this.endPointChange.Text = "...";
            this.endPointChange.UseVisualStyleBackColor = true;
            this.endPointChange.Click += new System.EventHandler(this.endPointChange_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "IP Endpoint";
            // 
            // maxConnections
            // 
            this.maxConnections.Location = new System.Drawing.Point(46, 72);
            this.maxConnections.Name = "maxConnections";
            this.maxConnections.Size = new System.Drawing.Size(86, 20);
            this.maxConnections.TabIndex = 9;
            this.maxConnections.Text = "100";
            this.maxConnections.Validating += new System.ComponentModel.CancelEventHandler(this.maxConnections_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Max Connections";
            // 
            // TcpServerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 138);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.maxConnections);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.endPointChange);
            this.Controls.Add(this.endPoint);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Name = "TcpServerDialog";
            this.Text = "TcpServerDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.TextBox endPoint;
        private System.Windows.Forms.Button endPointChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox maxConnections;
        private System.Windows.Forms.Label label2;
    }
}