using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace QWLadder
{
    class QWLadderRestApi
    {
        public string username;
        public string password;
        public string access_token;

        public delegate void DelegateLoginCallback(string status);
        private DelegateLoginCallback callback;

        public QWLadderRestApi(DelegateLoginCallback newCallback)
        {
            this.callback = newCallback;
            this.access_token = "";
        }

        // hints: http://technet.rapaport.com/info/lotupload/samplecode/full_example.aspx
        private string GetToken()
        {
            try
            {
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                NameValueCollection formData = new NameValueCollection();
                formData["userName"] = this.username + @"@qwladder.azurewebsites.net";
                formData["password"] = this.password;
                formData["grant_type"] = "password";

                //client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencode");
                var authResponse = client.UploadValues("https://qwladder.azurewebsites.net/token", "POST",
                    formData);
                var jss = new JavaScriptSerializer();
                var authData = jss.Deserialize<Dictionary<string, string>>(
                    Encoding.UTF8.GetString(authResponse));
                this.access_token = authData["access_token"];
                //accessToken = JsonObject.Parse(authResponse)["accessToken"];
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Ok";
        }

        public void PostLadders(System.Windows.Controls.ListBox AvailableLadders)
        {
            // how long does a post last?
            // 
            string ladders = "ladder[0]=delete";
            int i = 1;
            foreach (string ladder in AvailableLadders.SelectedItems)
            {
                ladders = ladders + "&ladder[" + i + "]=" + ladder;
                i++;
            }

            try
            {
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + this.access_token);

                var authResponse = client.UploadData("https://qwladder.azurewebsites.net/api/ladder",
                    "POST",
                    Encoding.UTF8.GetBytes(ladders));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //    return;
            }
            Console.WriteLine(ladders);
        }

        public void GetLadderCandidates(System.Windows.Controls.ListBox LadderCandidates)
        {
            // how long does a post last?
            // 

            try
            {
                string lastLadder = "";
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + this.access_token);

                var candidates = client.DownloadData("https://qwladder.azurewebsites.net/api/ladder");

                var jss = new JavaScriptSerializer();
                List<string[]> serversJson = jss.Deserialize<List<string[]>>(
                    Encoding.UTF8.GetString(candidates));

                LadderCandidates.Items.Clear();
                foreach (string[] server in serversJson) if (lastLadder != server[0])
                {
                    LadderCandidates.Items.Add(server[0] + " at server " + server[1] + " with ping " + server[2] + "ms");
                    lastLadder = server[0]; // only show the first match for each ladder
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //    return;
            }
        }

        public void UploadPings(int timeout)
        {
            Dictionary<string, long> Pings = ServerPinger.CollectPings(this.GetServerList(), timeout);

            if (Pings == null) return;

            int i = 1;
            string pings = "serverip[0]=delete&ping[0]=0";

            foreach (KeyValuePair<string, long> item in Pings)
            {
                pings = pings + "&serverip[" + i + "]=" + item.Key
                    + "&ping[" + i + "]=" + item.Value;
                i++;
            }
            
            try
            {
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + this.access_token);

                var authResponse = client.UploadData("https://qwladder.azurewebsites.net/api/pings", 
                    "POST",
                    Encoding.UTF8.GetBytes(pings));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            //    return;
            }
        }

        public List<string> GetServerList()
        {
            if (this.access_token == "")
                return null;
            try
            {
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;


                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + this.access_token);

                var authResponse = client.DownloadData("https://qwladder.azurewebsites.net/api/serverlist");
                var jss = new JavaScriptSerializer();
                List<string> serversJson = jss.Deserialize<List<string>>(
                    Encoding.UTF8.GetString(authResponse));

                return serversJson;
            }
            catch (Exception)
            {
            }
            return null;
        }


        // hints: http://stackoverflow.com/questions/20705371/error-in-using-webclient-object-rest-api-call-using-c-sharp
        private string Register()
        {
            try
            {
                // todo: use a jsonifier and stop this madness
                string userinfo = "{"
                    + @"""Email"": """ + this.username + @"@qwladder.azurewebsites.net"","
                    + @"""Password"": """ + this.password + @""","
                    + @"""ConfirmPassword"": """ + this.password + @"""}";
                var client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;

                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var content = client.UploadString("https://qwladder.azurewebsites.net/api/Account/register",
                    userinfo);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Ok";
        }

        public void Login()
        {
            string firstTry = GetToken();
            // this will attempt to authenticate the api user with qnet 1 time
            if (firstTry != "Ok")
            {
               string secondTry = Register();

               if (secondTry != "Ok")
                {
                    System.Windows.MessageBox.Show("Press Yes to be redirected to register with Q");
                    System.Diagnostics.Process.Start("https://www.quakenet.org/help/q/how-to-register-an-account-with-q");
                    this.callback("There was a problem");
                }
                else
                {
                    this.callback("Logged in");
                }
            }
            else
            {
                this.callback("Logged in");
            }

        }
    }
}
