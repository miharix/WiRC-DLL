using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.IO;


//using AForge.Video.VFW; //FOR VIDEO RECORDING IN AVI 
using System.Drawing;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace simpleWiRC
{
    internal class PinGenerator{

        static TripleDESCryptoServiceProvider tDESalg = new TripleDESCryptoServiceProvider();
        static Byte[] keyS = System.Text.Encoding.ASCII.GetBytes("Bravo cracker! Plese don");
        static Byte[] IVector = System.Text.Encoding.ASCII.GetBytes("'t publish PIN==free");

        public static byte[] EncryptTextToMemory(string Data, byte[] Key, byte[] IV)
        {
            try
            {
                // Create a MemoryStream.
                MemoryStream mStream = new MemoryStream();

                // Create a CryptoStream using the MemoryStream  
                // and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(mStream,
                    new TripleDESCryptoServiceProvider().CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write);

                // Convert the passed string to a byte array. 
                byte[] toEncrypt = new UTF8Encoding().GetBytes(Data);

                // Write the byte array to the crypto stream and flush it.
                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                // Get an array of bytes from the  
                // MemoryStream that holds the  
                // encrypted data. 
                byte[] ret = mStream.ToArray();

                // Close the streams.
                cStream.Close();
                mStream.Close();

                // Return the encrypted buffer. 
                return ret;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }

        }

        public static string DecryptTextFromMemory(byte[] Data, byte[] Key, byte[] IV)
        {
            try
            {
                // Create a new MemoryStream using the passed  
                // array of encrypted data.
                MemoryStream msDecrypt = new MemoryStream(Data);

                // Create a CryptoStream using the MemoryStream  
                // and the passed key and initialization vector (IV).
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    new TripleDESCryptoServiceProvider().CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Create buffer to hold the decrypted data. 
                byte[] fromEncrypt = new byte[Data.Length];

                // Read the decrypted data out of the crypto stream 
                // and place it into the temporary buffer.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the buffer into a string and return it. 
                return new UTF8Encoding().GetString(fromEncrypt);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }

        public String CreateAdminPIN(String from)
        {
           return BitConverter.ToString(EncryptTextToMemory("A:"+from, keyS, IVector));
        }
        public String CreateDevPIN(String from)
        {
            return BitConverter.ToString(EncryptTextToMemory("D:"+from, keyS, IVector));
        }

        public static String DecodePIN(String from)
        {
            String[] arr = from.Split('-');
            byte[] array = new byte[arr.Length];
            for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);

            return DecryptTextFromMemory(array, keyS, IVector);

        }

        public static Boolean IsAdminPinOK(String PIN)
        {   
            try
            {
                if (DecodePIN(PIN).Contains("A:"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            
           return false;
        }
        public static Boolean IsDevPinOK(String PIN)
        {
            try
            {
                if (DecodePIN(PIN).Contains("D:"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

    }

    public class Transmitter
    {
        private Byte ID;
        private Byte Priority;
        private String Name;

        internal Transmitter(Byte ID_num, Byte Prio, String TName)
        {
            ID = ID_num;
            Priority = Prio;
            Name = TName;
        }

        public String Get_Name() { return Name; }
        public Byte Get_ID() { return ID; }
        public Byte Get_Priority() { return Priority; }
    }

    public class WircReciwer
    {
        private IPAddress IP;
        private Byte[] HW;
        private Byte[] SW;
        private String Name;
        private String Serial;
        internal WircReciwer(Byte HW_major, Byte HW_minor, Byte SW_major, Byte SW_minor, String TName, String Tserial,IPAddress IPaddress)
        {
            HW = new Byte[2] { HW_major, HW_minor };
            SW = new Byte[2] { SW_major, SW_minor };
            Name = TName;
            Serial = Tserial;
            IP = IPaddress;
        }

        public String Get_Name() { return Name; }
      /*  internal Byte[] Get_SW() { return HW; }
        internal Byte[] Get_HW() { return HW; }*/
        public String Get_HW() { return HW[0].ToString() +"."+ HW[1].ToString(); }
        public String Get_SW() { return SW[0].ToString() + "."+ SW[1].ToString(); }
        public IPAddress Get_IP() { return IP; }
        public String Get_Serial() { return Serial; }

    }

    

    internal enum InitialCrcValue { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

    internal class Crc16Ccitt
    {
        const ushort poly = 4129;
        ushort[] table = new ushort[256];
        ushort initialValue = 0;

        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = this.initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            byte[] biti = BitConverter.GetBytes(crc);
            byte temp = biti[1];
            biti[1] = biti[0];
            biti[0] = temp;
            return biti;
        }

        public Crc16Ccitt(InitialCrcValue initialValue)
        {
            this.initialValue = (ushort)initialValue;
            ushort temp, a;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                    {
                        temp = (ushort)((temp << 1) ^ poly);
                    }
                    else
                    {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                table[i] = temp;
            }
        }

    }

    public class WiRCmk
    {
        List<WircReciwer> FoundRecivers = null;

        protected const UInt16 WRCD_port = 1984;

        protected Boolean change_config = false;

        protected UInt16 WiRC_listen = 8434; //status port - UDP port number where status masages sould be send
        protected IPAddress WRDC_IP;
        protected String Trans_name = "mk_dll_";
        protected String WRC_name = "WIRC1";
        protected UInt16 Cam_off_V = 7000; //in milivolts
        protected UInt16 WRC_off_V = 6000; //in milivolts
        protected UInt16[] ChT = new UInt16[12];
        protected UInt16[] ChV = new UInt16[12];
        protected UInt16[] ChP = new UInt16[12]; //servo position - control

        protected String WiFi_SSID = "miharix";
        protected String WiFi_Pass = "superdupergeslo";
        protected Byte WiFi_AP = 1;
        protected Byte WiFi_WPA = 1;
        protected Byte WiFi_Ch = 1;
        protected String WiFi_Country = "sl ";

        protected Byte Connection_ID = 0; //connection ID of the transmitter recived by WST
        //  protected Byte[] MD5;
        protected UInt16 CAM_port = 8412;
        //   protected Byte CAM_ID = 0;
        protected Byte CAM_count = 0;
        //  protected Byte access_notif = 0;
        protected UInt16 PCD_port = 0;
        protected UInt16[] Battery = new UInt16[2]; //batery 1 and 2 in mV
        protected UInt16[] Input = new UInt16[4];

        //protected Byte AcessG; //acess granted type
        protected Boolean connected = false; //for preventing dual conection from same pc
        protected Boolean TLENDsign = false;

        protected IPEndPoint IPconnector = null;//needed to esatblish TCP/IP connection
        protected Socket TCPsocket = null;

        protected Thread SearchThread = null; //at beginning for finding WiRC

        protected Thread controlThread = null;
        protected Thread StatusThread = null;
        protected Thread CAMThread = null;
        protected Thread AccessThread = null;
        protected Thread TCP_recive_filterThread = null;

        //     protected Boolean Input_Lock = false;
        //  protected Boolean Bat_Lock = false;

        private static readonly object ChP_Lock = new object();
        private static readonly object Bat_Lock = new object();
        private static readonly object Input_Lock = new object();
        private static readonly object Video_lock = new object();
        private static readonly object Acess_lock = new object(); //for changing access lavel
        private static readonly object TLENDsign_lock = new object(); //for sleeping while the transmiter list is created

        protected Boolean debuging = false; //enable debiging output

        MemoryStream VideoStream = null;
        protected Boolean Cam_state = false;

        protected Byte[] VideoFrame = null;


        protected String VideoLocation = "c:\\";

        protected Boolean Record = false;
        protected int VidCount;

        FileStream VideoFile;

        protected List<Transmitter> ActiveTransmitters = new List<Transmitter>();  //list of all transmitters after caling TLR

        protected Byte CurrAcessControl = 10;

        internal UInt32 FromNetworkOrder(Byte[] data, int offset, int lenght) //***** big engian the WinOS is low endian, stupid compatibility
        {
            UInt32 fix;

            Byte[] DataFix = new Byte[lenght];
            int ic = lenght - 1;
            for (int i = offset; i < offset + lenght; i++)
            {
                DataFix[ic] = data[i];
                ic--;
            }

            fix = BitConverter.ToUInt32(DataFix, 0);

            return fix;
        }

       /* private Boolean IsAdminPIN_OK(String PIN)
        {
            if (PIN == "miharix")
            {
                return true;
            }
            return false;
        }*/

        public Byte GetCameraCount()
        {
            return CAM_count;
        }

        public Boolean FirmwareUpdate(String Dir, String File, String AdminPIN)
        {
            if (PinGenerator.IsAdminPinOK(AdminPIN))
            {
                if (File.Substring(File.Length - 4, 4) == ".den")
                {
                    if (FTP_transfer(Dir, File)) //only send commad for update if everything was transfared perfect
                    {
                        Send_TCP(FWUP(MD5fromFile(Dir + "\\" + File)));
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("wrong Filetype");
                }
            }
            return false;
        }

        protected Boolean FTP_transfer(String Directory, String FileName)
        {
            Boolean AllOK = false;

            FileInfo toUpload = new FileInfo(Directory + "\\" + FileName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + WRDC_IP + "/" + FileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("anonymous", "");
            Stream ftpStream = request.GetRequestStream();
            FileStream file = File.OpenRead(Directory + "\\" + FileName);
            int length = 1024;
            byte[] buffer = new byte[length];
            int bytesRead = 0;

            do
            {
                bytesRead = file.Read(buffer, 0, length);
                ftpStream.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            file.Close();
            ftpStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            if (response.StatusCode != FtpStatusCode.ClosingData)
            {
                Console.WriteLine("Upload File Complete, status {0} {1}", response.StatusDescription, response.StatusCode);
                AllOK = false;
            }
            else
            {
                AllOK = true;
            }

            response.Close();
            return AllOK;
        }

        protected Byte[] MD5fromFile(String Dateipfad)
        {
            //Datei einlesen

            System.IO.FileStream FileCheck = System.IO.File.OpenRead(Dateipfad);
            // MD5-Hash aus dem Byte-Array berechnen
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] md5Hash = md5.ComputeHash(FileCheck);
            FileCheck.Close();

            //in string wandeln
            Console.WriteLine(BitConverter.ToString(md5Hash).Replace("-", "").ToLower());
            return md5Hash;
        }
        
        public WiRCmk(String Tr_name, String DevPin, UInt16[] HoldTime = null, UInt16[] FailSave = null,Boolean ChangeNameAndV=false, String AdminPin="notvalid", String WrcName = "WiRC_demo", UInt16 CamOff = 7000, UInt16 WrcOff = 6000)
        {
            if (!PinGenerator.IsDevPinOK(DevPin)) { throw new System.ArgumentException("DevPin(Developer PIN, is invalid", "DevPin"); }
            Battery[0] = 0;
            Battery[1] = 0;

            if (ChangeNameAndV)
            {
                if (!PinGenerator.IsAdminPinOK(AdminPin)) { throw new System.ArgumentException("DevPin(Developer PIN, is invalid", "DevPin"); }
                change_config = true;
                if (WrcName.Length > 0 && WrcName.Length < 63)
                {
                    WRC_name = WrcName;
                }
                Cam_off_V = CamOff;
                WRC_off_V = WrcOff;
            }

            if (Tr_name.Length < 56)
            {
                Trans_name += Tr_name;
            }


            for (int i = 0; i < 12; i++)
            {
                if (HoldTime != null)
                {
                    if ((HoldTime[i] >= 5000) && (HoldTime[i] <= 20000))
                    {
                        ChT[i] = HoldTime[i];
                    }
                    else
                    {
                        ChT[i] = 10000;
                    }
                }
                else
                {
                    ChT[i] = 10000;
                }

                if (FailSave != null)
                {
                    if ((FailSave[i] >= 800) && (FailSave[i] <= 2200))
                    {
                        ChV[i] = FailSave[i];
                        ChP[i] = ChV[i];
                    }
                    else
                    {
                      /*  if (i == 0)
                        {
                            ChV[i] = 800;
                            ChP[i] = 800;
                        }
                        else
                        {*/
                            ChV[i] = 1500;
                            ChP[i] = 1500;
                      //  }
                    }
                }
                else
                {
                  /*  if (i == 0)
                    {
                        ChV[i] = 800;
                        ChP[i] = 800;
                    }
                    else
                    {*/
                        ChV[i] = 1500;
                        ChP[i] = 1500;
                  //  }
                }
            }
        }



      


        public void log(String info)
        {
            if (debuging)
            {
              //  Console.WriteLine(info);
            }
        }

        public void GetControll()
        {
            requestAccessFor(Connection_ID);
        }
             
        public Boolean HasConntrol()
        {

            Byte cur_con;

            lock (Acess_lock)
            {
                cur_con = CurrAcessControl;
            }
            if (cur_con == 0)
            {
            //    Console.WriteLine("HAS CONTROL: " + cur_con);
                return true;
            }
            else
            {
           //     Console.WriteLine("NO CONTROL" + cur_con);
                return false;
            }

        }

        protected void findT(object num)
        {
            try
            {
                int max = (Int16)num;
                if (max > 0)
                {
                    for (Int16 i = 0; i < max; i++)
                    {
                        BCSA(Recive_UDP(WRCD_port, System.Net.IPAddress.Any));
                    }
                }
                else
                {
                    while (true)
                    {
                         BCSA(Recive_UDP(WRCD_port, System.Net.IPAddress.Any));
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Abording ACL: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("ACL error: " + e.Message);
            }
            finally
            {
                Console.WriteLine("ACL aborded");

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP_Group">On whitch subnet is the reciever</param>
        /// <param name="ExpextedTransNumber">How many WiRC recievers do you expect to be on same subnet, 0 for unknown</param>
        public List<WircReciwer> find(String IP_Group = "192.168.1.255", Int16 ExpextedTransNumber=1)
        {
            if (FoundRecivers != null) { FoundRecivers.RemoveRange(0, FoundRecivers.Count); }
            if (ExpextedTransNumber >= 0)
            {
                SearchThread = new Thread(new ParameterizedThreadStart(findT));
            }
            if (Send_UDP(WRCD_port, BCSD(), IP_Group))
            {
                SearchThread.Start(ExpextedTransNumber);
           //     Thread.Sleep(5000);
                if (ExpextedTransNumber > 0)
                {
                  //  Thread.Sleep(5000 * ExpextedTransNumber);
                    SearchThread.Join(TimeSpan.FromSeconds(3 * ExpextedTransNumber));  
                }
                if (ExpextedTransNumber == 0)
                {
                  //  Thread.Sleep(11000);
                    SearchThread.Join(TimeSpan.FromSeconds(500));  
                }
                if (SearchThread.IsAlive)
                {
                    SearchThread.Abort();
                //    SearchThread.Join();    
                }                
               // BCSA(Recive_UDP(WRCD_port, System.Net.IPAddress.Any));
            }
            return FoundRecivers;

        }

        /// <summary>
        /// Connect to WiRC reciever
        /// Remember to search for transmitters before connecting
        /// </summary>
        /// <param name="TransNumber">if more than one WiRC are in network insert number of chusen one, -1 for the last found</param>
        public void connect(Int16 ReciverNumber=-1)
        {
            

            try
            {
                if ((ReciverNumber >= 0) && (FoundRecivers.Count >= ReciverNumber))
                {
                    WRDC_IP = FoundRecivers[ReciverNumber].Get_IP();
                }

                establishTCP();

                Send_TCP(TL());

                if (change_config)
                {
                    Send_TCP(DCFG());
                }

                Send_TCP(CCFG());

                Send_TCP(FCFG());

                TCP_recive_filterThread = new Thread(new ThreadStart(TCP_recive_filter));
                TCP_recive_filterThread.Start();

                StatusThread = new Thread(new ThreadStart(PSD));
                StatusThread.Start();
                //WST();
                AccessThread = new Thread(new ThreadStart(WaitForAccess));
                AccessThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("connect: "+e.Message);
            }          
        }

        public UInt16 ServoInDegre(Double ServosMax, Double TargetDegre)
        {
            if (ServosMax == 0) { ServosMax = 0; }
            Double n = (TargetDegre * 1400) / ServosMax;

            if (n < 0) { n = 0; }
            if (n > 1400) { n = 1400; }

            Math.Round(n, 0, MidpointRounding.AwayFromZero);

            return (UInt16)(800 + n);
        }

        public void control(UInt16[] Servos)
        {
         
        //    Boolean allOK = true;
       /*     for (int i = 0; i < 12; i++)
            {
                
                 //   allOK = false;
                }
            }*/

            /*if (allOK) //all data OK go and prepare for sending
            {*/
                lock (ChP_Lock)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if ((Servos[i] >= 800 && Servos[i] <= 2200))
                        {
                            ChP[i] = Servos[i];
                        }
                    }
                }
            //}
        }

        public void control(UInt16 servo1=0, UInt16 servo2 = 0, UInt16 servo3 = 0, UInt16 servo4 = 0, UInt16 servo5 = 0, UInt16 servo6 = 0, UInt16 servo7 = 0, UInt16 servo8 = 0, UInt16 Digital1 = 0, UInt16 Digital2 = 0, UInt16 Digital3 = 0, UInt16 Digital4 = 0)
        {
            UInt16[] servos = new UInt16[12];
            servos[0] = servo1;
            servos[1] = servo2;
            servos[2] = servo3;
            servos[3] = servo4;
            servos[4] = servo5;
            servos[5] = servo6;
            servos[6] = servo7;
            servos[7] = servo8;
            servos[8] = Digital1;
            servos[9] = Digital2;
            servos[10] = Digital3;
            servos[11] = Digital4;

            control(servos);
        }

        public void control(Byte servo, UInt16 position)
        {
            if (servo >= 0 && servo <= 11)
            {
                if (position >= 800 && position <= 2200)
                {
                    lock (ChP_Lock)
                    {
                        ChP[servo] = position;
                    }
                }
                else
                {
                    Console.WriteLine("Servo limit out of bounds");
                }
            }
            else
            {
                Console.WriteLine("Servo port does't exist");
            }

        }

        public void all_control(UInt16 p)
        {
            if (p >= 800 && p <= 2200)
            {
                for (int i = 0; i < 12; i++)
                {
                    lock (ChP_Lock)
                    {
                        ChP[i] = p;
                    }
                }
            }
        }

        public Boolean config(String AdminPIN, Boolean IsAccespoint = true, String WiFiSSID = "Dension WiRC", String WiFiPass = "12345678", Boolean IsWPA = false, Byte ChanelNum = 3, String CountryCode = "US")
        {
            if ((CountryCode.Count() == 2) && PinGenerator.IsAdminPinOK(AdminPIN))
            {
                WiFi_SSID = WiFiSSID;
                WiFi_Pass = WiFiPass;
                WiFi_AP = Convert.ToByte(IsAccespoint);
                WiFi_WPA = Convert.ToByte(IsWPA);
                WiFi_Ch = ChanelNum;
                WiFi_Country = CountryCode;

                Send_TCP(WCFG());

                return true;
            }
            return false;
        }

        public void status(ref String OutputString)
        {
            OutputString = "";


            lock (Bat_Lock)
            {
                OutputString = "Battery 1: " + Battery[0] + "mV = " + ((float)Battery[0] / 1000) + "V" + "\nBattery 2: " + Battery[1] + "mV = " + ((float)Battery[1] / 1000) + "V";
            }

            for (int i = 0; i < 4; i++)
            {
                lock (Input_Lock)
                {
                    OutputString += "\nInput " + i + ": " + Input[i];
                }
            }



        }

        public Boolean status(UInt16 InputNum)
        {
            Boolean state = false;
            if (InputNum >= 0 && InputNum <= 3)
            {
                lock (Input_Lock)
                {
                    if (Input[InputNum] != 0)
                    {
                        state = true;
                    }
                }
            }
            return state;
        }

        /// <summary>
        /// Get the all status at once, Use this if you need more than one status at once.
        /// </summary>
        /// <returns>area of UInt16[6]; Battery 1(mV),Battery 2(mV),Input 1,Input 2,Input 3,Input 4</returns>
        public UInt16[] status()
        {
            UInt16[] state = new UInt16[6];


            for (int i = 0; i < 4; i++)
            {
                lock (Input_Lock)
                {
                    state[i] = Input[i];
                }
            }



            lock (Bat_Lock)
            {
                state[4] = Battery[0];
                state[5] = Battery[1];
            }


            return state;
        }

        public void VideoStart(ref MemoryStream Stream, Byte CamID=0)
        {

            if (HasConntrol())
            {
                VideoStream = new MemoryStream();
                Stream = VideoStream;
                Send_TCP(STST(CamID));//start the video stream of cam on ID
                CAMThread = new Thread(new ThreadStart(CAM));


                CAMThread.Start();
                Cam_state = true;
            }

        }

        public void VideoStop(Byte CamID)
        {
            Cam_state = false;
            CAMThread.Abort();
            CAMThread.Join();
            
            Send_TCP(EST(CamID));//stop the video stream
        }

        public void disconect()
        {
            try
            {
                if (controlThread != null)
                {
                    if (controlThread.IsAlive)
                    {
                        controlThread.Abort();
                        Console.WriteLine("controlThread abording");
                        controlThread.Join();
                        Console.WriteLine("controlThread joined");
                    }
                }


                if (TCP_recive_filterThread != null)
                {
                    if (TCP_recive_filterThread.IsAlive)
                    {
                        TCP_recive_filterThread.Abort();
                        Console.WriteLine("TCP recive abording");
                        
                        TCP_recive_filterThread.Join();
                        Console.WriteLine("TCP recive joined");
                    }
                }

                if (AccessThread != null)
                {
                    if (AccessThread.IsAlive)
                    {
                        AccessThread.Abort();
                        Console.WriteLine("Acces Thread abording");
                        AccessThread.Join();
                        Console.WriteLine("Acces Thread joined");
                    }
                }

                if (CAMThread != null)
                {
                    if (CAMThread.IsAlive)
                    {
                        CAMThread.Abort();
                        Console.WriteLine("CAMThread abording");
                        CAMThread.Join();
                        Console.WriteLine("CAMThread joined");
                    }
                }

             

                if (StatusThread != null)
                {
                    if (StatusThread.IsAlive)
                    {
                        StatusThread.Abort();
                        Console.WriteLine("StatusThread abording");
                        StatusThread.Join();
                        Console.WriteLine("StatusThread joined");
                    }
                }

                if (SearchThread != null)
                {
                    if (SearchThread.IsAlive)
                    {
                        SearchThread.Abort();
                        Console.WriteLine("SearchThread abording");
                        SearchThread.Join();
                        Console.WriteLine("SearchThread joined");
                    }
                }
                if (TCPsocket != null)
                {
                    TCPsocket.Shutdown(SocketShutdown.Both);
                    Console.WriteLine("TCPsocket shutdown");
                    TCPsocket.Close();
                    Console.WriteLine("TCPsocket closed");
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("disconect: "+e.Message);
            }
            Console.WriteLine("Disconnected");

        }

        public Boolean IsCameraON()
        {
            return Cam_state;
        }

        public Byte[] GetFrame()
        {
            return VideoFrame;
        }

        public MemoryStream GetJPEG()
        {
            Byte[] slika = GetFrame();
            if (slika != null)
            {
                return new MemoryStream(slika, 16, (int)FromNetworkOrder(slika, 12, 4));
            }
            else
            {

                Image NoImage = Properties.Resources.noImage;
                MemoryStream Offline=new MemoryStream();
               NoImage.Save(Offline,System.Drawing.Imaging.ImageFormat.Jpeg);
               return Offline;
            }
        }

        /// <summary>
        /// Start the recording to specified path
        /// </summary>
        /// <param name="Path">String path to the dir where recording should save file</param>
        public void StartRecod(String Path)
        {
            VideoLocation = Path + "\\WiRC_rec";
            DirectoryInfo dir = new DirectoryInfo(VideoLocation);
            if (!dir.Exists)
            {
                dir.Create();
            }
            else
            {
                VidCount = Directory.GetFiles(VideoLocation, "*.mjpg").Count();

            }
            VideoFile = File.Create(VideoLocation + "\\WiRC_rec" + VidCount + ".mjpg");



            Record = true;
        }

        /// <summary>
        /// Stops the video recodring
        /// </summary>
        public void StopRecod()
        {
            VideoFile.Close();
            Record = false;
        }

        /// <summary>
        /// Check if the recording is allredy in progress
        /// </summary>
        /// <returns>true if recoding is in progress else false</returns>
        public Boolean IsRecording()
        {
            return Record;
        }

        public List<Transmitter> tranmitters()
        {
            lock (TLENDsign_lock)
            {
                TLENDsign = false;
            }

            ActiveTransmitters.Clear();

            // Byte[] Data = new Byte[72];
            Send_TCP(TLR());
            Boolean sendAll;
            do
            {
                //    Thread.Sleep(10);
                //   Recive_TCP(ref Data);
                //   TLST(Data);
                lock (TLENDsign_lock)
                {
                    sendAll = TLENDsign;
                }
            } while (!sendAll);

            return ActiveTransmitters;

        }

        public void requestAccessFor(Byte TransID)
        {
            Send_TCP(AREQ(TransID));           
        }






        protected static byte[] dodajCRT(byte[] sporocilo)
        {
            Crc16Ccitt crc = new Crc16Ccitt(InitialCrcValue.Zeros);
            int dolzina = sporocilo.Length;
            byte[] ukaz = new byte[dolzina + 2];
            sporocilo.CopyTo(ukaz, 0);
            crc.ComputeChecksumBytes(sporocilo).CopyTo(ukaz, dolzina);

            return ukaz;
        }

        protected void establishTCP()
        {
            IPconnector = new IPEndPoint(WRDC_IP, WRCD_port);

            // Neue TCP Instanz
            TCPsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            TCPsocket.Connect(IPconnector);

            Console.WriteLine("Connected to: " + WRDC_IP + " port: " + WRCD_port);

        }

        protected void Send_TCP(Byte[] Data)
        {

            // Sende den data sream an den server
            TCPsocket.Send(Data, 0, Data.Length, SocketFlags.None);
            Console.WriteLine("Sent {0} bytes...", Data.Length);

        }

        protected void Recive_TCP(ref Byte[] Data)
        {


            Byte[] respond = new Byte[Data.Length];

            // sets the amount of time to linger after closing, using the LingerOption public property.
           // LingerOption lingerOption = new LingerOption(true, 0);
           // TCPsocket.LingerState = lingerOption;
            
            TCPsocket.ReceiveTimeout = 1000;
            try
            {
                TCPsocket.Receive(respond);
                Data = respond;
            }
            catch (Exception e)
            {
                Console.WriteLine("Recive TCP: "+e.Message);
                Data = null;
            }

        }

        protected Boolean Send_UDP(UInt16 Port, Byte[] Data, String IP_range)
        {

            try
            {
                var udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, WRCD_port));
                bool x = udpClient.Client.Poll(0, SelectMode.SelectError);
                bool y = udpClient.Client.Poll(0, SelectMode.SelectRead);
                bool z = udpClient.Client.Poll(0, SelectMode.SelectWrite);

                if (x || y || z)
                {
                    udpClient.Client.Close();
                    udpClient = new UdpClient();
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, WRCD_port));
                }

                //     UdpClient udpClient = new UdpClient(WRCD_port, AddressFamily.InterNetwork);
                /*
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    if (adapter.SupportsMulticast)
                    {
                        Console.WriteLine(adapter.Name);
                    }
                    foreach (var item in properties.UnicastAddresses)
                    {
                        if (item.Address.AddressFamily==AddressFamily.InterNetwork) //ipv4
                        {
                            Console.WriteLine(item.Address);
                        }
                    }

                }*/


                IPAddress destIp = IPAddress.Parse(IP_range);
                IPEndPoint groupEp = new IPEndPoint(destIp, WRCD_port);
                udpClient.Connect(groupEp);
                udpClient.Send(Data, Data.Length);
                udpClient.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Send UDP: "+e.ToString());
                //     listener.Close();
            }

            return false;
        }

 

        protected Byte[] Recive_UDP(UInt16 Port, IPAddress IP)
        {            
            IPEndPoint remoteEP = null;

            var udpClient = new UdpClient();
                      
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new System.Net.IPEndPoint(IP, Port));
            bool x = udpClient.Client.Poll(0, SelectMode.SelectError);
            bool y = udpClient.Client.Poll(0, SelectMode.SelectRead);
            bool z = udpClient.Client.Poll(0, SelectMode.SelectWrite);

            if (x || y || z)
            {
                udpClient.Client.Close();
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(new System.Net.IPEndPoint(IP, Port));
            }


            //    udpClient.Client.SendTimeout = 5;
                udpClient.Client.ReceiveTimeout = 5;

            var asyncResult = udpClient.BeginReceive(null, null);

            //       Console.WriteLine(udpClient.Client.SendTimeout.ToString() + "   " + udpClient.Client.ReceiveTimeout.ToString());

            //   asyncResult.AsyncWaitHandle.WaitOne(timeToWait);

            try
            {
                Console.WriteLine("čakam na sprejem ");

                Byte[] receivedData = udpClient.EndReceive(asyncResult, ref remoteEP);

                WRDC_IP = remoteEP.Address;

                Console.WriteLine("Sprejel od: " + WRDC_IP.ToString());

                udpClient.Close();
                return receivedData;
                // EndReceive worked and we have received data and remote endpoint
            }
            catch (Exception ex)
            {
                Console.WriteLine("Recive UDP: "+ex.Message.ToString());
            }
            return null;
        }

        protected void control()
        {
          
            UdpClient UDP_client = new UdpClient();
            try
            {

                IPEndPoint UDP_endpoint = new IPEndPoint(WRDC_IP, PCD_port);

                Byte[] Data = PCD();

                while (true)
                {
                    Data = PCD();

                    UDP_client.Send(Data, Data.Length, UDP_endpoint);
                    System.Threading.Thread.Sleep(55);
                   
                }

                
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Abording servo control transmition: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("UDP error: " + e.Message);
            }
            finally
            {
                UDP_client.Close();
                Console.WriteLine("Servo control transmition aborded");
            }
            return;
        }

        protected void CAM() //proces for thread for capturing MJPEG
        {
            UdpClient CAM_image = new UdpClient(CAM_port);

            try
            {
                //  byte[] packet=null;
                IPEndPoint remoteIPEndPoint = new IPEndPoint(WRDC_IP, CAM_port);


                int i = 0;
                while (true)
                {
                    CAM_image.Ttl = 50;
                    VideoFrame = CAM_image.Receive(ref remoteIPEndPoint);


                    /*  Console.WriteLine(BitConverter.ToString(packet));
                    */
                    //      VideoStream.Dispose();
                    //   VideoStream = new MemoryStream();
                    //  VideoStream.Close();
                    //   VideoStream.Flush();
                    //      VideoStream.Write(packet, 0, packet.Length);

                    if (Record)
                    {

                        VideoFile.Write(VideoFrame, 0, VideoFrame.Length);

                        //if (VideoFile.Length>) { }


                    }
                    //  Thread.Sleep(21);

                    i++;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Video t:" + e.Message);
            }
            finally
            {
                if (Record)
                {
                    VideoFile.Close();
                }
                CAM_image.Close();
                Cam_state = false;
                Console.WriteLine("Video terminated");
            }

        }

        protected void TCP_recive_filter()
        {
            Byte[] TCP_data = new Byte[74];
            while (true)
            {
                Recive_TCP(ref TCP_data);
                if (TCP_data != null)
                {
                    switch (TCP_data[2])
                    {
                        case 0x36:
                            AGR(TCP_data);
                            break;
                        case 0x33:
                            TLST(TCP_data);
                            break;
                        case 0x34:
                            TLEND(TCP_data);
                            break;
                        case 0x1A:
                            WST(TCP_data);
                            break;
                    }
                }
                TCP_data = new Byte[74];

                Thread.Sleep(200);
              
            }
        }
        
        protected void WaitForAccess()
        { //for thread for waithing when the transmitter doest have control
            try
            {
            //    Byte Curr_access=50;
                //int i = 0;
                while (true)
                {
                    Thread.Sleep(100);
                    Console.WriteLine("CCCCCCCC:");
                   // if (AGR())
                   // {
                       
                   // }
                    //Console.WriteLine("ACL " + i); i++;
                  if( HasConntrol()){
                      if (controlThread != null)
                      {
                          if (!controlThread.IsAlive)
                          {
                              controlThread = new Thread(new ThreadStart(control));
                              controlThread.Start();
                          }
                      }
                      else
                      {
                          controlThread = new Thread(new ThreadStart(control));
                          controlThread.Start();
                      }
                      
                  }
                  else
                  {
                      Console.WriteLine("NO CONTROL");
                      if (controlThread != null)
                      {
                          if (controlThread.IsAlive)
                          {
                              controlThread.Abort();
                          }
                      }
                  }


                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Abording ACL: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("ACL error: " + e.Message);
            }
            finally
            {
                Console.WriteLine("ACL aborded");

            }
            // return;

        }




        protected byte[] BCSD()
        {
            Byte[] ukaz = new Byte[9];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[5];
            sporocilo[0] = 0x01; //CMD
            sporocilo[1] = 0x03; //LEN
            sporocilo[2] = 0x00; //SYS - transmiter type (0=PC 1=iphone 2=Android)
            sporocilo[3] = 0x01; //version - transmiter major version
            sporocilo[4] = 0x00; //version - transmiter minor version



            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nBCSD:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

      

        protected void BCSA(Byte[] message)
        {

            if (message != null)
            {
                if (FoundRecivers == null) { 
                    FoundRecivers= new List<WircReciwer>();
                } //first found 
                
           /*     Console.Write("\n<----------------------------------");
                Console.Write("BCSA\n" + BitConverter.ToString(message) + "\n\nDECODE:\n\n");

                Console.WriteLine("HW Version: " + message[4] + "." + message[5]); //System.Text.Encoding.ASCII.GetString(message, 4, 1) + "." + System.Text.Encoding.ASCII.GetString(message, 5, 1));
                Console.WriteLine("SW Version: " + message[6] + "." + message[7]);// System.Text.Encoding.ASCII.GetString(message, 6, 1) + "." + System.Text.Encoding.ASCII.GetString(message, 7, 1));
                */
                String wrcname = System.Text.Encoding.ASCII.GetString(message, 8, 64);
                int pos = wrcname.IndexOf('\0');
                if (pos >= 0)
                {
                    wrcname = wrcname.Substring(0, pos);
                }

             //   Console.WriteLine("WRC Name: " + wrcname); //System.Text.Encoding.ASCII.GetString(message, 8, 64)//Split(new char[] {'\0'})[0]);//.Replace("\0", ""));
                String wrcserial = System.Text.Encoding.ASCII.GetString(message, 72, 7).Replace("\0", "");
                 pos = wrcserial.IndexOf('\0');
                if (pos >= 0)
                {
                    wrcserial = wrcserial.Substring(0, pos);
                }
              //  Console.WriteLine("Serial Number: " + wrcserial);


                FoundRecivers.Add(new WircReciwer(message[4], message[5], message[6], message[7], wrcname, wrcserial, WRDC_IP));

                Console.WriteLine("\n|---------------------------------|");
            }
            else
            {
                Console.WriteLine("ni 0 prejeto");
            }
        }

        protected byte[] TL()
        {
            Byte[] ukaz = new Byte[76];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[72];
            sporocilo[0] = 0x11; //CMD
            sporocilo[1] = 0x46; //LEN
            sporocilo[2] = 0x00; //SYS - transmiter type (0=PC 1=iphone 2=Android)
            sporocilo[3] = 0x01; //version - transmiter major version
            sporocilo[4] = 0x00; //version - transmiter minor version
            sporocilo[5] = 0xFF; // Prio - Prioritiy of teh transmitter 0 lowest 255 higest



            System.Text.Encoding.ASCII.GetBytes(Trans_name).CopyTo(sporocilo, 6); // Transmitter name - body 4-67

            Byte[] listenport_networko = new Byte[2];
            BitConverter.GetBytes(WiRC_listen).CopyTo(listenport_networko, 0);

            Byte tmp = listenport_networko[0];
            listenport_networko[0] = listenport_networko[1];
            listenport_networko[1] = tmp;
            listenport_networko.CopyTo(sporocilo, 70); // Status port - body 68-69

            dodajCRT(sporocilo).CopyTo(ukaz, 2);


            Console.Write("\n---------------------------------->");

            Console.Write("TL\n" + BitConverter.ToString(ukaz) + "\n\nDECODE:\n\n");

            Console.Write("Transmiter type: " + ukaz[4] + " (");
            switch (ukaz[4])
            {
                case 0:
                    Console.Write("PC)");
                    break;
                case 1:
                    Console.Write("iPhone)");
                    break;
                case 2:
                    Console.Write("Android)");
                    break;
            }
            Byte[] liport = new Byte[2];

            //LittleEndian  OS !!!!!!!
            liport[0] = ukaz[73];
            liport[1] = ukaz[72];

            Console.WriteLine("\nTransmiter version: " + ukaz[5] + "." + ukaz[6]);
            Console.WriteLine("Transmiter Prioritiy: " + ukaz[7]);

            String tname = System.Text.Encoding.ASCII.GetString(ukaz, 8, 64);
            int pos = tname.IndexOf('\0');
            if (pos >= 0)
            {
                tname = tname.Substring(0, pos);
            }

            Console.WriteLine("Transmiter Name: " + tname);
            Console.WriteLine("Status Port: " + BitConverter.ToInt16(liport, 0));
            Console.WriteLine("\n|---------------------------------|");

            return ukaz;
        }

        protected byte[] DCFG()
        {
            Byte[] ukaz = new Byte[74];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[70];
            sporocilo[0] = 0x12; //CMD
            sporocilo[1] = 0x44; //LEN

            System.Text.Encoding.ASCII.GetBytes(WRC_name).CopyTo(sporocilo, 2); // WRC name - body 0-63

            Byte[] networkOrder = new Byte[2];
            Byte nettemp = new Byte();
            BitConverter.GetBytes(Cam_off_V).CopyTo(networkOrder, 0);
            nettemp = networkOrder[1];
            networkOrder[1] = networkOrder[0];
            networkOrder[0] = nettemp;
            networkOrder.CopyTo(sporocilo, 66);
         //   BitConverter.GetBytes(Cam_off_V).CopyTo(sporocilo, 66); // cam off - body 64-65


            Byte[] networkOrder2 = new Byte[2];
            Byte nettemp2 = new Byte();
            BitConverter.GetBytes(Cam_off_V).CopyTo(networkOrder2, 0);
            nettemp2 = networkOrder2[1];
            networkOrder2[1] = networkOrder2[0];
            networkOrder2[0] = nettemp2;
            networkOrder2.CopyTo(sporocilo, 68);
         //   BitConverter.GetBytes(WRC_off_V).CopyTo(sporocilo, 68); // WRC off - body 66-67

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            //  Console.WriteLine("\nDCFG:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected byte[] CCFG()
        {

            Byte[] ukaz = new Byte[30];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[26];
            sporocilo[0] = 0x13; //CMD
            sporocilo[1] = 0x18; //LEN

            int temp1 = 0;
            //   Console.WriteLine(BitConverter.ToInt16(BitConverter.GetBytes(5000),0));
            for (int i = 0; i < 12; i++)
            {
                //    Console.WriteLine(temp1 + " -> "+ChT[i]);
                //  BitConverter.GetBytes(ChT[i]).CopyTo(sporocilo, temp1); // WRC off - body 0-1
                Byte[] servo = BitConverter.GetBytes(ChT[i]);
                Byte temp = servo[0];
                servo[0] = servo[1];
                servo[1] = temp;
                servo.CopyTo(sporocilo, temp1 + 2); // WRC off - body 0-1
                temp1 = temp1 + 2; //repeat for all ChT dont forget to shift for 2

            }

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.Write("\n---------------------------------->");
            Console.Write("\nCCFG:\n" + BitConverter.ToString(ukaz) + "\n\nDECODE:\n\n");

            int s = 1;
            for (int i = 4; i < 28; i = i + 2)
            {
                Byte[] servo = new Byte[2];
                //  Console.WriteLine(i + " -> " + ChT[i]);

                //LittleEndian  OS !!!!!!!
                servo[0] = ukaz[i + 1];
                servo[1] = ukaz[i];
                //    Console.Write(servo[0].ToString("X") +" "+ servo[1].ToString("X"));
                //      Console.WriteLine(BitConverter.ToString(servo));

                Console.WriteLine(" " + s + ".servo repeat time: " + BitConverter.ToString(ukaz, i, 2) + " -> " + BitConverter.ToInt16(servo, 0) + "ms");
                s++;
            }
            Console.WriteLine("\n|---------------------------------|");


            return ukaz;
        }

        protected byte[] FCFG()
        {

            Byte[] ukaz = new Byte[30];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[26];
            sporocilo[0] = 0x14; //CMD
            sporocilo[1] = 0x18; //LEN

            int temp1 = 0;
            for (int i = 0; i < 12; i++)
            {
                Byte[] servo = BitConverter.GetBytes(ChV[i]);
                Byte temp = servo[0];
                servo[0] = servo[1];
                servo[1] = temp;

                servo.CopyTo(sporocilo, temp1 + 2); // WRC off - body 0-1
                temp1 = temp1 + 2; //repeat for all ChT dont forget to shift for 2
            }

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.Write("\n---------------------------------->");
            Console.Write("\nFCFG:\n" + BitConverter.ToString(ukaz) + "\n\nDECODE:\n\n");

            int s = 1;
            for (int i = 4; i < 28; i = i + 2)
            {
                Byte[] servo = new Byte[2];

                //LittleEndian  OS !!!!!!!
                servo[0] = ukaz[i + 1];
                servo[1] = ukaz[i];

                Console.WriteLine(" " + s + ".servo failsafe: " + BitConverter.ToString(ukaz, i, 2) + " -> " + BitConverter.ToInt16(servo, 0) + "ms");
                s++;
            }
            Console.WriteLine("\n|---------------------------------|");




            return ukaz;
        }

        protected void WST(Byte[] wst)
        {
            /*   Byte[] wst = new Byte[10];

               Recive_TCP(ref wst);*/

            if (wst != null)
            {
                Console.Write("\n<---------------------------------");
                Console.Write("WST\n" + BitConverter.ToString(wst) + "\n\nDECODE:\n\n");
                Console.WriteLine("ID: " + wst[4]);

                //transmiter_ID
                Connection_ID = wst[4];

                Console.WriteLine("Number of camera devices: " + wst[5]);

                CAM_count = wst[5];

                Byte[] tmp = new Byte[2];
                tmp[0] = wst[7];
                tmp[1] = wst[6];

                PCD_port = BitConverter.ToUInt16(tmp, 0);

                Console.WriteLine("Port for PCD: " + PCD_port);


                Console.WriteLine("|---------------------------------|");
            }

        }

        protected byte[] PCD()
        {

            Byte[] ukaz = new Byte[30];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[26];
            sporocilo[0] = 0x21; //CMD
            sporocilo[1] = 0x18; //LEN

            int temp1 = 0;
            for (int i = 0; i < 12; i++)
            {
                Byte[] servo;
                lock (ChP_Lock)
                {
                    servo = BitConverter.GetBytes(ChP[i]);
                }
                Byte temp = servo[0];
                servo[0] = servo[1];
                servo[1] = temp;

                servo.CopyTo(sporocilo, temp1 + 2); // WRC off - body 0-1
                temp1 = temp1 + 2; //repeat for all ChT dont forget to shift for 2
            }

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            if (debuging)
            {

                Console.Write("\n---------------------------------->");
                Console.Write("\nPCD:\n" + BitConverter.ToString(ukaz) + "\n\nDECODE:\n\n");

                int s = 1;
                for (int i = 4; i < 28; i = i + 2)
                {
                    Byte[] servo = new Byte[2];
                    //  Console.WriteLine(i + " -> " + ChT[i]);

                    //LittleEndian  OS !!!!!!!
                    servo[0] = ukaz[i + 1];
                    servo[1] = ukaz[i];
                    //    Console.Write(servo[0].ToString("X") +" "+ servo[1].ToString("X"));
                    //      Console.WriteLine(BitConverter.ToString(servo));

                    Console.WriteLine(" " + s + ".servo goto: " + BitConverter.ToString(ukaz, i, 2) + " -> " + BitConverter.ToUInt16(servo, 0) + "ms");
                    s++;
                }
                Console.WriteLine("\n|---------------------------------|");
            }
            return ukaz;
        }

        protected void PSD()
        {

            UdpClient periodic = new UdpClient(WiRC_listen);

            try
            {




                byte[] packet = new byte[14];
                IPEndPoint remoteIPEndPoint = new IPEndPoint(WRDC_IP, WiRC_listen);
                //      periodic.Client.ReceiveTimeout = 200;
                /*  for (int di = 0; di < 50; di++)
                  {*/
                while (true)
                {
                    packet = periodic.Receive(ref remoteIPEndPoint);


                //       Console.Write("\n<---------------------------------");
                 //         Console.Write("\nPSD:\n" + BitConverter.ToString(packet) + "\n\nDECODE:\n\n");
          
                    Byte[] bat = new Byte[2];
                    bat[0] = packet[5];
                    bat[1] = packet[4];

                    lock (Bat_Lock)
                    {
                        Battery[0] = BitConverter.ToUInt16(bat, 0);
                    }



                    //         Console.WriteLine("Battery 1: " + Battery[0] + "mV = " + ((float)Battery[0] / 1000) + "V");

                    bat[0] = packet[7];
                    bat[1] = packet[6];

                    lock (Bat_Lock)
                    {
                        Battery[1] = BitConverter.ToUInt16(bat, 0);
                    }

                    //      Console.WriteLine("Battery 2: " + Battery[1] + "mV = " + ((float)Battery[1] / 1000) + "V");



                    for (int i = 0; i < 4; i++)
                    {
                        lock (Input_Lock)
                        {
                            Input[i] = packet[i + 8];
                            //        Console.WriteLine("Input " + i+1 + ": " + Input[i]);
                        }
                    }


                    Thread.Sleep(100);

                    //  Console.WriteLine("\n|---------------------------------|");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Periodic t: " + e.Message);
            }
            finally
            {
                periodic.Close();
            }
            return;

        }

        protected byte[] WCFG()
        {
            Byte[] ukaz = new Byte[108];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[104];
            sporocilo[0] = 0x31; //CMD
            sporocilo[1] = 0x66; //LEN

            System.Text.Encoding.ASCII.GetBytes(WiFi_SSID).CopyTo(sporocilo, 2); // WiFi_SSID - body 0-31

            System.Text.Encoding.ASCII.GetBytes(WiFi_Pass).CopyTo(sporocilo, 34); // WiFi_Pass - body 32-95

            sporocilo[98] = WiFi_AP;
            sporocilo[99] = WiFi_WPA;
            sporocilo[100] = WiFi_Ch;
            sporocilo[101] = System.Text.Encoding.ASCII.GetBytes(WiFi_Country)[0];
            sporocilo[102] = System.Text.Encoding.ASCII.GetBytes(WiFi_Country)[1];

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nWCFG:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected byte[] TLR()
        {
            Byte[] ukaz = new Byte[6];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame
            ukaz[2] = 0x32; //CMD
            ukaz[3] = 0x00; //LEN
            ukaz[4] = 0x63; //CRC
            ukaz[5] = 0xF7; //CRC

            Console.WriteLine("\nTLR:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected void TLST(Byte[] data)
        {
            if (data != null)
                if ((data[0] == 0xAA) && (data[1] == 0xBB) && (data[2] == 0x33))
                {
                    if (ActiveTransmitters == null)
                    {
                        ActiveTransmitters = new List<Transmitter>();
                    }

                    String tname = System.Text.Encoding.ASCII.GetString(data, 6, 64);
                    int pos = tname.IndexOf('\0');
                    if (pos >= 0)
                    {
                        tname = tname.Substring(0, pos);
                    }

                    Transmitter TransI = new Transmitter(data[4], data[5], tname);

                    ActiveTransmitters.Add(TransI);
                }

        }

        protected void TLEND(Byte[] data)
        {
            if ((data[0] == 0xAA) && (data[1] == 0xBB) && (data[2] == 0x34))
            {
                lock (TLENDsign_lock)
                {
                    TLENDsign = true;
                }
                return;
            }
            lock (TLENDsign_lock)
            {
                TLENDsign = false;
            }
        }

        protected byte[] AREQ(Byte Transmmitter_ID)
        {
            Byte[] ukaz = new Byte[7];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[3];
            sporocilo[0] = 0x35; //CMD
            sporocilo[1] = 0x01; //LEN
            sporocilo[2] = Transmmitter_ID; //ID - conection identifer

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nAREQ:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected Boolean AGR(Byte[] agr)
        {
            if (agr != null)
            {
                Console.Write("\n<---------------------------------");
                Console.Write("AGR\n" + BitConverter.ToString(agr) + "\n\nDECODE:\n\n");
                Console.WriteLine("ID: " + agr[4]);
                Console.WriteLine("Priority: " + agr[5]);

                String trname = System.Text.Encoding.ASCII.GetString(agr, 6, 64);
                int pos = trname.IndexOf('\0');
                if (pos >= 0)
                {
                    trname = trname.Substring(0, pos);
                }

                Console.WriteLine("Transmitter name: " + trname);

                //  Console.WriteLine("Transmitter name: " + Encoding.ASCII.GetString(agr,6,63));


                Console.Write("Notif: " + agr[70] + " (");
                //AcessG = agr[70];
                switch (agr[70])
                {
                    case 0:
                        Console.Write("Access granted)");
                        break;
                    case 1:
                        Console.Write("Access denied)");
                        break;
                    case 2:
                        Console.Write("Access revoked)");
                        break;
                    case 3:
                        Console.Write("Access notification)");
                        break;
                }

                //      Console.Write("Notif: " + agr[67] + " ("); if (agr[67] == 0) { Console.Write("Access granted)"); } if (agr[67] == 1) { Console.Write("Access denied)"); } if (agr[67] == 2) { Console.Write("Access revoked)"); } if (agr[67] == 3) { Console.Write("Access notification)"); }
                Console.WriteLine("\n|---------------------------------|");

                lock (Acess_lock)
                {
                    CurrAcessControl = agr[70];
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected byte[] FWUP(Byte[] MD5)
        {
            Byte[] ukaz = new Byte[22];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[18];
            sporocilo[0] = 0x37; //CMD
            sporocilo[1] = 0x10; //LEN

            MD5.CopyTo(sporocilo, 2); //MD5

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nFWUP:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected byte[] STST(Byte CAM_ID)
        {
            Byte[] ukaz = new Byte[9];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[5];
            sporocilo[0] = 0x41; //CMD
            sporocilo[1] = 0x03; //LEN
            sporocilo[2] = CAM_ID; //camera ID

            Byte[] CAM_port_net = BitConverter.GetBytes(CAM_port);
            Byte CAM_tmp = CAM_port_net[1];
            CAM_port_net[1] = CAM_port_net[0];
            CAM_port_net[0] = CAM_tmp;

            CAM_port_net.CopyTo(sporocilo, 3); //MD5

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nSTST:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }

        protected byte[] EST(Byte CAM_ID)
        {
            Byte[] ukaz = new Byte[7];
            ukaz[0] = 0xAA; //frame
            ukaz[1] = 0xBB; //frame

            Byte[] sporocilo = new Byte[3];
            sporocilo[0] = 0x42; //CMD
            sporocilo[1] = 0x01; //LEN
            sporocilo[2] = CAM_ID; //camera ID

            dodajCRT(sporocilo).CopyTo(ukaz, 2);

            Console.WriteLine("\nEST:\n" + BitConverter.ToString(ukaz));

            return ukaz;
        }



    }
}
