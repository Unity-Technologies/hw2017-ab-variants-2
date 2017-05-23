using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.AssetBundle.DataConverters;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

public static class AddressableAssetSettingsExtensions
{
    public static bool GenerateBuildInput(this AddressableAssetSettings settings, out BuildInput input)
    {
        return new AddressableAssetPacker().Convert(settings.GetEntries(), out input, false);
    }
}
