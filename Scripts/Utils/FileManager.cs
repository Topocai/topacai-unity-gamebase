using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;

namespace Topacai.Utils.Files
{
    public static class FileManager
    {
        //Get all files with a specified prefix and extension from a directory
        public static List<string> GetFilesByPrefix(string directoryPath, string prefix, string fileExtension = ".json")
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            string searchPattern = $"{prefix}*{fileExtension}"; // Prefix followed by any characters, followed by file extension
            string[] files = Directory.GetFiles(directoryPath, searchPattern);

            return new List<string>(files);
        }

        // Deserealize T class from all files inside the directory.
        public static List<T> DeserializeFiles<T>(List<string> filePaths)
        {
            List<T> deserializedObjects = new List<T>();

            foreach (string filePath in filePaths)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    T obj = JsonConvert.DeserializeObject<T>(jsonContent);
                    deserializedObjects.Add(obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing file {filePath}: {ex.Message}");
                }
            }

            return deserializedObjects;
        }
    }
}