using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Xml;

namespace QWLadder
{
    class ServerPinger
    {
        public static void GetServerList()
        {
            try
            {
                //MessageBox.Show("Getting server list");

                XmlDocument serverlist = new XmlDocument();
                serverlist.XmlResolver = null;
                serverlist.Load("http://localhost:54182/api/serverlist");

                foreach (XmlNode server in serverlist.SelectNodes("/rss/channel"))
                {
                    string title = server["title"].InnerText;
                    string ip = server["ip"].InnerText;
                    string port = server["port"].InnerText;
                    Console.WriteLine("Server: #{title} #{ip} #{port}");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
