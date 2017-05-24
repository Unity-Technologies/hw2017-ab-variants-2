using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;
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

    [MenuItem("AssetBundles/Build Bundles")]
    public static void BuildAssetBundles()
    {
        var outputPath = bundleBuildPath;
        
        if(Directory.Exists(outputPath))
            Directory.Delete(bundleBuildPath, true);

        Directory.CreateDirectory(outputPath);

        var results = CompileScripts();
       
        var settings = new BuildSettings();
        settings.target = EditorUserBuildSettings.activeBuildTarget;
        settings.group = EditorUserBuildSettings.selectedBuildTargetGroup;
        settings.typeDB = results.typeDB;
        settings.outputFolder = outputPath;

        var input = BuildInterface.GenerateBuildInput();
//        BuildInput input;
//        AddressableAssetSettings.GetDefault().GenerateBuildInput(out input);

//        var scenes = EditorBuildSettings.scenes;
//        var sceneInput = new BuildInput();
//        sceneInput.definitions = new BuildInput.Definition[scenes.Length];
//        for(var x = 0; x < sceneInput.definitions.Length; x++)
//        {
//            var def = new BuildInput.Definition();
//            def.assetBundleName = scenes[x].path.Replace("/", "_");
//            var addressableAsset = new BuildInput.AddressableAsset();
//            addressableAsset.address = scenes[x].path;
//            addressableAsset.asset = scenes[x].guid;
//            def.explicitAssets = new BuildInput.AddressableAsset[] { addressableAsset };
//            sceneInput.definitions[x] = def;
//        }

//        if(sceneInput.definitions.Length > 0)
//            ArrayUtility.AddRange<BuildInput.Definition>(ref input.definitions, sceneInput.definitions);

        if(input.definitions.Length == 0)
        {
            Debug.Log("No asset bundles to build.");
            return;
        }

        BuildOutput output;
        if(AssetBundleBuildPipeline.BuildAssetBundles(settings, input, out output))
        {
            var bundlesToCopy = new List<string>(output.results.Select(x => x.assetBundleName));

            CopyBundlesToStreamingAssets(bundlesToCopy);
        }
    }
    
    public static ScriptCompilationResult CompileScripts()
    {
        ScriptCompilationSettings input = new ScriptCompilationSettings();
        input.target = EditorUserBuildSettings.activeBuildTarget;
        input.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        input.options = ScriptCompilationOptions.None;
        input.outputFolder = "Library/ScriptAssemblies";
        return PlayerBuildInterface.CompilePlayerScripts(input);
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

//    static ContentCatalog GenerateContentCatalog()
//    {
//    }
//
//    static BuildInput GenerateBuildInputFromContentCatalog(ContentCatalog catalog)
//    {
//    }

}
