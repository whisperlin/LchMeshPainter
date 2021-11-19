using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using UnityEngine.SceneManagement;

namespace LCH
{
    public partial class LchMeshPainter  
    {
        
        public bool is2019OrNew = true;
        public EditorWindow window;
        /*[MenuItem("Tools/Lch Mesh Painter")]
        static void Init()
        {
            LchMeshPainter window = (LchMeshPainter)EditorWindow.GetWindow(typeof(LchMeshPainter));
            window.title = "Lch Mesh Painter";
            window.Show();
        }*/
        public void OnGUI()
        {
            OnDrawMainPanel();
            HandleKeyboard();
            if (selectObject == null)
            {
                if (null != brush)
                {
                    GameObject.DestroyImmediate(brush, true);
                }
                if (null != rtCamera)
                {
                    GameObject.DestroyImmediate(rtCamera, true);
                }

            }
        }
        private void CloseSelf(Scene arg0, Scene arg1)
        {
            window.Close();
        }
        private void CloseSelf1(Scene arg0)
        {
            window.Close();
        }
        public void OnDisable()
        {
            CheckUnSave();
            if (is2019OrNew)
            {
                SceneView.beforeSceneGui -= UpdateWindow;
                SceneManager.activeSceneChanged += CloseSelf;
                SceneManager.sceneUnloaded += CloseSelf1;
            }
            else
            {
               
                SceneView.onSceneGUIDelegate -= UpdateWindow;
               
                SceneManager.activeSceneChanged += CloseSelf;
                SceneManager.sceneUnloaded += CloseSelf1;
                
            }
 
            
            EditorApplication.update -= Update;
            ReleaseEditorObject();
            if (null != selectObjectMaterial)
            {
                selectObjectMaterial.SetTexture(setting.ctrlTextureName1, ctrlText[0]);
                selectObjectMaterial.SetTexture(setting.ctrlTextureName2, ctrlText[1]);
            }
            foreach (var t in editorCtrl1RT)
            {
                GameObject.DestroyImmediate(t, true);
            }
            texturePaninnerUndoRedo.Clear();
            vertexUndoRedo.Clear();
            uvAreaPerviewTexture.Clear();
            ReleaseBrush();
            if (null != selectObjectMesh)
            {
                selectObjectMesh.colors = baseColors;
            }
            SaveSetting();
            
        }

        

        void CheckUnSave()
        {
            if (isTextureChannelModify)
            {
                isTextureChannelModify = false;
                if (EditorUtility.DisplayDialog("", Languages.GetValue(49,"Save Texture channels Modify?"), Languages.GetValue(47, "Yes"), Languages.GetValue(48, "No")))
                {
                    SaveTextures();
                }

            }
            if (isVertexDirty)
            {
                isVertexDirty = false;
                if (EditorUtility.DisplayDialog("", Languages.GetValue(50, "Save Vertex color Modify?"), Languages.GetValue(47, "Yes"), Languages.GetValue(48, "No")))
                {
                    string path = AssetDatabase.GetAssetPath(selectObjectMesh);
                    if (path.EndsWith(".mesh"))
                    {
                        selectObjectMesh.colors = baseColors;
                        EditorUtility.SetDirty(selectObjectMesh);
                    }
                    else
                    {
                        string _path = EditorUtility.SaveFilePanelInProject(Languages.GetValue(21, "Save mesh"), "mesh", "mesh", Languages.GetValue(22, "save mesh to your project"));
                        if (_path.Length > 0)
                        {
                            //Debug.LogError(_path);
                            Mesh m = (Mesh)GameObject.Instantiate(selectObjectMesh);
                            AssetDatabase.CreateAsset(m, _path);
                            selectObjectMesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>(_path);
                            selectObject.sharedMesh = selectObjectMesh;
                            baseColors = selectObjectMesh.colors;
                        }
                    }
                }
            }
        }
        public void OnEnable()
        {
            isTextureChannelModify = false;
            isVertexModify = false;
            LoadSetting();
            Languages.Init();
            LoadBrushs();

            if (null == brushTexture)
                brushTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            if (is2019OrNew)
            {
                SceneView.beforeSceneGui -= UpdateWindow;
                SceneView.beforeSceneGui += UpdateWindow;
            }
            else
            {
                SceneView.onSceneGUIDelegate -= UpdateWindow;
                SceneView.onSceneGUIDelegate += UpdateWindow;
                
            }
 
            EditorApplication.update += Update;

        }
        void ReleaseEditorObject()
        {
            ReleaseMeshCollider();
            if (null != brush)
                GameObject.DestroyImmediate(brush, true);
            if (null != brushTexture)
                GameObject.DestroyImmediate(brushTexture, true);
            if (null != rtCamera)
                GameObject.DestroyImmediate(rtCamera.gameObject, true);
            if (null != brushing)
                GameObject.DestroyImmediate(brushing, true);
            terrainTools.Release();
            previewHelper.Release();

            ReleaseIcons();

        }

