namespace RDC.Plugins.ChromeZone
{
    partial class DebugWindow
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
            this.DebugListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // DebugListBox
            // 
            this.DebugListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DebugListBox.FormattingEnabled = true;
            this.DebugListBox.HorizontalScrollbar = true;
            this.DebugListBox.Location = new System.Drawing.Point(0, 0);
            this.DebugListBox.Name = "DebugListBox";
            this.DebugListBox.ScrollAlwaysVisible = true;
            this.DebugListBox.Size = new System.Drawing.Size(800, 450);
            this.DebugListBox.TabIndex = 0;
            // 
            // DebugWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.DebugListBox);
            this.Name = "DebugWindow";
            this.Text = "DebugWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox DebugListBox;
    }
}