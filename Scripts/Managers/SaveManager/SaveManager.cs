using System;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json;

namespace Topacai.Utils.SaveSystem
{
    public class SaveManager
    {
        private static string _savePath;
        private static string _profilesFileName;

        private static List<UserProfile> _profiles = new List<UserProfile>();

        /// <summary>
        /// Create paths for file and subfolder given a filename and a user profile
        /// and checks if them already exists
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="fileName"></param>
        /// <param name="subFolder"></param>
        /// <returns>
        /// directory = directory exists
        /// file = file exists
        /// directoryPath = the path where the file must be.
        /// filePath = direct path to the file.
        /// </returns>
        private static (bool directory, bool file, string directoryPath, string filePath) CheckDirectoryAndFile(UserProfile profile, string fileName, string subFolder = "")
        {
            string path = subFolder == "" ? Application.dataPath + _savePath + $"/{profile.ID}" : Application.dataPath + _savePath + $"/{profile.ID}/{subFolder}";

            return (
                Directory.Exists(path),
                File.Exists($"{path}/{fileName}"),
                path,
                path + $"/{fileName}"
            );
        }

        /// <summary>
        /// Checks if the recovered saved data contains the desired Data Type saved.
        /// </summary>
        /// <typeparam name="T">Expected Type</typeparam>
        /// <param name="expextedData">Use just to check the type</param>
        /// <param name="deserializedData">Deserealized SavedData struct</param>

        private static bool IsSameDataType<T>(T expextedData, SavedData deserializedData)
        {
            return typeof(T).FullName == deserializedData.ObjectType;
        }

        /// <summary>
        /// Checks if the recovered saved data contains the desired Data Type saved.
        /// </summary>
        /// <typeparam name="T">Expected Type</typeparam>
        /// <param name="deserializedData">Deserealized SavedData struct</param>
        private static bool IsSameDataType<T>(SavedData deserializedData)
        {
            return typeof(T).FullName == deserializedData.ObjectType;
        }

        /// <summary>
        /// Setup the savemanager system values with path saving data and others
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="profilesFileName"></param>
        public static void SetPaths(string savePath, string profilesFileName) { _savePath = savePath; _profilesFileName = profilesFileName; }

        /// <summary>
        /// Search, read and get profiles saved in data (or update profiles info)
        /// </summary>
        public static void GetProfiles()
        {
            if (!File.Exists(Application.dataPath + _savePath + _profilesFileName))
            {
                return;
            }

            _profiles = JsonConvert.DeserializeObject<List<UserProfile>>(File.ReadAllText(Application.dataPath + _savePath + _profilesFileName));
        }

        /// <summary>
        /// Serialize and save a user game profile and add to the current profiles data.
        /// </summary>
        public static void SaveProfile(UserProfile profile)
        {
            if (_profiles == null) GetProfiles();

            int profileIndex = _profiles.BinarySearch(profile);

            if (profileIndex < 0)
            {
                _profiles.Add(profile);
            }

            if (!Directory.Exists(Application.dataPath + _savePath + $"/{profile.ID}"))
            {
                Directory.CreateDirectory(Application.dataPath + _savePath + $"/{profile.ID}");
            }

            var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
            File.WriteAllText(Application.dataPath + _savePath + _profilesFileName, json);
        }

        /// <summary>
        /// Create a game user profile by name and save it to the game data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timePlayed"></param>
        /// <returns>The created profile</returns>
        public static UserProfile CreateProfile(string name, int timePlayed = 0)
        {
#if UNITY_EDITOR
            Debug.Log("Creating Profile" + name);
#endif
            var profile = new UserProfile(
                Guid.NewGuid().ToString(),
                name,
                timePlayed
                );
#if UNITY_EDITOR
            Debug.Log($"Created Profile {profile.Name} with id {profile.ID}");
#endif

            SaveProfile(profile);

            return profile;
        }

        /// <summary>
        /// Get a specific data from a game user profile searching by an *unique* name and stores the recovered data in out parameter.
        /// </summary>
        /// <typeparam name="T">Expected data type to get</typeparam>
        /// <param name="profile">Profile where search the data</param>
        /// <param name="fileName">Unique name of saved data</param>
        /// <param name="data">Variable to store the data</param>
        /// <param name="subFolder">(optional) subfolder path to search data</param>
        public static void GetProfileData<T>(UserProfile profile, string fileName, out T data, string subFolder = "")
        {
            var info = CheckDirectoryAndFile(profile, fileName, subFolder);

            if (!info.directory || !info.file)
            {
                data = default(T);
                throw new Exception($"Reading data ERROR! File {fileName} not found for profile {profile.Name}");
            }

            var json = File.ReadAllText(info.filePath);
            var savedData = JsonConvert.DeserializeObject<SavedData>(json);

            if (!IsSameDataType<T>(savedData))
            {
                data = default(T);
                throw new Exception($"Reading data ERROR! File {fileName} is not of type {typeof(T).FullName}");
            }

            data = (T)savedData.Data;
        }

        /// <summary>
        /// Save data in profile with a custom data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="profile"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="subFolder"></param>
        public static void SaveProfileData<T>(UserProfile profile, string fileName, T data, string subFolder = "")
        {
            var info = CheckDirectoryAndFile(profile, fileName, subFolder);

            if (!info.directory)
            {
                Directory.CreateDirectory(info.directoryPath);
            }

            if (info.file)
            {
                var existingData = JsonConvert.DeserializeObject<SavedData>(File.ReadAllText(info.filePath));

                if (!IsSameDataType(data, existingData))
                {
                    throw new Exception($"Saving data ERROR! File {fileName} is not of type {typeof(T).FullName}");
                }
            }

            var saveData = new SavedData()
            {
                ObjectType = typeof(T).FullName,
                Data = (object)data
            };

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(info.filePath, json);
        }
    }

    public struct SavedData
    {
        public string ObjectType;
        public object Data;
    }

    public struct UserProfile : IComparable
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public int TimePlayed { get; private set; }

        public UserProfile(string id, string name, int timePlayed)
        {
            ID = id;
            Name = name;
            TimePlayed = timePlayed;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            return ID.CompareTo(((UserProfile)obj).ID);
        }
    }
}
