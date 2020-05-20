using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientCore
{
    class Program
    {

        public static ManualResetEvent allDone = new ManualResetEvent(true);
        static byte[] recvBuffer = new byte[1024];
        static byte[] sendBuffer = new byte[1024];
        static string[] rMsg;
        static string finalMessage;
        static string uUsername = "Default";
        static string host = "127.0.0.1", port, password;
        static Socket s;
        static void Main(string[] args)
        {
            connect();
        }
        private static void startRecv()
        {
            while (true)
            {

                try
                {

                    if (s.Connected)
                    {

                        Array.Clear(recvBuffer, 0, recvBuffer.Length);
                        allDone.Reset();
                        s.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(recvCallback), s);
                        allDone.WaitOne();

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("ERROR [{0}]", ex.Message));
                }

                Thread.Sleep(1);
            }
        }
        static void recvCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndReceive(ar);

                allDone.Set();

                if (socket.Connected)
                {
                    //recvBuffer = new CryptoControl().Decrypt(userCSP, userPrivKey, recvBuffer);

                    if (recvBuffer.Length != 0)
                    {

                        rMsg = Encoding.UTF8.GetString(recvBuffer).Split('~');
                        finalMessage = rMsg[1];
                        if(finalMessage.Split(" ")[1] != String.Empty)
                            Console.WriteLine(finalMessage);

                    }

                }
            }
            catch (SocketException ex)
            {

                if (ex.Message.Contains("Se ha forzado la interrupci"))
                {

                    s.Close();
                    Console.WriteLine(String.Format("ERROR [{0}]", ex.ErrorCode));
                    System.Environment.Exit(1);

                }
                else
                {
                    Console.WriteLine(String.Format("ERROR [{0}]", ex.ErrorCode));
                    System.Environment.Exit(1);
                }
            }
        }

        private static void Send(string msg)
        {

            try
            {

                string pMsg = String.Format("~[{2}:{3}:{4}][{0}]: {1}~",
                    uUsername,
                    msg,
                    DateTime.Now.TimeOfDay.Hours,
                    DateTime.Now.TimeOfDay.Minutes,
                    Convert.ToInt32(DateTime.Now.TimeOfDay.Seconds));

                Array.Clear(sendBuffer, 0, sendBuffer.Length);
                sendBuffer = Encoding.UTF8.GetBytes(pMsg);
                s.Send(sendBuffer);
                //s.Send(new CryptoControl().Encrypt(serverCSP, userPubKey, sendBuffer));

            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("ERROR [{0}]", ex.Message));
            }
        }
        static bool SecurityAnsw()
        {

            while (true)
            {

                Array.Clear(recvBuffer, 0, recvBuffer.Length);
                s.Receive(recvBuffer);
                string answer = Encoding.UTF8.GetString(recvBuffer);

                if (answer.Contains("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        static void connect()
        {
            while (true)
            {
                Console.WriteLine("#WELCOME_TO_CLIENT ~~~~\n\n");
                Console.Write("\nIP Host: ");
                host = Console.ReadLine();
                Console.Write("\nPort: ");
                port = Console.ReadLine();
                Console.Write("\nUsername: ");
                uUsername = Console.ReadLine();
                Console.Write("\nPassword: ");
                password = Console.ReadLine();

                Thread secu = new Thread(new ThreadStart(startRecv));

                try
                {

                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified);
                    s.Connect(host, Convert.ToInt32(port));

                    sendBuffer = Encoding.UTF8.GetBytes(password);
                    s.Send(sendBuffer);

                    if (SecurityAnsw() == false)
                    {

                        Console.WriteLine("[X] CONNECTION REJECTED - [WRONG PASSWORD]");
                        s.Close();
                        Console.WriteLine("Contraseña rechazada por el servidor");
                        Console.ReadKey();
                        Console.Clear();

                    }
                    else
                    {

                        if (s.Connected)
                        {
                            secu.Start();
                            string welcomeMessage = String.Format("" +
                                "[O] CONNECTED:\n\n" +
                                "      Host:     {0}\n" +
                                "      Port:     {1}\n" +
                                "      Username: {2}\n\n" +
                                "~~WELCOME TO THE WILD~~\n", host, port, uUsername);
                            Console.WriteLine(welcomeMessage);
                            new Thread(new ThreadStart(connectedHandler)).Start();

                            break;
                        }

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(String.Format("ERROR [{0}]", ex.ErrorCode));
                }
            }
        }

        static void connectedHandler()
        {

            string message = "";

            while (true)
            {

                message = Console.ReadLine();
                try
                {
                    Send(message);

                    String uText = String.Format("\n[{2}:{3}:{4}][{0}]: {1}",
                        uUsername,
                        message,
                        DateTime.Now.TimeOfDay.Hours,
                        DateTime.Now.TimeOfDay.Minutes,
                        Convert.ToInt32(DateTime.Now.TimeOfDay.Seconds));
                    Console.WriteLine(uText);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR: {0}", ex.Message);
                }
            }
        }
    }
}
