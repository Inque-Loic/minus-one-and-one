using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MinusOneAndOneBuild
{
    const string ProductName = "负一和一";
    const string CompanyName = "PortfolioDemo";
    const string IconPath = "Assets/Brand/MinusOneAndOne_AppIcon.png";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string BuildPath = "Builds/Windows/负一和一.exe";

    [MenuItem("Tools/负一和一/Configure Windows Player")]
    public static void ConfigureWindowsPlayer()
    {
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = ProductName;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.portfoliodemo.minusoneandone");

        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.allowFullscreenSwitch = true;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.runInBackground = false;

        ConfigureIconImport();
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon != null)
        {
            Texture2D[] icons = { icon, icon, icon, icon, icon, icon, icon, icon };
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application);
        }
        else
        {
            Debug.LogWarning($"Build icon not found: {IconPath}");
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        Debug.Log("Configured Windows player: 1280x720 windowed, resizable, product name and icon set.");
    }

    [MenuItem("Tools/负一和一/Build Windows")]
    public static void BuildWindows()
    {
        ConfigureWindowsPlayer();

        string absoluteBuildPath = Path.GetFullPath(BuildPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absoluteBuildPath) ?? throw new InvalidOperationException("Invalid build path."));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = absoluteBuildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
            throw new Exception($"Windows build failed: {summary.result}");

        Debug.Log($"Windows build succeeded: {absoluteBuildPath} ({summary.totalSize / (1024f * 1024f):F1} MB)");
    }

    [MenuItem("Tools/Minus One And One/Build Windows")]
    public static void BuildWindowsEnglish()
    {
        BuildWindows();
    }

    static void ConfigureIconImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = true;
        importer.alphaIsTransparency = false;
        importer.isReadable = false;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }
}