        void OnDrawMainPanel()
        {
            panelType = GUILayout.Toolbar(panelType, new string[] { Languages.GetValue(0,"Mesh Painter"), Languages.GetValue(1, "Terrain Tool"), Languages.GetValue(2, "Build tree tools"), Languages.GetValue(3, "Setting"), Languages.GetValue(4, "Help") });
            switch (panelType)
            {
                case 0:
                    {
                        OnDrawPainterPanel();
                    }
                    break;
                case 1:
                    {
                        OnDrawMeshPanel();
                    }
                    break;
                case 2:
                    {
                        OnDrawTreePainter();
                        
                    }
                    break;
                case 3:
                    {
                        OnDrawSetting();
                    }
                    break;
                case 4:
                    {
                        OnDrawHelp();
                    }
                    break;
            }
        }

        

        void OnDrawMeshPanel()
        {
            meshPanelType = 0;
            TerrainToMesh(); 
            
        }
        MeshTools meshtools = new MeshTools();
        void UnityMeshObj()
        {
            EditorGUI.BeginChangeCheck();
            meshtools.selectingObj = (GameObject)EditorGUILayout.ObjectField("FBX or Obj:", meshtools.selectingObj, typeof(GameObject), false);
            if (null != meshtools.selectingObj)
            {
                meshtools.path = AssetDatabase.GetAssetPath(meshtools.selectingObj);
                meshtools.Check();
            }
            meshtools.smoonthNormalToColor = EditorGUILayout.Toggle("Write the Smoomthed Normal To Color for Cartoon Render", meshtools.smoonthNormalToColor);
            EditorGUI.BeginDisabledGroup(meshtools.selectingObj ==null);
            if (GUILayout.Button("Save"))
            {
                string path = EditorUtility.SaveFolderPanel("Select folder to Export", ".", "");
                if (path.Length > 0)
                {
                    path = path.Substring(Application.dataPath.Length - 6);
                    meshtools.Save(path);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        void TerrainToMesh()
        {
            EditorGUI.BeginChangeCheck();

            //你可以把unity地形转换为unity网格，它拥有更少的面数。
            GUILayout.Label(Languages.GetValue(29, "You can convert unity terrain to unity grid, which has fewer faces."));
            terrainTools.editorTerrain = (Terrain)EditorGUILayout.ObjectField(Languages.GetValue(30, "Terrain:"), terrainTools.editorTerrain, typeof(Terrain), true);
   
            if (EditorGUI.EndChangeCheck())
            {
                if (null != terrainTools.editorTerrain)
                {
                    terrainTools.MakeMesh();
                    terrainTools.CreateLodMesh();
                }
            }
            if (null != terrainTools.editorTerrain )
            {
                if (null != terrainTools.lodMesh && null != terrainTools.baseMesh)
                {
                    EditorGUI.BeginChangeCheck();
                    terrainTools.face = EditorGUILayout.IntSlider(Languages.GetValue(31, "face"), terrainTools.face, 2000, terrainTools.baseMesh.vertexCount);
                    if (EditorGUI.EndChangeCheck())
                    {
                        terrainTools.CreateLodMesh();
                    }
                    if (GUILayout.Button(Languages.GetValue(16,"Save")))
                    {
                        string path = EditorUtility.SaveFolderPanel(Languages.GetValue(31, "Select a folder in project to export terrain"), ".", "");

                        if (path.Length > 0)
                        {
                            string root = Application.dataPath.Substring(0,Application.dataPath.Length - 6);
                            if (path.StartsWith(root))
                            {
                                path = path.Substring(root.Length);
                                terrainTools.Save(path+"/");
                            }
                            else
                            {
                                EditorUtility.DisplayDialog(Languages.GetValue(33,"error"), Languages.GetValue(34, "Selected folder is not in project"), Languages.GetValue(35, "ok"));
                            }
                        }
                    }
                    previewHelper.ShowGUI(terrainTools.lodMesh, 300);
                }
                    
            }
            
        }

        void OnDrawSetting()
        {
            EditorGUI.BeginChangeCheck();
            setting.ctrlTextureName1 = EditorGUILayout.TextField(Languages.GetValue(13, "Control Texture 1"), setting.ctrlTextureName1);
            setting.ctrlTextureName2 = EditorGUILayout.TextField(Languages.GetValue(14, "Control Texture 2"), setting.ctrlTextureName2);
            setting.mainTexArray = EditorGUILayout.TextField(Languages.GetValue(59, "Texture2dArray "), setting.mainTexArray);
   

            setting.defaultTerrainShaderName = EditorGUILayout.TextField(Languages.GetValue(56, "Terrain default Shader name"), setting.defaultTerrainShaderName);
            setting.prefix = EditorGUILayout.TextField(Languages.GetValue(13, "Layer Texture  prefix"), setting.prefix);
            setting.norPrefix = EditorGUILayout.TextField(Languages.GetValue(14, "Normal Texture Prefix"), setting.norPrefix);

            GUILayout.Label(Languages.GetValue(52, "Export the terrain layers texture to  property of the material at ") + setting.prefix + "0," + setting.prefix + "1, " + setting.prefix + "2... ");
            GUILayout.Label(Languages.GetValue(53, "Export the terrain normals texture to  property of the material at ") + setting.norPrefix + "0," + setting.norPrefix + "1, " + setting.norPrefix + "2... ");

            setting.brushColor = EditorGUILayout.ColorField(Languages.GetValue(60,"Brush Color"), setting.brushColor);
            setting.drawingColor = EditorGUILayout.ColorField(Languages.GetValue(61, "Drawing Brush Color"),setting.drawingColor);
            
            //public Color drawingColor = new Color(0f, 0f, 0.5f, 0f);
            //public Color brushColor = new Color(0.5f, 0.5f, 0.5f, 0f);

            editorLayer = EditorGUILayout.IntSlider(Languages.GetValue(24, "Editor used Layer:"), editorLayer, 0, 30);
            if (Languages.languageNames.Length>0)
                Languages.curLanguage = EditorGUILayout.Popup(Languages.GetValue(25, "Language:"),Languages.curLanguage, Languages.languageNames);

            if (EditorGUI.EndChangeCheck())
            {
                SaveSetting();
            }
        }
        void OnSelectObjectChange()
        {
            selectMeshObjectRender = null;
            selectObjectMaterial = null;
            hasCtrl[0] = false;
            hasCtrl[1] = false;

            ctrlText[0] = null;
            ctrlText[1] = null;
            if (null != editorCtrl1RT[0])
            {
                GameObject.DestroyImmediate(editorCtrl1RT[0],true);
               
                editorCtrl1RT[0] = null;
            }
            if (null != editorCtrl1RT[1])
            {
                GameObject.DestroyImmediate(editorCtrl1RT[1], true);
                editorCtrl1RT[1] = null;
            }
            
            DrawUV();
        }
        void DrawUV()
        {
            var mesh = selectObject.sharedMesh;
            
            if (uvMark == UVMark.UV1)
            {
                if (mesh.uv2.Length == 0)
                {
                    EditorUtility.DisplayDialog(Languages.GetValue( 36,"Not uv1 on mesh"), Languages.GetValue(36, "Not uv1 on mesh"), Languages.GetValue(35, "ok"));
                    uvMark = UVMark.UV0;
                }
                else
                {
                    uvAreaPerviewTexture.DrawUVs(mesh.uv2, mesh.triangles);
                    return;
                }
                
            }
            if (uvMark == UVMark.UV2)
            {
                if (mesh.uv2.Length == 0)
                {
                    EditorUtility.DisplayDialog(Languages.GetValue(37, "Not uv2 on mesh"), Languages.GetValue(37, "Not uv2 on mesh"), Languages.GetValue(35, "ok"));
                    uvMark = UVMark.UV0;
                }
                else
                {
                    uvAreaPerviewTexture.DrawUVs(mesh.uv2, mesh.triangles);
                    return;
                }
            }
            uvAreaPerviewTexture.DrawUVs(mesh.uv, mesh.triangles);
        }
        void ReplayCtrl1Texture()
        {
           
            Texture t = selectObjectMaterial.GetTexture(setting.ctrlTextureName1);
            if (t is Texture2D)
            {
                ctrlText[0] = (Texture2D)t;
            }

        }
        void ReplayCtrl2Texture()
        {
            Texture t = selectObjectMaterial.GetTexture(setting.ctrlTextureName2); ;
            if (t is Texture2D)
            {
                ctrlText[1] = (Texture2D)t;
            }
        }
        void OnMaterialChange(Material old)
        {

            if (null != old)
            {
                old.SetTexture(setting.ctrlTextureName1, ctrlText[0]);
                old.SetTexture(setting.ctrlTextureName2, ctrlText[1]);
            }
            ctrlText[0] = null;
            ctrlText[1] = null;

            hasCtrl[0] = false;
            hasCtrl[1] = false;
            Shader shader = selectObjectMaterial.shader;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                var ps = shader.GetPropertyAttributes(i);
                string name = shader.GetPropertyName(i);
                if (name == setting.ctrlTextureName1)
                    hasCtrl[0] = true;
                if (name == setting.ctrlTextureName2)
                    hasCtrl[1] = true;
            }
 
            if (hasCtrl[1])
                ReplayCtrl1Texture();
            if (hasCtrl[1])
                ReplayCtrl2Texture();
            texturePaninnerUndoRedo.Clear();
            if (null != ctrlText[0] || null != ctrlText[1])
            {
                texturePaninnerUndoRedo.Add(ctrlText);
            }

            if (null != selectObjectMaterial)
            {
                if (selectObjectMaterial.IsKeywordEnabled("_CHANNEL_UV2"))
                {
                    uvMark = UVMark.UV2;
                }
                else if (selectObjectMaterial.IsKeywordEnabled("_CHANNEL_UV1"))
                {
                    uvMark = UVMark.UV1;
                }
                else
                {
                    uvMark = UVMark.UV0;
                }
            }
        }

        private Texture2D SaveRenderTextureToTga(RenderTexture rt, string path, bool newTexture)
        {
            RenderTexture prev = RenderTexture.active;
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Texture2D tga = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32,false,true);
            tga.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            byte[] bytes = LchEncode.EncodeToTGA(tga, 4);
            System.IO.DirectoryInfo topDir = System.IO.Directory.GetParent(path);
            if (!topDir.Exists)
            {
                topDir.Create();
            }
            System.IO.File.WriteAllBytes(path, bytes);
           
            RenderTexture.active = prev;
            rt.filterMode = FilterMode.Bilinear;
            if (newTexture)
                AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ; 
            importer.SaveAndReimport();
            
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
          
            AssetDatabase.SaveAssets();
            Texture2D.DestroyImmediate(tga);
            tga = null;
            return t;
        }

        void SaveTextures()
        {
            string path = null;
            for (int i = 0; i < 2; i++)
            {
                if (null != editorCtrl1RT[i] && hasCtrl[i])
                {
                    bool newTexture = false;

                    if (null == ctrlText[i])
                    {
                        if (null == path)
                        {
                            path = EditorUtility.SaveFilePanelInProject("Channels " + i + " saved poisition", " channel1", "tga", "Where is channel one saved");
                        }
                        else
                        {
                            path = path.Substring(0, path.Length - 4) + "_ctrl2." + path.Substring(path.Length - 3);
                        }
                        newTexture = true;
                    }
                    else
                    {
                        path = AssetDatabase.GetAssetPath(ctrlText[i]);
                    }
                    if (path.Length > 0)
                    {
                        ctrlText[i] = SaveRenderTextureToTga(editorCtrl1RT[i], path, newTexture);
                    }
                }
            }
        }
        void OnDrawVertexPainter()
        {
            EditorGUI.BeginChangeCheck();
            
            vertexBrushType = EditorGUILayout.Popup(Languages.GetValue(17, "Brush type:"),vertexBrushType, new string[] { Languages.GetValue(18, "Channels brush"), Languages.GetValue(19, "Custom Color")  });
            bool b = EditorGUI.EndChangeCheck();
           
             
            switch (vertexBrushType)
            {
                case 0:
                    {
                        GUILayout.Label(Languages.GetValue(15,"Channels:"));
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space(30, false);
                        string[] fourChanges = new string[4] {Languages.GetValue(100, "Channel 1"), Languages.GetValue(101, "Channel 2"), Languages.GetValue(102, "Channel 3"), Languages.GetValue(103, "Channel 4") };
                        editingChannel = GUILayout.SelectionGrid(editingChannel, fourChanges, 1);
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                case 1:
                    {
                        vertexBrushColor = EditorGUILayout.ColorField(Languages.GetValue(20,"Brush Color:"), vertexBrushColor);
                    }
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Languages.GetValue(16, "Save")))
            {
                string path = AssetDatabase.GetAssetPath(selectObjectMesh);
                if (path.EndsWith(".mesh"))
                {
                    selectObjectMesh.colors = baseColors;
                    EditorUtility.SetDirty(selectObjectMesh);
                }
                else
                {
                    string _path = EditorUtility.SaveFilePanelInProject(Languages.GetValue( 21,"Save mesh"), "mesh", "mesh", Languages.GetValue(22, "save mesh to your project"));
                    if (_path.Length > 0)
                    {
                        //Debug.LogError(_path);
                        Mesh m = (Mesh)GameObject.Instantiate(selectObjectMesh);
                        AssetDatabase.CreateAsset(m, _path);
                        selectObjectMesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>(_path);
                        selectObject.sharedMesh = selectObjectMesh;
                        baseColors = selectObjectMesh.colors;
                    }
                }
            }
            EditorGUILayout.Space(20, true);
            EditorGUILayout.EndHorizontal();
        }
        void OnDrawTexturePainter()
        {
            EditorGUI.BeginChangeCheck();
            uvMark = (UVMark)EditorGUILayout.EnumPopup(Languages.GetValue(12, "UV Channel:"), uvMark);
            bool b = EditorGUI.EndChangeCheck();
            if (null == selectObject)
                return;             
            if (b)
            {
                DrawUV();
            }
            GUILayout.Label(Languages.GetValue(13, "Control Texture 1 :") + setting.ctrlTextureName1);
            GUILayout.Label(Languages.GetValue(14, "Control Texture 2 :") + setting.ctrlTextureName2);
            GUILayout.Label(Languages.GetValue(59, "Texture2dArray :") + setting.mainTexArray);
            
            selectObjectMaterial = null;
            if (null == selectMeshObjectRender)
            {
                selectMeshObjectRender = selectObject.gameObject.GetComponent<MeshRenderer>();
            }
            if (null != selectMeshObjectRender)
            {
                selectObjectMaterial = selectMeshObjectRender.sharedMaterial;
            }
             
            if (lastSelectObjectMaterial != selectObjectMaterial)
            {
                OnMaterialChange(lastSelectObjectMaterial);
                lastSelectObjectMaterial = selectObjectMaterial;
            }
            else
            {
                if (uvMark == UVMark.UV0)
                {
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV2");
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV1");
                    selectObjectMaterial.EnableKeyword("_CHANNEL_UV0");

                }
                else if (uvMark == UVMark.UV1)
                {
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV2");
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV0");
                    selectObjectMaterial.EnableKeyword("_CHANNEL_UV1");

                }
                else if (uvMark == UVMark.UV2)
                {
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV0");
                    selectObjectMaterial.DisableKeyword("_CHANNEL_UV1");
                    selectObjectMaterial.EnableKeyword("__CHANNEL_UV2");

                }
            }
            if (null == selectObjectMaterial)
            {
                hasCtrl[0] = false;
                hasCtrl[1] = false;
                editingChannel = -1;
            }
            else
            {
                
                GUILayout.Label(Languages.GetValue(15, "Editing channel:"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(30, false);

                Texture2DArray t2d = null;
                hasCtrl[0] = false;
                hasCtrl[1] = false;
                Shader shader = selectObjectMaterial.shader;
                for (int i = 0; i < shader.GetPropertyCount(); i++)
                {
                    var ps = shader.GetPropertyAttributes(i);
                    string name = shader.GetPropertyName(i);
                    if (name == setting.ctrlTextureName1)
                        hasCtrl[0] = true;
                    if (name == setting.ctrlTextureName2)
                        hasCtrl[1] = true;

                    if (name == setting.mainTexArray)
                    {
                        t2d = (Texture2DArray)selectObjectMaterial.GetTexture(setting.mainTexArray);
                    }
                }
                
                if(selectTexture2dArray != t2d||null== t2dAryIcons8)
                {

                    selectTexture2dArray = t2d;
                    CreateT2dAryIcons();

                }
                UpdateT2dAryIcons();

                if (hasCtrl[0])
                {
                    ReplayCtrl1Texture();
                    if (editingChannel < 0)
                        editingChannel = 0;
                    if (hasCtrl[1])
                    {
                        ReplayCtrl2Texture();
                        editingChannel = GUILayout.SelectionGrid(editingChannel, t2dAryIcons8,null == selectTexture2dArray? 1:8);
                    }
                    else
                    {
                        editingChannel = GUILayout.SelectionGrid(editingChannel, t2dAryIcons4, null == selectTexture2dArray ? 1 : 8);
                    }
                }
                else
                {
                    editingChannel = -1;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(Languages.GetValue(16, "Save")))
                {
                    SaveTextures();
                }
                EditorGUILayout.Space(20, true);
                EditorGUILayout.EndHorizontal();
            }
        }
        void OnDrawPainterPanel()
        {
            EditorGUI.BeginChangeCheck();
            selectObject = (MeshFilter)EditorGUILayout.ObjectField(Languages.GetValue(51, "Object to be planted"), selectObject, typeof(MeshFilter), true);
            bool selectObjectChanged = EditorGUI.EndChangeCheck();
            if (selectObjectChanged)
            {
                OnSelectObjectChange();
            }
            enable = DrawEnableCtrl(Languages.GetValue( 5,"Working .."), Languages.GetValue(6, "Disable"), enable);
            drawType = GUILayout.Toolbar(drawType, new string[] { Languages.GetValue(7, "Paint to texture"), Languages.GetValue(-8, "Paint to Vertex") });
            if (drawType == 0)
            {
                DrawTextureBrushsToolBar();
                OnDrawTexturePainter();
            }
            else
            {
                DrawVertexBrushsToolBar();
                OnDrawVertexPainter();
            }
            if (null != brushTexture)
            {
                if (null == perviewMat)
                {
                    perviewMat = new Material(Shader.Find("Hidden/PreviewTexture"));
                }
                Rect rect = GUILayoutUtility.GetRect(300f, 300f);
                rect.width = 300f;
                perviewMat.SetTexture("_Tex2", uvAreaPerviewTexture.texture);

                float h = window.position.height - rect.y-5;
                if (h > rect.width)
                {
                    rect.width = h;
                    rect.height = h;
                }
                EditorGUI.DrawPreviewTexture(rect, brushTexture, perviewMat);
            }
        }

        private void UpdateWindow(SceneView sceneView)
        {
            if (enable )
            {
                ctrlIsDown = Event.current.control;
                switch (panelType)
                {
                    case 0:
                        {
                            if (selectObject != null)
                            {
                                Selection.activeGameObject = selectObject.gameObject;
                                CheckMeshCollider();
                                PainterPanelUpdate(sceneView);
                                HandleKeyboard();
                            }
                            
                        }
                        break;
                    case 2:
                        {
                            //plantObject
                            Selection.activeGameObject = null;
                            CheckBuildTreeCollider();
                        }
                        break;
                }
                
            }
        }
       
        
        void CheckBuildTreeCollider()
        {
            Vector2 mousePosition = Event.current.mousePosition;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            Vector3 point = Vector3.zero;

            //int id = GUIUtility.GetControlID(FocusType.Passive);
            treeRay = HandleUtility.GUIPointToWorldRay(mousePosition);

            
            treeBrushHit = Physics.Raycast(treeRay, out treeHit, 5000f, -1);
            SphereCapPos(treeHit.point, treeBrushSize, treeBrushHit);


            
            if (null != treeObj)
            {
                if (null != Event.current)
                {
                    
                    if (Event.current.button == 0)
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            if (Event.current.control)
                            {
                                DeleteObject(treeHit.point);
                            }
                            else
                            {
                                AddObject(treeRay, treeHit.point, treeHit.distance, treeHit.normal);
                            }

                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseMove)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseDrag)
                        {
                            if (Event.current.control)
                            {
                                DeleteObject(treeHit.point);
                            }
                            else
                            {
                                AddObject(treeRay, treeHit.point, treeHit.distance, treeHit.normal);
                            }
                            Event.current.Use();
                        }
                    }
                }
                /*else
                {
                    Debug.LogError("null event");
                }*/
            }
            
            



        }
        void CheckMeshCollider()
        {
            if (lastSelectObject != selectObject)
            {
                ReleaseMeshCollider();
                GameObject g = new GameObject("Collider Mesh");
                g.hideFlags = HideFlags.DontSaveInEditor| HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
                MeshCollider mc = g.AddComponent<MeshCollider>();
                mc.sharedMesh = selectObject.sharedMesh;
                mc.convex = false;
                selectMeshCollider = mc;
                lastSelectObject = selectObject;
                if (null == brushMat2)
                    brushMat2 = new Material(Shader.Find("Hidden/BrushToUV1"));
                MeshFilter mf = g.AddComponent<MeshFilter>();
                mf.sharedMesh = selectObject.sharedMesh;
                selectMeshRender = g.AddComponent<MeshRenderer>();
            }
            if (null != blendMat)
            {
                if (isDragging)
                {
                    blendMat.SetColor("_Color", setting.drawingColor);
                }
                else
                {
                    blendMat.SetColor("_Color", setting.brushColor);
                }
            }
        }
        void ReleaseMeshCollider()
        {
            if (null != selectMeshRender)
                GameObject.DestroyImmediate(selectMeshRender.gameObject, true);
            lastSelectObject = null;

             
        }

