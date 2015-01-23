using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

using System.Windows;
using System.Xml;

namespace QWLadder
{
    class ServerPinger
    {
        private static long CheckPing(string ip, int timeout)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128, 
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted. 
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            PingReply reply = pingSender.Send(ip, timeout, buffer, options);
            //Console.Write(ip);
            if (reply.Status == IPStatus.Success)
            {
                //Console.WriteLine("  {0}", reply.RoundtripTime);
                //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                return reply.RoundtripTime;
            }
            else
            {
                //Console.WriteLine(" timeout");
                return -1;
            }

        }

        public static Dictionary<string, long> CollectPings(List<string> serverlist, int timeout)
        {
            if (serverlist == null)
                return null;

            Dictionary<string, long> Pings = new Dictionary<string, long>();

            foreach(string serverip in serverlist)
            {
                long serverping = CheckPing(serverip, timeout);
                if (serverping != -1)
                {
                    Pings.Add(serverip, serverping);
                }
            }
            return Pings;
        }
    }
}
