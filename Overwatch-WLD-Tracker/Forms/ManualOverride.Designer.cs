namespace OverwatchWLDTracker.Forms
{
    partial class ManualOverride
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManualOverride));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.wins = new System.Windows.Forms.NumericUpDown();
            this.losses = new System.Windows.Forms.NumericUpDown();
            this.draws = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.wins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.losses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.draws)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Wins:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Losses";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Draws";
            // 
            // wins
            // 
            this.wins.Location = new System.Drawing.Point(68, 7);
            this.wins.Name = "wins";
            this.wins.Size = new System.Drawing.Size(120, 20);
            this.wins.TabIndex = 3;
            // 
            // losses
            // 
            this.losses.Location = new System.Drawing.Point(68, 39);
            this.losses.Name = "losses";
            this.losses.Size = new System.Drawing.Size(120, 20);
            this.losses.TabIndex = 4;
            // 
            // draws
            // 
            this.draws.Location = new System.Drawing.Point(68, 70);
            this.draws.Name = "draws";
            this.draws.Size = new System.Drawing.Size(120, 20);
            this.draws.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(176, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ManualOverride
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 126);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.draws);
            this.Controls.Add(this.losses);
            this.Controls.Add(this.wins);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(219, 165);
            this.MinimumSize = new System.Drawing.Size(219, 165);
            this.Name = "ManualOverride";
            this.Text = "ManualOverride";
            ((System.ComponentModel.ISupportInitialize)(this.wins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.losses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.draws)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown wins;
        private System.Windows.Forms.NumericUpDown losses;
        private System.Windows.Forms.NumericUpDown draws;
        private System.Windows.Forms.Button button1;
    }
}