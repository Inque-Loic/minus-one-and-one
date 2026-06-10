using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

public static class OpenSourceFontMigration
{
    const string OldChineseFontGuid = "8f366153fe50f01409f6c457b925f1c5";
    const string NotoFontGuid = "a32f25de5dda97b48a15f0e410402f26";
    const string SourceFontPath = "Assets/Fonts/OpenSource/NotoSansCJKsc-Regular.otf";
    const string FontAssetPath = "Assets/Fonts/OpenSource/NotoSansCJKsc-Dynamic.asset";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Minus One And One/Create And Apply Noto Font")]
    public static void CreateAndApplyNotoFont()
    {
        TMP_FontAsset fontAsset = EnsureFontAsset();
        if (fontAsset == null) return;

        foreach (TextMeshProUGUI text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (EditorUtility.IsPersistent(text)) continue;
            Undo.RecordObject(text, "Apply Noto font");
            ApplyFontToText(text, fontAsset);
            EditorUtility.SetDirty(text);
        }

        foreach (TMP_InputField inputField in Resources.FindObjectsOfTypeAll<TMP_InputField>())
        {
            if (EditorUtility.IsPersistent(inputField)) continue;
            Undo.RecordObject(inputField, "Apply Noto font");
            ApplyFontToInputField(inputField, fontAsset);
            EditorUtility.SetDirty(inputField);
        }

        RemoveSceneMaterialOverrides();

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

        ReplaceOldFontGuidInScene();
        AssetDatabase.SaveAssets();
        Debug.Log("Noto Sans CJK SC dynamic font has been created and applied.");
    }

    public static void PrepareOpenSourceFontsForBuild()
    {
        CreateAndApplyNotoFont();
        ReplaceOldFontGuidInScene();
    }

    [MenuItem("Tools/Minus One And One/Clear TMP Material Overrides")]
    public static void ClearTmpMaterialOverrides()
    {
        foreach (TextMeshProUGUI text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (EditorUtility.IsPersistent(text)) continue;
            Undo.RecordObject(text, "Clear TMP material overrides");
            ClearMaterialOverrides(text);
            text.SetAllDirty();
            EditorUtility.SetDirty(text);
        }

        RemoveSceneMaterialOverrides();
        EditorSceneManager.MarkAllScenesDirty();
        AssetDatabase.SaveAssets();
        Debug.Log("TMP material overrides have been cleared.");
    }

    [MenuItem("Tools/Minus One And One/Restore TMP Text Visibility")]
    public static void RestoreTmpTextVisibility()
    {
        foreach (TextMeshProUGUI text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (EditorUtility.IsPersistent(text)) continue;
            Undo.RecordObject(text, "Restore TMP text visibility");
            text.enabled = true;
            text.SetAllDirty();
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("TMP text visibility has been restored.");
    }

    static TMP_FontAsset EnsureFontAsset()
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
        {
            existing.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            existing.isMultiAtlasTexturesEnabled = true;
            EnsureAtlasAndMaterial(existing);
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
        EnsureAtlasAndMaterial(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);
        return fontAsset;
    }

    static void EnsureAtlasAndMaterial(TMP_FontAsset fontAsset)
    {
        Texture2D atlas = null;
        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
            atlas = fontAsset.atlasTextures[0];

        if (atlas == null)
        {
            atlas = new Texture2D(1, 1, TextureFormat.Alpha8, false);
            atlas.name = "NotoSansCJKsc Dynamic Atlas";
            fontAsset.atlasTextures = new[] { atlas };
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        Material material = fontAsset.material;
        if (material == null)
        {
            material = new Material(Shader.Find("TextMeshPro/Distance Field"));
            material.name = "NotoSansCJKsc Dynamic Atlas Material";
            AssetDatabase.AddObjectToAsset(material, fontAsset);
            fontAsset.material = material;
        }

        material.SetTexture(ShaderUtilities.ID_MainTex, atlas);
        material.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
        material.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);
        material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1);
        material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
        material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

        if (!HasFreeGlyphRect(fontAsset))
            SetFreeGlyphRects(fontAsset);

        fontAsset.TryAddCharacters("负一和一开始游戏测试玩家回合身份阵营分数讨论阶段公开变化指控信任返回主菜单下一接受拒绝确认行动尚无数据请输入批量统计胜利平局好人坏人本局结果摘要快速规则每选择接触安全对象累积伪装污染按钮观察说谎异常最后知道了提交猜分跳过必须卡牌棋子筹码奖杯目标图标");
        SetClearDynamicDataOnBuild(fontAsset, false);
        EditorUtility.SetDirty(atlas);
        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(fontAsset);
    }

