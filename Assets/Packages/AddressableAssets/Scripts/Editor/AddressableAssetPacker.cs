using UnityEditor.Build.Cache;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityEditor.Build.AssetBundle.DataConverters
{
    public class AddressableAssetPacker : IDataConverter<AddressableAssetEntry[], BuildInput>
    {
        public uint Version { get { return 1; } }

        private Hash128 CalculateInputHash(AddressableAssetEntry[] input)
        {
            return HashingMethods.CalculateMD5Hash(Version, input);
        }

        public bool Convert(AddressableAssetEntry[] input, out BuildInput output, bool useCache = true)
        {
            // If enabled, try loading from cache
            Hash128 hash = new Hash128();
            if (useCache)
            {
                hash = CalculateInputHash(input);
                if(LoadFromCache(hash, out output))
                    return true;
            }
            
            // Convert inputs
            output = new BuildInput();
            output.definitions = new BuildInput.Definition[0];

            if (input.IsNullOrEmpty())
            {
                BuildLogger.Log("Unable to continue packing addressable assets. Input is null or empty.");
                return true;
            }

            var outputDefList = new List<BuildInput.Definition>();

            foreach(var entry in input)
            {
                if(!entry.active)
                    continue;

                var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid.ToString());
                var address = string.IsNullOrEmpty(entry.address) ? assetPath : entry.address;

                var def = new BuildInput.Definition();
                def.assetBundleName = System.IO.Path.GetFileNameWithoutExtension(address) + "_" + entry.guid.ToString();
                def.explicitAssets = new[] { new BuildInput.AddressableAsset() { asset = entry.guid, address = address } };

                outputDefList.Add(def);
            }

            output.definitions = outputDefList.ToArray();
            
            // Cache results
            if (useCache)
                SaveToCache(hash, output);
            return true;
        }

        private bool LoadFromCache(Hash128 hash, out BuildInput output)
        {
            return BuildCache.TryLoadCachedResults(hash, out output);
        }

        private void SaveToCache(Hash128 hash, BuildInput output)
        {
            BuildCache.SaveCachedResults(hash, output);
        }
    }
}