        void FixColliderPosition()
        {
            if (null != selectMeshCollider)
            {
                selectMeshCollider.transform.parent = selectObject.transform.parent;
                selectMeshCollider.transform.position = selectObject.transform.position;
                selectMeshCollider.transform.localRotation = selectObject.transform.localRotation;
                selectMeshCollider.transform.localScale = selectObject.transform.localScale;
            }
        }

        Camera sceneCamera = null;
        void UpdateBrush(SceneView sceneView)
        {
            Camera cam = sceneView.camera;
            sceneCamera = cam;
            FixColliderPosition();
            Vector2 mousePosition = Event.current.mousePosition;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            selectMeshCollider.gameObject.hideFlags = HideFlags.DontSaveInEditor| HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;

            RaycastHit raycastHit;
            if (selectMeshCollider.Raycast(ray, out raycastHit, 5000f))
            {
                isMouseOnMesh = true;
                brush.transform.position = raycastHit.point;
                brush.transform.up = cam.transform.up;
                brush.transform.LookAt(raycastHit.point + ray.direction, cam.transform.up);
                brush.transform.localScale = new Vector3(brushSize, brushSize, brushSize);
                Shader.SetGlobalMatrix("worldToBrush", brush.transform.worldToLocalMatrix);
            }
            else
            {
                isMouseOnMesh = false;
                brush.transform.position = new Vector3(-10000f, -10000f, -10000f);
                brush.transform.forward = Vector3.forward;
                brush.transform.localScale = new Vector3(brushSize, brushSize, brushSize);

                Shader.SetGlobalMatrix("worldToBrush", brush.transform.worldToLocalMatrix);
            }
        }

