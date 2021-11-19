using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LCH
{
    partial class LchMeshPainter
    {
        private class TerrainTools
        {
            public Terrain editorTerrain;
            public float terrainQuality = 0.25f;
            public int face = 5000;
            public Mesh baseMesh;
            public Mesh lodMesh;
            public int GetConvertedMeshPolyCount()
            {
                if (null == editorTerrain)
                    return 0;
                TerrainData terrain = editorTerrain.terrainData;
                int w = terrain.heightmapResolution;
                int h = terrain.heightmapResolution;

                float tRes = 1f / terrainQuality;
                w = (int)((w - 1) / tRes + 1);
                h = (int)((h - 1) / tRes + 1);

                return (w - 1) * (h - 1) * 2;
            }
            public void CreateLodMesh()
            {
                if (null != lodMesh)
                    GameObject.DestroyImmediate(lodMesh);
                lodMesh = (Mesh)UnityEngine.Object.Instantiate(baseMesh);
                lodMesh.name = baseMesh.name;
                BuildLodMesh(lodMesh, face);
            }
            Vector3 meshScale;
            Vector2 uvScale;
            int w;
            int h;
            public void MakeMesh()
            {
                if (null != baseMesh)
                {
                    GameObject.DestroyImmediate(baseMesh, true);
                    baseMesh = null;
                }
                TerrainData terrainData = editorTerrain.terrainData;
                int vertexCountScale = 4;       
                w = terrainData.heightmapResolution;
                h = terrainData.heightmapResolution;
 
                Vector3 size = terrainData.size;
                float[,,] alphaMapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
                meshScale = new Vector3(size.x / (w - 1f) * vertexCountScale, 1, size.z / (h - 1f) * vertexCountScale);
                

                w = (w - 1) / vertexCountScale + 1;
                h = (h - 1) / vertexCountScale + 1;

                uvScale = new Vector2(1f / (w - 1f), 1f / (h - 1f));
                Vector3[] vertices = new Vector3[w * h];
                Vector2[] uvs = new Vector2[w * h];
                Vector4[] alphasWeight = new Vector4[w * h];            // [dev] 只支持4张图片

                // 顶点，uv，每个顶点每个图片所占比重
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        int index = j * w + i;
                        float z = terrainData.GetHeight(i * vertexCountScale, j * vertexCountScale);
                        vertices[index] = Vector3.Scale(new Vector3(i, z, j), meshScale);
                        Vector2 _uv = Vector2.Scale(new Vector2(i, j), uvScale);
                        uvs[index] = _uv;

                        // alpha map
                        int i2 = (int)(i * terrainData.alphamapWidth / (w - 1f));
                        int j2 = (int)(j * terrainData.alphamapHeight / (h - 1f));
                        i2 = Mathf.Min(terrainData.alphamapWidth - 1, i2);
                        j2 = Mathf.Min(terrainData.alphamapHeight - 1, j2);
                        var alpha0 = alphaMapData[j2, i2, 0];
                        var alpha1 = alphaMapData[j2, i2, 1];
                        var alpha2 = alphaMapData[j2, i2, 2];
                        var alpha3 = alphaMapData[j2, i2, 3];
                        alphasWeight[index] = new Vector4(alpha0, alpha1, alpha2, alpha3);
                    }
                }

           
                int[] triangles = new int[(w - 1) * (h - 1) * 6];
                int triangleIndex = 0;
                for (int i = 0; i < w - 1; i++)
                {
                    for (int j = 0; j < h - 1; j++)
                    {
                        int a = j * w + i;
                        int b = (j + 1) * w + i;
                        int c = (j + 1) * w + i + 1;
                        int d = j * w + i + 1;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = b;
                        triangles[triangleIndex++] = c;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = c;
                        triangles[triangleIndex++] = d;
                    }
                }

                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.uv = uvs;
                mesh.triangles = triangles;
                mesh.tangents = alphasWeight;
                mesh.RecalculateNormals();
                baseMesh = mesh;
            }
            public static bool ModifyTextureFormatPC(string path)
            {
                TextureImporterPlatformSettings textureSettings = new TextureImporterPlatformSettings();
                try
                {
                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
                    textureSettings = ti.GetPlatformTextureSettings("Standalone");
                    textureSettings.overridden = true;
                    textureSettings.format = TextureImporterFormat.RGBA32;
                    textureSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
                    ti.SetPlatformTextureSettings(textureSettings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString() + " " + textureSettings.format + "" + path + "\n" + e.StackTrace);
                }
                return false;
            }
            public  bool ModifyTextureFormat(string path, string platformString = "Android")
            {
                TextureImporterPlatformSettings textureSettings = new TextureImporterPlatformSettings();
                try
                {
                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
                    ti.sRGBTexture = false;
                    textureSettings = ti.GetPlatformTextureSettings(platformString);

                    textureSettings.overridden = true;
                    textureSettings.format = TextureImporterFormat.ASTC_4x4;

                    textureSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
                    ti.SetPlatformTextureSettings(textureSettings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
 
                    return true;


                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString() + " " + textureSettings.format + "" + path + "\n" + e.StackTrace);
                }
                return false;
            }

            string[] LAYER_KEY_WORLD = new string[] { "_LMPLAY_L3", "_LMPLAY_L4", "_LMPLAY_L5", "_LMPLAY_L6", "_LMPLAY_L7", "_LMPLAY_L8" };

            void SetLayerKeyWorld(Material mat,int index)
            {
                index = Mathf.Min(LAYER_KEY_WORLD.Length - 1, Mathf.Max(index - 3, 0) );
                for (int i = 0; i < LAYER_KEY_WORLD.Length; i++)
                {
                    if (index == i)
                    {
                        mat.EnableKeyword(LAYER_KEY_WORLD[i]);
                    }
                    else
                    {
                        mat.DisableKeyword(LAYER_KEY_WORLD[i]);
                    }
                }
            }
            public void Save(string path)
            {
                string mesh_path = path + "terrain.mesh";
                Vector3 _max = Vector3.Scale(new Vector3(w - 1, 0, h - 1), meshScale);
                var vs = lodMesh.vertices;
                Vector2[] uvs = new Vector2[vs.Length];
                for (int i = 0; i < vs.Length; i++)
                {
                    uvs[i] = new Vector2(vs[i].x / _max.x, vs[i].z / _max.z);
                }

                lodMesh.uv = uvs;
                //Vector3 meshScale;
                //int w;
                //int h;
                Mesh _mesh = GameObject.Instantiate(lodMesh);
                SaveAsset(mesh_path, _mesh);
                _mesh = AssetDatabase.LoadAssetAtPath<Mesh>(mesh_path);
                TerrainData terrain = editorTerrain.terrainData;
                Texture2D[] textures = terrain.alphamapTextures;

                Shader _shader = Shader.Find(setting.defaultTerrainShaderName);
                if (null == _shader)
                {
                    EditorUtility.DisplayDialog(Languages.GetValue(54, "Error"), "Shader " + setting.defaultTerrainShaderName + Languages.GetValue(55, "Not Found!!! You can change the default shader name on setting panel."), "Ok");
                }
                Material mat = new Material(_shader);
                
                for (int i = 0; i < textures.Length; i++)
                {
                    Texture2D t = textures[i];
                    byte[] date = LchEncode.EncodeToTGA(t);
                    string savePath = path + t.name+ ".tga";
                    System.IO.File.WriteAllBytes(savePath, date);
                    System.IO.File.WriteAllBytes(savePath, t.EncodeToPNG());
                    AssetDatabase.ImportAsset(savePath);
                    ModifyTextureFormat(savePath, "Android");
                    ModifyTextureFormat(savePath, "iPhone");
                    ModifyTextureFormatPC(savePath);
                    t = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
                    if (i == 0)
                    {
                        mat.SetTexture(setting.ctrlTextureName1, t);
                    }
                    else if (i == 1)
                    {
                        mat.SetTexture(setting.ctrlTextureName2, t);
                    }
                }


                
                var terrainLayers = terrain.terrainLayers;
                int layLen =  terrainLayers.Length;
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    var tl = terrainLayers[i];
                    mat.SetTexture(setting.prefix + i, tl.diffuseTexture);
                    mat.SetTexture(setting.norPrefix + i, tl.normalMapTexture);
                    Vector2 scale = new Vector2( terrain.baseMapResolution/ tl.tileSize.x,  terrain.baseMapResolution/ tl.tileSize.y);
                    mat.SetTextureScale(setting.prefix + i  , scale);
                }

                
                SetLayerKeyWorld(mat, layLen );
                 

                string mat_path = path + "terrain.mat";
                AssetDatabase.CreateAsset(mat,mat_path);
                

                mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(mat_path);
                GameObject g = new GameObject("Lch Terrain");
                g.AddComponent<MeshFilter>().sharedMesh = _mesh;
                g.AddComponent<MeshRenderer>().sharedMaterial = mat;

                string go_path = path + "terrainobj.prefab";

                Object prefab = PrefabUtility.CreateEmptyPrefab(go_path);
                PrefabUtility.ReplacePrefab(g, prefab, ReplacePrefabOptions.ConnectToPrefab);
              
            }

            void SaveAsset(string path, UnityEngine.Object obj)
            {
 
                /*UnityEngine.Object obj0 = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj0 == null)
                {
                    AssetDatabase.CreateAsset(obj, path);
                }
                else
                {
                    EditorUtility.CopySerialized(obj, obj0);
                    AssetDatabase.SaveAssets();
                }*/
                AssetDatabase.CreateAsset(obj, path);
                AssetDatabase.ImportAsset(path);
            }
            public void Release()
            {
                if (null != baseMesh)
                    GameObject.DestroyImmediate(baseMesh, true);
            }

        }

    }

}
