using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace WiRCpinGenerator
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "") { textBox2.Text = textBox1.Text; }
          
            textBox3.Text = PinGenerator.CreateDevPIN(textBox1.Text);
            textBox4.Text = PinGenerator.CreateAdminPIN(textBox2.Text);
            
        }
    }
    internal class PinGenerator
    {

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

        public static String CreateAdminPIN(String from)
        {
            return BitConverter.ToString(EncryptTextToMemory("A:" + from, keyS, IVector));
        }
        public static String CreateDevPIN(String from)
        {
            return BitConverter.ToString(EncryptTextToMemory("D:" + from, keyS, IVector));
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
}
