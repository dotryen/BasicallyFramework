using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Basically.Mapping {
    using Frameworks;

    public class Mappack {
        public readonly string name;
        public readonly ZipArchive file;

        private PackData data;
        private bool loaded;
        
        AssetBundle assets;
        AssetBundle maps;

        public PackData Data => data;
        public bool Loaded => loaded;

        private Mappack() {

        }

        private Mappack(ZipArchive zip, PackData data) {
            file = zip;
            this.data = data;

            loaded = false;
        }

        public bool Load() {
            if (loaded) return true;
            if (!GetBundleStreams(out var asset, out var map)) return false;

            assets = AssetBundle.LoadFromStream(asset);
            maps = AssetBundle.LoadFromStream(map);

            loaded = true;
            return true;
        }

        public Coroutine LoadAsync() {
            if (loaded) return null;
            if (!GetBundleStreams(out var asset, out var map)) return null;
            return ILoadAsync(asset, map).Run();
        }

        public void Unload() {
            if (!loaded) return;
            maps.Unload(true);
            assets.Unload(true);
            loaded = false;
        }

        IEnumerator ILoadAsync(Stream asset, Stream map) {
            var asyncAsset = AssetBundle.LoadFromStreamAsync(asset);
            var asyncMap = AssetBundle.LoadFromStreamAsync(map);

            if (!asyncAsset.isDone || !asyncMap.isDone) yield return null;

            assets = asyncAsset.assetBundle;
            maps = asyncMap.assetBundle;
            loaded = true;
        }

        bool GetBundleStreams(out Stream asset, out Stream map) {
            var assetEntry = file.GetEntry("asset");
            var mapsEntry = file.GetEntry("maps");

            if (assetEntry == null || mapsEntry == null) {
                asset = null;
                map = null;
                return false;
            }

            asset = assetEntry.Open();
            map = mapsEntry.Open();
            return true;
        }

        // STATIC

        public static Mappack Load(string fileLocation) {
            var file = ZipFile.OpenRead(fileLocation);
            var entry = file.GetEntry("manifest");
            if (entry == null) return null;

            byte[] manifest = entry.ToBytes();
            
            // read pack data
            PackData data = default;
            int offset = 0;
            data.name = ReadString(manifest, ref offset);
            data.description = ReadString(manifest, ref offset);
            data.author = ReadString(manifest, ref offset);

            var mapCount = BitConverter.ToInt32(manifest, offset);
            offset += 4;

            data.maps = new string[mapCount];
            for (int i = 0; i < mapCount; i++) {
                data.maps[i] = ReadString(manifest, ref offset);
            }

            return new Mappack(file, data);
        }

        static string ReadString(byte[] buffer, ref int offset) {
            var count = BitConverter.ToInt32(buffer, offset);
            offset += 4;

            var result = Encoding.UTF8.GetString(buffer, offset, count);
            offset += count;
            return result;
        }
    }

    public static class ZipArchiveExtensions {
        public static byte[] ToBytes(this ZipArchiveEntry entry) {
            var buffer = new byte[entry.Length];
            entry.Open().Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
