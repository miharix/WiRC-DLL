namespace WiRCmk_minimal_demo
{
    partial class minimal
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
            this.button1 = new System.Windows.Forms.Button();
            this.center = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.max = new System.Windows.Forms.Button();
            this.min = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 22);
            this.button1.TabIndex = 0;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // center
            // 
            this.center.Location = new System.Drawing.Point(83, 129);
            this.center.Name = "center";
            this.center.Size = new System.Drawing.Size(111, 23);
            this.center.TabIndex = 1;
            this.center.Text = "servo 1 center";
            this.center.UseVisualStyleBackColor = true;
            this.center.Click += new System.EventHandler(this.center_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(151, 60);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(81, 34);
            this.button3.TabIndex = 2;
            this.button3.Text = "disconect";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // max
            // 
            this.max.Location = new System.Drawing.Point(151, 167);
            this.max.Name = "max";
            this.max.Size = new System.Drawing.Size(111, 23);
            this.max.TabIndex = 3;
            this.max.Text = "servo 1 max";
            this.max.UseVisualStyleBackColor = true;
            this.max.Click += new System.EventHandler(this.max_Click);
            // 
            // min
            // 
            this.min.Location = new System.Drawing.Point(12, 167);
            this.min.Name = "min";
            this.min.Size = new System.Drawing.Size(111, 23);
            this.min.TabIndex = 4;
            this.min.Text = "servo 1 min";
            this.min.UseVisualStyleBackColor = true;
            this.min.Click += new System.EventHandler(this.min_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(115, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Conected to:";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(12, 196);
            this.trackBar1.Maximum = 2200;
            this.trackBar1.Minimum = 800;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(250, 45);
            this.trackBar1.TabIndex = 6;
            this.trackBar1.Value = 800;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // minimal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.min);
            this.Controls.Add(this.max);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.center);
            this.Controls.Add(this.button1);
            this.Name = "minimal";
            this.Text = "Minimal GUI";
            this.Load += new System.EventHandler(this.minimal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button center;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button max;
        private System.Windows.Forms.Button min;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}