        Camera rtCamera;
        void DrawBrushTexture()
        {
            if (null == sceneCamera)
                return;
            if (null != lastSelectObject && null != brushMat2)
            {
                if (null == rtCamera)
                {
                    GameObject g = new GameObject("Brush Camrea");
                    rtCamera = g.AddComponent<Camera>();
                    g.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
                }

                rtCamera.enabled = false;

                rtCamera.clearFlags = CameraClearFlags.SolidColor;
                rtCamera.fieldOfView = 120f;

                rtCamera.targetTexture = brushTexture;
                rtCamera.transform.position = sceneCamera.transform.position;
                rtCamera.transform.rotation = sceneCamera.transform.rotation;

                rtCamera.backgroundColor = Color.black;
                rtCamera.cullingMask = 1 << 30;
                selectMeshRender.gameObject.layer = 30;
                brushMat2.SetTexture("_MainTex", brushMarks[selectedBrush]);
                selectMeshRender.sharedMaterial = brushMat2;

                if (uvMark == UVMark.UV0)
                {
                    Shader.DisableKeyword("_CHANNEL_UV2");
                    Shader.DisableKeyword("_CHANNEL_UV1");
                    Shader.EnableKeyword( "_CHANNEL_UV0");
 
                }
                else if (uvMark == UVMark.UV1)
                {
                    Shader.DisableKeyword("_CHANNEL_UV2");
                    Shader.DisableKeyword("_CHANNEL_UV0");
                    Shader.EnableKeyword( "_CHANNEL_UV1");
 
                }
                else if (uvMark == UVMark.UV2)
                {
                    Shader.DisableKeyword("_CHANNEL_UV0");
                    Shader.DisableKeyword("_CHANNEL_UV1");
                    Shader.EnableKeyword( "_CHANNEL_UV2");
       
                }
                rtCamera.Render();

                if (null == blendMat)
                    blendMat = new Material(Shader.Find("Hidden/BrushBlend"));
                blendMat.SetTexture("_MainTex", brushTexture);


                
                selectMeshRender.sharedMaterial = blendMat;
            }
        }

