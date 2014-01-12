using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using simpleWiRC;
using SlimDX.DirectInput;
using System.IO;


using AForge.Video.VFW;

namespace WiRCmk_demo
{
    public partial class Form1 : Form
    {
       

        public Form1()
        {
            InitializeComponent();

            UInt16[] hold=new UInt16[12];
            for (int i = 0; i < 12; i++)
            {
                hold[i] = 20000;
            }

            WiRCfunctions = new WiRCmk("GUI", "C4-E9-FF-AD-E6-8D-C3-FE-8F-8A-2E-AB-37-02-1B-F0", hold, null, ChangeName.Checked, "6D-E4-54-CD-45-3F-7A-2D-5A-94-44-2B-C6-F1-4B-65", NewName.Text, Convert.ToUInt16(CoffV.Text), Convert.ToUInt16(WiRCoffV.Text));          
        }

        public

              AVIWriter writer;

         List<DeviceInstance> Naprave = new List<DeviceInstance>();
            DirectInput priklucki = new DirectInput();
            Joystick palca;
            WiRCmk WiRCfunctions;
            List<WircReciwer> WiRCmoduli;
            List<Transmitter> ActiveTransmitters;
            MemoryStream VideoStream;

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Text");
            int index = Country.FindString("SI");
            Country.SelectedIndex = index;
            if (priklucki.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly) !=null)
            {
                Naprave.AddRange(priklucki.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly));
                palca = new Joystick(priklucki, Naprave[0].InstanceGuid);
                palca.Acquire();
            }
        }
       


      

        private void Find_button_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
			{
			listBox1.Items.RemoveAt(i);
			}

            WiRCmoduli = WiRCfunctions.find(subnet.Text, 1);
            if (WiRCmoduli != null)
            {
                foreach (var Modul in WiRCmoduli)
                {
                    listBox1.Items.Add(Modul.Get_Name());
                }
            }
        }

        private void Connect_button_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                WiRCfunctions.connect((Int16)listBox1.SelectedIndex);
                CheckIfHasControl_Timer.Enabled = true;
            }
            else
            {
                WiRCfunctions.connect();
            }
        }

        private void MoveAll_button_Click(object sender, EventArgs e)
        {
            WiRCfunctions.all_control(Convert.ToUInt16(ServoPos_textBox.Text));
        }

        private void Disconect_button_Click(object sender, EventArgs e)
        {

            if (joystickStat.Enabled) { joystickStat.Enabled = false; };
            if (statusTimer.Enabled) { statusTimer.Enabled = false; };
            if (CheckIfHasControl_Timer.Enabled) { CheckIfHasControl_Timer.Enabled = false; };
            if (VideoDrawTimer.Enabled) { VideoDrawTimer.Enabled = false; };
            if (avi_record.Enabled) { avi_record.Enabled = false; };
            if (servodancer.Enabled) { servodancer.Enabled = false; };

            WiRCfunctions.disconect();
        }

        private void JoyStart_button_Click(object sender, EventArgs e)
        {
            if (joystickStat.Enabled)
            {
                joystickStat.Enabled = false;
            }
            else
            {
                joystickStat.Enabled = true;
            }
        }

        private void joystickStat_Tick(object sender, EventArgs e)
        {
           label2.Text = "Throthle: " + palca.GetCurrentState().X.ToString();
            label1.Text = "Stearing: " + palca.GetCurrentState().Y.ToString();

            WiRCfunctions.control(WiRCfunctions.ServoInDegre(UInt16.MaxValue, Math.Abs(palca.GetCurrentState().Y - UInt16.MaxValue)), WiRCfunctions.ServoInDegre(UInt16.MaxValue, Math.Abs(palca.GetCurrentState().X - UInt16.MaxValue)));
            

           
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                label3.Text="HW: "+ WiRCmoduli[listBox1.SelectedIndex].Get_HW();
                label4.Text = "SW: " + WiRCmoduli[listBox1.SelectedIndex].Get_HW();
                label5.Text = "IP: " + WiRCmoduli[listBox1.SelectedIndex].Get_IP();
                label6.Text = "Serial: " + WiRCmoduli[listBox1.SelectedIndex].Get_Serial();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (statusTimer.Enabled)
            {
                statusTimer.Stop();
            }
            else
            {
                statusTimer.Start();
            }
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            UInt16[] Status = WiRCfunctions.status();
            label9.Text = "Input 1: " + Status[0];
            label10.Text = "Input 2: " + Status[1];
            label11.Text = "Input 3: " + Status[2];
            label12.Text = "Input 4: " + Status[3];
            label7.Text = "Battery 1: " + (float)Status[4]/1000 + "V";
            label8.Text = "Battery 2: " + (float)Status[5] / 1000 + "V";
        }

        private void MoveServo_button_Click(object sender, EventArgs e)
        {
            WiRCfunctions.control(Convert.ToByte(ServoNumber.Text), Convert.ToUInt16(ServoPos.Text));
        }

        private void MoveServoDegree_button_Click(object sender, EventArgs e)
        {
            WiRCfunctions.control(Convert.ToByte(ServoNumber.Text), WiRCfunctions.ServoInDegre(Convert.ToUInt16(MaxServo.Text), Convert.ToUInt16(TagetDegree.Text)));
        }

        private void AllAtOne_Button_Click(object sender, EventArgs e)
        {
            UInt16 OU1;
            UInt16 OU2;
            UInt16 OU3;
            UInt16 OU4;

            if(out1.Checked){OU1=800;}else{OU1=2200;}
            if(out2.Checked){OU2=800;}else{OU2=2200;}
            if(out3.Checked){OU3=800;}else{OU3=2200;}
            if(out4.Checked){OU4=800;}else{OU4=2200;}
               
            // if the specified servo range is more than 2200 or les than 800 it will be skiped
            WiRCfunctions.control(Convert.ToUInt16(ser1.Text), Convert.ToUInt16(ser2.Text), Convert.ToUInt16(ser3.Text), Convert.ToUInt16(ser4.Text), Convert.ToUInt16(ser5.Text), Convert.ToUInt16(ser6.Text), Convert.ToUInt16(ser7.Text), Convert.ToUInt16(ser8.Text),OU1,OU2,OU3,OU4);

            //or do like this
            /*
             
            UInt16[] Servos = new UInt16[12];
            Servos[0] = Convert.ToUInt16(ser1.Text);
            Servos[1] = Convert.ToUInt16(ser2.Text);
            Servos[2] = Convert.ToUInt16(ser3.Text);
            Servos[3] = Convert.ToUInt16(ser4.Text);
            Servos[4] = Convert.ToUInt16(ser5.Text);
            Servos[5] = Convert.ToUInt16(ser6.Text);
            Servos[6] = Convert.ToUInt16(ser7.Text);
            Servos[7] = Convert.ToUInt16(ser8.Text);
            Servos[8] = OU1;
            Servos[8] = OU2;
            Servos[8] = OU3;
            Servos[8] = OU4;
              WiRCfunctions.control(Servos);
             */

        }

        private void InputBool_Click(object sender, EventArgs e)
        {
         label26.Text="Input state: "+WiRCfunctions.status((UInt16)numericUpDown1.Value);
        }

        private void OtherCheck_button_Click(object sender, EventArgs e)
        {            
           label27.Text = "Is Recording: " + WiRCfunctions.IsRecording();
           label28.Text = "Is Camera on: " + WiRCfunctions.IsCameraON();
           label36.Text = "Camera count: " + WiRCfunctions.GetCameraCount();
           numericUpDownCamID.Maximum = WiRCfunctions.GetCameraCount();
           if (numericUpDownCamID.Maximum > 0) {
               numericUpDownCamID.Minimum = 0;
               numericUpDownCamID.Value = numericUpDownCamID.Maximum;
           }
        }

        private void Config_Button_Click(object sender, EventArgs e)
        {

            if (WiRCfunctions.config("6D-E4-54-CD-45-3F-7A-2D-5A-94-44-2B-C6-F1-4B-65", Is_Accespoint.Checked, SSID.Text, Password.Text, WPA.Checked, (Byte)ChanellNum.Value, Country.SelectedItem.ToString().Substring(0, 2)))
            {
                MessageBox.Show("All OK, restart WiRC");
            }
            else
            {
                MessageBox.Show("Config Failed");
            }

        }

        private void Camera_Click(object sender, EventArgs e)
        {
            if(numericUpDownCamID.Maximum>0){

                if (!WiRCfunctions.IsCameraON())
                {
                    VideoStream = new MemoryStream();
                    WiRCfunctions.VideoStart(ref VideoStream, (Byte)(numericUpDownCamID.Value - 1));
                    VideoDrawTimer.Enabled = true;
                    Camera.Text = "Camery OFF";
                  
                    
                }
                else
                {
                    VideoDrawTimer.Enabled = false;
                    WiRCfunctions.VideoStop((Byte)(numericUpDownCamID.Value - 1));
                    Camera.Text = "Camery ON";
                }
          
            }
        }

        private void Transmitters_button_Click(object sender, EventArgs e)
        {
      ActiveTransmitters = WiRCfunctions.tranmitters();
            for (int i = 0; i < listBox2.Items.Count; i++) //just clean list if multiple times presed
            {
                listBox2.Items.RemoveAt(i);
            }

            foreach (var Trans in ActiveTransmitters)
            {
                listBox2.Items.Add(Trans.Get_Name());
            }
            
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.Items.Count > 0)
            {
                label34.Text = "ID: " + ActiveTransmitters[listBox2.SelectedIndex].Get_ID();
                label35.Text = "Priority: " + ActiveTransmitters[listBox2.SelectedIndex].Get_Priority();
            }
        }

        private void RequestAcces_button_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                WiRCfunctions.requestAccessFor((Byte)listBox2.SelectedIndex);
            }
        }

        private void GetBackControl_button_Click(object sender, EventArgs e)
        {
            WiRCfunctions.GetControll();
        }

        private void CheckIfHasControl_Timer_Tick(object sender, EventArgs e)
        {
            if (!WiRCfunctions.HasConntrol())
            {
                GetBackControl_button.BackColor = Color.Red;
            }
            else
            {
                GetBackControl_button.BackColor = Color.Green;
            }
            
        }


        public UInt32 FromNetworkOrder(Byte[] data,int offset,int lenght)
        {
            UInt32 fix;

            Byte[] DataFix = new Byte[lenght];
            int ic = lenght-1;
            for (int i = offset; i < offset+lenght; i++)
            {
                DataFix[ic] = data[i];
                ic--;
            }

            fix = BitConverter.ToUInt32(DataFix,0);
            
            return fix;
        }

        private void VideoDrawTimer_Tick(object sender, EventArgs e)
        {
         /*   Byte[] slika = WiRCfunctions.GetFrame();

            label39.Text = FromNetworkOrder(slika, 0, 4).ToString();
            label40.Text = FromNetworkOrder(slika, 4, 4).ToString();
            label41.Text = FromNetworkOrder(slika, 8, 4).ToString();
            label42.Text = FromNetworkOrder(slika, 12, 4).ToString();*/

            //     MemoryStream imageStream = new MemoryStream(slika, 16, (int)FromNetworkOrder(slika, 12, 4));
          pictureBox1.BackgroundImage = System.Drawing.Image.FromStream(WiRCfunctions.GetJPEG());//System.Drawing.Image.FromStream(imageStream);


        }


        private void Firmware_button_Click(object sender, EventArgs e)
        {
            string PfadAndFile = string.Empty;
            string OnlyFile = string.Empty;
            string OnlyPfad = string.Empty;


            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "den files (*.den)|*.den";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                PfadAndFile = openFileDialog1.FileName;
                OnlyFile = openFileDialog1.SafeFileName;
                OnlyPfad = PfadAndFile.Remove(PfadAndFile.LastIndexOf(OnlyFile));



                if (WiRCfunctions.FirmwareUpdate(OnlyPfad,OnlyFile,"6D-E4-54-CD-45-3F-7A-2D-5A-94-44-2B-C6-F1-4B-65"))
                {
                    MessageBox.Show("All OK, WiRC is restarting. Plese restart the transmitter.");
                }
                else
                {
                    MessageBox.Show("Update Failed");
                }

                //MessageBox.Show(OnlyPfad + OnlyFile);
            }

        }

        private void RecordAVI_Click(object sender, EventArgs e)
        {
            // instantiate AVI writer, use WMV3 codec
          

            if (avi_record.Enabled)
            {
                avi_record.Enabled = false;
                writer.Close();
            }
            else
            {
                writer = new AVIWriter("wmv3");
                writer.FrameRate = 30;
                writer.Open("test.avi", pictureBox1.BackgroundImage.Width, pictureBox1.BackgroundImage.Height);
                avi_record.Enabled = true;
            }
            // create new AVI file and open it
           
            // create frame image
          
       
         
        }

        private void avi_record_Tick(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.BackgroundImage);


            // add the image as a new frame of video file
            if (writer != null)
            {
                writer.AddFrame(image);
            }
        }

        private void RecordMJPEG_button_Click(object sender, EventArgs e)
        {
            WiRCfunctions.StartRecod("mjpegVideo.avi");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if(servodancer.Enabled){
                servodancer.Enabled = false;
            }else{
                servodancer.Enabled = true;
            }
        }

        private byte i = 0;
        private ushort pos = 800;

        private void servodancer_Tick(object sender, EventArgs e)
        {
            

          
                WiRCfunctions.control(i, pos);
               
                if (pos >= 2200)
                {
                    if (i >= 7)
                    {
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                    pos = 800;
                }
                else
                {

                    pos += 200;
                }


            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (joystickStat.Enabled) { joystickStat.Enabled = false; };
            if (statusTimer.Enabled) { statusTimer.Enabled = false; };
            if (CheckIfHasControl_Timer.Enabled) { CheckIfHasControl_Timer.Enabled = false; };
            if (VideoDrawTimer.Enabled) { VideoDrawTimer.Enabled = false; };
            if (avi_record.Enabled) { avi_record.Enabled = false; };
            if (servodancer.Enabled) { servodancer.Enabled = false; };

            
            
            
            if (palca != null)
            {
                palca.Unacquire();
            }


            if (WiRCfunctions != null)
            {
                WiRCfunctions.disconect();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

     

      

     
       

     

       

    

        

     

      

    

      
      

       
    }
}
