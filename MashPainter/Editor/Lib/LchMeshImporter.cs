using System.Collections;
using System.Collections.Generic;

using System.IO;

using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

 
//[UnityEditor.AssetImporters.ScriptedImporter(1, "meshex")]

public class LchMeshImporter : ScriptedImporter
{
    /*public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
    {
        string path = ctx.assetPath;
    }*/
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string path = ctx.assetPath;
    }
} 
