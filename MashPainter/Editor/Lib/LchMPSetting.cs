using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LCH
{
    [System.Serializable]
    public class Setting
    {
        public string defaultTerrainShaderName = "Lch Mesh Painter/Scene Common";
        public string prefix = "_Splat";
        public string norPrefix = "_BumpMap";
        public string ctrlTextureName1 = "_Control";
        public string ctrlTextureName2 = "_Control2";
        public string mainTexArray = "_MainTexArray";
        //_MainTexArray
        public  int curLanguage = 0;

        public Color drawingColor = new Color(0f, 0f, 0.5f,0f);
        public Color brushColor = new Color(0.75f, 0.75f, 0.75f, 0f);

    }
    public partial class LchMeshPainter 
    {
        private static Setting setting = new Setting();

        public void SaveSetting()
        {
            setting.curLanguage = Languages.curLanguage ;
            string data = JsonUtility.ToJson(setting);
            System.IO.File.WriteAllText("lchMeshPainter.conf",data);
        }

        public void LoadSetting()
        {
            string path = "lchMeshPainter.conf";
            try
            {
                string json = System.IO.File.ReadAllText(path);
                setting = JsonUtility.FromJson<Setting>(json);
                Languages.curLanguage = setting.curLanguage;
            }
            catch (Exception e)
            {
            }
            
        }

    }
}