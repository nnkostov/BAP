using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace LegoTrainProject
{
    [Serializable]
    public class TrainProject
    {
        /// <summary>
        /// Project file format version for migration support.
        /// </summary>
        public int FormatVersion { get; set; } = 2;

        public List<Hub> RegisteredTrains = new List<Hub>();
        public List<TrainProgram> Programs = new List<TrainProgram>();
        public TrainProgramEvent GlobalCode = new TrainProgramEvent(TrainProgramEvent.EventType.Global_Code);
        public Sections Sections = new Sections();
        public bool ShowSectionProgram = false;

        [NonSerialized]
        [JsonIgnore]
        private string _path = null;

        [JsonIgnore]
        public string Path
        {
            get => _path;
            set => _path = value;
        }

        /// <summary>
        /// Loads a project from file. Supports both JSON and legacy binary formats.
        /// </summary>
        public static TrainProject Load(string path)
        {
            try
            {
                return ProjectSerializer.Load(path);
            }
            catch (ProjectSerializationException ex)
            {
                MainBoard.WriteLine("ERROR - Could not open file: " + ex.Message, Color.Red);
                return null;
            }
            catch (Exception ex)
            {
                MainBoard.WriteLine("ERROR - Could not open file: " + ex.Message, Color.Red);
                return null;
            }
        }

        /// <summary>
        /// Gets a hub by its device ID.
        /// </summary>
        public Hub GetHubByDeviceId(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            for (int i = 0; i < RegisteredTrains.Count; i++)
            {
                if (RegisteredTrains[i].DeviceId == deviceId)
                    return RegisteredTrains[i];
            }

            return null;
        }

        /// <summary>
        /// Gets a hub by its device ID (legacy method name for compatibility).
        /// </summary>
        public Hub GetHubIndexByDeviceId(string deviceId)
        {
            return GetHubByDeviceId(deviceId);
        }

        /// <summary>
        /// Saves the project to its current path.
        /// </summary>
        public bool Save()
        {
            if (Path != null)
            {
                SaveAs(Path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves the project to the specified path using JSON format.
        /// </summary>
        public void SaveAs(string path)
        {
            try
            {
                ProjectSerializer.Save(this, path);
                MainBoard.WriteLine("Project saved successfully.", Color.Green);
            }
            catch (ProjectSerializationException ex)
            {
                MainBoard.WriteLine("ERROR - Could not save file: " + ex.Message, Color.Red);
            }
            catch (Exception ex)
            {
                MainBoard.WriteLine("ERROR - Could not save file: " + ex.Message, Color.Red);
            }
        }

        /// <summary>
        /// Migrates a legacy binary project file to JSON format.
        /// </summary>
        public static void MigrateLegacyProject(string legacyPath, string newPath = null)
        {
            try
            {
                ProjectSerializer.MigrateToJson(legacyPath, newPath);
                MainBoard.WriteLine("Project migrated successfully to JSON format.", Color.Green);
            }
            catch (Exception ex)
            {
                MainBoard.WriteLine("ERROR - Could not migrate project: " + ex.Message, Color.Red);
            }
        }
    }
}
