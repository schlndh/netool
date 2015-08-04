namespace Netool.Views.Editor
{
    partial class HexView
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
            this.data = new Be.Windows.Forms.HexBox();
            this.SuspendLayout();
            //
            // data
            //
            this.data.ColumnInfoVisible = true;
            this.data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.data.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.data.LineInfoVisible = true;
            this.data.Location = new System.Drawing.Point(0, 0);
            this.data.Name = "data";
            this.data.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.data.Size = new System.Drawing.Size(501, 329);
            this.data.StringViewVisible = true;
            this.data.TabIndex = 1;
            this.data.VScrollBarVisible = true;
            //
            // HexView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 329);
            this.Controls.Add(this.data);
            this.Name = "HexView";
            this.Text = "HexView";
            this.ResumeLayout(false);

        }

        #endregion

        private Be.Windows.Forms.HexBox data;


    }
}