using System;
using System.Collections.Generic;
using System.Linq;
using MIG.Config;

namespace MIG.Interfaces.HomeAutomation
{
    public class Harmony : MigInterface
    {
        public static string Event_Node_Description = "Harmony Activity";

        HarmonyController controller;
        List<InterfaceModule> interfaceModules = new List<InterfaceModule>();

        public Harmony()
        {
            controller = new HarmonyController();
        }

        #region MIG Interface members

        public event InterfaceModulesChangedEventHandler InterfaceModulesChanged;
        public event InterfacePropertyChangedEventHandler InterfacePropertyChanged;

        public string Domain
        {
            get
            {
                string domain = this.GetType().Namespace.ToString();
                domain = domain.Substring(domain.LastIndexOf(".") + 1) + "." + this.GetType().Name.ToString();
                return domain;
            }
        }

        public List<InterfaceModule> GetModules()
        {
            return interfaceModules;
        }

        public List<Option> Options { get; set; }

        public void OnSetOption(Option option)
        {
            if (IsEnabled)
                Connect();
        }

        public bool IsConnected => controller.IsConnected;

        public bool Connect()
        {
            if (this.Options == null || !this.Options.Any())
            {
                Console.Out.Write("No options found for Harmony");
                OnInterfaceModulesChanged(this.GetDomain());

                return false;
            }

            string username = this.Options.FirstOrDefault(o => o.Name == "Username")?.Value;
            string password = this.Options.FirstOrDefault(o => o.Name == "Password")?.Value;
            string ipAddress = this.Options.FirstOrDefault(o => o.Name == "IPAddress")?.Value;
            
            controller.Connect(username, password, ipAddress);

            var activities = controller.GetActivities();
            
            foreach (var activity in activities)
            {
                interfaceModules.Add(new InterfaceModule
                {
                    Domain = Domain,
                    Address = activity.id,
                    Description = activity.label,
                    ModuleType = ModuleTypes.Switch,
                    CustomData = new { Name = activity.label },
                });
            }
            
            interfaceModules.Add(new InterfaceModule
            {
                Domain = Domain,
                Address = "123456789",
                Description = "RESET Harmony API",
                ModuleType = ModuleTypes.Switch,
            });

            var current = controller.GetCurrentActivity();
            OnInterfacePropertyChanged(this.GetDomain(), current, Event_Node_Description, ModuleEvents.Status_Level, 1);

            return true;
        }

        public void Disconnect()
        {
            //controller.Close();
        }

        public bool IsDevicePresent()
        {
            return true;
        }

        public bool IsEnabled { get; set; }

        public object InterfaceControl(MigInterfaceCommand command)
        {
            string returnValue = "";
            bool raisePropertyChanged = false;
            string parameterPath = "Status.Level";
            string raiseParameter = "";
            switch (command.Command)
            {
                case "Control.On":
                    var oldActivity = controller.StartActivity(command.Address);
                    if (!string.IsNullOrEmpty(oldActivity))
                    {
                        OnInterfacePropertyChanged(this.GetDomain(), oldActivity, Event_Node_Description, parameterPath, "0");
                    }
                    raisePropertyChanged = true;
                    raiseParameter = "1";
                    break;
                case "Control.Off":
                    raisePropertyChanged = true;
                    bool isOff = controller.TurnOff(command.Address);
                    if(isOff)
                        raiseParameter = "0";
                    else
                        returnValue = isOff.ToString();                
                    break;
                case "Control.Toggle":
                    controller.Toggle(command.Address);
                    raisePropertyChanged = true;
                    break;
                case "Control.Reset":
                    controller.Reset();
                    raiseParameter = "Reset";
                    raisePropertyChanged = true;
                    break;
                default:
                    Console.WriteLine("TS:" + command.Command + " | " + command.Address);
                    break;
            }

            if (raisePropertyChanged)
            {
                try
                {
                    OnInterfacePropertyChanged(this.GetDomain(), command.Address, Event_Node_Description, parameterPath, raiseParameter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception catched on OnInterfacePropertyChanged. Message: {0}", ex.Message);
                }
            }

            return returnValue;
        }

        #endregion

        #region Events

        protected virtual void OnInterfaceModulesChanged(string domain)
        {
            if (InterfaceModulesChanged == null) return;

            var args = new InterfaceModulesChangedEventArgs(domain);
            InterfaceModulesChanged(this, args);
        }

        protected virtual void OnInterfacePropertyChanged(string domain, string source, string description, string propertyPath, object propertyValue)
        {
            if (InterfacePropertyChanged == null) return;

            var args = new InterfacePropertyChangedEventArgs(domain, source, description, propertyPath, propertyValue);
            InterfacePropertyChanged(this, args);
        }

        #endregion

        private ModuleTypes GetDeviceType(string protocol)
        {
            if (protocol.IndexOf("dimmer", StringComparison.Ordinal) > -1)
                return ModuleTypes.Dimmer;
            if (protocol.IndexOf("switch", StringComparison.Ordinal) > -1)
                return ModuleTypes.Switch;
            return ModuleTypes.Generic;
        }

        public static class ModuleEvents
        {
            public static string Status_Level =
                "Status.Level";
            public static string Sensor_Temperature =
                "Sensor.Temperature";
            public static string Sensor_Luminance =
                "Sensor.Luminance";
            public static string Sensor_Humidity =
                "Sensor.Humidity";
            public static string Sensor_DoorWindow =
                "Sensor.DoorWindow";
            public static string Sensor_Tamper =
                "Sensor.Tamper";
        }
    }
}
