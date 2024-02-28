using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Runtime.Data
{
    public static class Settings
    {
        private static Dictionary<string, object> data;

        public static T Index<T>(string key, T fallback = default)
        {
            var settings = GetOrLoad();
            if (!settings.ContainsKey(key)) settings.Add(key, fallback);
            return settings[key] is T entry ? entry : fallback;
        }
        
        private static Dictionary<string, object> GetOrLoad() => data ?? LoadFromFile();

        public static string Filename => Path.Combine(Application.dataPath, "settings.xml");

#if UNITY_EDITOR
        [MenuItem("Data/Settings/Force Load")]
        private static void LoadFromFileMenu() => LoadFromFile();

        [MenuItem("Data/Settings/Force Save")]
        public static void SaveMenu() => Save();
#endif
        
        private static Dictionary<string, object> LoadFromFile()
        {
            data = new Dictionary<string, object>();
            if (File.Exists(Filename))
            {
                using var fs = new FileStream(Filename, FileMode.Open);
                var element = XElement.Load(fs);
                foreach (var pair in element.Elements())
                {
                    if (bool.TryParse(pair.Value, out var v0)) data.Add(pair.Name.LocalName, v0);
                    else if (float.TryParse(pair.Value, out var v1)) data.Add(pair.Name.LocalName, v1);
                    else if (int.TryParse(pair.Value, out var v2)) data.Add(pair.Name.LocalName, v2);
                    else data.Add(pair.Name.LocalName, pair.Value);
                }
                
                Debug.Log($"Loaded Settings from \"{Filename}\"");
            }
            else
            {
                Debug.Log($"No Settings File found at \"{Filename}\"");
                Save();
            }

            return data;
        }

        public static void Save()
        {
            if (data == null) return;

            var element = new XElement("root", data.Select(pair => new XElement(pair.Key, pair.Value)));
            using var fs = new FileStream(Filename, FileMode.OpenOrCreate);
            element.Save(fs);
            
            Debug.Log($"Saved Settings to \"{Filename}\"");
        }
    }
}