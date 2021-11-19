using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LCH
{
    partial class LchMeshPainter
    {
        private class MeshTools
        {
            public GameObject selectingObj;
            public string path;
            public bool smoonthNormalToColor = true;

            public ModelImporterNormalCalculationMode calculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
            public ModelImporterNormalSmoothingSource normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
            public float normalSmoothingAngle = 120f;
            public void Check()
            {
                if (path == null || path.Length == 0)
                {
                    path = null;
                    selectingObj = null;
                }
                var _path = path.ToLower();
                if (_path.EndsWith(".fbx") || _path.EndsWith(".obj"))
                {
                    return;
                }
                path = null;
                selectingObj = null;
            }
            static void SaveAsset(string path, UnityEngine.Object obj)
            {
                UnityEngine.Object obj0 = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj0 == null)
                {
                    AssetDatabase.CreateAsset(obj, path);
                }
                else
                {
                    EditorUtility.CopySerialized(obj, obj0);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.ImportAsset(path);
            }
            public Mesh ConvertToUnityMesh(Mesh mesh, string saveFolder, bool copyColorFromNormal = false)
            {
                string path = AssetDatabase.GetAssetPath(mesh);
                if (path.EndsWith(".asset"))
                    return mesh;
                Mesh mesh1 = (Mesh)UnityEngine.Object.Instantiate(mesh);

                if (copyColorFromNormal)
                {
                    mesh.RecalculateNormals();

                    var normals = mesh.normals;

                    Color[] colors = new Color[normals.Length];
                    Vector2[] uv3 = new Vector2[normals.Length];
                    Vector2[] uv4 = new Vector2[normals.Length];
                    for (int i = 0; i < normals.Length; i++)
                    {
                        var n = normals[i];
                        n = n.normalized;
                        colors[i] = new Color((n.x * 0.5f + 0.5f), (n.y * 0.5f + 0.5f), (n.z * 0.5f + 0.5f), 1f);
                        uv3[i] = new Vector2(n.x, n.y);
                        uv4[i] = new Vector2(n.z, n.y);
                    }
                    mesh1.colors = colors;
                    mesh1.uv3 = uv3;
                    mesh1.uv4 = uv4;
                    mesh1.RecalculateNormals();
                }
                string savePath = saveFolder + "/" + mesh.name + "_mesh.asset";
                SaveAsset(savePath, mesh1);
                mesh1 = AssetDatabase.LoadAssetAtPath<Mesh>(savePath);
                return mesh1;
            }
            public void Save(string saveFolder)
            {
                string name = path.Substring(0, path.LastIndexOf("."));
                name = name.Substring(name.LastIndexOf("/") + 1);
                if (smoonthNormalToColor)
                {
                    selectingObj = SmoonthNormal(selectingObj);

                }
                GameObject g = GameObject.Instantiate(selectingObj);
                g.name = name;

                SkinnedMeshRenderer[] skinnedMeshRenderers = g.GetComponentsInChildren<SkinnedMeshRenderer>();

                MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();


                foreach (var v in skinnedMeshRenderers)
                {
                    v.sharedMesh = ConvertToUnityMesh(v.sharedMesh, saveFolder, smoonthNormalToColor);
                }
                foreach (var v in meshFilters)
                {
                    v.sharedMesh = ConvertToUnityMesh(v.sharedMesh, saveFolder, smoonthNormalToColor);
                }

                string perfebPath = saveFolder + "/" + name + ".prefab";


                PrefabUtility.SaveAsPrefabAssetAndConnect(g, perfebPath, InteractionMode.UserAction);
                GameObject.DestroyImmediate(g, true);
            }

            private GameObject SmoonthNormal(GameObject selectingObj)
            {
                string basePath = AssetDatabase.GetAssetPath(selectingObj);
                ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(path);
                modelImporter.importNormals = ModelImporterNormals.Calculate;
                modelImporter.importBlendShapeNormals = ModelImporterNormals.Calculate;
                modelImporter.normalCalculationMode = calculationMode;
                modelImporter.normalSmoothingAngle = normalSmoothingAngle;
                modelImporter.normalSmoothingSource = normalSmoothingSource;
                modelImporter.SaveAndReimport();
                selectingObj = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
                return selectingObj;
            }
        }
    }
}