        private void PainterPanelUpdate(SceneView sceneView)
        {
            if (null == brush)
            {
                brush = new GameObject("Brush");
                brush.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            }
            if (null != selectMeshCollider)
            {
                if (null != Event.current)
                {
                    UpdateBrush(sceneView);

                    if (Event.current.button == 0)
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            isDragging = true;
                            OnLeftMouseDown();
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            isDragging = false;
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseMove)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseDrag)
                        {
                            OnLeftMouseDraging();
                            Event.current.Use();
                        }
                    }
                }
                else
                {
                    Debug.LogError("null event");
                }
            }
            else
            {
                //Debug.LogError("selectMeshCollider = null");
            }
        }
        void HandleKeyboard()
        {
            if (null != Event.current && Event.current.type == EventType.KeyDown && Event.current.control && Event.current.control )
            {
                if (Event.current.keyCode == KeyCode.Z)
                {
                    if (drawType == 0)
                    {
                        texturePaninnerUndoRedo.UnDo(editorCtrl1RT);
                    }
                    else
                    {
                        vertexUndoRedo.UnDo(selectObjectMesh);
                    }
                }
                else if (Event.current.keyCode == KeyCode.Y)
                {
                    if (drawType == 0)
                    {
                        texturePaninnerUndoRedo.ReDo(editorCtrl1RT);
                    }
                    else
                    {
                        vertexUndoRedo.ReDo(selectObjectMesh);
                    }
                }


                
            }
        }
        void UpdatePainterPanel()
        {
            DrawBrushTexture();
            if (textureNeedPaint)
            {
                OnPlaint();
                textureNeedPaint = false;
            }
            if (isTextureChannelDirty)
            {
                isTextureChannelModify = true;
                if (isDragging)
                {
                    dirtyTime = 0f;
                }
                else
                {
                    dirtyTime += Time.deltaTime;
                }
                if (dirtyTime > 2f)
                {
                    isTextureChannelDirty = false;
                    texturePaninnerUndoRedo.Add(editorCtrl1RT);
                }
            }
            if (isVertexDirty)
            {
                
                isVertexModify = true;
                if (isDragging)
                {
                    dirtyTime = 0f;
                }
                else
                {
                    dirtyTime += Time.deltaTime;
                }
                if (dirtyTime > 2f)
                {
                    isVertexDirty = false;
                    if (null != selectObjectMesh)
                        vertexUndoRedo.Add(selectObjectMesh.colors);
                }
            }
            if (vertexNeedPaint)
            {
                OnPlaintVertex();
                vertexNeedPaint = false;
            }
            if (!textureNeedPaint && !vertexNeedPaint)
            {
                window.Repaint();
            }
        }
        private void Update()
        {
            if (null == selectObject)
            {
                selectObjectMesh = null;
            }
            else
            {
                selectObjectMesh = selectObject.sharedMesh;
            }
            if (selectObjectMesh != lastSelectObjectMesh)
            {
                lastSelectObjectMesh = selectObjectMesh;
                OnSelectedMeshChange();
            }
            uvAreaPerviewTexture.Update ();

            if (enable)
            {

                switch (panelType)
                {
                    case 0:
                        {

                            UpdatePainterPanel();
                        }
                        break;
                    case 2:
                        {
                            UpdateBuildingTree();
                        }
                        break;
                }

            }
            SceneView.RepaintAll();
            
        }
        void UpdateBuildingTree()
        {
            
        }
        private void OnLeftMouseDown()
        {
            if (enable)
            {
                switch (panelType)
                {
                    case 0:
                        {
                            if (drawType == 0)
                            {
                                textureNeedPaint = true;
                            }
                            else
                            {
                                vertexNeedPaint = true;
                            }

                        }
                        break;
                    case 2:
                        {
                            
                        }
                        break;
                }

            }
            
            
        }
        void OnPlaintVertex()
        {
            if (!isDragging)
                return;
            if (!isMouseOnMesh)
                return;
            if (!sceneCamera)
                return;
            if (selectObjectMesh == null)
                return;
            if (null == brush)
                return;

            Vector3 [] vertices = selectObjectMesh.vertices;
            Vector3[] normals = selectObjectMesh.normals;
            Color[] colors = selectObjectMesh.colors;
            if (colors.Length == 0)
            {
                colors = new Color[vertices.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
            }
            var localToWorld = selectObject.transform.localToWorldMatrix;
            var viewForward = sceneCamera.transform.forward;

            var worldToBrush = brush.transform.worldToLocalMatrix;

            
            if (vertexBrushType ==0)
            {
                switch (editingChannel)
                {
                    case 1:
                        vertexBrushColor = new Color(0f, 1f, 0f, 0f);
                        break;
                    case 2:
                        vertexBrushColor = new Color(0f, 0f, 1f, 0f);
                        break;
                    case 3:
                        vertexBrushColor = new Color(0f, 0f, 0f, 1f);
                        break;
                    default:
                        vertexBrushColor = new Color(1f, 0f, 0f, 0f);
                        break;
                }
            }
            
            for (int i = 0; i < vertices.Length; i++)
            {
                var worldForward = localToWorld.MultiplyVector(normals[i]);
                if (Vector3.Dot(worldForward, viewForward) <= 0)
                {
                    var worldPos = localToWorld.MultiplyPoint(vertices[i]);
                    var localPos = worldToBrush.MultiplyPoint(worldPos);
                   
                    if (Mathf.Abs(localPos.x) <= 1f && Mathf.Abs(localPos.y) < 1f)
                    {
                        
                        float r = Mathf.Sqrt(localPos.x * localPos.x + localPos.y * localPos.y);
                        r = Mathf.Clamp01(1f- r);
                        if (r > 0)
                        {
                            colors[i] = Color.Lerp(colors[i], vertexBrushColor, r* brushStrength);
                            isVertexDirty = true;
                        }
                    }
                }
            }
            selectObjectMesh.colors = colors;
        }
        private void OnPlaint()
        {
            if (!isDragging)
                return;
            if (!isMouseOnMesh)
                return;

            if (null == brushing)
                brushing = new Material(Shader.Find("Hidden/Bushing"));

            isTextureChannelDirty = true;
            Texture[] source = new Texture[2];

            if (null != editorCtrl1RT[0])
            {
                source[0] = editorCtrl1RT[0];
                editorCtrl1RT[0] = new RenderTexture(source[0].width, source[0].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            }
            else if (null == ctrlText[0])
            {
                source[0] = Texture2D.redTexture;
                editorCtrl1RT[0] = new RenderTexture(1024, 1022, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      
            }
            else
            {
                source[0] = ctrlText[0];
                editorCtrl1RT[0] = new RenderTexture(ctrlText[0].width, ctrlText[0].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            }
            if (hasCtrl[1])
            {
                if (null != editorCtrl1RT[1])
                {
                    source[1] = editorCtrl1RT[1];
                    editorCtrl1RT[1] = new RenderTexture(source[1].width, source[1].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                }
                else if (null == ctrlText[1])
                {
                    source[1] = Texture2D.blackTexture;
                    editorCtrl1RT[1] = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                }
                else
                {
                    source[1] = ctrlText[1];
                    editorCtrl1RT[1] = new RenderTexture(ctrlText[1].width, ctrlText[1].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                }
            }

            int ctrlIndex = editingChannel / 4;
            int channelIndex = editingChannel % 4;
            brushing.SetTexture("_OperaingTex", source[ctrlIndex]);
            brushing.SetFloat("_BrushIndexInOperaTex", channelIndex);
            brushing.SetFloat("_BrushIndexInMainTex", editingChannel);
            brushing.SetTexture("_BrushTex", brushTexture);
            brushing.SetTexture("_MainTex", ctrlText[0]);
            brushing.SetFloat("_BrushStrong", brushStrength);
            brushing.SetFloat("_BrushMaxStrong", brushMaxStrength);
            
            Graphics.Blit(source[0], editorCtrl1RT[0], brushing);
            selectObjectMaterial.SetTexture(setting.ctrlTextureName1, editorCtrl1RT[0]);
            if (hasCtrl[1])
            {
                if (editingChannel < 4)
                {
                    brushing.SetFloat("_BrushIndexInMainTex", 5);
                }
                else
                {
                    brushing.SetFloat("_BrushIndexInMainTex", channelIndex);
                }
                brushing.SetTexture("_MainTex", ctrlText[1]);

                Graphics.Blit(source[1], editorCtrl1RT[1], brushing);
                selectObjectMaterial.SetTexture(setting.ctrlTextureName2, editorCtrl1RT[1]);
            }
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != null && source[i] is RenderTexture)
                {
                    GameObject.DestroyImmediate(source[i], true);
                }
            }
        }
        private void OnLeftMouseDraging()
        {
            if (enable)
            {

                switch (panelType)
                {
                    case 0:
                        {
                            if (drawType == 0)
                            {
                                textureNeedPaint = true;
                            }
                            else
                            {
                                vertexNeedPaint = true;
                            }

                        }
                        break;
                    case 2:
                        {
                            
                        }
                        break;
                }

            }
        }
        void OnSelectedMeshChange()
        {
            vertexUndoRedo.Clear();
            if (selectObjectMesh == null)
            {
                baseColors = null;
            }
            else
            {
                baseColors = selectObjectMesh.colors;
                vertexUndoRedo.Add(baseColors);
            }
        }
    }
}
 