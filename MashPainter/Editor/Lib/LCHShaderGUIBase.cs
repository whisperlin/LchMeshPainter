using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System;
using UnityEngine.Rendering;

//
// 摘要:
//     Backface culling mode.
public enum CullMode
{
    //
    // 摘要:
    //     Disable culling.
    Off = 0,
    //
    // 摘要:
    //     Cull front-facing geometry.
    Front = 1,
    //
    // 摘要:
    //     Cull back-facing geometry.
    Back = 2
}


public enum LCHCullModel
{

    [EnumAttirbute("开双面")]
    Off = 0,
    //Front = 1,
    [EnumAttirbute("正面剔除")]
    Back = 2
    
}
 
public class MyToggleDrawer : MaterialPropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        // Setup
        bool value = (prop.floatValue != 0.0f);

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        // Show the toggle control
        value = EditorGUI.Toggle(position, label, value);

        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            // Set the new value if it has changed
            prop.floatValue = value ? 1.0f : 0.0f;
        }
    }
}
public class LCHShaderGUIBase : ShaderGUI
{
     

    public bool IsVisible(ref string displayName, Material targetMat)
    {
        bool _result = true;
        string[] lines = displayName.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {

           
            string line = lines[i].Trim();
            if(line.Length==0)
                continue;
            int index0 = line.LastIndexOf("[");
            int index1 = line.LastIndexOf("]");
            if (index0 > 0 && index1 > index0)
            {

                index0++;
                string keyWorld = line.Substring(index0, index1 - index0);
                string[] condictions = keyWorld.Split('&');
                bool isOk = true;
                for (int j = 0; j < condictions.Length && isOk; j++)
                {
                    string cnd = condictions[j];
                    string[] keys = cnd.Split('=');
                    if (keys.Length == 1)
                    {
                        displayName = line.Substring(0, index0 - 1);
                        if (keys[0].StartsWith("!"))
                        {
                            bool b = targetMat.IsKeywordEnabled(keys[0].Substring(1));
                            if (b)
                                isOk = false;
                        }
                        else
                        {
                            if (!targetMat.IsKeywordEnabled(keys[0]))
                                isOk =  false;
                        }

                    }
                    else if (keys.Length == 2)
                    {
                        displayName = line.Substring(0, index0 - 1);
                        if (int.TryParse(keys[1], out int n))
                        {
                            if (targetMat.HasProperty(keys[0]))
                            {
                                if (!(Mathf.Abs(targetMat.GetFloat(keys[0]) - n) < 0.01f))
                                    isOk =  false;
                            }
                        }
                    }
                }
                if (isOk)
                    return true;
                else
                    _result = false; 

            }
        }

        return _result;
    }
    public bool ModifyTextureFormatSmall(string path, string platformString = "Android" )
    {
        TextureImporterPlatformSettings textureSettings = new TextureImporterPlatformSettings();
        try
        {
            bool textureModify = false; ;
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
            textureSettings = ti.GetPlatformTextureSettings(platformString);
            if (
                textureSettings.format == TextureImporterFormat.RGBA32
                    || textureSettings.format == TextureImporterFormat.RGBA16
                    || textureSettings.format == TextureImporterFormat.ARGB32
                    || textureSettings.format == TextureImporterFormat.ARGB16
                    )
            {
                
                textureSettings.format = TextureImporterFormat.ASTC_5x5;
                textureModify = true;



            }

            else if (textureSettings.format == TextureImporterFormat.RGB16
                    || textureSettings.format == TextureImporterFormat.RGB24
                    )
            {
                textureSettings.format = TextureImporterFormat.ASTC_5x5;
                textureModify = true;
            }

            if (textureSettings.maxTextureSize > 512)
            {
                textureSettings.maxTextureSize = 512;
                textureModify = true;
            }
            textureSettings.overridden = true;
            textureSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
            if (textureModify)
            {
                ti.SetPlatformTextureSettings(textureSettings);
                AssetDatabase.SaveAssets();
                return true;
            }
           
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString() + " " + textureSettings.format + "" + path + "\n" + e.StackTrace);
        }
        return false;
    }
    public bool ModifyTextureFormat(string path, string platformString = "Android", bool smallChange = false)
    {
        TextureImporterPlatformSettings textureSettings = new TextureImporterPlatformSettings();
        try
        {
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
            textureSettings = ti.GetPlatformTextureSettings(platformString);
            if (
                textureSettings.format == TextureImporterFormat.RGBA32
                    || textureSettings.format == TextureImporterFormat.RGBA16
                    || textureSettings.format == TextureImporterFormat.ARGB32
                    || textureSettings.format == TextureImporterFormat.ARGB16
                    )
            {
                textureSettings.overridden = true;
                if (smallChange)
                {
                    textureSettings.format = TextureImporterFormat.ASTC_5x5;
                }
                else
                {
                    textureSettings.format = TextureImporterFormat.ASTC_4x4;
                }

                textureSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
                ti.SetPlatformTextureSettings(textureSettings);
                AssetDatabase.SaveAssets();
                //AssetDatabase.ImportAsset(path);
                Debug.Log("修改图片" + path + " " + textureSettings.format.ToString());
                return true;
            }

            else if (textureSettings.format == TextureImporterFormat.RGB16
                    || textureSettings.format == TextureImporterFormat.RGB24
                    )
            {
                textureSettings.overridden = true;
                if (ti.alphaIsTransparency)
                {
                    if (smallChange)
                    {
                        textureSettings.maxTextureSize = 512;
                        textureSettings.format = TextureImporterFormat.ASTC_5x5;
                    }
                    else
                    {
                        textureSettings.format = TextureImporterFormat.ASTC_4x4;
                    }
                }
                else
                {
                    if (smallChange)
                    {
                        textureSettings.maxTextureSize = 512;
                        textureSettings.format = TextureImporterFormat.ASTC_5x5;
                    }
                    else
                    {
                        textureSettings.format = TextureImporterFormat.ASTC_4x4;
                    }
                }

                textureSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
                ti.SetPlatformTextureSettings(textureSettings);
                AssetDatabase.SaveAssets();
                Debug.Log("修改图片" + path + " " + textureSettings.format.ToString());
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString() + " " + textureSettings.format + "" + path + "\n" + e.StackTrace);
        }
        return false;
    }
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        for (int i = 0; i < properties.Length; i++)
        {
            var pop = properties[i];
            if (pop.flags == MaterialProperty.PropFlags.HideInInspector)
            {
                continue;
            }
            
            string displayName = pop.displayName;

            bool clearTex = displayName.Contains("{clear}");
            if (clearTex)
            {
                displayName.Replace("{clear}", "");
            }
            if (IsVisible(ref displayName, targetMat))
            {

                materialEditor.ShaderProperty(pop, displayName);
            }
            else
            {
                if (clearTex)
                {
                    targetMat.SetTexture(pop.name, null);
                    EditorUtility.SetDirty(targetMat);
                }
            }
        }
        GUILayout.Space(20);
        materialEditor.RenderQueueField();
    }
     
}

 