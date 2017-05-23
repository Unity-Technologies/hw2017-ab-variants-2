using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Experimental.Build.AssetBundle;
using System.Linq;

[InitializeOnLoad]
public class MyBuildProcess
{
    static MyBuildProcess()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerDelegate);
    }

    public static void BuildPlayerDelegate(BuildPlayerOptions options)
    {
        BuildPipeline.BuildPlayer(options);
        BuildAssetBundles();
    }

    public static string bundleBuildPath
    {
        get
        {
            // [project folder]/AssetBundles
            var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AssetBundles");

            // e.g. [project folder]/AssetBundles/Android
            return Path.Combine(path, EditorUserBuildSettings.activeBuildTarget.ToString());
        }
    }

    public static string streamingAssetsBundlePath
    {
        get
        {
            var path = Path.Combine(Application.streamingAssetsPath, "bundles");
            return Path.Combine(path, EditorUserBuildSettings.activeBuildTarget.ToString());
        }
    }

    public static string scriptsFolder
    {
        get
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildLocation = EditorUserBuildSettings.GetBuildLocation(buildTarget);

            switch(buildTarget)
            {
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return Path.Combine(buildLocation, "Contents/Resources/Data/Managed");
                default:
                    return "";
            }
        }
    }

    [MenuItem("AssetBundles/Build Bundles")]
    public static void BuildAssetBundles()
    {
        var outputPath = bundleBuildPath;
        
        if(Directory.Exists(outputPath))
            Directory.Delete(bundleBuildPath, true);

        Directory.CreateDirectory(outputPath);

        var settings = new BuildSettings();
        settings.target = EditorUserBuildSettings.activeBuildTarget;
        settings.group = EditorUserBuildSettings.selectedBuildTargetGroup;
        settings.scriptsFolder = scriptsFolder;
        if(!Directory.Exists(settings.scriptsFolder))
        {
            Debug.LogError("Script path " + settings.scriptsFolder + " doesn't exist!");
            return;
        }

        settings.outputFolder = outputPath;

        BuildOutput output;
        if(AssetBundleBuildPipeline.BuildAssetBundles(settings, out output))
        {
            var bundlesToCopy = new List<string>(output.results.Select(x => x.assetBundleName));

            CopyBundlesToStreamingAssets(bundlesToCopy);
        }
    }

    static void CopyBundlesToStreamingAssets(List<string> bundlesToCopy)
    {
        // First clean out the existing bundles from streaming assets
        if(Directory.Exists(streamingAssetsBundlePath))
            Directory.Delete(streamingAssetsBundlePath, true);

        Directory.CreateDirectory(streamingAssetsBundlePath);

        foreach(var bundleName in bundlesToCopy)
        {
            var copyFromPath = Path.Combine(bundleBuildPath, bundleName);
            var copyToPath = Path.Combine(streamingAssetsBundlePath, bundleName);
            Debug.LogFormat("Copying asset bundle: {0} => {1}", copyFromPath, copyToPath);

            File.Copy(copyFromPath, copyToPath);
        }
    }
}
