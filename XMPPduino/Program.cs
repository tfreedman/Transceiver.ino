using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using System.IO.Ports;

namespace XMPPduino
{
    class Program
    {
        private static System.IO.Ports.SerialPort serialPort1;

        static bool _bWait;
        private static XmppClientConnection client = null;

        static void Main(string[] args)
        {
            XmppClientConnection xmppCon = new XmppClientConnection();

            Console.Title = "Console Client";

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
 
            try
            {
                xmppCon.OnRosterStart += new ObjectHandler(xmppCon_OnRosterStart);
                xmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
                xmppCon.OnRosterEnd += new ObjectHandler(xmppCon_OnRosterEnd);
                xmppCon.OnPresence += new PresenceHandler(xmppCon_OnPresence);
                xmppCon.OnMessage += new MessageHandler(xmppCon_OnMessage);
                xmppCon.OnLogin += new ObjectHandler(xmppCon_OnLogin);

                xmppCon.Open();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Wait("Login to server, please wait");

            PrintCommands();

            bool bQuit = false;

            while (!bQuit)
            {
                string command = Console.ReadLine();
                string[] commands = command.Split(' ');

                switch (commands[0].ToLower())
                {
                    case "help":
                        PrintCommands();
                        break;
                    case "quit":
                        bQuit = true;
                        break;
                    case "msg":
                        string msg = command.Substring(command.IndexOf(commands[2]));
                        xmppCon.Send(new Message(new Jid(commands[1]), MessageType.chat, msg));
                        break;
                    case "status":
                        switch (commands[1])
                        {
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

            // close connection
            xmppCon.Close();
        }

        private static void PrintCommands()
        {
            PrintHelp("You are logged in to the server now.");
            PrintHelp("");
            PrintHelp("Available commands are:");
            PrintHelp("msg toJid text");
            PrintHelp("status show{online, away, xa, chat} status");
            PrintHelp("help");
            PrintHelp("quit");
            PrintHelp("");
            PrintHelp("Examples:");
            PrintHelp("msg test@server.com Hello World");
            PrintHelp("msg test@server.com/Office Hello World");
            PrintHelp("status chat free for chat");
            PrintHelp("");
        }

        private static void Wait(string statusMessage)
        {
            int i = 0;
            _bWait = true;

            while (_bWait)
            {
                i++;
                if (i == 60)
                    _bWait = false;

                Thread.Sleep(500);
            }
        }

        static void xmppCon_OnLogin(object sender)
        {
            Console.WriteLine();
            PrintEvent("Logged in to server");
        }

        static void xmppCon_OnRosterEnd(object sender)
        {
            _bWait = false;
            Console.WriteLine();
            PrintInfo("All contacts received");
        }

        static void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            PrintInfo(String.Format("Got contact: {0}", item.Jid));
        }

        static void xmppCon_OnRosterStart(object sender)
        {
            PrintEvent("Getting contacts now");
        }

        static void xmppCon_OnPresence(object sender, Presence pres)
        {
            PrintInfo(String.Format("Got presence from: {0}", pres.From.ToString()));
            PrintInfo(String.Format("type: {0}", pres.Type.ToString()));
            PrintInfo(String.Format("status: {0}", pres.Status));
            PrintInfo("");
        }

        static void xmppCon_OnMessage(object sender, Message msg)
        {

            if (msg.Body != null)
            {
                PrintEvent(String.Format("Got message from: {0}", msg.From.ToString()));
                PrintEvent("message: " + msg.Body);
                PrintInfo("");
            }
        }

        static void PrintEvent(string msg)
        {
            ConsoleColor current = Console.BackgroundColor;

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(msg);

            Console.BackgroundColor = current;
        }

        static void PrintInfo(string msg)
        {
            ConsoleColor current = Console.BackgroundColor;

            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);

            Console.BackgroundColor = current;
        }

        static void PrintHelp(string msg)
        {
            ConsoleColor current = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);

            Console.ForegroundColor = current;
        }

        //static void Main(string[] args)
        //{

        //System.ComponentModel.IContainer components = new System.ComponentModel.Container();
        //serialPort1 = new System.IO.Ports.SerialPort(components);
        //serialPort1.PortName = "COM3";
        //serialPort1.BaudRate = 115200;

        //serialPort1.Open();
        //if (!serialPort1.IsOpen)
        //{
        //    Console.WriteLine("Oops");
        //    return;
        //}

        //// this turns on !
        //serialPort1.DtrEnable = true;

        //// callback for text coming back from the arduino
        //serialPort1.DataReceived += OnReceived;

        //// give it 2 secs to start up the sketch
        //System.Threading.Thread.Sleep(2000);

        //using (serialPort1)
        //{
        //    Console.WriteLine("sending green command");
        //    serialPort1.Write(new byte[] { 114 }, 0, 1);
        //    System.Threading.Thread.Sleep(1000);

        //    Console.WriteLine("sending red command");
        //    serialPort1.Write(new byte[] { 103 }, 0, 1);

        //    System.Threading.Thread.Sleep(15000);
        //}
        //}

        //private static void OnReceived(object sender, SerialDataReceivedEventArgs c)
        //{
        //    int ch;
        //    try
        //    {
        //        // write out text coming back from the arduino
        //        Console.Write(serialPort1.ReadExisting());
        //    }
        //    catch (Exception exc) { }
        //}
    }
}
