using TMPro;
using UnityEngine;

public static class TMPFontResolver
{
    private const string PreferredFontAssetPath = "Assets/Fonts/KoPubWorld Batang Medium SDF.asset";
    private const string PreferredFontName = "KoPubWorld Batang Medium SDF";

    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset GetPreferredFont()
    {
        if (cachedFont != null)
        {
            return cachedFont;
        }

#if UNITY_EDITOR
        cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PreferredFontAssetPath);

        if (cachedFont != null)
        {
            Debug.Log($"TMPFontResolver loaded font from asset path: {cachedFont.name}");
            return cachedFont;
        }
#endif

        TMP_FontAsset[] fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

        for (int index = 0; index < fontAssets.Length; index++)
        {
            TMP_FontAsset fontAsset = fontAssets[index];

            if (fontAsset != null && fontAsset.name == PreferredFontName)
            {
                cachedFont = fontAsset;
                Debug.Log($"TMPFontResolver found font by name: {cachedFont.name}");
                return cachedFont;
            }
        }

        cachedFont = TMP_Settings.defaultFontAsset;

        if (cachedFont != null)
        {
            Debug.LogWarning($"TMPFontResolver could not find '{PreferredFontName}'. Falling back to {cachedFont.name}.");
        }
        else
        {
            Debug.LogWarning($"TMPFontResolver could not find '{PreferredFontName}' and no TMP default font is configured.");
        }

        return cachedFont;
    }
}