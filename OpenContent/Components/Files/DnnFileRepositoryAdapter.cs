using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.Files
{
    public class DnnFileRepositoryAdapter : IFileRepositoryAdapter
    {
        public T LoadJsonFileFromCacheOrDisk<T>(FileUri file)
        {
            try
            {
                T jsonObject = default(T);
                if (file.FileExists)
                {
                    string cacheKey = file.FilePath;
                    jsonObject = App.Services.CacheAdapter.GetCache<T>(cacheKey);
                    if (jsonObject == null)
                    {
                        string content = File.ReadAllText(file.PhysicalFilePath);
                        jsonObject = JsonConvert.DeserializeObject<T>(content);
                        App.Services.CacheAdapter.SetCache(cacheKey, jsonObject, file.PhysicalFilePath);
                    }
                }
                return jsonObject;
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Failed to load json file {file.FilePath}. Error: {ex}");
                throw;
            }
        }

        public JToken LoadJsonFromCacheOrDisk(FileUri fileUri)
        {
            string cacheKey = fileUri.FilePath;
            var json = App.Services.CacheAdapter.GetCache<JObject>(cacheKey);
            if (json == null)
            {
                var fileContent = ReadFileFromDisk(fileUri);
                json = fileContent.ToJObject($"file [{fileUri.FilePath}]") as JObject;

                if (json != null)
                {
                    App.Services.CacheAdapter.SetCache(cacheKey, json, fileUri.PhysicalFilePath);
                }
            }
            return json;
        }

        public JToken LoadJsonFileFromDisk(string filename)
        {
            if (!File.Exists(filename)) return null;

            JToken json = null;
            string fileContent = File.ReadAllText(filename);
            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                json = fileContent.ToJObject($"file [{filename}]") as JObject;
            }
            return json;
        }

        public static string ReadFileFromDisk(FileUri file)
        {
            if (file == null) return null;
            if (!file.FileExists) return null;
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(file.PhysicalFilePath);
                if (string.IsNullOrWhiteSpace(fileContent)) return null;
            }
            catch (Exception ex)
            {
                var mess = $"Error reading file [{file.FilePath}]";
                App.Services.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
            return fileContent;
        }
    }
}