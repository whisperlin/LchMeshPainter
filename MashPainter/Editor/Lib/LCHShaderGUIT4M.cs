using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System;
using UnityEngine.Rendering;


public class LCHShaderGUIT4M : LCHShaderGUIBase
{
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
 