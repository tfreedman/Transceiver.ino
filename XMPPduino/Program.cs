using System;
using System.Threading;
using agsXMPP;
using agsXMPP.protocol.client;
using System.IO.Ports;

namespace XMPPduino {
    class Program {
        private static System.IO.Ports.SerialPort serialPort;

        static bool _bWait;
        static bool loggedIn = false;

        private static XmppClientConnection client = null;
        public static XmppClientConnection xmppCon;

        static void Main(string[] args) {
            Console.Title = "XMPPduino";

            xmppCon = new XmppClientConnection();

            System.ComponentModel.IContainer components = new System.ComponentModel.Container();
            serialPort = new System.IO.Ports.SerialPort(components);

            serialPort.PortName = Config.COM_PORT;
            serialPort.BaudRate = Config.Baud_Rate;

            xmppCon = new XmppClientConnection();
            xmppCon.Username = Config.Username;
            xmppCon.Password = Config.Password;
            xmppCon.SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct;
            xmppCon.ConnectServer = "talk.google.com";
            xmppCon.Port = 5222;
            xmppCon.UseStartTLS = true;
            xmppCon.AutoResolveConnectServer = false;
            xmppCon.Show = ShowType.chat;
            xmppCon.Server = Config.Server;

            xmppCon.AutoAgents = false;
            xmppCon.AutoPresence = true;
            xmppCon.AutoRoster = true;

            try {
                xmppCon.OnRosterStart += new ObjectHandler(xmppCon_OnRosterStart);
                xmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
                xmppCon.OnRosterEnd += new ObjectHandler(xmppCon_OnRosterEnd);
                xmppCon.OnPresence += new PresenceHandler(xmppCon_OnPresence);
                xmppCon.OnMessage += new MessageHandler(xmppCon_OnMessage);
                xmppCon.OnLogin += new ObjectHandler(xmppCon_OnLogin);
                xmppCon.Open();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            Wait("Login to server, please wait");
            bool bQuit = false;

            while (!bQuit) {
                string command = Console.ReadLine();
                string[] commands = command.Split(' ');

                switch (commands[0].ToLower()) {
                    case "quit":
                        bQuit = true;
                        break;
                    case "msg":
                        string msg = command.Substring(command.IndexOf(commands[2]));
                        xmppCon.Send(new Message(new Jid(commands[1]), MessageType.chat, msg));
                        break;
                    case "status":
                        switch (commands[1]) {
                            case "online":
                                xmppCon.Show = ShowType.NONE;
                                break;
                            case "away":
                                xmppCon.Show = ShowType.away;
                                break;
                            case "xa":
                                xmppCon.Show = ShowType.xa;
                                break;
                            case "chat":
                                xmppCon.Show = ShowType.chat;
                                break;
                        }
                        string status = command.Substring(command.IndexOf(commands[2]));
                        xmppCon.Status = status;
                        xmppCon.SendMyPresence();
                        break;
                }
            }
            xmppCon.Close();
        }

        private static void Wait(string statusMessage) {
            int i = 0;
            _bWait = true;
            while (_bWait) {
                i++;
                if (i == 60)
                    _bWait = false;
                Thread.Sleep(500);
            }
        }

        static void xmppCon_OnLogin(object sender) {

            Console.WriteLine();
            PrintEvent("Logged in to server");

            serialPort.Open();
            if (!serialPort.IsOpen) {
                Console.WriteLine("Oops");
                return;
            }
            loggedIn = true;

            serialPort.DtrEnable = true;
            serialPort.DataReceived += OnReceived;

            // give it 2 secs to start up the sketch
            System.Threading.Thread.Sleep(2000);
        }

        static void xmppCon_OnRosterEnd(object sender) {
            _bWait = false;
            Console.WriteLine();
            PrintInfo("All contacts received");
        }

        static void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item) {
            PrintInfo(String.Format("Got contact: {0}", item.Jid));
        }

        static void xmppCon_OnRosterStart(object sender) {
            PrintEvent("Getting contacts now");
        }

        static void xmppCon_OnPresence(object sender, Presence pres) {
            PrintInfo(String.Format("Got presence from: {0}", pres.From.ToString()));
            PrintInfo(String.Format("type: {0}", pres.Type.ToString()));
            PrintInfo(String.Format("status: {0}", pres.Status));
            PrintInfo("");
        }

        static void xmppCon_OnMessage(object sender, Message msg) {
            if (msg.Body != null) {
                PrintEvent("XMPP: " + msg.Body);
                serialPort.Write(":" + msg.Body);
            }
        }

        static void PrintEvent(string msg) {
            Console.WriteLine(msg);
        }

        static void PrintInfo(string msg) {
            Console.WriteLine(msg);
        }

        static void PrintHelp(string msg) {
            Console.WriteLine(msg);
        }

        private static void OnReceived(object sender, SerialDataReceivedEventArgs c) {
            try {
                if (loggedIn) {
                    string msg = serialPort.ReadExisting();
                    msg = msg.Replace(Environment.NewLine, "");
                    if (msg.Equals("MBX:Open"))
                        msg = "You've got mail!";

                    xmppCon.Send(new Message(new Jid(Config.Receiver), MessageType.chat, msg));
                    Console.WriteLine("RECV: " + msg);
                }
            }
            catch (Exception exc) { }
        }
    }
}