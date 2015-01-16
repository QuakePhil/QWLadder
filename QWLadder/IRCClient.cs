using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Threading;

namespace QWLadder
{
    class IRCClient
    {
        public string username;
        public string password;
        public bool loginInProgress;

        private System.Net.Sockets.TcpClient socket;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        private System.Windows.Controls.Primitives.TextBoxBase loggingControl;

        public delegate void DelegateLoginCallback(string status);
        private DelegateLoginCallback callback;

        public IRCClient(System.Windows.Controls.Primitives.TextBoxBase newLogControl,
            DelegateLoginCallback newCallback)
        {
            this.loggingControl = newLogControl;
            this.loginInProgress = false;
            this.callback = newCallback;
        }

        private void Write(string msg) //, NetworkStream serverStream)
        {
            //byte[] outStream = System.Text.Encoding.ASCII.GetBytes(msg);
            //serverStream.Write(outStream, 0, outStream.Length);
            //serverStream.Flush();
            Console.WriteLine("SEND: " + msg);
            this.writer.WriteLine(msg + "\r\n");
            this.writer.Flush();
        }

        private void LogEvent(string eventMessage, int style = 0)
        {
            /*if (style == 0)
                this.loggingControl.AppendText(eventMessage + "\r\n");
            else
            {
                //this.loggingControl.SelectionStart = IRClines.TextLength;
                //this.loggingControl.SelectionLength = 0;
                this.loggingControl.AppendText(eventMessage + "\r\n");
            }*/
            Console.WriteLine(eventMessage);
        }

        private void ClientLoop()
        {
            string guestNickname = "Guest" + RandomString(8);

            Write("NICK " + guestNickname);
            Write("PASS *");
            Write("USER guest 8 * :\"" + guestNickname + "\"");

            //try
            {
                while (this.socket != null && this.socket.Connected)
                {
                    var line = this.reader.ReadLine();
                    if (line == null)
                        break;
                    Console.WriteLine(line);// LogEvent(line);

                    if (line.StartsWith("PING"))
                    {
                        Write(line.Replace("PING", "PONG"));
                    }
                    if (line.ToLower().Contains("welcome") && line.ToLower().Contains("quakenet") && line.ToLower().Contains("network"))
                    {
                        Write("PRIVMSG Q@CServe.quakenet.org :AUTH " + this.username + " " + this.password);
                    }
                    if (line.ToLower().Contains("username") && line.ToLower().Contains("password") && line.ToLower().Contains("incorrect"))
                    {
                        // todo: cleanly close thread here (and elsewhere? - or maybe give the thread a ttl of, say, 10 seconds?)
                        Console.WriteLine("Login failed");
                        this.callback("Login failed");
                    }

                    if (line.Trim() == ":Q!TheQBot@CServe.quakenet.org NOTICE " + guestNickname
                        + " :You are now logged in as " + this.username + ".")
                    {
                        this.callback("Logged in");
                    }
                }
                if (!this.socket.Connected)
                {
                    this.socket.Close();
                    this.stream.Close();
                    Console.WriteLine("Disconnected");
                }
                /*                
                            IRCWrite(authUsername.Text, serverStream);

                            Console.WriteLine(IRCRead(serverStream));

                            string cmd = "/msg Q@CServe.quakenet.org AUTH " + authUsername.Text + " " + authPassword.Password;
                            IRCWrite(cmd, serverStream);
                            Console.WriteLine(cmd);
                            Console.WriteLine(IRCRead(serverStream));

                            clientSocket.Close();
                            serverStream.Close();*/
            }
            //catch (Exception)
            {
            }
        }

        private string RandomString(int size)
        {
            Random rand = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = chars[rand.Next(chars.Length)];
            }
            return new string(buffer);
        }
        /*Try to remember login and password - none found - show login page
Try to login automatically to IRC - no success - show alert, and show login page
when login is executed, two things are saved:
* checkbox "login auto"
* login/pass info
*/

        public void Login()
        {
            /*
            // more info about settings here: http://msdn.microsoft.com/en-us/library/aa730869%28VS.80%29.aspx

            Properties.Settings.Default.Login = authUsername.Text;
            Properties.Settings.Default.Password = authPassword.Password;
            Properties.Settings.Default.AutoLogin = (bool)autoLogin.IsChecked;
            Properties.Settings.Default.Save();

            tabLadders.IsEnabled = true;
            tabMapPref.IsEnabled = true;
            tabLogin.Header = "Logged in as " + authUsername.Text.Trim();
            tabLadders.Focus();*/

            this.socket = new System.Net.Sockets.TcpClient();
            this.stream = default(NetworkStream);

            this.socket.Connect("irc.quakenet.org", 6667);
            this.stream = this.socket.GetStream();

            this.writer = new StreamWriter(this.stream, Encoding.Default);
            this.reader = new StreamReader(this.stream, Encoding.Default);
            
            new Thread(ClientLoop).Start();
            //ClientLoop(); // figure out threads, in particular how to enable buttons after call stack:
            // external code -> calls
            // ClientLoop ->
            // LoginCallback (wpf controls not easily accessible?)


        }
    }
}
