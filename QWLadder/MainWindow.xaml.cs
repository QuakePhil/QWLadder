using Microsoft.Win32; // for UniRegKeyx86 and co
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
// Dispatcher

// should move all the IRC stuff into its own class

namespace QWLadder
{
    /// <summary>
    /// QW Ladder launched implementation, including IRC authentication
    /// </summary>

    public partial class MainWindow : Window
    {
        private List<QuakeClient> clientList;
        private bool loginInProgress;

        private QWLadderRestApi restApi;

        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            InitializeComponent();

            string[] Ladders = new string[4];
            Ladders[0] = "Duel - Hoonymode";
            Ladders[1] = "Duel - Classic";
            Ladders[2] = "2 vs 2";
            Ladders[3] = "4 vs 4";

            foreach (string LadderName in Ladders)
                AvailableLadders.Items.Add(LadderName);

            // todo: this discovery should be managed better?
            DiscoverQuakeClients();
            if (Properties.Settings.Default.PingTimeout != 0)
            {
                PingTimeout.Value = Properties.Settings.Default.PingTimeout;
            }

/*            //oninitialize
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Icon = IconHandles["QuickLaunch"];
            // onloaded
            notifyIcon.Visible = true;
 */

            restApi = new QWLadderRestApi(LoginCallback);
            AttemptAutoLogin();
        }

        private void AttemptAutoLogin()
        {
            if (Properties.Settings.Default.Login != "")
            {
                restApi.username = Properties.Settings.Default.Login.ToString();
                authUsername.Text = restApi.username;
            }
            if (Properties.Settings.Default.Password != "")
            {
                restApi.password = Properties.Settings.Default.Password.ToString();
                authPassword.Password = restApi.password;
            }

            autoLogin.IsChecked = Properties.Settings.Default.AutoLogin;
            if (authUsername.Text == "" && authPassword.Password == "")
                autoLogin.IsChecked = true;

            if ((authUsername.Text != "" || authPassword.Password != "") && (bool)autoLogin.IsChecked)
            {
                CheckLogin();
            }
        }

        private void DiscoverQuakeClients()
        {
            this.clientList = new List<QuakeClient>();

            // search common Registry Keys for exe locations
            RegistryKey HKCU = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\qw\shell\Open\Command");
            string clientPath = (string)HKCU.GetValue("");
            string clientName = "Unknown";
            HKCU.Close();

            if (clientPath.ToLower().Contains("ezquake.exe"))
            {
                clientName = "EZQuake (Software Renderer)";
            }
            else if (clientPath.ToLower().Contains("ezquake-gl.exe"))
            {
                clientName = "EZQuake";
            }

            if (clientName != "Unknown")
            {
                this.clientList.Add(new QuakeClient(clientName, clientPath));
                ClientName.Content = clientName;
                ClientPath.Text = clientPath;
            }
        }

        private void SearchForGames(object sender, RoutedEventArgs e)
        {
            restApi.UploadPings((int)(PingTimeout.Value));

            restApi.PostLadders(AvailableLadders);

            restApi.GetLadderCandidates(LadderCandidates);

            //FileSearch.TraverseTree("C:\\", "ezquake*.exe");
           
            //this.clientList.First().ExecuteQuakeClient();
            //System.Windows.MessageBox.Show("Scan complete");
        }
        
        public void LoginCallback(string message)
        {
            if (this.loginInProgress)
            {
                if (message == "Logged in")
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Action(() => 
                            {
                            Properties.Settings.Default.Login = authUsername.Text;
                            Properties.Settings.Default.Password = authPassword.Password;
                            Properties.Settings.Default.AutoLogin = (bool)autoLogin.IsChecked;
                            Properties.Settings.Default.Save();
                            tabLadders.IsEnabled = true;
                            tabMapPref.IsEnabled = true;
                            tabLogin.Header = "Logged in as " + authUsername.Text.Trim();
                            tabLadders.Focus();
                            })
                        );
                }
                else
                {
                    System.Windows.MessageBox.Show(message);
                    this.loginInProgress = false;
                    //this.loginButton.IsEnabled = true; // todo: make this work when called from ircclient thread
                    // (use delegate? invoke?)
                }
            }
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            if (!this.loginInProgress && authUsername.Text.Length > 0)
                CheckLogin();
        }

        private void CheckLogin()
        {
            this.loginInProgress = true;
            //this.loginButton.IsEnabled = false;

            restApi.username = authUsername.Text;
            restApi.password = authPassword.Password;
            restApi.Login();
        }

        private void PingTimeoutChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PingTimeoutLabel.Content ="Server ping timeout: " + Math.Round(PingTimeout.Value, 0).ToString() + "ms";
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Action(() =>
                            {
                                Properties.Settings.Default.PingTimeout = (int)(PingTimeout.Value);
                                Properties.Settings.Default.Save();
                            }));
        }

    }
}
