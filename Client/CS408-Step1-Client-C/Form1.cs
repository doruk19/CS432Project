/*
 Name: İnanç Dokurel-Doruk Gür
 ID:17575-17699
 Title:CS408 Project Client Side Step 1
 Description: This project implements the GUI and the background code of the CS408 term project (Client Side)
 * */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

namespace CS408_Step1_Client_C
{
    public partial class frmConnect : Form
    {
        Thread thrReceive; //this thread will allow us to receive any time the server broadcasts.
        string username;
        string pass;
        byte[] buffer;
        bool terminating = false;
        string serverIP;
        int serverPort;
        string hexRandom;
        string signedHexRandom;
        string enc_pub_priv = null;
        string pub_key;
        string priv_key;
        static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public frmConnect()
        {
            InitializeComponent();
        }
        void RequestServer(string command)  //creates the requests to send to the server and sends them
        {

            string com = command;
            if (command == "Disconnect")
                com = "~disconnect~";
            else if (command == "User List")
                com = "~list~";
            else if (command == "Connect")
                com = "~";
            else if (command == "Init")
                com = "~init~";
            else if (command == "Authenticate Signature")
                com = "~authenticate~" + hexRandom + "~" + signedHexRandom + "~";
            else if (command == "Ticket Request")
                com = "~req~";
            byte[] buffer = Encoding.Default.GetBytes(username + com);
            client.Send(buffer);
            if (rtbEvent.InvokeRequired)
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += command + " Requested" + Environment.NewLine; }));
            }
        }
        private void btnConnect_Click(object sender, EventArgs e) //connects to the server and arranges GUI according to the current state
        {

            username = txtUsername.Text;
            pass = txtPassword.Text;
            serverIP = txtIP.Text;
            serverPort = Convert.ToInt32(numPort.Value);

            int counter = 0;
            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("enc_" + username + "_pub_priv.txt");
                while ((line = file.ReadLine()) != null)
                    enc_pub_priv = line;
                pub_key = System.IO.File.ReadAllText(username + "_pub.txt");
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Public key is: " + Environment.NewLine + pub_key + Environment.NewLine + Environment.NewLine; }));


                byte[] byte_enc = hexStringToByteArray(enc_pub_priv);
                byte[] sha256 = hashWithSHA256(pass);
                byte[] byte_key = new byte[16];
                Array.Copy(sha256, 0, byte_key, 0, 16);
                byte[] byte_IV = new byte[16];
                Array.Copy(sha256, 16, byte_IV, 0, 16);
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Key for AES128 decryption: " + Environment.NewLine + generateHexStringFromByteArray(byte_key) + Environment.NewLine + Environment.NewLine; }));

                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "IV for AES128 decryption: " + Environment.NewLine + generateHexStringFromByteArray(byte_IV) + Environment.NewLine + Environment.NewLine; }));

                byte[] decryptedAES128 = decryptWithAES128(byte_enc, byte_key, byte_IV);

                if (decryptedAES128 == null)
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Wrong username or password" + Environment.NewLine; }));
                else
                {
                    priv_key = Encoding.Default.GetString(decryptedAES128);
                    
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Client-side authentication successful" + Environment.NewLine; }));
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Decrypted key is: " + Environment.NewLine + priv_key + Environment.NewLine + Environment.NewLine; }));

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {

                        client.Connect(serverIP, serverPort);
                        RequestServer("Init");
                        thrReceive = new Thread(new ThreadStart(Receive));
                        thrReceive.Start();
                        grpConnect.Visible = false;
                        grpUserList.Visible = false;
                        btnConnect.Visible = false;
                        btnDisconnect.Visible = true;
                        btnUserList.Visible = false;
                        lstUserList.Visible = false;
                    }
                    catch
                    {
                        rtbEvent.AppendText("Cannot connect to the specified server\n");
                    }
                }
            }
            catch
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "No user found with that username" + Environment.NewLine; }));
            }

        }
        void disconnect(Socket suket)       //disconnects the user and arranges the GUI according to that
        {
            terminating = true;

            lstUserList.Invoke(new MethodInvoker(delegate { lstUserList.Items.Clear(); }));
            btnDisconnect.Invoke(new MethodInvoker(delegate { btnDisconnect.Visible = false; }));
            grpConnect.Invoke(new MethodInvoker(delegate { grpConnect.Visible = true; }));
            grpUserList.Invoke(new MethodInvoker(delegate { grpUserList.Visible = false; }));
            btnUserList.Invoke(new MethodInvoker(delegate { btnUserList.Visible = false; }));
            btnConnect.Invoke(new MethodInvoker(delegate { btnConnect.Visible = true; }));
            priv_key = null;


        }

        static byte[] hashWithSHA256(string input)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create a hasher object from System.Security.Cryptography
            SHA256CryptoServiceProvider sha256Hasher = new SHA256CryptoServiceProvider();
            // hash and save the resulting byte array
            byte[] result = sha256Hasher.ComputeHash(byteInput);

            return result;
        }

#pragma warning disable IDE1006 // Naming Styles
        static byte[] decryptWithAES128(byte[] byteInput, byte[] key, byte[] IV)
