using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*ADD*/
using simpleWiRC;

namespace WiRCmk_minimal_demo
{
    public partial class minimal : Form
    {
        /*ADD*/
        WiRCmk WiRCfunctions;
        List<WircReciwer> WiRCmoduli;

        public minimal()
        {
            InitializeComponent();
        }

        private void minimal_Load(object sender, EventArgs e)
        {
            WiRCfunctions = new WiRCmk("GUImini", "4A-40-5F-77-C2-47-29-8D-08-88-76-7E-B1-AA-4E-0F", null, null, false, "none", "WIRC_demo", 7000, 6000); 
        }

        private void button1_Click(object sender, EventArgs e)
        {

            WiRCmoduli = WiRCfunctions.find("10.10.10.255", 1);
            if (WiRCmoduli != null)
            {
                label1.Text = "Connected to:" + WiRCmoduli[0].Get_Name();
                WiRCfunctions.connect(0);
            }
           
        }

  

        private void button3_Click(object sender, EventArgs e)
        {
            WiRCfunctions.disconect();
        }

        private void center_Click(object sender, EventArgs e)
        {
            WiRCfunctions.control(0, WiRCfunctions.ServoInDegre(60, 30));
        }

        private void max_Click(object sender, EventArgs e)
        {
            WiRCfunctions.control(0, WiRCfunctions.ServoInDegre(60, 60));
        }

        private void min_Click(object sender, EventArgs e)
        {
            WiRCfunctions.control(0, WiRCfunctions.ServoInDegre(60, 0));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            WiRCfunctions.control(0, Convert.ToUInt16(trackBar1.Value));
        }
    }
}
