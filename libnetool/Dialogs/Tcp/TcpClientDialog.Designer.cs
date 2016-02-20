namespace Netool.Dialogs.Tcp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TcpClientDialog));
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.localEndPoint = new Netool.Windows.Forms.IPEndPointControl();
            this.remoteEndPoint = new Netool.Windows.Forms.IPEndPointControl();
            this.label1 = new System.Windows.Forms.Label();
            this.socketSettings = new Netool.Views.Components.SocketSettings();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            //
            // cancel
            //
            this.cancel.CausesValidation = false;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(509, 4);
            this.cancel.Margin = new System.Windows.Forms.Padding(4);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(100, 28);
            this.cancel.TabIndex = 14;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            //
            // ok
            //
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(401, 4);
            this.ok.Margin = new System.Windows.Forms.Padding(4);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(100, 28);
            this.ok.TabIndex = 13;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.localEndPoint, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.remoteEndPoint, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.socketSettings, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(621, 568);
            this.tableLayoutPanel1.TabIndex = 11;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(4, 198);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(155, 37);
            this.label3.TabIndex = 16;
            this.label3.Text = "Advanced socket setup";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(4, 99);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 37);
            this.label2.TabIndex = 14;
            this.label2.Text = "Remote EndPoint";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flowLayoutPanel1
            //
            this.flowLayoutPanel1.Controls.Add(this.cancel);
            this.flowLayoutPanel1.Controls.Add(this.ok);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 523);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flowLayoutPanel1.Size = new System.Drawing.Size(613, 41);
            this.flowLayoutPanel1.TabIndex = 0;
            //
            // localEndPoint
            //
            this.localEndPoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localEndPoint.EndPoint = null;
            this.localEndPoint.IPLabel = "IP";
            this.localEndPoint.Location = new System.Drawing.Point(5, 42);
            this.localEndPoint.Margin = new System.Windows.Forms.Padding(5);
            this.localEndPoint.Name = "localEndPoint";
            this.localEndPoint.PortLabel = "Port";
            this.localEndPoint.PreferedAddressFamily = Netool.Windows.Forms.IPAddressControl.PreferedFamily.None;
            this.localEndPoint.ShowLabels = true;
            this.localEndPoint.Size = new System.Drawing.Size(611, 52);
            this.localEndPoint.TabIndex = 11;
            this.localEndPoint.Validating += new System.ComponentModel.CancelEventHandler(this.endPoint_Validating);
            //
            // remoteEndPoint
            //
            this.remoteEndPoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteEndPoint.EndPoint = null;
            this.remoteEndPoint.IPLabel = "IP";
            this.remoteEndPoint.Location = new System.Drawing.Point(5, 141);
            this.remoteEndPoint.Margin = new System.Windows.Forms.Padding(5);
            this.remoteEndPoint.Name = "remoteEndPoint";
            this.remoteEndPoint.PortLabel = "Port";
            this.remoteEndPoint.PreferedAddressFamily = Netool.Windows.Forms.IPAddressControl.PreferedFamily.None;
            this.remoteEndPoint.ShowLabels = true;
            this.remoteEndPoint.Size = new System.Drawing.Size(611, 52);
            this.remoteEndPoint.TabIndex = 12;
            this.remoteEndPoint.Validating += new System.ComponentModel.CancelEventHandler(this.endPoint_Validating);
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 37);
            this.label1.TabIndex = 13;
            this.label1.Text = "Local EndPoint";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // socketSettings
            //
            this.socketSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.socketSettings.Location = new System.Drawing.Point(3, 238);
            this.socketSettings.Name = "socketSettings";
            this.socketSettings.Size = new System.Drawing.Size(615, 278);
            this.socketSettings.TabIndex = 15;
            //
            // TcpClientDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(621, 568);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TcpClientDialog";
            this.Text = "Tcp Server Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TcpClientDialog_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Windows.Forms.IPEndPointControl localEndPoint;
        private System.Windows.Forms.Label label2;
        private Windows.Forms.IPEndPointControl remoteEndPoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private Views.Components.SocketSettings socketSettings;
    }
}