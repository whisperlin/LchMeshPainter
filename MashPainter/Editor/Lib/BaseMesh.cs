using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LCH
{
    public partial class LchMeshPainter
    {
        private enum MeshTopology
        {
            Mixed, // Tris & Quads
            Triangles,
            Quads
        }

        private class BaseMesh
        {
            public List<Vertex> vertices = new List<Vertex>();
            public List<Face> faces = new List<Face>();
            public int numValidVerts = 0;
            public int numValidFaces = 0;
            public int numMaterials = 1;
            public bool hasVertexColors = false;
            public bool hasBoneWeights = false;
            public MeshTopology topology = MeshTopology.Mixed;
            public bool hasUV1 = false;
            public bool hasUV2 = false;
            public float equalityTolerance = 0.0f;

            public Matrix4x4[] bindposes = null;
            public bool calculateTangents = true;
            protected int uniqueTag = 1 << 24;

            public int GetUniqueTag()
            {
                return uniqueTag++;
            }



            public int vertCount() { return vertices.Count; }

            public Vertex corner(int faceIndex, int cornerIndex)
            {
                return vertices[faces[faceIndex].v[cornerIndex]];
            }

            public int AddVertex(Vector3 coords)
            {
                int vid = vertices.Count;
                vertices.Add(new Vertex(coords));
                numValidVerts++;
                return vid;
            }

            public bool IsVertexValid(int vertexIndex)
            {
                return vertices[vertexIndex].valid;
            }

            public int CompareVertices(int vertexIndexA, int vertexIndexB, float positionTolerance)
            {
                Vertex va = vertices[vertexIndexA];
                Vertex vb = vertices[vertexIndexB];

                int res = Vector3CompareWithTolerance(va.coords, vb.coords, positionTolerance);
                if (res != 0) return res;
                if (hasBoneWeights)
                {
                    res = BoneWeightCompare(va.boneWeight, vb.boneWeight);
                    if (res != 0) return res;
                }
                return 0;
            }

            public void ReplaceVertex(int vertexIndexOld, int vertexIndexNew)
            {
                IndexList linkedFaces = vertices[vertexIndexOld].linkedFaces;
                int num = linkedFaces.Count;
                for (int i = 0; i < num; ++i)
                {
                    int faceIndex = linkedFaces[i];
                    int n2 = faces[faceIndex].ReplaceVertex(vertexIndexOld, vertexIndexNew);
                    if (n2 != 1)
                    {
                        Debug.LogError("Weird error vertex found " + n2 + " times in face.");
                    }
                    vertices[vertexIndexNew].linkedFaces.Add(faceIndex);
                }
                vertices[vertexIndexOld].linkedFaces.Clear();
            }

            public Vector3 CalculateVertexNormal(int vertexIndex)
            {
                Vertex vert = vertices[vertexIndex];
                Vector3 n = Vector3.zero;
                IndexList linkedFaces = vert.linkedFaces;
                int num = linkedFaces.Count;
                for (int i = 0; i < num; ++i)
                {
                    int faceIndex = linkedFaces[i];
                    n += faces[faceIndex].normal;
                }
                NormalizeSmallVector(ref n);
                vert.normal = n;
                return n;
            }

            public void CalculateVertexNormals()
            {
                int numVerts = vertCount();
                for (int i = 0; i < numVerts; ++i)
                {
                    CalculateVertexNormal(i);
                }
            }

            // VERTEXPAIRS //

            public bool isVertexPairValid(VertexPair vp)
            {
                if (vp.v[0] == vp.v[1]) return false;
                if (IsVertexValid(vp.v[0]) == false) return false;
                if (IsVertexValid(vp.v[1]) == false) return false;
                return true;
            }

            public Vector3 CalculateVertexPairCenter(VertexPair vp)
            {
                return 0.5f * (vertices[vp.v[0]].coords + vertices[vp.v[1]].coords);
            }

            // FACES //

            public int faceCount() { return faces.Count; }

            public int AddFace(Face f)
            {
                int faceIndex = faces.Count;
                faces.Add(f);

                topology = MeshTopology.Mixed;

                for (int i = 0; i < f.cornerCount; ++i)
                {
                    vertices[f.v[i]].linkedFaces.Add(faceIndex);
                }
                numValidFaces++;
                return faceIndex;
            }

            public void UnlinkFace(int faceIndex)
            {
                Face f = faces[faceIndex];
                if (!f.valid)
                {
                    //				Debug.LogError("Unlinking invalid face!");
                }
                else
                {
                    numValidFaces--;
                    f.valid = false;

                    for (int i = 0; i < f.cornerCount; ++i)
                    {
                        vertices[f.v[i]].linkedFaces.Remove(faceIndex);
                    }
                }
            }

            public Vector3 CalculateFaceNormal(int faceIndex)
            {
                int i;
                Face face = faces[faceIndex];
                int vertCount = face.cornerCount;
                int[] vindex = face.v;
                Vector3[] vec = new Vector3[vertCount + 1];

                for (i = 0; i < vertCount - 1; ++i) vec[i] = vertices[vindex[i + 1]].coords - vertices[vindex[i]].coords;
                vec[i] = vertices[vindex[0]].coords - vertices[vindex[i]].coords;
                Vector3 normal = Vector3.zero;
                if (vertCount == 3)
                {
                    normal = Vector3.Cross(vec[0], vec[1]);
                }
                else
                {
                    // Sum normals based on consequtive pairs of edges
                    vec[vertCount] = vec[0];
                    for (i = 0; i < vertCount; ++i)
                    {
                        normal += Vector3.Cross(vec[i], vec[i + 1]);
                    }
                }
                NormalizeSmallVector(ref normal);
                face.normal = normal;
                return face.normal;
            }

            public void CalculateFaceNormals()
            {
                int numFaces = faces.Count;
                for (int i = 0; i < numFaces; ++i)
                {
                    CalculateFaceNormal(i);
                }
            }

            public Vector3 CalculateFaceCenter(int faceIndex)
            {
                Face f = faces[faceIndex];
                Vector3 result = Vector3.zero;
                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    result += vertices[f.v[cornerIndex]].coords;
                }
                return result * (1.0f / ((float)f.cornerCount));
            }

            public Vector2 CalculateFaceCenterUV1(int faceIndex)
            {
                Face f = faces[faceIndex];
                Vector2 result = Vector2.zero;
                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    result += f.uv1[cornerIndex];
                }
                return result * (1.0f / ((float)f.cornerCount));
            }

            public Vector2 CalculateFaceCenterUV2(int faceIndex)
            {
                Face f = faces[faceIndex];
                Vector2 result = Vector2.zero;
                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    result += f.uv2[cornerIndex];
                }
                return result * (1.0f / ((float)f.cornerCount));
            }

            public Vector3 CalculateFaceCenterVertexNormal(int faceIndex)
            {
                Face f = faces[faceIndex];
                Vector3 result = Vector3.zero;
                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    result += f.vertexNormal[cornerIndex];
                }
                return result * (1.0f / ((float)f.cornerCount));
            }

            public BoneWeight CalculateFaceCenterBoneWeight(int faceIndex)
            {
                Face f = faces[faceIndex];
                Dictionary<int, float> resultWeightForBone = new Dictionary<int, float>(4 * f.cornerCount);

                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    Vertex v = vertices[f.v[cornerIndex]];
                    BoneWeight bw = v.boneWeight;

                    int[] indexes = { bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3 };
                    float[] weights = { bw.weight0, bw.weight1, bw.weight2, bw.weight3 };

                    for (int i = 0; i < 4; ++i)
                    {
                        if (resultWeightForBone.ContainsKey(indexes[i]))
                        {
                            resultWeightForBone[indexes[i]] += weights[i];
                        }
                        else
                        {
                            resultWeightForBone.Add(indexes[i], weights[i]);
                        }
                    }
                }

                // Sort by weight
                List<KeyValuePair<int, float>> mList = new List<KeyValuePair<int, float>>(resultWeightForBone);

                mList.Sort((x, y) => y.Value.CompareTo(x.Value));
                // Make sure there are at least 4 entries
                while (mList.Count < 4) mList.Add(new KeyValuePair<int, float>(0, 0.0f));

                float weightSum = mList[0].Value + mList[1].Value + mList[2].Value + mList[3].Value;
                float weightFact = 1.0f;
                if (weightSum != 0.0f) weightFact = 1.0f / weightSum;

                BoneWeight result = new BoneWeight();
                result.boneIndex0 = mList[0].Key;
                result.weight0 = mList[0].Value * weightFact;
                result.boneIndex1 = mList[1].Key;
                result.weight1 = mList[1].Value * weightFact;
                result.boneIndex2 = mList[2].Key;
                result.weight2 = mList[2].Value * weightFact;
                result.boneIndex3 = mList[3].Key;
                result.weight3 = mList[3].Value * weightFact;

                return result;
            }

            public Color CalculateFaceCenterColor(int faceIndex)
            {
                Face f = faces[faceIndex];
                Color result = Color.clear;
                for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    result += vertices[f.v[cornerIndex]].color;
                }
                return result * (1.0f / ((float)f.cornerCount));
            }

            public float CalculateFaceArea(int faceIndex)
            {
                Face f = faces[faceIndex];
                if (f.cornerCount != 3)
                {
                    Debug.LogError("Currently only the area of triangles can be calculated.");
                }

                Vector3 a = vertices[f.v[1]].coords - vertices[f.v[0]].coords;
                Vector3 b = vertices[f.v[2]].coords - vertices[f.v[0]].coords;
                float area = Vector3.Cross(a, b).magnitude;
                area = Mathf.Abs(0.5f * area);
                return area;
            }

            public float CornerAngle(int faceIndex, int cornerIndex)
            {
                Face f = faces[faceIndex];
                int[] fv = f.v;
                int vertCount = f.cornerCount;
                int prev = cornerIndex - 1;
                int next = cornerIndex + 1;
                if (prev < 0) prev = vertCount - 1;
                else if (next >= vertCount) next = 0;

                Vector3 p = vertices[fv[cornerIndex]].coords;
                Vector3 a = vertices[fv[prev]].coords - p;
                Vector3 b = vertices[fv[next]].coords - p;
                return Vector3.Angle(a, b);
            }

            // ALGORITHMS

            public virtual void Clear()
            {
                vertices.Clear();
                faces.Clear();
                numValidVerts = 0;
                numValidFaces = 0;
                numMaterials = 1;
                hasVertexColors = false;
                hasBoneWeights = false;
                topology = MeshTopology.Mixed;
                hasUV1 = false;
                hasUV2 = false;
                equalityTolerance = 0.0f;

                bindposes = null;
                calculateTangents = true;
            }

            public MeshTopology DetermineMeshTopology()
            {
                if (faces.Count == 0)
                {
                    topology = MeshTopology.Mixed;
                }
                else
                {
                    Face f0 = faces[0];
                    int f0CornerCount = f0.cornerCount;
                    int faceCount = faces.Count;
                    int faceIndex = 1;
                    for (; faceIndex < faceCount; ++faceIndex)
                    {
                        if (faces[faceIndex].cornerCount != f0CornerCount) break;
                    }
                    if (faceIndex == faceCount)
                    {
                        if (f0CornerCount == 4) topology = MeshTopology.Quads;
                        else topology = MeshTopology.Triangles;
                    }
                    else
                    {
                        topology = MeshTopology.Mixed;
                    }
                }
                return topology;
            }

            // collects the vertices that are connected to vertexIndex
            // TODO: optimize .. store Face array and get neighbours via switch for quads
            public void CollectVerticesAroundVertex(int vertexIndex, ref List<int> list)
            {
                int mark = GetUniqueTag();
                IndexList surroundingFaces = vertices[vertexIndex].linkedFaces; // surrounding faces
                int numFaces = surroundingFaces.Count;

                if (topology == MeshTopology.Triangles)
                {
                    vertices[vertexIndex].mark = mark;
                    for (int i = 0; i < numFaces; ++i)
                    {
                        Face face = faces[surroundingFaces[i]];
                        for (int cornerIndex = 0; cornerIndex < 3; ++cornerIndex)
                        { // three points!
                            int vertIndex = face.v[cornerIndex];
                            if (vertices[vertIndex].mark != mark)
                            {
                                list.Add(vertIndex);
                                vertices[vertIndex].mark = mark;
                            }
                        }
                    }
                }
                else
                {
                    // For n-gons collect prev and next corner vertices
                    for (int i = 0; i < numFaces; ++i)
                    {
                        Face face = faces[surroundingFaces[i]];
                        int next = 0;
                        int corner = face.cornerCount - 1;
                        int prev = face.cornerCount - 2;
                        for (; next < face.cornerCount; ++next)
                        {
                            if (face.v[corner] == vertexIndex)
                            {
                                int vert = face.v[next];
                                if (vertices[vert].mark != mark)
                                {
                                    list.Add(vert);
                                    vertices[vert].mark = mark;
                                }
                                vert = face.v[prev];
                                if (vertices[vert].mark != mark)
                                {
                                    list.Add(vert);
                                    vertices[vert].mark = mark;
                                }
                                break;
                            }
                            prev = corner;
                            corner = next;
                        }
                    }
                }
            }

            // What this basically does is search for common faces in the two linkedFaces lists
            // It uses & destroys face marks
            public void CollectVertexPairFaces(VertexPair pair, IndexList commonFaces)
            {
                IndexList il0 = vertices[pair.v[0]].linkedFaces; // surrounding faces
                IndexList il1 = vertices[pair.v[1]].linkedFaces; // surrounding faces		
                int[] v0Faces = il0.array;
                int[] v1Faces = il1.array;
                int v0Count = il0.Count;
                int v1Count = il1.Count;
                int tag = GetUniqueTag();

                commonFaces.GrowToCapacity(v1Count);
                for (int i = 0; i < v0Count; ++i) faces[v0Faces[i]].mark = tag;
                for (int i = 0; i < v1Count; ++i)
                {
                    int faceIndex = v1Faces[i];
                    if (faces[faceIndex].mark == tag)
                    {
                        commonFaces.AddUnsafe(faceIndex);
                    }
                }
            }

            public void CollectCollapseFacesForVertexPair(VertexPair pair, IndexList changeFaces0, IndexList changeFaces1, IndexList commonFaces)
            {
                IndexList il0 = vertices[pair.v[0]].linkedFaces;
                IndexList il1 = vertices[pair.v[1]].linkedFaces;
                int[] v0Faces = il0.array;
                int[] v1Faces = il1.array;
                int v0Count = il0.Count;
                int v1Count = il1.Count;
                int tag = GetUniqueTag();

                // Grow target lists to save on checks later
                changeFaces0.GrowToCapacity(v0Count);
                changeFaces1.GrowToCapacity(v1Count);
                commonFaces.GrowToCapacity(v0Count); // could be min(v0count, v1count), but that's probably slower

                for (int i = 0; i < v1Count; ++i) faces[v1Faces[i]].mark = tag;
                for (int i = 0; i < v0Count; ++i)
                {
                    int faceIndex = v0Faces[i];
                    Face f = faces[faceIndex];
                    //	if (f.valid) {
                    if (f.mark == tag)
                    {
                        commonFaces.AddUnsafe(faceIndex);
                        f.mark = 0;
                    }
                    else
                    {
                        changeFaces0.AddUnsafe(faceIndex);
                    }
                    //	}
                }
                for (int i = 0; i < v1Count; ++i)
                {
                    int faceIndex = v1Faces[i];
                    Face f = faces[faceIndex];
                    if (/*f.valid &&*/ f.mark == tag)
                    {
                        changeFaces1.AddUnsafe(faceIndex);
                    }
                }
            }

            IndexList changeFaces0 = new IndexList(32);
            IndexList changeFaces1 = new IndexList(32);
            IndexList removeFaces = new IndexList(32);


            public int CollapseVertexPair(CollapseInfo info)
            {
                if (topology != MeshTopology.Triangles)
                {
                    Debug.LogError("LodlMesh: Collapsing a vertex pair requires a triangle mesh");
                    return 0;
                }

                VertexPair pair = info.vp;
                int vindex0 = pair.v[0];
                int vindex1 = pair.v[1];
                Vertex vertex0 = vertices[vindex0];
                Vertex vertex1 = vertices[vindex1];
                int i, j;

                changeFaces0.Clear();
                changeFaces1.Clear();
                removeFaces.Clear();
                CollectCollapseFacesForVertexPair(pair, changeFaces0, changeFaces1, removeFaces);

                // Adjust parameters of vertex0 to the new position
                float ratio1 = info.Ratio(this);

                // try baricentric projection on all the removeFaces (usually 2)
                int projFaceIndex = -1;
                Face projFace = null;
                int projCorner0 = 0, projCorner1 = 0;
                Vector3 bari = Vector3.zero;
                int[] v = null;
                for (i = 0; i < removeFaces.Count; ++i)
                {
                    Face f = faces[removeFaces[i]];
                    v = f.v;
                    bari = BaricentricProjection(info.targetPosition, vertices[v[0]].coords, vertices[v[1]].coords, vertices[v[2]].coords);
                    if (AreBaricentricCoordsInsideTriangle(bari))
                    {
                        projFaceIndex = removeFaces[i];
                        projFace = f;
                        projCorner0 = projFace.CornerIndexTriangle(vindex0);
                        projCorner1 = projFace.CornerIndexTriangle(vindex1);
                        break;
                    }
                }
                // There must not be invalid faces in changeFaces0 or changeFaces1 !!!
                /*	for (i = 0; i < changeFaces0.Count; ++i) if (faces[changeFaces0[i]].valid == false) Debug.LogError("NOOO!");
                    for (i = 0; i < changeFaces1.Count; ++i) if (faces[changeFaces1[i]].valid == false) Debug.LogError("NOOO!");
                    for (i = 0; i < removeFaces.Count; ++i) if (faces[removeFaces[i]].valid == false) Debug.LogError("NOOO!");
                    */
                // Deal with vertex colors and boneweights. these are per vertex.
                if (projFace != null)
                {
                    if (hasVertexColors) vertex0.color = bari.x * vertices[v[0]].color + bari.y * vertices[v[1]].color + bari.z * vertices[v[2]].color;
                    if (hasBoneWeights) vertex0.boneWeight = BoneWeightBaricentricInterpolation(vertices[v[0]].boneWeight, vertices[v[1]].boneWeight, vertices[v[2]].boneWeight, bari.x, bari.y, bari.z);
                }
                else
                {
                    if (hasVertexColors) vertex0.color = Color.Lerp(vertex0.color, vertex1.color, ratio1);
                    if (hasBoneWeights) vertex0.boneWeight = BoneWeightLerp(vertex0.boneWeight, vertex1.boneWeight, ratio1);
                }

                // Determine corner numbers for v0 in changefaces0 and v1 in changefaces1
                IndexList corners0 = new IndexList(changeFaces0.Count);
                for (i = 0; i < changeFaces0.Count; ++i) corners0[i] = faces[changeFaces0[i]].CornerIndexTriangle(vindex0);
                IndexList corners1 = new IndexList(changeFaces1.Count);
                for (i = 0; i < changeFaces1.Count; ++i) corners1[i] = faces[changeFaces1[i]].CornerIndexTriangle(vindex1);

                #region Face-Dependent Attributes (Vertex normals, uv1, uv2)

                // NORMALS
                int count = 0, filterTag = GetUniqueTag();
                Vector3 projNormalNew = Vector3.zero;
                if (projFace != null)
                {
                    projNormalNew = bari.x * projFace.vertexNormal[0] + bari.y * projFace.vertexNormal[1] + bari.z * projFace.vertexNormal[2];
                    count = _replaceCornerNormalInFaceGroup(projFace.vertexNormal[projCorner0], projNormalNew, changeFaces0, corners0, filterTag);
                }
                if (count < changeFaces0.Count)
                {
                    // there are faces which cannot use baricentric projection
                    for (j = 0; j < removeFaces.Count; ++j)
                    {
                        if (removeFaces[j] != projFaceIndex)
                        {
                            Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                            Vector3 oldNormal = f2.vertexNormal[c0];
                            _replaceCornerNormalInFaceGroup(oldNormal, Vector3.Lerp(oldNormal, f2.vertexNormal[c1], ratio1), changeFaces0, corners0, filterTag);
                        }
                    }
                }

                count = 0; filterTag = GetUniqueTag();
                if (projFace != null)
                {
                    count = _replaceCornerNormalInFaceGroup(projFace.vertexNormal[projCorner1], projNormalNew, changeFaces1, corners1, filterTag);
                }
                if (count < changeFaces1.Count)
                {
                    // there are faces which cannot use baricentric projection
                    for (j = 0; j < removeFaces.Count; ++j)
                    {
                        if (removeFaces[j] != projFaceIndex)
                        {
                            Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                            Vector3 oldNormal = f2.vertexNormal[c1];
                            _replaceCornerNormalInFaceGroup(oldNormal, Vector3.Lerp(f2.vertexNormal[c0], oldNormal, ratio1), changeFaces1, corners1, filterTag);
                        }
                    }
                }

                if (hasUV1)
                {
                    count = 0; filterTag = GetUniqueTag();
                    Vector2 projUV1New = Vector2.zero;
                    if (projFace != null)
                    {
                        projUV1New = bari.x * projFace.uv1[0] + bari.y * projFace.uv1[1] + bari.z * projFace.uv1[2];
                        count = _replaceCornerUV1InFaceGroup(projFace.uv1[projCorner0], projUV1New, changeFaces0, corners0, filterTag);
                    }
                    if (count < changeFaces0.Count)
                    {
                        // there are faces which cannot use baricentric projection
                        for (j = 0; j < removeFaces.Count; ++j)
                        {
                            if (removeFaces[j] != projFaceIndex)
                            {
                                Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                                Vector2 oldUV1 = f2.uv1[c0];
                                _replaceCornerUV1InFaceGroup(oldUV1, Vector2.Lerp(oldUV1, f2.uv1[c1], ratio1), changeFaces0, corners0, filterTag);
                            }
                        }
                    }

                    count = 0; filterTag = GetUniqueTag();
                    if (projFace != null)
                    {
                        count = _replaceCornerUV1InFaceGroup(projFace.uv1[projCorner1], projUV1New, changeFaces1, corners1, filterTag);
                    }
                    if (count < changeFaces1.Count)
                    {
                        // there are faces which cannot use baricentric projection
                        for (j = 0; j < removeFaces.Count; ++j)
                        {
                            if (removeFaces[j] != projFaceIndex)
                            {
                                Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                                Vector2 oldUV1 = f2.uv1[c1];
                                _replaceCornerUV1InFaceGroup(oldUV1, Vector2.Lerp(f2.uv1[c0], oldUV1, ratio1), changeFaces1, corners1, filterTag);
                            }
                        }
                    }
                }

                if (hasUV2)
                {
                    count = 0; filterTag = GetUniqueTag();
                    Vector2 projUV2New = Vector2.zero;
                    if (projFace != null)
                    {
                        projUV2New = bari.x * projFace.uv2[0] + bari.y * projFace.uv2[1] + bari.z * projFace.uv2[2];
                        count = _replaceCornerUV2InFaceGroup(projFace.uv2[projCorner0], projUV2New, changeFaces0, corners0, filterTag);
                    }
                    if (count < changeFaces0.Count)
                    {
                        // there are faces which cannot use baricentric projection
                        for (j = 0; j < removeFaces.Count; ++j)
                        {
                            if (removeFaces[j] != projFaceIndex)
                            {
                                Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                                Vector2 oldUV2 = f2.uv2[c0];
                                _replaceCornerUV2InFaceGroup(oldUV2, Vector2.Lerp(oldUV2, f2.uv2[c1], ratio1), changeFaces0, corners0, filterTag);
                            }
                        }
                    }

                    count = 0; filterTag = GetUniqueTag();
                    if (projFace != null)
                    {
                        count = _replaceCornerUV2InFaceGroup(projFace.uv2[projCorner1], projUV2New, changeFaces1, corners1, filterTag);
                    }
                    if (count < changeFaces1.Count)
                    {
                        // there are faces which cannot use baricentric projection
                        for (j = 0; j < removeFaces.Count; ++j)
                        {
                            if (removeFaces[j] != projFaceIndex)
                            {
                                Face f2 = faces[removeFaces[j]]; int c0 = f2.CornerIndexTriangle(vindex0), c1 = f2.CornerIndexTriangle(vindex1);
                                Vector2 oldUV2 = f2.uv2[c1];
                                _replaceCornerUV2InFaceGroup(oldUV2, Vector2.Lerp(f2.uv2[c0], oldUV2, ratio1), changeFaces1, corners1, filterTag);
                            }
                        }
                    }
                }
                #endregion
                // Move vertex to goal position
                vertex0.coords = info.targetPosition;

                // remove faces
                //	Debug.Log("Change faces 1 num: " + changeFaces0.Count);
                //	Debug.Log("Change faces 2 num: " + changeFaces1.Count);
                //	Debug.Log("Remove faces num: " + removeFaces.Count);
                for (i = 0; i < removeFaces.Count; ++i)
                {
                    UnlinkFace(removeFaces[i]);
                }

                // change vertex on vindex1 faces, update surrounding faces on vindex0
                for (i = 0; i < changeFaces1.Count; ++i)
                {
                    int faceIndex = changeFaces1[i];
                    Face f = faces[faceIndex];
                    if (f.valid)
                    {
                        f.ReplaceVertex(vindex1, vindex0);
                        vertex0.linkedFaces.Add(faceIndex);
                    }
                }

                // mark vindex1 as invalid
                vertex1.linkedFaces.Clear();
                if (vertex1.valid == true)
                {
                    numValidVerts--;
                    vertex1.valid = false;
                }
                else
                {
                    Debug.LogError("vindex1 was already invalid");
                }
                return vindex0;
            }

            int _replaceCornerNormalInFaceGroup(Vector3 oldNormal, Vector3 newNormal, IndexList faceList, IndexList corners, int filterTag)
            {
                int count = 0;
                for (int i = 0; i < faceList.Count; ++i)
                {
                    Face f = faces[faceList[i]];
                    if (f.mark == filterTag) continue;
                    if (Vector3CompareWithTolerance(f.vertexNormal[corners[i]], oldNormal, equalityTolerance) == 0)
                    {
                        f.vertexNormal[corners[i]] = newNormal;
                        f.mark = filterTag;
                        count++;
                    }
                }
                return count;
            }

            int _replaceCornerUV1InFaceGroup(Vector2 oldUV1, Vector2 newUV1, IndexList faceList, IndexList corners, int filterTag)
            {
                int count = 0;
                for (int i = 0; i < faceList.Count; ++i)
                {
                    Face f = faces[faceList[i]];
                    if (f.mark == filterTag) continue;
                    if (Vector2CompareWithTolerance(f.uv1[corners[i]], oldUV1, equalityTolerance) == 0)
                    {
                        f.uv1[corners[i]] = newUV1;
                        f.mark = filterTag;
                        count++;
                    }
                }
                return count;
            }

            int _replaceCornerUV2InFaceGroup(Vector2 oldUV2, Vector2 newUV2, IndexList faceList, IndexList corners, int filterTag)
            {
                int count = 0;
                for (int i = 0; i < faceList.Count; ++i)
                {
                    Face f = faces[faceList[i]];
                    if (f.mark == filterTag) continue;
                    if (Vector2CompareWithTolerance(f.uv2[corners[i]], oldUV2, equalityTolerance) == 0)
                    {
                        f.uv2[corners[i]] = newUV2;
                        f.mark = filterTag;
                        count++;
                    }
                }
                return count;
            }

            public int InvalidateUnconnectedVertices()
            {
                int vertCount = vertices.Count;
                int count = 0;
                for (int i = 0; i < vertCount; ++i)
                {
                    Vertex v = vertices[i];
                    if (v.valid)
                    {
                        if (v.linkedFaces.Count == 0)
                        {
                            //Debug.Log("Removed vertex without faces");
                            v.valid = false;
                            numValidVerts--;
                            count++;
                        }
                    }
                }
                return count;
            }

            public int InvalidateDegenerateFaces()
            {
                int faceCount = faces.Count;
                int count = 0;
                for (int faceIndex = 0; faceIndex < faceCount; ++faceIndex)
                {
                    Face f = faces[faceIndex];
                    if (f.valid)
                    {
                        int[] v = f.v;
                        int numCorners = f.cornerCount;
                        for (int j = 0; j < numCorners; ++j)
                        {
                            if (IsVertexValid(v[j]) == false)
                            {
                                UnlinkFace(faceIndex);
                                count++;
                                break; // j!
                            }
                            for (int k = j + 1; k < numCorners; ++k)
                            {
                                if (v[j] == v[k])
                                {
                                    UnlinkFace(faceIndex);
                                    count++;
                                    j = numCorners; break;
                                }
                            }
                        }
                        // todo? maybe get rid of faces with area 0?
                    }
                }
                return count;
            }

            public void RebuildMesh(ref int[] vertTable, ref int[] faceTable, bool verbose = false)
            {
                InvalidateUnconnectedVertices();
                InvalidateDegenerateFaces();
                int numVerts = vertCount();
                int numFaces = faceCount();

                vertTable = new int[numVerts];
                faceTable = new int[numFaces];
                List<Vertex> newVerts = new List<Vertex>();
                List<Face> newFaces = new List<Face>();

                for (int i = 0; i < numVerts; ++i)
                {
                    if (vertices[i].valid)
                    {
                        vertTable[i] = newVerts.Count;
                        newVerts.Add(vertices[i]);
                    }
                    else
                    {
                        vertTable[i] = -1;
                    }
                }
                for (int i = 0; i < numFaces; ++i)
                {
                    if (faces[i].valid)
                    {
                        faceTable[i] = newFaces.Count;
                        newFaces.Add(faces[i]);
                    }
                    else
                    {
                        faceTable[i] = -1;
                    }
                }
                vertices = newVerts;
                faces = newFaces;
                // Update index
                if (verbose)
                {
                    Debug.Log("Rebuild Vertex Count " + numVerts + " -> " + vertCount());
                    Debug.Log("Rebuild Face Count " + numFaces + " -> " + faceCount());
                }
                numVerts = vertCount();
                numFaces = faceCount();
                for (int i = 0; i < numVerts; ++i)
                {
                    IndexList lFaces = vertices[i].linkedFaces;
                    IndexList lFacesNew = new IndexList(18);
                    for (int j = 0; j < lFaces.Count; ++j)
                    {
                        int l = faceTable[lFaces[j]];
                        if (l != -1) lFacesNew.Add(l);
                        else Debug.LogError("!!!!!");
                    }
                    vertices[i].linkedFaces = lFacesNew;
                }
                for (int i = 0; i < numFaces; ++i)
                {
                    int[] v = faces[i].v;
                    for (int j = 0; j < faces[i].cornerCount; ++j)
                    {
                        v[j] = vertTable[v[j]];
                    }
                }
                numValidVerts = numVerts;
                numValidFaces = numFaces;
            }

            public void RebuildVertexLinkedFaces()
            {
                int numVerts = vertCount();
                int numFaces = faceCount();

                for (int vertexIndex = 0; vertexIndex < numVerts; ++vertexIndex)
                {
                    vertices[vertexIndex].linkedFaces.Clear();
                }
                for (int faceIndex = 0; faceIndex < numFaces; ++faceIndex)
                {
                    Face f = faces[faceIndex];
                    for (int cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                    {
                        vertices[f.v[cornerIndex]].linkedFaces.Add(faceIndex);
                    }
                }
            }

        }
        /// <summary>
        /// A polygonal mesh with edge information.
        /// </summary>
        private class MeshEdges : BaseMesh
        {
            public List<Edge> edges = null;
            protected List<int>[] linkedEdges = null; // List of edge indexes linked to each vertex

            public int edgeCount() { return (edges != null) ? edges.Count : -1; }
            public List<int> linkedEdgesForVert(int vertexIndex) { return linkedEdges[vertexIndex]; }

            public void AddEdge(Edge e)
            {
                edges.Add(e);
            }

            public bool IsEdgeValid(int edgeIndex)
            {
                Edge e = edges[edgeIndex];
                if (e.linkedFaces.Count == 0) return false;
                return isVertexPairValid(e);
            }

            public float EdgeLength(int edgeIndex)
            {
                int[] v = edges[edgeIndex].v;
                return Vector3.Distance(vertices[v[0]].coords, vertices[v[1]].coords);
            }

            public float EdgeLengthSqr(int edgeIndex)
            {
                int[] v = edges[edgeIndex].v;
                return (vertices[v[0]].coords - vertices[v[1]].coords).sqrMagnitude;
            }

            public void UnlinkEdge(int edgeIndex)
            {
                if (IsEdgeValid(edgeIndex))
                {
                }
                else
                {
                    Edge e = edges[edgeIndex];
                    for (int i = 0; i < 2; ++i)
                    {
                        linkedEdges[e.v[i]].Remove(edgeIndex);
                    }
                    e.Invalidate();
                }
            }

            public void ClearEdges()
            {
                edges = null;
                linkedEdges = null;
            }

            public bool IsEdgeBorder(int edgeIndex)
            {
                return (edges[edgeIndex].linkedFaces.Count == 1);
            }

            // TODO: do this better with one call to determine various edge flags
            // Requires edges.linkedfaces to be valid!
            public bool IsEdgeUV1Seam(int edgeIndex)
            {
                Edge e = edges[edgeIndex];
                IndexList linkedFaces = e.linkedFaces;
                switch (linkedFaces.Count)
                {
                    case 0:
                    case 1:
                        return false;
                    case 2:
                        // Check if the verts have the same uv2 coords on both faces
                        Face f0 = faces[linkedFaces[0]];
                        Face f1 = faces[linkedFaces[1]];
                        int vi0 = e.v[0];
                        //	Debug.Log("Edge " + e.v[0] + "->" + e.v[1]);
                        //	Debug.Log("Face0 n " + f0.cornerCount + " " + f0.v[0] + " " + f0.v[1] + " " + f0.v[2] + " " + f0.v[3]);
                        if (f0.uv1[f0.CornerIndex(vi0)] != f1.uv1[f1.CornerIndex(vi0)]) return true;
                        int vi1 = e.v[1];
                        //	Debug.Log("Face1 n + " + f1.cornerCount + " " + f1.v[0] + " " + f1.v[1] + " " + f1.v[2] + " " + f1.v[3]);
                        return (f0.uv1[f0.CornerIndex(vi1)] != f1.uv1[f1.CornerIndex(vi1)]);
                    default: // > 2
                        return true;
                }
            }

            // requires edges.linkedfaces to be valid
            public bool IsEdgeUV2Seam(int edgeIndex)
            {
                Edge e = edges[edgeIndex];
                IndexList linkedFaces = e.linkedFaces;
                switch (linkedFaces.Count)
                {
                    case 0:
                    case 1:
                        return false;
                    case 2:
                        // Check if the verts have the same uv2 coords on both faces
                        Face f0 = faces[linkedFaces[0]];
                        Face f1 = faces[linkedFaces[1]];
                        int vi0 = e.v[0];
                        if (f0.uv2[f0.CornerIndex(vi0)] != f1.uv2[f1.CornerIndex(vi0)]) return true;
                        int vi1 = e.v[1];
                        return (f0.uv2[f0.CornerIndex(vi1)] != f1.uv2[f1.CornerIndex(vi1)]);
                    default:
                        return true;
                }
            }

            // requires edges.linkedfaces to be valid
            public bool isEdgeMaterialSeam(int edgeIndex)
            {
                IndexList linkedFaces = edges[edgeIndex].linkedFaces;
                if (linkedFaces.Count < 2) return false;

                int mat = faces[linkedFaces[0]].material;
                for (int i = 1; i < linkedFaces.Count; ++i)
                {
                    if (mat != faces[linkedFaces[i]].material) return true;
                }
                return false;
            }

            // ALGORITHMS //

            public int EdgeIndexForVertices(int vindex0, int vindex1)
            {
                int edgeIndex;
                List<int> linkedEdgeIndex = linkedEdges[vindex0];
                int count = linkedEdgeIndex.Count;

                for (int i = 0; i < count; ++i)
                {
                    edgeIndex = linkedEdgeIndex[i];
                    if (edges[edgeIndex].OtherVertex(vindex0) == vindex1) return edgeIndex;
                }
                //	Debug.LogError("Bad bad bad. Linked edge that doesn't contain the linked vertex v0:" + vindex0 + " v1: " + vindex1 + " linkedEdgeCountV0: " + linkedEdgeIndex.Count);
                return -1;
            }

            public void GenerateEdgeTopology()
            {
                CalculateEdgeLinkedFaces();
            }

            public void GenerateEdgeList()
            {
                int numVerts = vertCount();

                // alloc new arrays, let gc do the cleanup
                edges = new List<Edge>();

                linkedEdges = new List<int>[numVerts];
                for (int i = 0; i < numVerts; ++i)
                {
                    linkedEdges[i] = new List<int>();
                }

                List<int> surroundingVerts = new List<int>();
                for (int i = 0; i < numVerts; ++i)
                {
                    surroundingVerts.Clear();
                    CollectVerticesAroundVertex(i, ref surroundingVerts);
                    int count = surroundingVerts.Count;
                    for (int j = 0; j < count; ++j)
                    {
                        int vertexIndex = surroundingVerts[j];
                        if (i < vertexIndex)
                        {
                            Edge edge = new Edge(i, vertexIndex);
                            int edgeIndex = edges.Count;
                            linkedEdges[i].Add(edgeIndex);
                            linkedEdges[vertexIndex].Add(edgeIndex);
                            edges.Add(edge);
                        }
                    }
                }
            }

            public void RegenerateVertexLinkedEdges()
            {
                int numVerts = vertCount();
                int numEdges = edges.Count;
                linkedEdges = new List<int>[numVerts];
                for (int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
                {
                    linkedEdges[vertIndex] = new List<int>();
                }

                for (int edgeIndex = 0; edgeIndex < numEdges; ++edgeIndex)
                {
                    int[] vi = edges[edgeIndex].v;
                    linkedEdges[vi[0]].Add(edgeIndex);
                    linkedEdges[vi[1]].Add(edgeIndex);
                }
            }

            public void CalculateEdgeLinkedFaces()
            {
                int numEdges = edgeCount();
                for (int i = 0; i < numEdges; ++i)
                {
                    Edge edge = edges[i];
                    edge.linkedFaces.Clear();
                    CollectVertexPairFaces(edge, edge.linkedFaces);
                    /*	if (edge.linkedFaces.Count > 2) {
                            Debug.Log("WARNING! An edge has " + edge.linkedFaces.Count + " faces!");
                        }*/
                }
            }

            public List<int>[] CalculateFaceLinkedEdges()
            {
                // Needs linkedFaces on edges to be valid !!!!
                int numFaces = faceCount();
                List<int>[] result = new List<int>[numFaces];
                for (int i = 0; i < numFaces; ++i)
                {
                    result[i] = new List<int>();
                }

                int numEdges = edgeCount();
                for (int i = 0; i < numEdges; ++i)
                {
                    IndexList edgeFaces = edges[i].linkedFaces;
                    for (int j = 0; j < edgeFaces.Count; ++j)
                    {
                        result[edgeFaces[j]].Add(i);
                    }
                }
                return result;
            }

            void _markGroupFaceNeightbours(int vertexIndex, int faceIndex, ref List<int>[] faceLinkedEdges, int groupIndex)
            {
                Stack<int> stack = new Stack<int>();
                stack.Push(faceIndex);
                do
                {
                    faceIndex = stack.Pop();
                    Face f = faces[faceIndex];
                    if (f.mark == -1)
                    {
                        f.mark = groupIndex; // mark face group. -1 = available
                                             // locate neighbours connected by non-creases
                        List<int> faceEdges = faceLinkedEdges[faceIndex];
                        // Search for other faces connected with non-crease edges
                        for (int e = 0; e < faceEdges.Count; ++e)
                        {
                            Edge edge = edges[faceEdges[e]];
                            if (IsEdgeValid(faceEdges[e]) && edge.crease < 1.0f && edge.ContainsVertex(vertexIndex))
                            {
                                IndexList edgeFaces = edge.linkedFaces;
                                for (int k = 0; k < edgeFaces.Count; ++k)
                                {
                                    int edgeFaceIndex = edgeFaces[k];
                                    if (faces[edgeFaceIndex].mark == -1)
                                    {
                                        stack.Push(edgeFaceIndex);
                                    }
                                }
                            }
                        }
                    }
                } while (stack.Count > 0);
            }

            public void CalculateFaceVertexNormalsFromEdgeCreasesForVertex(int vertexIndex, ref List<int>[] faceLinkedEdges)
            {
                int i, j, grp;
                List<int> grpCornerIndex = new List<int>();

                Vertex v = vertices[vertexIndex];
                IndexList vertexFaces = v.linkedFaces;
                int vertexFaceCount = vertexFaces.Count;
                //	List<int> vertexEdges = linkedEdges[vertexIndex];
                // Clear face marks around vertex
                for (j = 0; j < vertexFaceCount; ++j)
                {
                    faces[vertexFaces[j]].mark = -1; // TODO: could be faster with uniqueTag
                }
                // This will add each facemark to a groupIndex
                int groupIndex = 0;
                for (j = 0; j < vertexFaceCount; ++j)
                {
                    int faceIndex = vertexFaces[j];
                    if (faces[faceIndex].mark == -1)
                    { // face still available
                        _markGroupFaceNeightbours(vertexIndex, vertexFaces[j], ref faceLinkedEdges, groupIndex);
                        groupIndex++;
                    }
                }
                // Build group arrays
                List<int>[] groups = new List<int>[groupIndex]; // are these too many allocations?
                for (i = 0; i < groupIndex; ++i) groups[i] = new List<int>();
                for (i = 0; i < vertexFaceCount; ++i)
                {
                    int faceIndex = vertexFaces[i];
                    int mark = faces[faceIndex].mark;
                    groups[mark].Add(faceIndex);
                }

                // Calculate and set normal for each face on the vertex based on the groups
                Vector3 normal;
                for (grp = 0; grp < groupIndex; ++grp)
                {
                    normal = Vector3.zero;
                    List<int> grpFaces = groups[grp];
                    int cnt = grpFaces.Count;
                    grpCornerIndex.Clear();
                    for (i = 0; i < cnt; ++i)
                    {
                        Face f = faces[grpFaces[i]];
                        if (f.normal == Vector3.zero)
                        {
                            //	Debug.Log("face has zero normal .. valid " + f.valid);
                        }
                        // Multiply with corner angle (=SLOW?)
                        int corner = f.CornerIndex(vertexIndex);
                        float fact = CornerAngle(grpFaces[i], corner);
                        normal += f.normal * fact;
                        grpCornerIndex.Add(corner);
                    }
                    NormalizeSmallVector(ref normal);
                    if (normal == Vector3.zero)
                    {
                        //	Debug.Log("NORMAL == ZERO facecount " + cnt);
                    }
                    // Now set the normal to all group faces
                    for (i = 0; i < cnt; ++i)
                    {
                        faces[grpFaces[i]].vertexNormal[grpCornerIndex[i]] = normal;
                    }
                }
            }

            /// <summary>
            /// Calculates the face vertex normals based on the edge crease information. Edges marked as
            /// creases will get split normals.
            /// </summary>
            public void CalculateFaceVertexNormalsFromEdgeCreases()
            {
                CalculateFaceNormals(); // maybe not needed
                CalculateEdgeLinkedFaces(); // maybe not needed
                List<int>[] faceLinkedEdges = CalculateFaceLinkedEdges();
                int numVerts = vertCount();
                for (int vertexIndex = 0; vertexIndex < numVerts; ++vertexIndex)
                {
                    if (IsVertexValid(vertexIndex))
                    {
                        CalculateFaceVertexNormalsFromEdgeCreasesForVertex(vertexIndex, ref faceLinkedEdges);
                    }
                }
            }

            public new int CollapseVertexPair(CollapseInfo info)
            {
                VertexPair pair = info.vp;
                int v0 = pair.v[0];
                int v1 = pair.v[1];
                List<int> v0Edges = linkedEdges[v0];
                List<int> v1Edges = linkedEdges[v1];
                int i;
                // Update edges 
                // Mark the vertices that are connected by edges
                for (i = 0; i < v1Edges.Count; ++i)
                {
                    int edgeIndex = v1Edges[i];
                    int other = edges[edgeIndex].OtherVertex(v1);
                    vertices[other].mark = -1;
                }
                for (i = 0; i < v0Edges.Count; ++i)
                {
                    int edgeIndex = v0Edges[i];
                    int other = edges[edgeIndex].OtherVertex(v0);
                    vertices[other].mark = edgeIndex;
                }
                // now v1 verts that are only connected to v1 have value -1, double edge-connected verts have the edgeindex as mark				
                for (i = 0; i < v1Edges.Count; ++i)
                {
                    int edgeIndex = v1Edges[i];
                    if (vertices[edges[edgeIndex].OtherVertex(v1)].mark == -1)
                    {
                        edges[edgeIndex].ReplaceVertex(v1, v0);
                        if (IsEdgeValid(edgeIndex))
                        {
                            v0Edges.Add(edgeIndex);
                        }
                    }
                    else
                    {
                        Edge e1 = edges[edgeIndex];
                        int vindex = e1.OtherVertex(v1);
                        Edge e0 = edges[vertices[vindex].mark]; // vertex mark is edge index!
                                                                // There has to be another edge connecting v0 to vertex vindex
                        e0.crease = Mathf.Max(e1.crease, e0.crease); // keep the max crease value
                        UnlinkEdge(edgeIndex); // no more need for this!
                    }
                }
                // Remove invalid edges from mesh
                for (i = v0Edges.Count - 1; i >= 0; --i)
                { // backwards should be faster and i stays valid!
                    int edgeIndex = v0Edges[i];
                    if (IsEdgeValid(edgeIndex) == false)
                    {
                        UnlinkEdge(edgeIndex);
                        //v0Edges.Remove(edgeIndex);
                    }
                }

                // Deal with vertices and faces in baseclass
                info.vp = new VertexPair(v0, v1); // the original might have been invalidated THIS IS BAD
                base.CollapseVertexPair(info);
                v1Edges.Clear();

                // rebuild linkedfaces for the remaining edges
                for (i = 0; i < v0Edges.Count; ++i)
                {
                    Edge edge = edges[v0Edges[i]];
                    edge.linkedFaces.Clear();
                    CollectVertexPairFaces(edge, edge.linkedFaces);
                }

                return v0;
            }

            public void RebuildMesh(bool verbose = false)
            {
                /*		for (int i = 0; i < edgeCount; ++i) {
                            if (IsEdgeValid(i)) {
                                edgeTable[i] = newEdges.Count;
                                newEdges.Add(edges[i]);
                            }
                        } */

                int[] vertexTable = null;
                int[] faceTable = null; // not really used for now
                base.RebuildMesh(ref vertexTable, ref faceTable, verbose);

                int edgeCount = edges.Count;
                List<Edge> newEdges = new List<Edge>();
                //edges = newEdges;
                //edgeCount = edges.Count;
                for (int i = 0; i < edgeCount; ++i)
                {
                    Edge e = edges[i];
                    if (e.v[0] != e.v[1])
                    {
                        e.v[0] = vertexTable[e.v[0]];
                        e.v[1] = vertexTable[e.v[1]];
                        if (e.linkedFaces.Count > 0 && e.v[0] != -1 && e.v[1] != -1 && e.v[0] != e.v[1])
                        { // -1 is for invalid verts
                            newEdges.Add(e);
                        }
                    }
                    e.linkedFaces.Clear();
                }
                edges = newEdges;
                if (verbose)
                {
                    Debug.Log("Rebuild Edge Count " + edgeCount + " -> " + newEdges.Count);
                }

                RegenerateVertexLinkedEdges();
                CalculateEdgeLinkedFaces();
            }

            public float CalculateEdgeAngle(int edgeIndex)
            {
                IndexList edgefaces = edges[edgeIndex].linkedFaces;
                if (edgefaces.Count != 2) return 180.0f;
                Face f1 = faces[edgefaces[0]];
                Face f2 = faces[edgefaces[1]];
                return Vector3.Angle(f1.normal, f2.normal);
            }

            public bool CanEdgeBeDissolved(int edgeIndex)
            {
                // TODO!
                Edge e = edges[edgeIndex];
                // Don't dissolve creases
                if (e.crease == 1.0f) return false;
                // Only dissolve edges with two linked faces
                if (e.linkedFaces.Count != 2) return false;

                int f1index = e.linkedFaces[0];
                int f2index = e.linkedFaces[1];
                Face f1 = faces[f1index];
                Face f2 = faces[f2index];
                // Don't dissolve material borders
                if (f1.material != f2.material) return false;

                int f1corner1 = f1.CornerIndex(e.v[0]);
                int f1corner2 = f1.CornerIndex(e.v[1]);
                int f2corner1 = f2.CornerIndex(e.v[0]);
                int f2corner2 = f2.CornerIndex(e.v[1]);
                if (hasUV1)
                {
                    // Don't dissolve uv1 borders
                    if (f1.uv1[f1corner1] != f2.uv1[f2corner1]) return false;
                    if (f1.uv1[f1corner2] != f2.uv1[f2corner2]) return false;
                }
                if (hasUV2)
                {
                    if (f1.uv2[f1corner1] != f2.uv2[f2corner1]) return false;
                    if (f1.uv2[f1corner2] != f2.uv2[f2corner2]) return false;
                }
                return true;
            }

            // Dissolve an edge between two triangles.
            // the first is a quad afterwards and the second triangle is destroyed
            // expects valid input, no checks!
            public void DissolveEdgeTriangles(int edgeIndex)
            {
                int i;

                Edge e = edges[edgeIndex];
                // e needs to have two linked faces
                int f1index = e.linkedFaces[0];
                int f2index = e.linkedFaces[1];
                Face f1 = faces[f1index];
                Face f2 = faces[f2index];
                // f1 and f2 have two common vertices, need to be triangles
                int v2uniqueCorner = -1;
                for (i = 0; i < 3; ++i)
                {
                    if (e.ContainsVertex(f2.v[i]) == false)
                    {
                        v2uniqueCorner = i;
                        break;
                    }
                }
                for (i = 0; i < 3; ++i)
                {
                    if (e.ContainsVertex(f1.v[i]) == false)
                    {
                        int insertPos = i + 2;
                        if (insertPos >= 3) insertPos -= 3;

                        f1.cornerCount = 4; // f1 is now a quad
                        UnlinkFace(f2index);
                        vertices[f2.v[v2uniqueCorner]].linkedFaces.Add(f1index);
                        // Move verts backwards
                        for (int j = 3; j > insertPos; --j)
                        {
                            f1.CopyVertexInfoFromFace(f1, j - 1, j);
                        }
                        f1.CopyVertexInfoFromFace(f2, v2uniqueCorner, insertPos);
                        e.linkedFaces.Clear();
                        break;
                    }
                }
            }

            public override void Clear()
            {
                ClearEdges();
                base.Clear();
            }
        }
    }
}

