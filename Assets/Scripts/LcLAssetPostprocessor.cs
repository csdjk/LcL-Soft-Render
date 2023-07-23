using UnityEditor;
using UnityEngine;

public class LcLAssetPostprocessor : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        // 将模型资源设置为可读
        var importer = assetImporter as ModelImporter;
        importer.isReadable = true;
    }

    void OnPreprocessTexture()
    {
        // 将贴图资源设置为可读
        var importer = assetImporter as TextureImporter;
        importer.isReadable = true;
    }
}