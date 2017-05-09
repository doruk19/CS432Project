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
using System.IO;

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
        string serv_pub;
        string filename;
        string ticket;
        int packetNum;
        string dataHex;
        int packetCounter = 0;
        byte[] byte_hmac_key = new byte[32];
        byte[] byte_session_key = new byte[16];
        byte[] byte_session_IV = new byte[16];
        private readonly Mutex m = new Mutex();


        StringBuilder sb = new StringBuilder(1024);


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
            else if (command == "Ticket Send")
                com = "~"+ticket;
            else if(command == "Download")
            {
                com = "~download~" + filename + "~";
            }
            else if(command=="Upload")
            {
                com = "~upload~" + filename + "~" + packetNum + "~"+dataHex;
            }
            byte[] buffer = Encoding.Default.GetBytes(username + com);
            client.Send(buffer);
            if (rtbEvent.InvokeRequired)
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText( command + " Requested" + Environment.NewLine); }));
            }
        }
        private void connect(int port,bool isAuthServer)
        {
            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("enc_" + username + "_pub_priv.txt");
                while ((line = file.ReadLine()) != null)
                    enc_pub_priv = line;
                pub_key = System.IO.File.ReadAllText(username + "_pub.txt");
                if (rtbEvent.InvokeRequired)
                {
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Public key is: " + Environment.NewLine + pub_key + Environment.NewLine + Environment.NewLine); }));
                }

                byte[] byte_enc = hexStringToByteArray(enc_pub_priv);
                byte[] sha256 = hashWithSHA256(pass);
                byte[] byte_key = new byte[16];
                Array.Copy(sha256, 0, byte_key, 0, 16);
                byte[] byte_IV = new byte[16];
                Array.Copy(sha256, 16, byte_IV, 0, 16);
                if (rtbEvent.InvokeRequired)
                {
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Key for AES128 decryption: " + Environment.NewLine + generateHexStringFromByteArray(byte_key) + Environment.NewLine + Environment.NewLine); }));
                }
                    if (rtbEvent.InvokeRequired)
                {
                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("IV for AES128 decryption: " + Environment.NewLine + generateHexStringFromByteArray(byte_IV) + Environment.NewLine + Environment.NewLine); }));
                }
                byte[] decryptedAES128 = decryptWithAES128(byte_enc, byte_key, byte_IV, CipherMode.CFB);

                if (decryptedAES128 == null)
                {
                    if (rtbEvent.InvokeRequired)
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Wrong username or password" + Environment.NewLine); }));
                    }
                }
                else
                {
                    priv_key = Encoding.Default.GetString(decryptedAES128);

                    if (rtbEvent.InvokeRequired)
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Client-side authentication successful" + Environment.NewLine); }));
                    }
                    if (rtbEvent.InvokeRequired)
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Decrypted key is: " + Environment.NewLine + priv_key + Environment.NewLine + Environment.NewLine); }));
                    }
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {

                        client.Connect(serverIP, port);
                        if (isAuthServer)
                        {
                            RequestServer("Init");
                        }
                        else
                        {
                            RequestServer("Ticket Send");
                        }
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
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.AppendText("Cannot connect to the specified server\n");
                        }
                    }
                }
            }
            catch
            {
                if (isAuthServer)
                    if (rtbEvent.InvokeRequired)
                    {
                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("No user found with that username" + Environment.NewLine); }));
                    }
            }
        }
        private void btnConnect_Click(object sender, EventArgs e) //connects to the server and arranges GUI according to the current state
        {

            username = txtUsername.Text;
            pass = txtPassword.Text;
            serverIP = txtIP.Text;
            serverPort = Convert.ToInt32(numAuthPort.Value);
            connect(serverPort,true);

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
            client.Close();
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
        static byte[] decryptWithAES128(byte[] byteInput, byte[] key, byte[] IV,CipherMode a)
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
            aesObject.Mode = a;
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
            if (rtbEvent.InvokeRequired)
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Connected to the server." + Environment.NewLine); }));
            }
            while (connected)
            {
                try
                {
                    byte[] buffer = new byte[8192];

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
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("User list requested" + Environment.NewLine); }));
                        }
                        // resets the listbox and adds the users into the list
                        lstUserList.Invoke(new MethodInvoker(delegate { lstUserList.Items.Clear(); }));
                        while (newmessage != "")
                        {

                            lstUserList.Invoke(new MethodInvoker(delegate { lstUserList.Items.Add(newmessage.Substring(0, newmessage.IndexOf("~"))); }));
                            newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        }
                        //to access Rich Text Box element in the GUI
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("User list received" + Environment.NewLine); }));
                        }
                    }
                    else if (command == "init")
                    {
                        hexRandom = newmessage.Substring(0, newmessage.Length - 1);
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Server generated random number is:" + Environment.NewLine + hexRandom + Environment.NewLine + Environment.NewLine); }));
                        }
                            byte[] signatureRSA = signWithRSA(hexRandom, 2048, priv_key);
                        signedHexRandom = generateHexStringFromByteArray(signatureRSA);
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Signature is:" + Environment.NewLine + signedHexRandom + Environment.NewLine + Environment.NewLine); }));
                        }
                            RequestServer("Authenticate Signature");


                    }
                    else if (command == "invalid")
                    {
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Duplicate username. Choose a different username." + Environment.NewLine); }));
                        }
                            disconnect(client);
                    }
                    else if (command == "fb")
                    {
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Feedback received" + Environment.NewLine); }));
                        }
                            string fbAns = newmessage.Substring(0, newmessage.IndexOf("~"));
                        byte[] byte_fbAns= Encoding.Default.GetBytes(fbAns);
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        string hex = newmessage.Substring(0, newmessage.IndexOf("~"));
                        serv_pub = System.IO.File.ReadAllText("auth_server_pub.txt");
                        bool verificationResult = verifyWithRSA(byte_fbAns, 2048, serv_pub, hexStringToByteArray(hex));
                        if (verificationResult == true)
                        {
                            if (rtbEvent.InvokeRequired)
                            {
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Valid Signature" + Environment.NewLine); }));
                            }
                            if (fbAns == "yes")
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Successfully authenticated \n"); }));
                                }
                                else if (fbAns == "no")
                                    if (rtbEvent.InvokeRequired)
                                    {
                                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Not authenticated, try again \n"); }));
                                    }
                        }
                        else
                        {
                            if (rtbEvent.InvokeRequired)
                            {
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Invalid Signature" + Environment.NewLine); }));
                            }
                        }
                    }
                    else if (command == "ticket")
                    {
                        ticket = newmessage;
                        string plainSignature, plainUser, plainFS;
                        plainSignature = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        plainUser = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~") + 1);
                        plainFS = newmessage.Substring(0,newmessage.IndexOf("~"));
                        byte[] bytePlainUser = hexStringToByteArray(plainUser);
                        byte[] bytePlainSignature = hexStringToByteArray(plainSignature);
                        Console.WriteLine(plainSignature);
                        byte[] decryptedUserPlaintext = decryptWithRSA(bytePlainUser, 1024, priv_key);

                        if (verifyWithRSA(decryptedUserPlaintext, 1024, serv_pub, bytePlainSignature))                                                //YANLIS PUB_KEY OLABILIR!!
                        {
                            Array.Copy(decryptedUserPlaintext, byte_session_key, 16);

                            Array.Copy(decryptedUserPlaintext, 16, byte_session_IV, 0, 16);

                            Array.Copy(decryptedUserPlaintext, 32, byte_hmac_key, 0, 32);
                            if (rtbEvent.InvokeRequired)
                            {
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Session Key: \n" + generateHexStringFromByteArray(byte_session_key) + "\n Session IV: \n" + generateHexStringFromByteArray(byte_session_IV) + "\n HMAC Key: \n" + generateHexStringFromByteArray(byte_hmac_key) + "\n"); }));
                            }
                            RequestServer("Disconnect");
                            connect(Convert.ToInt32(numFilePort.Value), false);
                        }
                        else
                        {
                            if (rtbEvent.InvokeRequired)
                            {
                                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Verification of ticket from the Authentication Server failed! \n"); }));
                            }
                        }                    
                    }
                    
                    
                    else if(command =="file")
                    {
                        m.WaitOne();
                        try
                        {
                            newmessage = newmessage.Substring(0, newmessage.IndexOf("~"));
                            if (packetCounter % 100 == 0)
                            {
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText(packetCounter + "   Download in Progress: " + packetCounter * 100 / packetNum + "% Remaining packets: " + (packetNum - packetCounter).ToString() + " \n"); }));
                                }
                            }
                            sb.Append(newmessage);


                            packetCounter++;
                            if (packetCounter== packetNum)
                            {
                                string file_str = sb.ToString();
                                string given_hmac = file_str.Substring(file_str.Length - 64, 64);
                                file_str = file_str.Substring(0, file_str.Length - 64);
                                byte[] decrypted = decryptWithAES128(hexStringToByteArray(file_str), byte_session_key, byte_session_IV, CipherMode.CBC);
                                byte[] hmac = applyHMACwithSHA256(ref decrypted, byte_hmac_key);
                                byte[] given_hmac_byte = hexStringToByteArray(given_hmac);
                                if (hmac.SequenceEqual(given_hmac_byte))
                                {
                                    File.WriteAllBytes(filename, decrypted);
                                    if (rtbEvent.InvokeRequired)
                                    {
                                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Successfully downloaded file " + filename + ".\n"); }));
                                    }
                                }
                                else
                                {
                                    if (rtbEvent.InvokeRequired)
                                    {
                                        rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Failed downloading file " + filename + ".\n"); }));
                                    }
                                }

                            }
                            

                        }
                        finally
                        {
                            m.ReleaseMutex();
                        }

                    }
                    else if(command =="fs")
                    {
                        string com = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage = newmessage.Substring(newmessage.IndexOf("~")+1);
                        string sign = newmessage.Substring(0, newmessage.IndexOf("~"));
                        newmessage= newmessage.Substring(newmessage.IndexOf("~") + 1);
                        if (verifyWithRSA(Encoding.Default.GetBytes(com), 1024, serv_pub, hexStringToByteArray(sign)))
                        {
                            if (com == "yes")
                            {
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Ticket is valid\n"); }));
                                }
                            }
                            else if (com == "no")
                            {
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Ticket is invalid, disconnecting... \n"); }));
                                }
                                disconnect(client);
                            }
                            else if (com == "nf")
                            {
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("File not found... \n"); }));
                                }
                            }
                            else if (com.Substring(0, 2) == "ok")
                            {
                                packetNum = Convert.ToInt32(com.Substring(2));
                                if (rtbEvent.InvokeRequired)
                                {
                                    rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("File found, number of packets to send is" + packetNum + " \n"); }));
                                }
                            }
                        }
                    }
                }             
                catch
                {
                    if (!terminating)
                        if (rtbEvent.InvokeRequired)
                        {
                            rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Connection has been terminated by the server...\n"); }));
                        }
                        connected = false;
                }
            }
        }
        static byte[] applyHMACwithSHA256(ref byte[] input, byte[] key)
        {
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(input);

            return result;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)        //Disconnects the user and shows the related info
        {
            if (rtbEvent.InvokeRequired)
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Disconnect requested. " + Environment.NewLine + Environment.NewLine + Environment.NewLine); }));
            }
            RequestServer("Disconnect");
            disconnect(client);
            rtbEvent.AppendText( "Disconnect granted." + Environment.NewLine);
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
        static bool verifyWithRSA(byte[] input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
           
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(input, "SHA256", signature);
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

        private void rtbEvent_TextChanged_1(object sender, EventArgs e)
        {

            rtbEvent.SelectionStart = rtbEvent.Text.Length;
            rtbEvent.ScrollToCaret();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            filename = "die_wacht_am_rhein.ogg";
            RequestServer("Download");
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            filename = "die_wacht_am_rhein.ogg";

            byte[] file= File.ReadAllBytes(filename);
            byte[] hmac = applyHMACwithSHA256(ref file, byte_hmac_key);
            byte[] encrypted = encryptWithAES128(file, byte_session_key, byte_session_IV);
            int length = 0;
            if ((hmac.Length + encrypted.Length) % (8 * 256) == 0)
            {
                length = (hmac.Length + encrypted.Length) / (8 * 256);
            }
            else
            {
                length = (hmac.Length + encrypted.Length) / (8 * 256) + 1;
            }
            string response = username+"~upload~"+filename+"~" + length + "~";
            StringBuilder sb = new StringBuilder(1024);
            sb.Append(generateHexStringFromByteArray(encrypted));
            sb.Append(generateHexStringFromByteArray(hmac));
            string file_str = sb.ToString();
            for (int i = 0; i < length; i++)
            {
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText((i+1).ToString() + "   Download in Progress: " + (i+1) * 100 / length + "% Remaining packets: " + (length - i-1).ToString() + " \n"); }));
                string packet = file_str.Substring(8 * 512 * i, Math.Min(8 * 512, file_str.Length - 8 * 512 * i));
                string message = response + packet + "~";
                rtbEvent.Invoke(new MethodInvoker(delegate { rtbEvent.AppendText("Packet " + length + " packages \n"); }));

                client.Send(Encoding.Default.GetBytes(message));

                System.Threading.Thread.Sleep(50);

            }
            
        }
        static byte[] encryptWithAES128(byte[] input, byte[] key, byte[] IV)
        {
            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CBC;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(input, 0, input.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }
    }
}
