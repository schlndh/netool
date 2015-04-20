namespace Netool.Dialogs
{
    partial class TcpClientDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.localEndPointChange = new System.Windows.Forms.Button();
            this.localEndPoint = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.remoteEndPointChange = new System.Windows.Forms.Button();
            this.remoteEndPoint = new System.Windows.Forms.TextBox();
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Local IP Endpoint";
            // 
            // localEndPointChange
            // 
            this.localEndPointChange.CausesValidation = false;
            this.localEndPointChange.Location = new System.Drawing.Point(181, 25);
            this.localEndPointChange.Name = "localEndPointChange";
            this.localEndPointChange.Size = new System.Drawing.Size(26, 20);
            this.localEndPointChange.TabIndex = 10;
            this.localEndPointChange.Text = "...";
            this.localEndPointChange.UseVisualStyleBackColor = true;
            this.localEndPointChange.Click += new System.EventHandler(this.localEndPointChange_Click);
            // 
            // localEndPoint
            // 
            this.localEndPoint.Location = new System.Drawing.Point(31, 25);
            this.localEndPoint.Name = "localEndPoint";
            this.localEndPoint.ReadOnly = true;
            this.localEndPoint.Size = new System.Drawing.Size(144, 20);
            this.localEndPoint.TabIndex = 9;
            this.localEndPoint.Validating += new System.ComponentModel.CancelEventHandler(this.localEndPoint_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Remote IP Endpoint";
            // 
            // remoteEndPointChange
            // 
            this.remoteEndPointChange.CausesValidation = false;
            this.remoteEndPointChange.Location = new System.Drawing.Point(181, 65);
            this.remoteEndPointChange.Name = "remoteEndPointChange";
            this.remoteEndPointChange.Size = new System.Drawing.Size(26, 20);
            this.remoteEndPointChange.TabIndex = 13;
            this.remoteEndPointChange.Text = "...";
            this.remoteEndPointChange.UseVisualStyleBackColor = true;
            this.remoteEndPointChange.Click += new System.EventHandler(this.remoteEndPointChange_Click);
            // 
            // remoteEndPoint
            // 
            this.remoteEndPoint.Location = new System.Drawing.Point(31, 65);
            this.remoteEndPoint.Name = "remoteEndPoint";
            this.remoteEndPoint.ReadOnly = true;
            this.remoteEndPoint.Size = new System.Drawing.Size(144, 20);
            this.remoteEndPoint.TabIndex = 12;
            this.remoteEndPoint.Validating += new System.ComponentModel.CancelEventHandler(this.remoteEndPoint_Validating);
            // 
            // cancel
            // 
            this.cancel.CausesValidation = false;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(121, 91);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 16;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // ok
            // 
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(40, 91);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 15;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            // 
            // TcpClientDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 130);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.remoteEndPointChange);
            this.Controls.Add(this.remoteEndPoint);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.localEndPointChange);
            this.Controls.Add(this.localEndPoint);
            this.Name = "TcpClientDialog";
            this.Text = "TcpClientDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button localEndPointChange;
        private System.Windows.Forms.TextBox localEndPoint;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button remoteEndPointChange;
        private System.Windows.Forms.TextBox remoteEndPoint;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
    }
}