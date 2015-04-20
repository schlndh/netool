namespace Netool.Views
{
    partial class ServerView
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
            this.stop = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.send = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.clients = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sendData = new System.Windows.Forms.TextBox();
            this.start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // stop
            // 
            this.stop.Location = new System.Drawing.Point(402, 346);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(75, 23);
            this.stop.TabIndex = 0;
            this.stop.Text = "Stop";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(13, 13);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.log.Size = new System.Drawing.Size(464, 166);
            this.log.TabIndex = 2;
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(240, 346);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(75, 23);
            this.send.TabIndex = 3;
            this.send.Text = "Send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(321, 346);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 4;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // clients
            // 
            this.clients.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clients.FormattingEnabled = true;
            this.clients.Location = new System.Drawing.Point(13, 198);
            this.clients.Name = "clients";
            this.clients.Size = new System.Drawing.Size(212, 21);
            this.clients.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Client";
            // 
            // sendData
            // 
            this.sendData.Location = new System.Drawing.Point(13, 226);
            this.sendData.Multiline = true;
            this.sendData.Name = "sendData";
            this.sendData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sendData.Size = new System.Drawing.Size(464, 106);
            this.sendData.TabIndex = 7;
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(159, 346);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(75, 23);
            this.start.TabIndex = 8;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // ServerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 381);
            this.Controls.Add(this.start);
            this.Controls.Add(this.sendData);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clients);
            this.Controls.Add(this.close);
            this.Controls.Add(this.send);
            this.Controls.Add(this.log);
            this.Controls.Add(this.stop);
            this.Name = "ServerView";
            this.Text = "ServerView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.ComboBox clients;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sendData;
        private System.Windows.Forms.Button start;
    }
}