#pragma warning restore IDE1006 // Naming Styles
        {
            // convert input string to byte array
            //byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }
        static byte[] encryptWithRSA(byte[] input, int algoLength, string xmlString)
        {
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.Encrypt(input, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // RSA decryption with varying bit length
        static byte[] decryptWithRSA(byte[] input, int algoLength, string xmlString)
        {
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(input, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private void Receive()          //to recieve messages from the server and update GUI elements according to the messages
        {
            bool connected = true;
            //to access Rich Text Box element in the GUI

            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Connected to the server." + Environment.NewLine; }));

            while (connected)
            {
                try
                {
                    byte[] buffer = new byte[1024];

                    int rec = client.Receive(buffer);

                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }

                    string newmessage = Encoding.Default.GetString(buffer);
                    newmessage = newmessage.Substring(0, newmessage.IndexOf("\0")); //gets the server messages
                    string command = newmessage.Substring(0, newmessage.IndexOf("~"));// gets the commands from the messages(e.g. disconnect, list)
                    newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                    if (command == "disconnect")
                    {
                        disconnect(client);
                    }
                    else if (command == "list")
                    {

                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "User list requested" + Environment.NewLine; }));
                        // resets the listbox and adds the users into the list
                        lstUserList.Invoke(new MethodInvoker(delegate { lstUserList.Items.Clear(); }));
                        while (newmessage != "")
                        {

                            lstUserList.Invoke(new MethodInvoker(delegate { lstUserList.Items.Add(newmessage.Substring(0, newmessage.IndexOf("~"))); }));
                            newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        }
                        //to access Rich Text Box element in the GUI

                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "User list received" + Environment.NewLine; }));

                    }
                    else if (command == "init")
                    {
                        hexRandom = newmessage.Substring(0, newmessage.Length - 1);
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Server generated random number is:" + Environment.NewLine + hexRandom + Environment.NewLine + Environment.NewLine; }));
                        byte[] signatureRSA = signWithRSA(hexRandom, 2048, priv_key);
                        signedHexRandom = generateHexStringFromByteArray(signatureRSA);
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Signature is:"+ Environment.NewLine+ signedHexRandom+ Environment.NewLine + Environment.NewLine; }));
                        RequestServer("Authenticate Signature");


                    }
                    else if (command == "invalid")
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Duplicate username. Choose a different username." + Environment.NewLine; }));
                        disconnect(client);
                    }
                    else if (command == "fb")
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Feedback received" + Environment.NewLine; }));
                        string fbAns = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        string hex = newmessage.Substring(0, newmessage.IndexOf("~"));
                        string serv_pub = System.IO.File.ReadAllText("auth_server_pub.txt");
                        bool verificationResult = verifyWithRSA(fbAns, 2048, serv_pub, hexStringToByteArray(hex));
                        if (verificationResult == true)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Valid Signature" + Environment.NewLine; }));
                            if (fbAns == "yes")
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Successfully authenticated"; }));
                            else if (fbAns == "no")
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Not authenticated, try again"; }));
                        }
                        else
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Invalid Signature" + Environment.NewLine; }));
                        }
                    }
                    else if (command == "ticket")
                    {
                        string plainSignature, plainUser, plainFS;
                        plainSignature = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        plainUser = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        plainFS = newmessage.Substring(newmessage.IndexOf("~"));
                        byte[] bytePlainUser = new byte[1024];
                        byte[] bytePlainSignature = new byte[1024];
                        bytePlainUser = hexStringToByteArray(plainUser);
                        bytePlainSignature = hexStringToByteArray(plainSignature);
                        String decryptedUserPlaintext = generateHexStringFromByteArray(decryptWithRSA(bytePlainUser, 1024, priv_key));
                        if(verifyWithRSA(decryptedUserPlaintext, 1024, pub_key, bytePlainSignature))                                                //YANLIS PUB_KEY OLABILIR!!
                        {
                            String tmp = decryptedUserPlaintext;
                            tmp = tmp.Substring(tmp.IndexOf("~")+1);
                            String session_key = tmp.Substring(0, tmp.IndexOf("~"));
                            tmp = tmp.Substring(tmp.IndexOf("~") + 1);
                            String session_IV = tmp.Substring(0, tmp.IndexOf("~"));
                            tmp = tmp.Substring(tmp.IndexOf("~") + 1);
                            String hmac_key= tmp.Substring(0, tmp.IndexOf("~"));
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Session Key: \n"+ session_key+"\n Session IV: \n"+session_IV+"\n HMAC Key: \n"+hmac_key+"\n"; }));
                        }
                        else
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Verification of ticket from the Authentication Server failed! \n"; }));
                        }                       
                    }
                }
                catch
                {
                    if (!terminating)
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Connection has been terminated by the server...\n"; }));
                    connected = false;
                }
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)        //Disconnects the user and shows the related info
        {

            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.Text += "Disconnect requested. " + Environment.NewLine + Environment.NewLine + Environment.NewLine; }));
            RequestServer("Disconnect");
            disconnect(client);
            rtbEvent.Text += "Disconnect granted." + Environment.NewLine;
        }

        private void btnUserList_Click(object sender, EventArgs e)  //requests the user list from the server
        {
            RequestServer("User List");
        }

        private void frmConnect_FormClosing(object sender, FormClosingEventArgs e)
        {
            terminating = true;
            client.Close();
        }

        // verifying with RSA
        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // helper functions
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void grpUserList_Enter(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // signing with RSA
        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = hexStringToByteArray(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            RequestServer("Ticket Request");
        }
    }
}
