using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class OpenSourceFontMigration
{
    const string SourceFontPath = "Assets/Fonts/OpenSource/NotoSansCJKsc-Regular.otf";
    const string FontAssetPath = "Assets/Fonts/OpenSource/NotoSansCJKsc-Dynamic.asset";

    [MenuItem("Tools/Minus One And One/Create And Apply Noto Font")]
    public static void CreateAndApplyNotoFont()
    {
        TMP_FontAsset fontAsset = EnsureFontAsset();
        if (fontAsset == null) return;

        foreach (TextMeshProUGUI text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (EditorUtility.IsPersistent(text)) continue;
            Undo.RecordObject(text, "Apply Noto font");
            text.font = fontAsset;
            text.fontSharedMaterial = fontAsset.material;
            text.fontMaterial = null;
            text.SetAllDirty();
            EditorUtility.SetDirty(text);
        }

        ApplyFontToPrefab(fontAsset, "Assets/PlayerButton.prefab");
        ApplyFontToPrefab(fontAsset, "Assets/DiscussionTargetButton.prefab");
        ApplyFontToPrefab(fontAsset, "Assets/DiscussionMessageItem.prefab");
        ApplyFontToPrefab(fontAsset, "Assets/Prefabs/GuessingInputRow.prefab");

        UIManager manager = Object.FindFirstObjectByType<UIManager>();
        if (manager != null)
        {
            Undo.RecordObject(manager, "Assign Noto font");
            manager.chineseFont = fontAsset;
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Noto Sans CJK SC dynamic font has been created and applied.");
    }

    static TMP_FontAsset EnsureFontAsset()
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
        {
            existing.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            existing.isMultiAtlasTexturesEnabled = true;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"Noto font file not found: {SourceFontPath}");
            return null;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(FontAssetPath) ?? "Assets/Fonts/OpenSource");
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);

        fontAsset.name = "NotoSansCJKsc Dynamic";
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.isMultiAtlasTexturesEnabled = true;
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);
        return fontAsset;
    }

    static void ApplyFontToPrefab(TMP_FontAsset fontAsset, string prefabPath)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null) return;

        bool changed = false;
        foreach (TextMeshProUGUI text in prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.font = fontAsset;
            text.fontSharedMaterial = fontAsset.material;
            text.fontMaterial = null;
            text.SetAllDirty();
            changed = true;
        }

        if (changed)
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
}
