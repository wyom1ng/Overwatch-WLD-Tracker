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
    public partial class Format : Form
    {
        private bool seperate = false;
        public Format()
        {
            InitializeComponent();
        }

        void label1_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("%CSR% \t\t- will be replaced with your current SR\n" +
                          "%DSR% \t\t- will be replaced with difference in SR\n" +
                          "%DRAW% \t- will be replaced with the number of draws\n" +
                          "%HERO% \t- will be replaced with your current hero\n" +
                          "%LOSS% \t- will be replaced with the number of losses\n" +
                          "%MAP% \t- will be replaced with the current map\n" +
                          "%SSR% \t\t- will be replaced with your starting SR\n" +
                          "%WIN% \t\t- will be replaced with the number of wins\n" +
                          "%WR% \t\t- will be replaced with your win percentage\n" +
                          "%ELIM% \t- will be replaced with the number of eliminations\n" +
                          "%DMG% \t- will be replaced with the amount of damage\n" +
                          "%OBJK% \t- will be replaced with the number of objective kills\n" +
                          "%HEAL% \t- will be replaced with the amount of healing done\n" +
                          "%DEATH% \t- will be replaced with the number of deaths\n" +
                          "%EPL% \t\t- will be replaced with your eliminations per life\n\n" +
                          "The number should be set to your Scoreboard key code\n" +
                          "Default (Tab) is 9", label1);
        }

        void label1_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(label1);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !textBox1.Enabled;
            seperate = !seperate;
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void submit_Click(object sender, EventArgs e)
        {
            Vars.outp.Seperate = seperate;
            Vars.outp.Format = textBox1.Text;
            Vars.outp.SBKeyCode = (int)numericUpDown1.Value;
            FileWriter.Save();
            this.Hide();
        }

        private void Format_Load(object sender, EventArgs e)
        {
            //load text into textbox
            numericUpDown1.Value = Vars.outp.SBKeyCode;
            textBox1.Text = Vars.outp.Format;
            checkBox1.Checked = Vars.outp.Seperate;
        }
    }
}
