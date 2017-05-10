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

            //Constructor for the user
            public User(string uname, IPAddress uaddress)
            {
                name = uname;
                userAddress = uaddress;
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
        List<User> authenticated = new List<User>();
        string server_pubpriv = System.IO.File.ReadAllText("auth_server_pub_priv.txt");
        string fs_pub = System.IO.File.ReadAllText("file_server_pub.txt");

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
                rtbEventLog.AppendText("Server has the key pair: " + server_pubpriv + "./n");

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
                    Byte[] buffer = new byte[8192];
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
                            //If the command is list, it sends the list of currently connected users.
                            if (command == "init")
                            {

                                byte[] rand = new byte[16];

                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " trying to authenticate.\n"); }));
                                generateRandomNumber(ref rand);
                                string random = generateHexStringFromByteArray(rand);
                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Random number " + random + " has been sent to" + userName + "\n"); }));
                                string message = "init~" + random + "~";
                                byte[] send = Encoding.Default.GetBytes(message);
                                n.Send(send);
                            }
                            else if (command == "authenticate")
                            {
                                receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                                string random = receivedData.Substring(0, receivedData.IndexOf("~"));
                                receivedData = receivedData.Substring(receivedData.IndexOf("~") + 1);
                                string randomSig = receivedData.Substring(0, receivedData.IndexOf("~"));

                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " has sent signature " + randomSig + " to validate number: " + random + "\n"); }));

                                byte[] byte_random = StringToByteArray(random);
                                byte[] byte_sig = StringToByteArray(randomSig);
                                string user_pub = System.IO.File.ReadAllText(userName + "_pub.txt");
                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Trying to authenticate with the pub key: " + user_pub + ".\n"); }));
                                bool verification = verifyWithRSA(byte_random, 1024, user_pub, byte_sig);
                                string verif_result;
                                if (verification)
                                {
                                    if (rtbEventLog.InvokeRequired)
                                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " has been authenticated.\n"); }));
                                    verif_result = "yes";
                                    authenticated.Add(findUser(userName));
                                    if (rtbAuthenticatedUsers.InvokeRequired)
                                        rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.Clear(); }));
                                    foreach (User u in authenticated)
                                    {
                                        if (rtbAuthenticatedUsers.InvokeRequired)
                                            rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.AppendText(u.getName() + "\n"); }));
                                    }
                                }
                                else
                                {
                                    if (rtbEventLog.InvokeRequired)
                                        rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " has failed to authenticate.\n"); }));
                                    verif_result = "no";
                                }

                                string message = "fb~" + verif_result + "~";
                                byte[] signature = signWithRSA(Encoding.Default.GetBytes(verif_result), 1024, server_pubpriv);
                                message += (generateHexStringFromByteArray(signature) + "~");
                                byte[] send = Encoding.Default.GetBytes(message);
                                n.Send(send);

                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate
                                    {
                                        rtbEventLog.AppendText(verif_result + " feedback with signature " +
generateHexStringFromByteArray(signature) + " has been sent to " + userName + ".\n");
                                    }));
                            }
                            //If the command is disconnect, disconnects the user
                            else if (command == "disconnect")
                            {
                                byte[] send = Encoding.Default.GetBytes("disconnect~");
                                n.Send(send);
                                n.Shutdown(SocketShutdown.Both);


                                throw new SocketException();
                            }
                            else if (command == "req")
                            {
                                byte[] hmac_key_byte = new byte[32];
                                byte[] session_key_byte = new byte[16];
                                byte[] session_IV_byte = new byte[16];

                                generateRandomNumber(ref hmac_key_byte);
                                generateRandomNumber(ref session_key_byte);
                                generateRandomNumber(ref session_IV_byte);

                                string hmac_key = generateHexStringFromByteArray(hmac_key_byte);
                                string session_key = generateHexStringFromByteArray(session_key_byte);
                                string session_IV = generateHexStringFromByteArray(session_IV_byte);

                                if (rtbEventLog.InvokeRequired)
                                {
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("HMAC key " + hmac_key + " has been generated for " + userName + ".\n"); }));
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Session key " + session_key + " has been generated for " + userName + ".\n"); }));
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Session IV " + session_IV + " has been generated for " + userName + ".\n"); }));
                                }
                                byte[] header = Encoding.Default.GetBytes(userName + "~");
                                byte[] plaintext_byte = new byte[header.Length+64];
                                Array.Copy(header, plaintext_byte, header.Length);
                                Array.Copy(session_key_byte, 0, plaintext_byte, header.Length, 16);

                                Array.Copy(session_IV_byte, 0, plaintext_byte, header.Length+16, 16);

                                Array.Copy(hmac_key_byte, 0, plaintext_byte, header.Length+32, 32);
                                string plaintext_signature = generateHexStringFromByteArray(signWithRSA(plaintext_byte, 1024, server_pubpriv));
                                Console.WriteLine(plaintext_signature);
                                string plaintext_fs = generateHexStringFromByteArray(encryptWithRSA(plaintext_byte, 1024, fs_pub));
                                
                                string user_pub = System.IO.File.ReadAllText(userName + "_pub.txt");

                                string plaintext_user = generateHexStringFromByteArray(encryptWithRSA(plaintext_byte, 1024, user_pub));
                                //Console.WriteLine(generateHexStringFromByteArray(plaintext_byte));
                                string ticket = plaintext_signature + "~" + plaintext_user + "~" + plaintext_fs + "~";

                                byte[] send = Encoding.Default.GetBytes("ticket~"+ticket);
                                n.Send(send);

                                if (rtbEventLog.InvokeRequired)
                                    rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Ticket " + ticket + " has been generated for " + userName + ".\n"); }));
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

                        User u = new User(userName, ((IPEndPoint)n.RemoteEndPoint).Address);
                        users.Add(u);
                        byte[] rand = new byte[16];
                        if (rtbEventLog.InvokeRequired)
                        {
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " trying to authenticate.\n"); }));
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(userName + " connected.\n"); }));
                        }

                        generateRandomNumber(ref rand);
                        string random = generateHexStringFromByteArray(rand);
                        if (rtbEventLog.InvokeRequired)
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText("Random number " + random + " has been sent to " + userName + "\n"); }));
                        string message = "init~" + random + "~";
                        byte[] send = Encoding.Default.GetBytes(message);
                        n.Send(send);
                    }

                }
                catch
                {
                    //Steps for disconnecting the user

                    User u = findUser(((IPEndPoint)n.RemoteEndPoint).Address);
                    if (!terminating)
                        if (rtbEventLog.InvokeRequired)
                            rtbEventLog.Invoke(new MethodInvoker(delegate { rtbEventLog.AppendText(u.getName() + " disconnected.\n"); }));

                    authenticated.Remove(u);
                    if (rtbAuthenticatedUsers.InvokeRequired)
                        rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.Clear(); }));
                    foreach (User t in authenticated)
                    {
                        if (rtbAuthenticatedUsers.InvokeRequired)
                            rtbAuthenticatedUsers.Invoke(new MethodInvoker(delegate { rtbAuthenticatedUsers.AppendText(t.getName() + "\n"); }));
                    }
                    users.Remove(u);
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

        private void rtbEventLog_TextChanged(object sender, EventArgs e)
        {
            rtbEventLog.SelectionStart = rtbEventLog.Text.Length;
            rtbEventLog.ScrollToCaret();
        }
    }
}