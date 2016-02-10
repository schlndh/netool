namespace Netool.Views
{
    partial class WebSocketMessageView
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
            this.finCheckBox = new System.Windows.Forms.CheckBox();
            this.rsv1CheckBox = new System.Windows.Forms.CheckBox();
            this.rsv2CheckBox = new System.Windows.Forms.CheckBox();
            this.rsv3CheckBox = new System.Windows.Forms.CheckBox();
            this.maskCheckBox = new System.Windows.Forms.CheckBox();
            this.dataViewSelection = new Netool.Views.Components.DataViewSelection();
            this.maskTextBox = new System.Windows.Forms.TextBox();
            this.maskNoteLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.opcodeComboBox = new System.Windows.Forms.ComboBox();
            this.payloadLengthLabel = new System.Windows.Forms.Label();
            this.payloadLengthTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataViewSelection, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 280);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.finCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.rsv1CheckBox);
            this.flowLayoutPanel1.Controls.Add(this.rsv2CheckBox);
            this.flowLayoutPanel1.Controls.Add(this.rsv3CheckBox);
            this.flowLayoutPanel1.Controls.Add(this.maskCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.maskTextBox);
            this.flowLayoutPanel1.Controls.Add(this.maskNoteLabel);
            this.flowLayoutPanel1.Controls.Add(this.label8);
            this.flowLayoutPanel1.Controls.Add(this.opcodeComboBox);
            this.flowLayoutPanel1.Controls.Add(this.payloadLengthLabel);
            this.flowLayoutPanel1.Controls.Add(this.payloadLengthTextBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(540, 53);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // finCheckBox
            // 
            this.finCheckBox.AutoSize = true;
            this.finCheckBox.Checked = true;
            this.finCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.finCheckBox.Location = new System.Drawing.Point(3, 3);
            this.finCheckBox.Name = "finCheckBox";
            this.finCheckBox.Size = new System.Drawing.Size(43, 17);
            this.finCheckBox.TabIndex = 8;
            this.finCheckBox.Text = "FIN";
            this.finCheckBox.UseVisualStyleBackColor = true;
            // 
            // rsv1CheckBox
            // 
            this.rsv1CheckBox.AutoSize = true;
            this.rsv1CheckBox.Location = new System.Drawing.Point(52, 3);
            this.rsv1CheckBox.Name = "rsv1CheckBox";
            this.rsv1CheckBox.Size = new System.Drawing.Size(54, 17);
            this.rsv1CheckBox.TabIndex = 9;
            this.rsv1CheckBox.Text = "RSV1";
            this.rsv1CheckBox.UseVisualStyleBackColor = true;
            // 
            // rsv2CheckBox
            // 
            this.rsv2CheckBox.AutoSize = true;
            this.rsv2CheckBox.Location = new System.Drawing.Point(112, 3);
            this.rsv2CheckBox.Name = "rsv2CheckBox";
            this.rsv2CheckBox.Size = new System.Drawing.Size(54, 17);
            this.rsv2CheckBox.TabIndex = 10;
            this.rsv2CheckBox.Text = "RSV2";
            this.rsv2CheckBox.UseVisualStyleBackColor = true;
            // 
            // rsv3CheckBox
            // 
            this.rsv3CheckBox.AutoSize = true;
            this.rsv3CheckBox.Location = new System.Drawing.Point(172, 3);
            this.rsv3CheckBox.Name = "rsv3CheckBox";
            this.rsv3CheckBox.Size = new System.Drawing.Size(54, 17);
            this.rsv3CheckBox.TabIndex = 11;
            this.rsv3CheckBox.Text = "RSV3";
            this.rsv3CheckBox.UseVisualStyleBackColor = true;
            // 
            // maskCheckBox
            // 
            this.maskCheckBox.AutoSize = true;
            this.maskCheckBox.Location = new System.Drawing.Point(232, 3);
            this.maskCheckBox.Name = "maskCheckBox";
            this.maskCheckBox.Size = new System.Drawing.Size(56, 17);
            this.maskCheckBox.TabIndex = 12;
            this.maskCheckBox.Text = "MASK";
            this.maskCheckBox.UseVisualStyleBackColor = true;
            this.maskCheckBox.CheckedChanged += new System.EventHandler(this.maskCheckBox_CheckedChanged);
            // 
            // dataViewSelection
            // 
            this.dataViewSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataViewSelection.IsEditor = false;
            this.dataViewSelection.Label = "Data view type:";
            this.dataViewSelection.Location = new System.Drawing.Point(3, 62);
            this.dataViewSelection.Name = "dataViewSelection";
            this.dataViewSelection.SelectedIndex = -1;
            this.dataViewSelection.Size = new System.Drawing.Size(540, 215);
            this.dataViewSelection.Stream = null;
            this.dataViewSelection.TabIndex = 3;
            // 
            // maskTextBox
            // 
            this.maskTextBox.Location = new System.Drawing.Point(294, 3);
            this.maskTextBox.Name = "maskTextBox";
            this.maskTextBox.Size = new System.Drawing.Size(109, 20);
            this.maskTextBox.TabIndex = 13;
            // 
            // maskNoteLabel
            // 
            this.maskNoteLabel.AutoSize = true;
            this.maskNoteLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maskNoteLabel.Location = new System.Drawing.Point(409, 0);
            this.maskNoteLabel.Name = "maskNoteLabel";
            this.maskNoteLabel.Size = new System.Drawing.Size(99, 26);
            this.maskNoteLabel.TabIndex = 14;
            this.maskNoteLabel.Text = "(Big-Endian UInt32)";
            this.maskNoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 27);
            this.label8.TabIndex = 15;
            this.label8.Text = "Opcode";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // opcodeComboBox
            // 
            this.opcodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.opcodeComboBox.FormattingEnabled = true;
            this.opcodeComboBox.Location = new System.Drawing.Point(54, 29);
            this.opcodeComboBox.Name = "opcodeComboBox";
            this.opcodeComboBox.Size = new System.Drawing.Size(121, 21);
            this.opcodeComboBox.TabIndex = 16;
            // 
            // payloadLengthLabel
            // 
            this.payloadLengthLabel.AutoSize = true;
            this.payloadLengthLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.payloadLengthLabel.Location = new System.Drawing.Point(181, 26);
            this.payloadLengthLabel.Name = "payloadLengthLabel";
            this.payloadLengthLabel.Size = new System.Drawing.Size(81, 27);
            this.payloadLengthLabel.TabIndex = 17;
            this.payloadLengthLabel.Text = "Payload Length";
            this.payloadLengthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // payloadLengthTextBox
            // 
            this.payloadLengthTextBox.Location = new System.Drawing.Point(268, 29);
            this.payloadLengthTextBox.Name = "payloadLengthTextBox";
            this.payloadLengthTextBox.Size = new System.Drawing.Size(100, 20);
            this.payloadLengthTextBox.TabIndex = 18;
            // 
            // WebSocketMessageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 280);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "WebSocketMessageView";
            this.Text = "WebSocketMessageView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox finCheckBox;
        private System.Windows.Forms.CheckBox rsv1CheckBox;
        private System.Windows.Forms.CheckBox rsv2CheckBox;
        private System.Windows.Forms.CheckBox rsv3CheckBox;
        private System.Windows.Forms.CheckBox maskCheckBox;
        private Components.DataViewSelection dataViewSelection;
        private System.Windows.Forms.TextBox maskTextBox;
        private System.Windows.Forms.Label maskNoteLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox opcodeComboBox;
        private System.Windows.Forms.Label payloadLengthLabel;
        private System.Windows.Forms.TextBox payloadLengthTextBox;
    }
}