using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Basically.Mapping {
    using Frameworks;

    public static class MapManager {
#if !UNITY_EDITOR
        public const string FOLDER_END = "/../Maps/";
#else
        public const string FOLDER_END = "/Maps/";
#endif
        public const string FILE_EXTENSION = ".bpack";

        static Dictionary<string, Mappack> packs;
        static string currentMap;
        static string currentlyLoading;

        public static event Action BeforeLoad;
        public static event Action AfterLoad;

        public static string CurrentMap => currentMap;
        public static bool Loading => currentlyLoading != string.Empty;
        public static string MapLoading => currentlyLoading;

        public static void Initialize() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            packs = new Dictionary<string, Mappack>();
            currentlyLoading = string.Empty;

            // load all packs or something like that
            var archivedPacks = GetAllPacks();
            
            foreach (var pack in archivedPacks) {
                var name = Path.GetFileNameWithoutExtension(pack);
                var loadedPack = Mappack.Load(pack);

                if (loadedPack == null) {
                    Debug.Log($"Pack {name} failed to load!");
                    continue;
                }

                packs.Add(name, loadedPack);
            }
        }

        public static bool LoadMap(string map) {
            if (!LoadChecks(map, out var pack, out var mapName)) return false;

            BeforeLoad?.Invoke();
            currentlyLoading = map;
            if (pack == null) {
                SceneManager.LoadScene(map);
            } else {
                pack.Load();
                SceneManager.LoadScene(mapName);
            }
            return true;
        }

        public static Coroutine LoadMapAsync(string map) {
            if (!LoadChecks(map, out var pack, out var mapName)) return null;

            BeforeLoad?.Invoke();
            currentlyLoading = map;
            return ILoadMapAsync(pack, pack == null ? map : mapName).Run();
        }

        static IEnumerator ILoadMapAsync(Mappack pack, string map) {
            if (pack != null) yield return pack.LoadAsync();
            SceneManager.LoadSceneAsync(map);
        }

        static bool LoadChecks(string fullName, out Mappack pack, out string mapName) {
            var split = fullName.Split('/');

            if (split.Length == 1) {
                if (Application.CanStreamedLevelBeLoaded(fullName)) {
                    pack = null;
                    mapName = null;
                    return true;
                }
            } else {
                if (packs.TryGetValue(split[0], out var value)) {
                    if (value.Data.maps.Any(x => x == split[0])) {
                        pack = value;
                        mapName = split[1];
                        return true;
                    }
                }
            }

            pack = null;
            mapName = null;
            return false;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            currentMap = currentlyLoading;
            currentlyLoading = string.Empty;

            AfterLoad?.Invoke();
        }

        #region Utility

        public static string Folder => Application.dataPath + FOLDER_END;

        public static string[] GetAllPacks() {
            return Directory.GetFiles(Folder).Where(x => x.EndsWith(FILE_EXTENSION)).ToArray();
        }

        #endregion
    }
}
