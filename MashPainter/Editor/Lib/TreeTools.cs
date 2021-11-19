using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LCH
{
    public partial class LchMeshPainter
    {

        private static Transform capSphere;
        public GameObject treeObj;
        public Transform tree_parant;
        public int groundMark = -1  ;

        float treeMinVal = 1f;
        float minLimit = 0.1f;
        float treeBrushSize = 1f;
        bool toYAxis = false;
        bool treeReandomRot = true;
        float minScale = 0.9f;
        float treeMaxScale = 1.1f;
        RaycastHit treeHit;
        Ray treeRay;
        bool treeBrushHit = false;
        bool paintTree = false;
        bool ctrlIsDown = false;

        bool  DrawEnableCtrl(string enableText, string disableText, bool b)
        {
            var old = GUI.backgroundColor;
            if (b)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(enableText))
                {
                    b = false;
                }
            }
            else
            {
                GUI.backgroundColor = Color.grey;
                if (GUILayout.Button(disableText))
                {
                    b = true;
                }
            }
            GUI.backgroundColor = old;
            return b;
        }
        GUIStyle boldtext;
        //public int groundMark = 1 << 10;
        private void OnDrawTreePainter()
        {
            enable = DrawEnableCtrl(Languages.GetValue(5, "Working .."), Languages.GetValue(6, "Disable"), enable);



            treeObj = EditorGUILayout.ObjectField(Languages.GetValue(38, "Object to be planted"), treeObj, typeof(GameObject), false) as GameObject;
            tree_parant = EditorGUILayout.ObjectField(Languages.GetValue(39, "Root node to plant the tree"), tree_parant, typeof(Transform), true) as Transform;

            GUILayout.BeginHorizontal();
            GUILayout.Label(Languages.GetValue(40, "Layer of the ground"));
            LayerMask tempMask = EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(groundMark), InternalEditorUtility.layers);
            groundMark = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            GUILayout.EndHorizontal();

            treeMinVal = EditorGUILayout.Slider(Languages.GetValue(41, "Min interva"), treeMinVal, 0.1f, 20f);

            treeBrushSize = EditorGUILayout.Slider(Languages.GetValue(8, "Brush radius"), treeBrushSize, 1f, 30f);

            toYAxis = EditorGUILayout.ToggleLeft(Languages.GetValue(42, "Perpendicular to Y axis"), toYAxis);

            treeReandomRot = EditorGUILayout.ToggleLeft(Languages.GetValue(43,"Random rotation"), treeReandomRot);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Languages.GetValue(44,"Min scale / Max scale:") + minScale + "/" + treeMaxScale);
            EditorGUILayout.MinMaxSlider(ref minScale, ref treeMaxScale, 0.1f, 2.0f);
            GUILayout.EndHorizontal();
            if (null == boldtext)
            {
                boldtext = new GUIStyle(GUI.skin.label);
                boldtext.fontStyle = FontStyle.Bold;
                boldtext.fontSize = (int)(boldtext.fontSize * 1.5f);
                boldtext.normal.textColor = Color.yellow;
            }
            
            GUILayout.Label(Languages.GetValue(45, "Eraser brush: Hold down Ctrl and drag the brush to remove trees"), boldtext);
        }

        private void SphereCapPos(Vector3 point, float scale, bool hit)
        {
            if (capSphere == null)
            {
                GameObject go = GameObject.Find("[SphereCapPos]");
                if (go == null)
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.name = "[SphereCapPos]";

                    Collider collider = go.GetComponent<Collider>();
                    GameObject.DestroyImmediate(collider);

                    Material mat = new Material(Shader.Find("Hidden/Brush Color"));
                    mat.SetColor("_Color", new Color(0f, 0f, 1f, 0.3f));
                    mat.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;

                    Renderer renderer = go.GetComponent<Renderer>();
                    renderer.sharedMaterial = mat;
                    
                }

                go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
                capSphere = go.transform;
                capSphere.rotation = Quaternion.identity;

            }
            capSphere.hideFlags = HideFlags.DontSaveInEditor| HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            capSphere.gameObject.SetActive(hit);
            capSphere.localScale = Vector3.one * scale;
            capSphere.position = point;
        }

        void DeleteObject(Vector3 pos)
        {
            if (null == tree_parant)
            {
                return;
            }

            int count = tree_parant.childCount;
            float bs = treeBrushSize * 0.5f;
            for (int i = count - 1; i >= 0; i--)
            {
                Transform t = tree_parant.GetChild(i);
                if (Vector3.Distance(pos, t.position) < bs)
                {
                    GameObject.DestroyImmediate(t.gameObject, true);
                }
            }
        }
        void AddObjectByRay(Ray ray, float scale)
        {
            RaycastHit hit;
            bool hitGround = Physics.Raycast(ray, out hit, 1000f, groundMark);
            if (hitGround)
            {
                int count = tree_parant.childCount;
                Vector3 p = hit.point;
                for (int i = 0; i < count; i++)
                {
                    Transform t = tree_parant.GetChild(i);
                    if (Vector3.Distance(p, t.position) < treeMinVal)
                    {
                        return;
                    }
                }
                GameObject g = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(treeObj);
                g.transform.parent = tree_parant;
                g.transform.position = hit.point;
                g.transform.localScale = new Vector3(scale, scale, scale);
                if (toYAxis)
                {
                    g.transform.up = Vector3.up;
                }
                else
                {
                    g.transform.up = hit.normal;
                }
                if (treeReandomRot)
                {
                    g.transform.Rotate(0, Random.Range(0f, 360f), 0, Space.Self);
                }

            }
        }
        void AddObject(Ray ray, Vector3 pos, float distance, Vector3 normal)
        {
            if (null == treeObj)
                return;
            if (null == tree_parant)
            {
                tree_parant = new GameObject("root of tree").transform;
            }
            int count = (int)((treeBrushSize * treeBrushSize) / (treeMinVal * treeMinVal));
            //这里就是反正求两个垂直的轴，这里是避免跟上方向重合

            Vector3 axis = normal.normalized;
            Vector3 dir2 = Vector3.up;

            if (Mathf.Abs(Vector3.Dot(Vector3.up, axis)) > 0.9f)
            {
                dir2 = Vector3.Cross(axis, Vector3.left).normalized;
            }
            else
            {
                dir2 = Vector3.Cross(axis, Vector3.up).normalized;
            }
            float bs = treeBrushSize * 0.5f;
            float scal = Random.Range(minScale, treeMaxScale);
            AddObjectByRay(ray, scal);
            for (int i = 1; i < count; i++)
            {
                scal = Random.Range(minScale, treeMaxScale);
                Vector3 newVec = Quaternion.AngleAxis(Random.Range(0, 360), axis) * dir2;
                Ray r = new Ray(ray.origin + newVec * Random.Range(0, bs), ray.direction);
                AddObjectByRay(r, scal);

            }



        }

    }
}