namespace Netool.Views
{
    partial class ClientView
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
            this.log = new System.Windows.Forms.TextBox();
            this.sendData = new System.Windows.Forms.TextBox();
            this.send = new System.Windows.Forms.Button();
            this.stop = new System.Windows.Forms.Button();
            this.start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(12, 12);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.log.Size = new System.Drawing.Size(464, 166);
            this.log.TabIndex = 3;
            // 
            // sendData
            // 
            this.sendData.Location = new System.Drawing.Point(12, 197);
            this.sendData.Multiline = true;
            this.sendData.Name = "sendData";
            this.sendData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sendData.Size = new System.Drawing.Size(464, 106);
            this.sendData.TabIndex = 8;
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(311, 309);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(75, 23);
            this.send.TabIndex = 10;
            this.send.Text = "Send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // stop
            // 
            this.stop.Location = new System.Drawing.Point(392, 309);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(75, 23);
            this.stop.TabIndex = 9;
            this.stop.Text = "Stop";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(230, 309);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(75, 23);
            this.start.TabIndex = 11;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // ClientView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 344);
            this.Controls.Add(this.start);
            this.Controls.Add(this.send);
            this.Controls.Add(this.stop);
            this.Controls.Add(this.sendData);
            this.Controls.Add(this.log);
            this.Name = "ClientView";
            this.Text = "ClientView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.TextBox sendData;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.Button start;
    }
}