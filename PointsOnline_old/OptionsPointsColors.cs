using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PointsOnlineProject
{
    public partial class OptionsPointsColors : Form
    {
        public OptionsPointsColors()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            pictureBox1.BackColor = colorDialog1.Color;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            pictureBox2.BackColor = colorDialog1.Color;
        }
    }
}
