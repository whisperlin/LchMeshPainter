using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NormalTextureArrays : EditorWindow
{

    public List<Texture2D> texs = new List<Texture2D>();
    [MenuItem("TA/地形/法线合并Texture2dArray")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        NormalTextureArrays window = (NormalTextureArrays)EditorWindow.GetWindow(typeof(NormalTextureArrays));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        int count = EditorGUILayout.IntField("数组个数", texs.Count);
        if (EditorGUI.EndChangeCheck())
        {
            if (count < 0)
                count = 0;
            while (texs.Count < count)
            {
                texs.Add(null);
            }
            while (texs.Count > count)
            {
                texs.RemoveAt(texs.Count - 1);
            }
        }
        for (int i = 0; i < texs.Count; i++)
        {
            texs[i] = EditorGUILayout.ObjectField("法线" + i, texs[i], typeof(Texture2D),false, GUILayout.Height(50) ) as Texture2D;
        }
        bool allFix = true;
        for (int i = 0; i < texs.Count; i++)
        {
            if (null == texs[i])
            {
                allFix = false;
                break;
            }
            //string path = AssetDatabase.GetAssetPath(texs[i]);
            
        }
        if (allFix)
        {
            if (GUILayout.Button("确定"))
            {
                for (int i = 0; i < texs.Count; i++)
                {
                    string path = AssetDatabase.GetAssetPath(texs[i]);
                    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (null == ti)
                        continue;
                    if ( !ti.isReadable)
                    {
                        ti.isReadable = true;
                        ti.SaveAndReimport();
                    }
                }
                for (int i = 1; i < texs.Count; i++)
                {
                    if (texs[i].width != texs[0].width || texs[i].height != texs[0].height)
                    {
                        EditorUtility.DisplayDialog("错误", string.Format("输入图片第{0}张与第一张不一致",i), "确定");
                        return;
                    }
                }
                int arrAyCount = texs.Count/ 2;
                if (arrAyCount != (texs.Count + 1) / 2)
                    arrAyCount++;


                string path1 = EditorUtility.SaveFilePanel("", ".", "normalArray", "tga");
                if (path1.Length < 0)
                {
                    return;
                }
                else if (!path1.StartsWith(Application.dataPath))
                {
                    EditorUtility.DisplayDialog("", "保存路径必须在工程内部", "确定");
                    return;
                }
                path1 = path1.Substring(Application.dataPath.Length - 6);
                string[] paths = new string[arrAyCount];
                Texture2D[]  temps = new Texture2D[arrAyCount];
                int Id = Random.Range(0, int.MaxValue - 100);

                
                for (int i = 0; i < arrAyCount; i++)
                {
                    temps[i] = new Texture2D(texs[0].width, texs[0].height,TextureFormat.RGBA32,true);
                    temps[i].name = "TempForTextureArray"+(Id+i);
                    int index0 = i * 2;
                    int index1 = index0 + 1;
                    if (index1 == texs.Count)
                    {
                        Color[] colors = texs[index0].GetPixels();
                        temps[i].SetPixels(colors);
                    }
                    else
                    {
                        Color[] colors = texs[index0].GetPixels();
                        Color[] colors2 = texs[index1].GetPixels();
                        for (int j = 0; j < colors.Length; j++)
                        {
                            colors[j].b = colors2[j].r;
                            colors[j].a = colors2[j].g;
                        }
                        temps[i].SetPixels(colors);
                    }
                    temps[i].Apply();
                    string path = path1.Substring(0,path1.Length-4)+ "_"+i.ToString()+".tga";
                    Debug.Log("保存法线贴图"+path);
                    System.IO.File.WriteAllBytes(path, temps[i].EncodeToTGA());
                    AssetDatabase.ImportAsset(path);
                    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

                    TextureImporterPlatformSettings ti_Android = ti.GetPlatformTextureSettings("Android");
                    ti.isReadable = true;
                    ti_Android.format = TextureImporterFormat.ASTC_4x4;
                    ti_Android.overridden = true;
                    ti.SetPlatformTextureSettings(ti_Android);
                    ti.SaveAndReimport();
   

                }
                //texture2DArray.Apply();

                 
                 
            }
        }
        
    }
}
