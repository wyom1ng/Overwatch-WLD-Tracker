using System;
using System.Windows.Forms;

namespace OverwatchWLDTracker.Forms
{
    public partial class Format : Form
    {
        private bool seperate = false;
        private bool json = false;
        public Format()
        {
            InitializeComponent();
        }

        void label1_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("%WIN% \t\t- will be replaced with the number of wins\n" + 
                          "%LOSS% \t- will be replaced with the number of losses\n" +
                          "%DRAW% \t- will be replaced with the number of draws\n" +
                          "%WR% \t\t- will be replaced with your win percentage\n",
                          label1);
        }

        void label1_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(label1);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !textBox1.Enabled;
            checkBox2.Enabled = !checkBox2.Enabled;

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
            Vars.outp.Json = json;
            Vars.outp.Format = textBox1.Text;
            FileWriter.Save();
            FileWriter.Out();
            Hide();
        }

        private void Format_Load(object sender, EventArgs e)
        {
            //load text into textbox
            textBox1.Text = Vars.outp.Format;
            checkBox1.Checked = Vars.outp.Seperate;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = !textBox1.Enabled;
            checkBox1.Enabled = !checkBox1.Enabled;

            json = !json;
        }
    }
}
