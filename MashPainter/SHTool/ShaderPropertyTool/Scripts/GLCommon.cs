using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLCommon 
{
    static Material material;


    public static bool PosInRect(Rect rect, Vector2 pos,out Vector2 result)
    {
        if (rect.Contains(pos))
        {
            
    
            result.x = pos.x - rect.x;
            result.y = pos.y - rect.y;
            return true;
        }
        else
        {
            result.x = 0f;
            result.y = 0f;
            return false;
        }
        
    }
    public static void InitMaterial()
    {
        if (null == material)
            material = new Material(Shader.Find("Editor/Color"));
    }
    public static void SetClipRect(Rect layoutRectangle)
    {
        GUI.BeginClip(layoutRectangle);
        GL.PushMatrix();
        
    }
    public static void EndClip()
    {
        GL.PopMatrix();
        GUI.EndClip();
    }

    public static void SetColor(Color color)
    {
        InitMaterial();
        material.SetColor("_Color", color);
        material.SetPass(0);
    }
    public static void DrawRect(Rect layoutRectangle)
    {
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

      
         
        GL.Vertex3(layoutRectangle.x, layoutRectangle.y, 0);
        GL.Vertex3(layoutRectangle.x+layoutRectangle.width, layoutRectangle.y, 0);
        GL.Vertex3(layoutRectangle.x + layoutRectangle.width, layoutRectangle.y+layoutRectangle.height, 0);
        GL.Vertex3(layoutRectangle.x , layoutRectangle.y+layoutRectangle.height, 0);
        GL.End();
    }

    public static void DrawRectUV(Rect layoutRectangle)
    {
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

        GL.TexCoord(new Vector3(0, 0, 0));
        GL.Vertex3(layoutRectangle.x, layoutRectangle.y, 0);
        GL.TexCoord(new Vector3(1, 0, 0));
        GL.Vertex3(layoutRectangle.x+layoutRectangle.width, layoutRectangle.y, 0);
        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex3(layoutRectangle.x+layoutRectangle.width, layoutRectangle.y+layoutRectangle.height, 0);
        GL.TexCoord(new Vector3(0, 1, 0));
        GL.Vertex3(layoutRectangle.x, layoutRectangle.y+layoutRectangle.height, 0);
        GL.End();
    }
    public static void DrawLine(Vector3 begin ,Vector3 end)
    {
        GL.Begin(GL.LINES);
        GL.Vertex3(begin.x, begin.y, begin.z);
        GL.Vertex3(end.x, end.y, end.z);
        GL.End();
    }

    
}
