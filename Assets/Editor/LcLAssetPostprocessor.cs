using UnityEditor;
using UnityEngine;
public class LcLAssetPostprocessor : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        var importer = assetImporter as ModelImporter;
        importer.isReadable = true;
    }

    void OnPreprocessTexture()
    {
        var importer = assetImporter as TextureImporter;
        importer.isReadable = true;
    }
}