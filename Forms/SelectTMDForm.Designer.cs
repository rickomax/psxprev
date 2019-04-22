namespace PSXPrev.Forms
{
    partial class SelectTMDForm
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
            this.okButton = new System.Windows.Forms.Button();
            this.mainLabel = new System.Windows.Forms.Label();
            this.TMDListBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(107, 58);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(91, 24);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // mainLabel
            // 
            this.mainLabel.Location = new System.Drawing.Point(9, 12);
            this.mainLabel.Name = "mainLabel";
            this.mainLabel.Size = new System.Drawing.Size(283, 20);
            this.mainLabel.TabIndex = 5;
            this.mainLabel.Text = "Select a TMD Model";
            this.mainLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TMDListBox
            // 
            this.TMDListBox.FormattingEnabled = true;
            this.TMDListBox.Location = new System.Drawing.Point(12, 35);
            this.TMDListBox.Name = "TMDListBox";
            this.TMDListBox.Size = new System.Drawing.Size(280, 21);
            this.TMDListBox.TabIndex = 6;
            // 
            // SelectTMDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 90);
            this.Controls.Add(this.TMDListBox);
            this.Controls.Add(this.mainLabel);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectTMDForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PSXPrev";
            this.Load += new System.EventHandler(this.SelectTMDForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label mainLabel;
        private System.Windows.Forms.ComboBox TMDListBox;
    }
}