using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RDC.Plugins.ChromeZone.Core
{
    public static class Settings
    {
        public static Objects.SettingsWrapper SettingsWrapper { get; set; }

        public static bool LoadSettings()
        {
            var value = System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"];
            var SettingsFile = Path.Combine(value, @"ChromeZoneSettings.json");

            if (!File.Exists(SettingsFile))
            {
                return false;
            }

            try
            {
                SettingsWrapper = JsonConvert.DeserializeObject<Objects.SettingsWrapper>(File.ReadAllText(SettingsFile));
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

    }
}
