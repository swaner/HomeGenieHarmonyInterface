using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using HarmonyHub;

namespace MIG.Interfaces
{
    public class HarmonyController
    {
        private HarmonyClient client;
        private string sessionToken;
        private bool alwaysReset = false;

        public bool IsConnected { get; set; }

        public HarmonyController()
        {
            this.alwaysReset = File.Exists("HarmonyAlwaysReset");
        }

        public bool Connect(string username, string password, string ipAddress)
        {
            this.username = username;
            this.password = password;
            this.ipAddress = ipAddress;

            this.Connect();
            return true;
        }

        private void Connect()
        {
            const int harmonyPort = 5222;
            if (File.Exists("SessionToken"))
            {
                sessionToken = File.ReadAllText("SessionToken");
                Console.WriteLine("Reusing token: {0}", sessionToken);
            }
            else
            {
                sessionToken = LoginToLogitech(username, password, ipAddress, harmonyPort);
            }

            client = new HarmonyClient(ipAddress, harmonyPort, sessionToken);
            IsConnected = true;
        }

        public static string LoginToLogitech(string email, string password, string ipAddress, int harmonyPort)
        {
            string userAuthToken = HarmonyLogin.GetUserAuthToken(email, password);
            if (string.IsNullOrEmpty(userAuthToken))
            {
                throw new Exception("Could not get token from Logitech server.");
            }

            File.WriteAllText("UserAuthToken", userAuthToken);

            var authentication = new HarmonyAuthenticationClient(ipAddress, harmonyPort);

            string sessionToken = authentication.SwapAuthToken(userAuthToken);
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new Exception("Could not swap token on Harmony Hub.");
            }

            File.WriteAllText("SessionToken", sessionToken);

            Console.WriteLine("Date Time : {0}", DateTime.Now);
            Console.WriteLine("User Token: {0}", userAuthToken);
            Console.WriteLine("Sess Token: {0}", sessionToken);

            return sessionToken;
        }

        public IEnumerable<Activity> GetActivities()
        {
            return Config.activity;
        }

        private HarmonyConfigResult _config;
        private string ipAddress;
        private string password;
        private string username;

        public HarmonyConfigResult Config
        {
            get
            {
                if (_config == null)
                { 
                    client.GetConfig();
                    while (string.IsNullOrEmpty(client.Config)) { }
                    File.WriteAllText("HubConfig", client.Config);
                    HarmonyConfigResult harmonyConfig = new JavaScriptSerializer().Deserialize<HarmonyConfigResult>(client.Config);
                    _config = harmonyConfig;
                }

                return _config;
            }
        }

        public string GetCurrentActivity()
        {
            client.GetCurrentActivity();
            // now wait for it to be populated
            while (string.IsNullOrEmpty(client.CurrentActivity)) { }
            return client.CurrentActivity;
        }

        public string StartActivity(string address)
        {
            if(alwaysReset)
            { 
                this.Reset();
            }
            else
            {
                if (address == "123456789")
                {
                    Reset();
                }
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            string current = GetCurrentActivity();
            watch.Stop();
            if (watch.Elapsed.Seconds > 4 && !alwaysReset)
                this.Reset();

            client.StartActivity(address);
            
            return current;
        }

        public void Reset()
        {
            _config = null;
            File.Delete("SessionToken");
            Connect();
        }

        public bool TurnOff(string address)
        {
            string current = GetCurrentActivity();
            if (current != address)
                return false;

            client.TurnOff();
            return true;
        }

        public void Toggle(string address)
        {
            if(!TurnOff(address))
                StartActivity(address);
        }
    }
}
