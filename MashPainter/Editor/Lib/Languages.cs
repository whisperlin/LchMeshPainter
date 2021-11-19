using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;

namespace LCH
{
    public class Languages
    {
        public static string [] languageNames; 
        public static int curLanguage = 1;
        public static Dictionary<string, Dictionary<int, string>> languages = new Dictionary<string, Dictionary<int, string>>();
        public static string GetValue(int key,string defaultValue)
        {
            try
            {
                return languages[languageNames[curLanguage]][key];
            }
            catch (System.Exception e)
            {
                return defaultValue;
            }
            
        }
        public static void Init()
        {
            string[] results;
            results = AssetDatabase.FindAssets("LCHMeshPainterLanguage");
            if (results.Length == 0)
            {
                return;
            }
            string xmlPath = AssetDatabase.GUIDToAssetPath(results[0]);
            XmlDocument doc = new XmlDocument();
            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
            string  contents = System.IO.File.ReadAllText(xmlPath, utf8);
            doc.LoadXml(contents);
            List<string> ns = new List<string>();
            XmlNode xn = doc.SelectSingleNode("Languages");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                XmlElement xe = (XmlElement)xn1;
                string languageName = xe.GetAttribute("name");
                ns.Add(languageName);
                Dictionary<int, string> data = new Dictionary<int, string>();
                XmlNodeList items = xe.ChildNodes;
                foreach (XmlNode _item in items)
                {
                    XmlElement _i = (XmlElement)_item;
                    try
                    {
                        data[int.Parse(_i.GetAttribute("key"))] = _i.GetAttribute("value");
                    }
                    catch (System.Exception e)
                    {
                    }
                }
                languages[languageName] = data;
            }
            languageNames = ns.ToArray();
        }
        //LCHMeshPainterLanguage.xml
    }
}