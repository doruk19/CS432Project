/*
 Name: K.Doruk Gür - İnanç Dokurel
 ID: 17699 - 17575
 Title: CS408 Project Step 1 - Server Side
 Description: This GUI program simulates the server side of the projects step 1.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Server
{

    public partial class ServerForm : Form
    {
        //Structure definition of users. Each user has a user name and ip address associated with it
        public struct User
        {
            string name;
            IPAddress userAddress;
            byte[] hmac_key;
            byte[] session_key;
            byte[] session_iv;
            int packetMax;
            int packetCtr;
            string fileName;
            StringBuilder sb;

            //Constructor for the user
            public User(string uname, IPAddress uaddress, byte[] uhmac,byte[]ukey, byte[]uiv)
            {
                name = uname;
                userAddress = uaddress;
                hmac_key = uhmac;
                session_key = ukey;
                session_iv = uiv;
                packetMax = 0;
                packetCtr = 0;
                fileName = "";
                sb = new StringBuilder(1024);

            }
            //Returns the name of the user
            public string getName()
            {
                return name;
            }
            //Returns the IP of the user
            public IPAddress getIP()
            {
                return userAddress;
            }
            public byte[] getKey()
            {
                return session_key;
            }
            public byte[] getIV()
            {
                return session_iv;
            }
            public byte[] getHMAC()
            {
                return hmac_key;
            }
            public void appendFile(string filebit)
            {
                sb.Append(filebit);
            }
            public void incrementCount(string s)
            {
                packetCtr++;
                if (packetCtr >= 1)
                {
                    fileName = s;
                }
            }
            public void Clear()
            {
                packetCtr = 0;
                fileName = "";
                packetMax=0;
                sb.Clear();
            }
            public StringBuilder getFile()
            {
                return sb;
            }
            public void setPacketLength(int length)
            {
                packetMax = 0;
            }
            public int getPacketLength()
            {
                return packetMax;
            }
            public int getCurrentPacket()
            {
                return packetCtr;
            }
            //Override method for equality of two users. Used for comparing whether the user with same 
            public bool Equals(User rhs)
            {
                return (name == rhs.getName()) && (userAddress == rhs.getIP());
            }

        }
        //Global variables. Booleans are for the states of the server
        bool listening = false;
        bool terminating = false;
        bool accepting = true;
        //Server's socket
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //First list is for all the active connections with the clients, the second one is the connected users data
        List<Socket> sockets = new List<Socket>();
        List<User> users = new List<User>();
        string as_pub = System.IO.File.ReadAllText("auth_server_pub.txt");
        string fs_pub_priv = System.IO.File.ReadAllText("file_server_pub_priv.txt");

        public ServerForm()
        {
            InitializeComponent();
        }

        //Loads server's ip to a label in GUI.
        private void ServerForm_Load(object sender, EventArgs e)
        {
            lblIP.Text = getLocalIP();
        }
        //Gets the local ip of the machine. This becomes the server's ip.
        private string getLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "127.0.0.1";
        }
        //Starts the server
        private void btnStart_Click(object sender, EventArgs e)
        {
            //Gets the port number from the component in GUI.
            int port = (int)numPort.Value;
            Thread threadAccept;
            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, port));
                rtbEventLog.AppendText("Server started.\n");
                rtbEventLog.AppendText("Server has the key pair: " + fs_pub_priv+ "./n");

                //The queue has a cap of 10 at most
                server.Listen(10);

                //Starts the background thread that accepts connections
                threadAccept = new Thread(new ThreadStart(AcceptingThread));
                threadAccept.Start();
                //Sets the status of the server listening and closes any user interaction. The user can only monitor activities after that
                listening = true;
                btnStart.Enabled = false;
                numPort.Enabled = false;
            }
            catch
            {
                //If there is an error, shows it on the eventlog.
                rtbEventLog.AppendText("Cannot create a server with the specified port number " + port + ".\n");
                rtbEventLog.AppendText("Terminating...\n");
            }

        }

        //This is the thread definition for the accepting thread
        private void AcceptingThread()
        {

            //It runs until the state of the server is not accepting anymore.
            while (accepting)
            {
                try
                {
                    //Tries to add incoming connection to the connection list, shows it in the event log. Since GUI is in another thread, methodinvoker required
                    sockets.Add(server.Accept());
                    if (rtbEventLog.InvokeRequired)
                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("New client connecting...\n"); }));
                    //Starts a thread to receive data from incoming connection
                    Thread threadReceive;
                    threadReceive = new Thread(new ThreadStart(ReceivingThread));
                    threadReceive.Start();
                }
                catch
                {
                    //Sets the state non accepting and therefore ends thread
                    if (terminating)
                        accepting = false;
                    else
                      if (rtbEventLog.InvokeRequired)
                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Server has stopped listening.\n"); }));

                }
            }
        }
        //This is the thread for receiving data from a connection
        private void ReceivingThread()
        {
            bool connected = true;
            Socket n = sockets[sockets.Count - 1];
            //Does this while connected to the incoming connection
            while (connected)
            {
                try
                {
                    //Receives data
                    Byte[] buffer = new byte[1024];
                    int received = n.Receive(buffer);

                    //If there is none, throws exception
                    if (received <= 0)
                    {
                        throw new SocketException();
                    }
                    //Parses the username and the data sent
                    string receivedData = Encoding.Default.GetString(buffer);
                    string userName = receivedData.Substring(0, receivedData.IndexOf("~"));
                    receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);

                    //If it finds the user within the connected users list checks for the IP of the incoming connection
                    if (!(findUser(userName).Equals(new User())))
                    {

                        //If it has the same IP with the connected users list it means that connected user sends a command so it tries to parse what the command is
                        if (!(findUser(((IPEndPoint)n.RemoteEndPoint).Address).Equals(new User())))
                        {
                            string command = receivedData.Substring(0, receivedData.IndexOf("~"));
                            receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                            //If the command is list, it sends the list of currently connected users.
                            if (command == "download")
                            {
                                string file_name = receivedData.Substring(0, receivedData.IndexOf("~"));
                                receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                                if (!File.Exists(file_name))
                                {
                                    string response = "fs~nf~";
                                    byte[] response_byte = Encoding.Default.GetBytes("nf");
                                    response = response + generateHexStringFromByteArray(signWithRSA(response_byte, 1024, fs_pub_priv)) + "~";
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User " + userName + " requested a non-existent file. \n"); }));
                                    n.Send(Encoding.Default.GetBytes(response));
                                }
                                else
                                {
                                    byte[] file = File.ReadAllBytes(file_name);
                                    User u = findUser(userName);
                                    byte[] hmac_key = u.getHMAC();
                                    byte[] session_key = u.getKey();
                                    byte[] session_IV = u.getIV();

                                    byte[] hmac = applyHMACwithSHA256(ref file, hmac_key);
                                    byte[] encrypted = encryptWithAES128(file, session_key, session_IV);

                                    int length = (hmac.Length + encrypted.Length) / 256 + 1;
                                    string response = "fs~ok~"+length+"~";
                                    byte[] response_byte = Encoding.Default.GetBytes("ok"+length);
                                    response = response + generateHexStringFromByteArray(signWithRSA(response_byte, 1024, fs_pub_priv)) + "~";
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User " + userName + " requested a valid file, download starting. \n"); }));
                                    n.Send(Encoding.Default.GetBytes(response));
                                    
                                    StringBuilder sb = new StringBuilder(1024);
                                    sb.Append(generateHexStringFromByteArray(encrypted));
                                    sb.Append(generateHexStringFromByteArray(hmac));
                                    string file_str = sb.ToString();
                                    for(int i = 0; i < length; i++)
                                    {
                                        string packet = file_str.Substring(0 + 512 * i, 512);
                                        string message = "file~" + packet + "~";
                                        n.Send(Encoding.Default.GetBytes(message));
                                    }

                                }
                               }
                            else if (command == "disconnect")
                            {
                                byte[] send = Encoding.Default.GetBytes("disconnect~");
                                n.Send(send);
                                n.Shutdown(SocketShutdown.Both);


                                throw new SocketException();
                            }
                            else if (command =="upload")
                            {
                                string file_name = receivedData.Substring(0, receivedData.IndexOf("~"));
                                receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                                string packetlength = receivedData.Substring(0, receivedData.IndexOf("~"));
                                receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                                string packet = receivedData.Substring(0, receivedData.IndexOf("~"));

                                User u = findUser(userName);
                                u.appendFile(packet);
                                u.setPacketLength(Int32.Parse(packetlength));
                                u.incrementCount(file_name);
                                if (u.getCurrentPacket() == u.getPacketLength())
                                {
                                    byte[] hmac_key = u.getHMAC();
                                    byte[] session_key = u.getKey();
                                    byte[] session_IV = u.getIV();

                                    string file_str = u.getFile().ToString();
                                    string given_hmac = file_str.Substring(file_str.Length - 512, 512);
                                    file_str = file_str.Substring(0,file_str.Length - 512);
                                    byte[] decrypted = decryptWithAES128(StringToByteArray(file_str), session_key, session_IV);
                                    byte[] hmac = applyHMACwithSHA256(ref decrypted, hmac_key);
                                    string response="";
                                    if (hmac == StringToByteArray(given_hmac))
                                    {
                                        response = "uok";
                                        File.WriteAllBytes(file_name, decrypted);
                                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User " + userName + " successfully uploaded file " +file_name + ".\n"); }));
                                        
                                    }
                                    else
                                    {
                                        response = "uf";
                                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User " + userName + " failed to upload " + file_name + ".\n"); }));

                                    }
                                    u.Clear();
                                    string message = "fs~" + response + "~" + generateHexStringFromByteArray(signWithRSA(Encoding.Default.GetBytes(response), 1024, fs_pub_priv)) + "~";
                                    n.Send(Encoding.Default.GetBytes(message));
                                }
                                int index = 0;
                                foreach (User t in users)
                                    if (u.getName() == t.getName())
                                        index = users.IndexOf(t);
                                users[index] = u;
                            }


                        }
                        //If the ip doesn't match, this means somebody else is trying to use the same username so it doesn't allow it and disconnects the user.
                        else
                        {
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Invalid Username\n"); }));
                            byte[] send = Encoding.Default.GetBytes("invalid~");
                            n.Send(send);
                            n.Shutdown(SocketShutdown.Both);
                            n.Disconnect(true);
                        }

                    }
                    //If the name doesn't match it means there is a new user connecting, so it adds to the currently connected user data
                    else
                    {
                        string plainSignature, plainUser, plainFS,response;
                        response = "no";
                        plainSignature = receivedData.Substring(0, receivedData.IndexOf("~"));
                        receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                        plainUser = receivedData.Substring(0, receivedData.IndexOf("~"));
                        receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                        plainFS = receivedData.Substring(0, receivedData.IndexOf("~"));
                        byte[] bytePlainFS = StringToByteArray(plainFS);
                        byte[] bytePlainSignature = StringToByteArray(plainSignature);
                        Console.WriteLine(plainSignature);
                        byte[] decrypted = decryptWithRSA(bytePlainFS, 1024, fs_pub_priv);

                        if (verifyWithRSA(decrypted, 1024, as_pub, bytePlainSignature))                                                //YANLIS PUB_KEY OLABILIR!!
                        {
                            byte[] byte_session_key = new byte[16];
                            Array.Copy(decrypted, byte_session_key, 16);
                            byte[] byte_session_IV = new byte[16];
                            Array.Copy(decrypted, 16, byte_session_IV, 0, 16);
                            byte[] byte_hmac_key = new byte[32];
                            Array.Copy(decrypted, 32, byte_hmac_key, 0, 32);
                            int user_length = decrypted.Length - 65;
                            byte[] user_byte =new byte[user_length];
                            Array.Copy(decrypted, 0, user_byte, 0, user_length);
                            string ticket_user = Encoding.Default.GetString(user_byte);
                            if(ticket_user == userName)
                            {
                                rtbEventLog.Invoke(new MethodInvoker(delegate {
                                    rtbEventLog.AppendText("User " + userName + " has connected with a valid ticket. \n" ); }));
                                User u = new User(userName, ((IPEndPoint)n.RemoteEndPoint).Address,byte_hmac_key,byte_session_key,byte_session_IV);
                                users.Add(u);
                                response = "yes";
                            }
                            else
                            {
                                rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User " + userName + " tries to use someone else's ticket, connection terminating. \n"); }));
                                response = "no";
                            }
                            
                        }
                        else
                        {
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("User "+ userName + " does not have a valid ticket, connection terminating. \n"); }));
                            response = "no";
                        }
                        string message = "fs~" + response + "~";
                        message = message + generateHexStringFromByteArray(signWithRSA(Encoding.Default.GetBytes(response), 1024, fs_pub_priv))+"~";
                        byte[] send = Encoding.Default.GetBytes(message);
                        n.Send(send);
                        if (response == "no")
                        {
                            n.Close();
                            sockets.Remove(n);
                        }
                    }

                }
                catch
                {
                    //Steps for disconnecting the user

                    User u = findUser(((IPEndPoint)n.RemoteEndPoint).Address);
                    if (!terminating)
                        if (rtbEventLog.InvokeRequired)
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(u.getName() + " disconnected.\n"); }));

                    users.Remove(u);
                    if (rtbAuthenticatedUsers.InvokeRequired)
                        rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.Clear(); }));
                    foreach (User t in users)
                    {
                        if (rtbAuthenticatedUsers.InvokeRequired)
                            rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.AppendText(t.getName() + "\n"); }));
                    }
                    n.Close();
                    sockets.Remove(n);

                    connected = false;
                }
            }

        }
        //Method to find user based on its username
        public User findUser(string name)
        {
            foreach (User n in users)
            {
                if (n.getName() == name)
                    return n;
            }

            return new User();
        }
        //Method to find user based on its ip
        public User findUser(IPAddress ip)
        {
            foreach (User n in users)
            {
                if (n.getIP() == ip)
                    return n;
            }
            return new User();
        }
        //Method for generating a random number
        public void generateRandomNumber(ref byte[] number)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(number);
        }
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }
        public static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        static byte[] signWithRSA(byte[] byteInput, int algoLength, string xmlString)
        {
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

        static bool verifyWithRSA(byte[] byteInput, int algoLength, string xmlString, byte[] signature)
        {
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

        // HMAC with SHA-256
        static byte[] applyHMACwithSHA256(ref byte[] input, byte[] key)
        {
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(input);

            return result;
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
        static byte[] decryptWithAES128(byte[] input, byte[] key, byte[] IV)
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
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(input, 0, input.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        //Sends disconnect signal to all connected clients and terminates all connections
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {


            byte[] send = Encoding.Default.GetBytes("disconnect~");

            foreach (Socket n in sockets)
            {
                n.Send(send);
            }

            terminating = true;
            server.Close();

        }

        private void lblIPLabel_Click(object sender, EventArgs e)
        {

        }
    }
}