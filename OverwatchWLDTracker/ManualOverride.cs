using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverwatchWLDTracker
{
    public partial class ManualOverride : Form
    {
        public ManualOverride()
        {
            InitializeComponent();
            wins.Value = Vars.wld.Wins;
            losses.Value = Vars.wld.Losses;
            draws.Value = Vars.wld.Draws;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Vars.wld.Wins = (int)wins.Value;
            Vars.wld.Losses = (int)losses.Value;
            Vars.wld.Draws = (int)draws.Value;
            FileWriter.Out();
            Close();
        }
    }
}
