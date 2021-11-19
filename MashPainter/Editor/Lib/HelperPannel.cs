using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LCH
{
    partial class LchMeshPainter
    {
        Vector2 helpPos = Vector2.one;
        Texture2D logo = null;
        private void OnDrawHelp()
        {
            if (null == logo)
            {
                string [] rs = AssetDatabase.FindAssets("LchMeshPainterLogo");
                if (rs.Length == 0)
                {
                    logo = Texture2D.whiteTexture;
                }
                else
                {
                    string path = AssetDatabase.GUIDToAssetPath(rs[0]);
                    logo = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                     
                }
                
            }
            helpPos = GUILayout.BeginScrollView(helpPos);

            if (null != logo)
            {
                var rect = GUILayoutUtility.GetRect(logo.width, logo.height);
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit, true, 10.0F);
            }

            GUILayout.Label(Languages.GetValue(26, "Short cut:"));
            GUILayout.Label(Languages.GetValue(27, "Ctrl+Alt+Z to undo."));
            GUILayout.Label(Languages.GetValue(28, "Ctrl+Alt+Y to redo."));

            GUILayout.Label(Languages.GetValue(26, "When planting trees:"));
            GUILayout.Label(Languages.GetValue(45, "Eraser brush: Hold down Ctrl and drag the brush to remove trees."));

            if (Languages.curLanguage != 0)
            {
                GUILayout.Label("Author: Lin Chunhua   mail:309762472@qq.com  :Wechar: linchunhua830116");
            }
            else
            {
                GUILayout.Label("作者:林春华   mail:309762472@qq.com  :微信: 18928762880");
            }

            GUILayout.EndScrollView();
        }
    }
}

