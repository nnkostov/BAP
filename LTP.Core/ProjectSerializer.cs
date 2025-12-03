using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LegoTrainProject
{
    /// <summary>
    /// Handles project serialization with JSON format.
    /// Provides migration path from legacy BinaryFormatter format.
    /// </summary>
    public static class ProjectSerializer
    {
        private const string JSON_FILE_EXTENSION = ".bapx";
        private const string LEGACY_FILE_EXTENSION = ".bap";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new NonSerializedContractResolver()
        };

        /// <summary>
        /// Saves a project to JSON format.
        /// </summary>
        public static void Save(TrainProject project, string path)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            try
            {
                string json = JsonConvert.SerializeObject(project, Settings);
                File.WriteAllText(path, json);
                project.Path = path;
            }
            catch (Exception ex)
            {
                throw new ProjectSerializationException($"Failed to save project to {path}", ex);
            }
        }

        /// <summary>
        /// Loads a project from file. Automatically detects format (JSON or legacy binary).
        /// </summary>
        public static TrainProject Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("Project file not found", path);

            try
            {
                // Try JSON format first
                if (IsJsonFormat(path))
                {
                    return LoadFromJson(path);
                }

                // Fall back to legacy binary format
                return LoadFromLegacyBinary(path);
            }
            catch (ProjectSerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ProjectSerializationException($"Failed to load project from {path}", ex);
            }
        }

        /// <summary>
        /// Loads a project from JSON format.
        /// </summary>
        private static TrainProject LoadFromJson(string path)
        {
            string json = File.ReadAllText(path);
            var project = JsonConvert.DeserializeObject<TrainProject>(json, Settings);

            if (project == null)
                throw new ProjectSerializationException("Deserialized project was null");

            project.Path = path;
            CleanupLoadedProject(project);

            return project;
        }

        /// <summary>
        /// Loads a project from legacy BinaryFormatter format.
        /// </summary>
        [Obsolete("BinaryFormatter is deprecated. Use JSON format for new projects.")]
        private static TrainProject LoadFromLegacyBinary(string path)
        {
            IFormatter formatter = new BinaryFormatter();

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (stream.Length == 0)
                    throw new ProjectSerializationException("Project file is empty");

                var project = (TrainProject)formatter.Deserialize(stream);
                project.Path = path;
                CleanupLoadedProject(project);

                return project;
            }
        }

        /// <summary>
        /// Migrates a legacy binary project file to JSON format.
        /// </summary>
        public static void MigrateToJson(string legacyPath, string newPath = null)
        {
            if (string.IsNullOrWhiteSpace(legacyPath))
                throw new ArgumentException("Legacy path cannot be null or empty", nameof(legacyPath));

            var project = LoadFromLegacyBinary(legacyPath);

            newPath = newPath ?? System.IO.Path.ChangeExtension(legacyPath, JSON_FILE_EXTENSION);
            Save(project, newPath);
        }

        /// <summary>
        /// Checks if a file is in JSON format by reading the first character.
        /// </summary>
        private static bool IsJsonFormat(string path)
        {
            try
            {
                using (var reader = new StreamReader(path))
                {
                    int firstChar = reader.Peek();
                    // JSON files typically start with { or [
                    return firstChar == '{' || firstChar == '[';
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cleans up a loaded project by clearing transient event handlers.
        /// </summary>
        private static void CleanupLoadedProject(TrainProject project)
        {
            if (project.RegisteredTrains != null)
            {
                foreach (Hub train in project.RegisteredTrains)
                {
                    train.CleanAllEvents();
                }
            }
        }
    }

    /// <summary>
    /// Custom contract resolver that respects [NonSerialized] attribute for JSON serialization.
    /// </summary>
    internal class NonSerializedContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Check for NonSerialized attribute on fields
            if (member is System.Reflection.FieldInfo fieldInfo)
            {
                if (Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute)))
                {
                    property.ShouldSerialize = _ => false;
                }
            }

            // Check for field: NonSerialized on events (used in Hub.cs)
            if (member.Name.EndsWith("Triggered") || member.Name.EndsWith("Updated"))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }

    /// <summary>
    /// Exception thrown when project serialization fails.
    /// </summary>
    public class ProjectSerializationException : Exception
    {
        public ProjectSerializationException(string message) : base(message) { }
        public ProjectSerializationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
