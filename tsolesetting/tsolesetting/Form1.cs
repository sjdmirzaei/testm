using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tsolesetting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var formsetting = new FormSettings();
            formsetting.ShowDialog();
            //panelControl1.Controls.Add(formsetting);

            //Hey There!
            //it's a test!!!
        }
    }
}
