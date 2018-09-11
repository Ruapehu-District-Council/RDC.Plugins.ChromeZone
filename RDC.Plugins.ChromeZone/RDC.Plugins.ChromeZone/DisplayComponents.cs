using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDC.Plugins.ChromeZone
{
    public partial class DisplayComponents : Origen.Ozone.Plugins.OzonePlugin
    {
        public System.Windows.Forms.ToolStripContainer toolStripContainer;
        public System.Windows.Forms.ToolStrip toolStrip1;
        public System.Windows.Forms.ToolStripButton backButton;
        public System.Windows.Forms.ToolStripButton forwardButton;
        public ToolStripTextBox urlTextBox;
        public System.Windows.Forms.ToolStripButton goButton;
        public ToolStripButton refreshButton;
        public ToolStripButton homeButton;
        public ToolStripProgressBar toolStripProgressBar1;
        public ToolStripSeparator toolStripSeparator1;


        public void InitializeComponent()
        {
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.homeButton = new System.Windows.Forms.ToolStripButton();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.forwardButton = new System.Windows.Forms.ToolStripButton();
            this.refreshButton = new System.Windows.Forms.ToolStripButton();
            this.urlTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(1313, 585);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.RightToolStripPanelVisible = false;
            this.toolStripContainer.Size = new System.Drawing.Size(1313, 610);
            this.toolStripContainer.TabIndex = 0;
            this.toolStripContainer.Text = "toolStripContainer1";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.homeButton,
            this.backButton,
            this.toolStripSeparator1,
            this.forwardButton,
            this.refreshButton,
            this.urlTextBox,
            this.toolStripProgressBar1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(1313, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // homeButton
            // 
            this.homeButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.homeButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.homeButton.Name = "homeButton";
            this.homeButton.Size = new System.Drawing.Size(23, 22);
           
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.Transparent;
            this.backButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.backButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(23, 22);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // forwardButton
            // 
            this.forwardButton.BackColor = System.Drawing.Color.Transparent;
            this.forwardButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.forwardButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(23, 22);
            // 
            // refreshButton
            // 
            this.refreshButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.refreshButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(23, 22);
            // 
            // urlTextBox
            // 
            this.urlTextBox.AutoSize = false;
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.ReadOnly = true;
            this.urlTextBox.Size = new System.Drawing.Size(800, 25);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 22);
            this.toolStripProgressBar1.Visible = false;
            this.toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            this.toolStripProgressBar1.Value = 100;
            // 
            // DisplayComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer);
            this.Name = "DisplayComponents";
            this.Size = new System.Drawing.Size(1313, 610);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);


            //Setup the icons for the buttons
            Assembly asm = Assembly.GetExecutingAssembly();
            Bitmap backButtonImage = new Bitmap(asm.GetManifestResourceStream("RDC.Plugins.ChromeZone.Icons.arrow-left.png"));
            Bitmap forwardButtonImage = new Bitmap(asm.GetManifestResourceStream("RDC.Plugins.ChromeZone.Icons.arrow-right.png"));
            Bitmap refreshButtonImage = new Bitmap(asm.GetManifestResourceStream("RDC.Plugins.ChromeZone.Icons.refresh.png"));
            Bitmap homeButtonImage = new Bitmap(asm.GetManifestResourceStream("RDC.Plugins.ChromeZone.Icons.home.png"));

            this.backButton.Image = backButtonImage;
            this.forwardButton.Image = forwardButtonImage;
            this.refreshButton.Image = refreshButtonImage;
            this.homeButton.Image = homeButtonImage;

        }

    }
}