    static void SetClearDynamicDataOnBuild(TMP_FontAsset fontAsset, bool value)
    {
        SerializedObject serialized = new SerializedObject(fontAsset);
        SerializedProperty property = serialized.FindProperty("m_ClearDynamicDataOnBuild");
        if (property != null)
            property.boolValue = value;
        serialized.ApplyModifiedProperties();
    }

    static void SetFreeGlyphRects(TMP_FontAsset fontAsset)
    {
        SerializedObject serialized = new SerializedObject(fontAsset);
        SerializedProperty property = serialized.FindProperty("m_FreeGlyphRects");
        if (property == null) return;

        property.ClearArray();
        property.InsertArrayElementAtIndex(0);
        SerializedProperty rect = property.GetArrayElementAtIndex(0);
        rect.FindPropertyRelative("m_X").intValue = 0;
        rect.FindPropertyRelative("m_Y").intValue = 0;
        rect.FindPropertyRelative("m_Width").intValue = fontAsset.atlasWidth - 1;
        rect.FindPropertyRelative("m_Height").intValue = fontAsset.atlasHeight - 1;
        serialized.ApplyModifiedProperties();
    }

    static bool HasFreeGlyphRect(TMP_FontAsset fontAsset)
    {
        SerializedObject serialized = new SerializedObject(fontAsset);
        SerializedProperty property = serialized.FindProperty("m_FreeGlyphRects");
        return property != null && property.isArray && property.arraySize > 0;
    }

    static void ApplyFontToPrefab(TMP_FontAsset fontAsset, string prefabPath)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null) return;

        bool changed = false;
        foreach (TextMeshProUGUI text in prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            ApplyFontToText(text, fontAsset);
            changed = true;
        }

        foreach (TMP_InputField inputField in prefabRoot.GetComponentsInChildren<TMP_InputField>(true))
        {
            ApplyFontToInputField(inputField, fontAsset);
            changed = true;
        }

        if (changed)
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }

    static void ApplyFontToText(TextMeshProUGUI text, TMP_FontAsset fontAsset)
    {
        text.enabled = true;
        text.font = fontAsset;
        text.fontSharedMaterial = fontAsset.material;
        ClearMaterialOverrides(text);
        text.SetAllDirty();
    }

    static void ClearMaterialOverrides(TextMeshProUGUI text)
    {
        SerializedObject serialized = new SerializedObject(text);

        SerializedProperty fontMaterial = serialized.FindProperty("m_fontMaterial");
        if (fontMaterial != null)
            fontMaterial.objectReferenceValue = null;

        SerializedProperty fontMaterials = serialized.FindProperty("m_fontMaterials");
        if (fontMaterials != null)
            fontMaterials.ClearArray();

        SerializedProperty fontSharedMaterials = serialized.FindProperty("m_fontSharedMaterials");
        if (fontSharedMaterials != null)
            fontSharedMaterials.ClearArray();

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void ApplyFontToInputField(TMP_InputField inputField, TMP_FontAsset fontAsset)
    {
        inputField.fontAsset = fontAsset;

        SerializedObject serialized = new SerializedObject(inputField);
        SerializedProperty globalFontAsset = serialized.FindProperty("m_GlobalFontAsset");
        if (globalFontAsset != null)
            globalFontAsset.objectReferenceValue = fontAsset;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void ReplaceOldFontGuidInScene()
    {
        if (!File.Exists(ScenePath)) return;

        string sceneText = File.ReadAllText(ScenePath);
        string updated = sceneText.Replace(OldChineseFontGuid, NotoFontGuid);
        if (updated == sceneText) return;

        File.WriteAllText(ScenePath, updated);
        AssetDatabase.ImportAsset(ScenePath);
    }

    static void RemoveSceneMaterialOverrides()
    {
        foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (EditorUtility.IsPersistent(material)) continue;
            if (!material.name.StartsWith("NotoSansCJKsc Dynamic Atlas Material (Instance)")) continue;
            Object.DestroyImmediate(material);
        }
    }
}
