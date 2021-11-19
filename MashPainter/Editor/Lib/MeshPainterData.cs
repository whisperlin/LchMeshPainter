using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LCH
{
    public enum UVMark
    {
        UV0 = 0,
        UV1 = 1,
        UV2 = 2,
    }
    partial class LchMeshPainter
    {
        int editorLayer = 30;
        
        UVMark uvMark = UVMark.UV0;
        int panelType = 0;
        int vertexBrushType = 0;
        int meshPanelType = 0;
        int drawType;
        bool enable = true;
        bool isMouseOnMesh = false;
        bool isDragging = false;
        bool isTextureChannelDirty = false;
        bool isVertexDirty = false;
        bool isTextureChannelModify = false;
        bool isVertexModify = false;
        float dirtyTime = 0f;

        bool[] hasCtrl = new bool[] { false, false };
        int editingChannel = -1;

        bool textureNeedPaint = false;
        bool vertexNeedPaint = false;
        Texture2D[] ctrlText = new Texture2D[2] { null, null };
        RenderTexture[] editorCtrl1RT = new RenderTexture[2] { null, null };
        RenderTexture testRt;
        RenderTextureTypeUndoRedo texturePaninnerUndoRedo = new RenderTextureTypeUndoRedo();
        VertexUndoRedo vertexUndoRedo = new VertexUndoRedo();
        //Terrain
        TerrainTools terrainTools = new TerrainTools();

        LchUVAreaPerviewTexture uvAreaPerviewTexture = new LchUVAreaPerviewTexture();
        PreviewHelper previewHelper = new PreviewHelper();

        RenderTexture brushTexture;
        MeshFilter selectObject;

        Texture2DArray selectTexture2dArray;
 
        GUIContent[] t2dAryIcons8;
        GUIContent[] t2dAryIcons4;
        MeshRenderer selectMeshObjectRender;
        Material selectObjectMaterial;
        Material lastSelectObjectMaterial;
        MeshFilter lastSelectObject;
        MeshCollider selectMeshCollider;
        MeshRenderer selectMeshRender;
        GameObject brush;
        Material brushMat2;
        Material blendMat;
        Material brushing;
        Material perviewMat;
        Color vertexBrushColor = Color.red;

        Color[] baseColors = null;
        Mesh selectObjectMesh;
        Mesh lastSelectObjectMesh;

        void ReleaseIcons()
        {
            if (null != t2dAryIcons8)
            {
                for (int i = 0; i < 8; i++)
                {
                    GameObject.DestroyImmediate(t2dAryIcons8[i].image, true);
                }
            }

            t2dAryIcons8 = null;
            t2dAryIcons4 = null;
        }
        void CreateT2dAryIcons()
        {
             
             
            t2dAryIcons8 = new GUIContent[8];
            t2dAryIcons4 = new GUIContent[4];
            Material mat = new Material(Shader.Find("Hidden/Tex2dArayToTex"));
            for (int i = 0; i < 8; i++)
            {
                var ct = new GUIContent();
                if (null == selectTexture2dArray)
                {
                    ct.image = null;
                }
                else
                {
                    mat.SetInt("_Index",i);
                    RenderTexture rt = new RenderTexture(64, 64, 0);
                    Graphics.Blit(selectTexture2dArray, rt, mat);
                    ct.image = rt;
                }
                
                t2dAryIcons8[i] = ct;
                if (i < 4)
                    t2dAryIcons4[i] = ct;
            }
            GameObject.DestroyImmediate(mat, true);
        }

        void UpdateT2dAryIcons()
        {
            for (int i = 0; i < 8; i++)
            {
                string txt = Languages.GetValue(100+i, "Channel "+i);
                t2dAryIcons8[i].text = txt;
                if (i < 4)
                    t2dAryIcons4[i].text = txt;
            }
        }


    }
}

