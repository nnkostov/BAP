using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace LegoTrainProject.LTP.Core.Devices
{
    [Serializable]
    public class ConnectionLimitationSettings
    {
        private const int CurrentFormatVersion = 1;

        public enum LimitationType
        {
            None = 0,
            OnlyProject = 1,
            OnlySetList = 2
        }

        [JsonProperty]
        public int FormatVersion { get; set; } = CurrentFormatVersion;

        public LimitationType currentLimitation = 0;
        public string setListOfDevices = "90842B04349A" + Environment.NewLine + "78842B043412";

        internal bool IsMacAddressAllowed(ulong bluetoothAddress, TrainProject project)
        {
            if (currentLimitation == LimitationType.None)
                return true;
            else if (currentLimitation == LimitationType.OnlyProject)
            {
                foreach (Hub hub in project.RegisteredTrains)
                {
                    if (hub.BluetoothAddress == bluetoothAddress)
                        return true;
                }
            }
            else
            {
                List<string> listStrLineElements = setListOfDevices.Split(
                                new[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.None).ToList();
                string macAddress = string.Format("{0:X}", bluetoothAddress);

                foreach (string address in listStrLineElements)
                {
                    if (address.Trim(' ') == macAddress)
                        return true;
                }
            }

            return false;
        }

        public static ConnectionLimitationSettings Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);

                    // Try JSON first (new format starts with '{')
                    if (content.TrimStart().StartsWith("{"))
                    {
                        var settings = JsonConvert.DeserializeObject<ConnectionLimitationSettings>(content);
                        MainBoard.WriteLine("Global connection preferences loaded!", System.Drawing.Color.Green);
                        return settings;
                    }

                    // Fall back to legacy binary format
                    var legacySettings = LoadLegacyFormat(path);
                    if (legacySettings != null)
                    {
                        // Auto-migrate to JSON format
                        legacySettings.SaveAs(path);
                        MainBoard.WriteLine("Connection preferences migrated to JSON format.", System.Drawing.Color.Green);
                        return legacySettings;
                    }
                }

                MainBoard.WriteLine("No global connection preferences file exist. Creating default ones.");
                return new ConnectionLimitationSettings();
            }
            catch (Exception ex)
            {
                MainBoard.WriteLine("ERROR - Could not open file: " + ex.Message, System.Drawing.Color.Red);
                return new ConnectionLimitationSettings();
            }
        }

#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
        private static ConnectionLimitationSettings LoadLegacyFormat(string path)
        {
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    return (ConnectionLimitationSettings)formatter.Deserialize(stream);
                }
            }
            catch
            {
                return null;
            }
        }
#pragma warning restore SYSLIB0011

        public void SaveAs(string path)
        {
            try
            {
                FormatVersion = CurrentFormatVersion;
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                MainBoard.WriteLine("ERROR - Could not save file: " + ex.Message, System.Drawing.Color.Red);
            }
        }
    }
}
