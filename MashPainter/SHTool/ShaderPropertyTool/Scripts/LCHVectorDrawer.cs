using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public class LCHVectorDrawer : MaterialPropertyDrawer
{
    public LCHVectorDrawer(  )
    {
    }
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 80;
    }
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        Rect rect = position;
        rect.height = 20;

        Vector4 v4 = prop.vectorValue;
        EditorGUI.BeginChangeCheck();

        string pattern = @"(?<t>[^\(]+)\((?<x>[^,]+),(?<y>[^\)]+)\)";

        int i = 0;
        int j = 0;
        var result = Regex.Matches(label, pattern, RegexOptions.Multiline);
        
        while (j < result.Count)
        {
            Match match = result[j];
            string title = match.Groups["t"].Value;
            string x = match.Groups["x"].Value;
            string y = match.Groups["y"].Value;

            if (title.Contains("<minmax>") && i <3)
            {
                title = title.Replace("<minmax>", "");
                float _x = v4[i];
                float _y = v4[i+1];
                EditorGUI.MinMaxSlider(rect, title, ref _x, ref _y, float.Parse(x), float.Parse(y));
                v4[i] = _x;
                v4[i + 1] = _y;
                i +=2;
            }
            else if(i<4)
            {
                try
                {
                    v4[i] = EditorGUI.Slider(rect, title, v4[i ], float.Parse(x), float.Parse(y));
                }
                catch (Exception e)
                {
                }
                i++;
            }
            j++;
            rect.y += 20;
        }
         
        if (EditorGUI.EndChangeCheck())
        {
            prop.vectorValue = v4;
        }
    }
}
 
