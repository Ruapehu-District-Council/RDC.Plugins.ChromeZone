using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDC.Plugins.ChromeZone
{
    public partial class DebugWindow : Form
    {
        public DebugWindow(List<string> Items)
        {
            InitializeComponent();

            Items.ForEach(AddDebugItem);
        }

        public void SetTabName(string TabName)
        {
            Text = $"Tab Name: {TabName}";
        }

        public void AddDebugItem(string Item)
        {
            DebugListBox.Items.Add(Item);
        }
    }
}
