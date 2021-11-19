using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PreviewHelper  
{
    public Editor gameObjectEditor;
    public Object lastObj;

    public void ShowGUI( UnityEngine.Object obj ,int width)
    {
        var rect = GUILayoutUtility.GetRect(width, width);
        GUIStyle bgColor = new GUIStyle();
        if(null != gameObjectEditor && lastObj != obj)
            GameObject.DestroyImmediate(gameObjectEditor);
        lastObj = obj;
        if (gameObjectEditor == null)
            gameObjectEditor = Editor.CreateEditor(obj);
        gameObjectEditor.OnInteractivePreviewGUI(rect, bgColor);
    }
    public void Release()
    {
        if (null != gameObjectEditor  )
            GameObject.DestroyImmediate(gameObjectEditor);
        lastObj = null;
    }
}
