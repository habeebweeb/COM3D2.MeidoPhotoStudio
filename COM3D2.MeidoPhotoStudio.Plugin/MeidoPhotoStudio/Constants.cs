﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Constants
    {
        public static readonly string customPosePath;
        public static readonly string scenesPath;
        public static readonly string kankyoPath;
        public static readonly string configPath;
        public static readonly int mainWindowID = 765;
        public static readonly int messageWindowID = 961;
        public static readonly int sceneManagerWindowID = 876;
        public static readonly int sceneManagerModalID = 283;
        public static readonly int dropdownWindowID = 777;
        public enum Window
        {
            Call, Pose, Face, BG, BG2, Message, Save, SaveModal
        }
        public enum Scene
        {
            Daily = 3, Edit = 5
        }
        public static readonly List<string> PoseGroupList;
        public static readonly Dictionary<string, List<string>> PoseDict;
        public static readonly Dictionary<string, List<KeyValuePair<string, string>>> CustomPoseDict;
        public static int CustomPoseGroupsIndex { get; private set; }
        public static readonly List<string> FaceBlendList;
        public static readonly List<string> BGList;

        static Constants()
        {
            string modsPath = Path.Combine(Path.GetFullPath(".\\"), @"Mod\MeidoPhotoStudio");
            customPosePath = Path.Combine(modsPath, "Custom Poses");
            scenesPath = Path.Combine(modsPath, "Scenes");
            kankyoPath = Path.Combine(modsPath, "Environments");
            configPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Config\MeidoPhotoStudio");

            PoseDict = new Dictionary<string, List<string>>();
            PoseGroupList = new List<string>();
            CustomPoseDict = new Dictionary<string, List<KeyValuePair<string, string>>>();

            FaceBlendList = new List<string>();

            BGList = new List<string>();
        }

        public static void Initialize()
        {
            foreach (string dir in new[] { customPosePath, scenesPath, kankyoPath, configPath })
            {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }

            // Load Poses
            string poseListJson = File.ReadAllText(Path.Combine(configPath, "mm_pose_list.json"));
            List<SerializePoseList> poseLists = JsonConvert.DeserializeObject<List<SerializePoseList>>(poseListJson);

            foreach (SerializePoseList poseList in poseLists)
            {
                PoseDict[poseList.UIName] = poseList.PoseList;
                PoseGroupList.Add(poseList.UIName);
                CustomPoseGroupsIndex++;
            }

            Action<string> GetPoses = (directory) =>
            {
                List<KeyValuePair<string, string>> poseList = new List<KeyValuePair<string, string>>();
                foreach (string file in Directory.GetFiles(directory))
                {
                    if (Path.GetExtension(file) == ".anm")
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        poseList.Add(new KeyValuePair<string, string>(fileName, file));
                    }
                }
                if (poseList.Count > 0)
                {
                    string poseGroupName = new DirectoryInfo(directory).Name;
                    PoseGroupList.Add(poseGroupName);
                    CustomPoseDict[poseGroupName] = poseList;
                }
            };

            GetPoses(customPosePath);

            // TODO: Get rest of poses

            foreach (string directory in Directory.GetDirectories(customPosePath))
            {
                GetPoses(directory);
            }

            // Load Face Blends Presets
            using (CsvParser csvParser = OpenCsvParser("phot_face_list.nei"))
            {
                for (int cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
                {
                    if (csvParser.IsCellToExistData(3, cell_y))
                    {
                        string blendValue = csvParser.GetCellAsString(3, cell_y);
                        FaceBlendList.Add(blendValue);
                    }
                }
            }

            // Load BGs
            PhotoBGData.Create();
            List<PhotoBGData> photList = PhotoBGData.data;

            // COM3D2 BGs
            foreach (PhotoBGData bgData in photList)
            {
                if (!string.IsNullOrEmpty(bgData.create_prefab_name))
                {
                    string bg = bgData.create_prefab_name;
                    BGList.Add(bg);
                }
            }

            // CM3D2 BGs
            if (GameUty.IsEnabledCompatibilityMode)
            {
                using (CsvParser csvParser = OpenCsvParser("phot_bg_list.nei", GameUty.FileSystemOld))
                {
                    for (int cell_y = 1; cell_y < csvParser.max_cell_y; cell_y++)
                    {
                        if (csvParser.IsCellToExistData(3, cell_y))
                        {
                            string bg = csvParser.GetCellAsString(3, cell_y);
                            BGList.Add(bg);
                        }
                    }
                }
            }
        }

        private static CsvParser OpenCsvParser(string nei, AFileSystemBase fs)
        {
            try
            {
                if (fs.IsExistentFile(nei))
                {
                    AFileBase file = fs.FileOpen(nei);
                    CsvParser csvParser = new CsvParser();
                    if (csvParser.Open(file)) return csvParser;
                }
            }
            catch { }
            return null;
        }

        private static CsvParser OpenCsvParser(string nei)
        {
            return OpenCsvParser(nei, GameUty.FileSystem);
        }

        public class SerializePoseList
        {
            public string UIName { get; set; }
            public List<string> PoseList { get; set; }
        }
    }

    public static class Translation
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;
        public static string CurrentLanguage { get; set; }

        public static void Initialize(string language)
        {
            CurrentLanguage = language;

            string translationFile = $"translations.{language}.json";
            string translationPath = Path.Combine(Constants.configPath, translationFile);
            string translationJson = File.ReadAllText(translationPath);

            JObject translation = JObject.Parse(translationJson);

            Translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (JProperty translationProp in translation.AsJEnumerable())
            {
                JToken token = translationProp.Value;
                Translations[translationProp.Path] = token.ToObject<Dictionary<string, string>>();
            }
        }

        public static string Get(string category, string text)
        {
            if (!Translations.ContainsKey(category))
            {
                Debug.LogWarning($"Could not find category '{category}'");
                return null;
            }

            if (!Translations[category].ContainsKey(text))
            {
                Debug.LogWarning($"Could not find translation for '{text}'");
                return null;
            }
            return Translations[category][text];
        }

        public static string[] GetList(string category, IEnumerable<string> list)
        {
            return list.Select(uiName => Get(category, uiName) ?? uiName).ToArray();
        }

        public static string[] GetList(string category, IEnumerable<KeyValuePair<string, string>> list)
        {
            return list.Select(kvp => Get(category, kvp.Key) ?? kvp.Key).ToArray();
        }
    }
}
