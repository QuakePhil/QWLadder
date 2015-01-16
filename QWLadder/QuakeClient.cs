using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

namespace QWLadder
{
    class QuakeClient
    {
        public string clientName;
        public string fullPath;

        public QuakeClient(string newClientName, string newFullPath)
        {
            clientName = newClientName;
            fullPath = newFullPath;
        }

        // Opens urls and .html documents using Internet Explorer. 
        public void ExecuteQuakeClient()
        {
            //Process.Start(fullPath.Replace('"',' ')); // ,arguments);
            Process.Start(@"C:\quake\ezquake-gl.exe");
        }
    }
}
