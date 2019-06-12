using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverwatchWLDTracker.Forms
{
    public partial class ManualOverride : Form
    {
        public ManualOverride()
        {
            InitializeComponent();
            wins.Value = Vars.wld.wins;
            losses.Value = Vars.wld.losses;
            draws.Value = Vars.wld.draws;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Vars.wld.wins = (int)wins.Value;
            Vars.wld.losses = (int)losses.Value;
            Vars.wld.draws = (int)draws.Value;
            FileWriter.Out();
            Close();
        }
    }
}