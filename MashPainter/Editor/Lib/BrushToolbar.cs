using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LCH
{

    partial class LchMeshPainter
    {
       
        Texture[] brushMarks = null;
        int selectedBrush = 0;
        float brushStrength = 1f;
        float brushMaxStrength = 1f;
        float brushSize = 1f;
        string rootPath;

        void LoadBrushs()
        {
            if (brushMarks != null)
                return;
            RenderTexture rt = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Material mat = new Material(Shader.Find("Hidden/FirstBrush"));
            Graphics.Blit(rt,rt, mat);
            GameObject.DestroyImmediate(mat);
            string[] results;
            results = AssetDatabase.FindAssets("LMP Brushes");
            if (results.Length > 0)
            {
                rootPath = AssetDatabase.GUIDToAssetPath(results[0]);
            }
            results = AssetDatabase.FindAssets("t:texture2D", new string[] { rootPath });
            brushMarks = new Texture[results.Length+1];
            brushMarks[0] = rt;
            for (int i = 0; i < results.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(results[i]);
                Texture2D t= AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                brushMarks[i + 1] = t;
            }
        }
        void ReleaseBrush()
        {
            if (null != brushMarks[0])
                GameObject.DestroyImmediate(brushMarks[0]);
           
            
        }
        private void DrawTextureBrushsToolBar()
        {
            selectedBrush = GUILayout.Toolbar(selectedBrush, brushMarks, GUILayout.Height(30));
            EditorGUILayout.BeginHorizontal();

            
            GUILayout.Label(Languages.GetValue(9, "Brush strength"));
            brushStrength = EditorGUILayout.Slider(brushStrength, 0f, 1f);
            GUILayout.Label(Languages.GetValue(58, "Max strength"));
            brushMaxStrength = EditorGUILayout.Slider(brushMaxStrength, 0f, 1f);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Languages.GetValue(8, "Brush size"));
            brushSize = EditorGUILayout.Slider(brushSize, 0.01f, 50f);

            

            EditorGUI.BeginDisabledGroup(!texturePaninnerUndoRedo.CanUndo());
            if (GUILayout.Button(Languages.GetValue(10,"Undo")))
            {
                texturePaninnerUndoRedo.UnDo(editorCtrl1RT);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!texturePaninnerUndoRedo.CanRedo());
            if (GUILayout.Button(Languages.GetValue(11,"Redo")))
            {
                texturePaninnerUndoRedo.ReDo(editorCtrl1RT);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            
        }
        private void DrawVertexBrushsToolBar()
        {
            EditorGUI.BeginDisabledGroup(true);
            selectedBrush = 0;
            GUILayout.Toolbar(selectedBrush, brushMarks, GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Languages.GetValue(8, "Brush size"));
            brushSize = EditorGUILayout.Slider(brushSize, 0.2f, 50f);
            GUILayout.Label(Languages.GetValue(9, "Brush strength"));
            brushStrength = EditorGUILayout.Slider(brushStrength, 0f, 1f);
            EditorGUI.BeginDisabledGroup(!vertexUndoRedo.CanUndo());
            if (GUILayout.Button(Languages.GetValue(10, "Undo")))
            {
                vertexUndoRedo.UnDo(selectObjectMesh);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!vertexUndoRedo.CanRedo());
            if (GUILayout.Button(Languages.GetValue(11, "Redo")))
            {
                vertexUndoRedo.ReDo(selectObjectMesh);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
         
    }
}
 