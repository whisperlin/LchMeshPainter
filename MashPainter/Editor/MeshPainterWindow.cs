using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
namespace LCH
{
    public partial class LchMeshPainterWindow : EditorWindow
    {
        LchMeshPainter painter = new LchMeshPainter();
        [MenuItem("TA/地形/创建材质")]
        static void CreateMat()
        {
            string path = EditorUtility.SaveFilePanelInProject("材质文件", "terrain", "mat", "保存材质");
            if (path.Length>0)
            {
            
                string root = path.Substring(0,path.Length-4);
                string end = path.Substring(path.Length - 3);
                 
                Material mat = new Material(Shader.Find("Lch/Terrain Phone(Shadow Mask)"));
                Texture2D t = new Texture2D(1024,1024, TextureFormat.RGBA32,true);
                Color [] cs = t.GetPixels();
                for (int i = 0; i < cs.Length; i++)
                {
                    cs[i] = new Color(1, 0, 0, 0);
                }
                t.SetPixels(cs);
                t.Apply();
                var bytes = t.EncodeToTGA();
                string path1 = root + "_ctrl1.tga";
                System.IO.File.WriteAllBytes(path1, bytes);
                for (int i = 0; i < cs.Length; i++)
                {
                    cs[i] = new Color(0,0,0,0);
                }
                t.SetPixels(cs);
                t.Apply();
                bytes = t.EncodeToTGA();
                string path2 = root + "_ctrl2.tga";
                System.IO.File.WriteAllBytes(path2, bytes);
                AssetDatabase.ImportAsset(path1);
                AssetDatabase.ImportAsset(path2);

                TextureImporter ti1 = (TextureImporter)TextureImporter.GetAtPath(path1);
                TextureImporter ti2 = (TextureImporter)TextureImporter.GetAtPath(path2);
                ti1.sRGBTexture = false;
                ti2.sRGBTexture = false;
                ti1.textureCompression = TextureImporterCompression.CompressedHQ;
                ti2.textureCompression = TextureImporterCompression.CompressedHQ;
                ti1.SaveAndReimport();
                ti2.SaveAndReimport();

                mat.SetTexture("_Control", AssetDatabase.LoadAssetAtPath<Texture2D>(path1));
                mat.SetTexture("_Control2", AssetDatabase.LoadAssetAtPath<Texture2D>(path2));
                UnityEngine.Object obj0 = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj0 == null)
                {
                    AssetDatabase.CreateAsset(mat, path);
                }
                else
                {
                    EditorUtility.CopySerialized(mat, obj0);
                    AssetDatabase.SaveAssets();
                }
                AssetDatabase.ImportAsset(path);
            }
        }
        [MenuItem("TA/地形/Lch Mesh Painter")]
        static void Init()
        {
            LchMeshPainterWindow window = (LchMeshPainterWindow)EditorWindow.GetWindow(typeof(LchMeshPainterWindow));
#if UNITY_2019_1_OR_NEWER
            window.painter.is2019OrNew = true;
#else
            window.painter.is2019OrNew = false;
#endif
            window.painter.window = window;
            window.title = "Lch Mesh Painter";
            
            window.Show();
        }

        public void OnGUI()
        {
            painter.window = this;
            painter.OnGUI();
        }
        public void OnDisable()
        {
            painter.window = this;
            painter.OnDisable();

        }
         
        public void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            painter.is2019OrNew = true;
#else
            painter.is2019OrNew = false;
#endif
            painter.window = this;
            painter.OnEnable();
        }
    }
}