using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 


namespace LCH {	
	 
	public partial class LchMeshPainter
    {
        private static void BuildLodMesh(Mesh target, int faceCount, bool highQuality = true)
        {
            MeshEdges kmesh = new  MeshEdges();
            Simplify sim = new Simplify();
            SimplifyParameters simpars = new SimplifyParameters();

            UnityMeshToMeshEdges(target, kmesh);
            simpars.targetFaceCount = faceCount;
            simpars.recalculateVertexPositions = highQuality;
            simpars.checkTopology = !highQuality;
            simpars.maxEdgesPerVertex = highQuality ? 18 : 0;
            if (highQuality == false)
            {
                simpars.preventNonManifoldEdges = false;
                simpars.boneWeightProtection = 0.0f;
                simpars.vertexColorProtection = 0.0f;
            }
            sim.Execute(ref kmesh, simpars);
            MeshEdgesToUnityMesh(kmesh, target);
        }

        private static void UnityMeshToMeshEdges(UnityEngine.Mesh unityMesh, MeshEdges meshEdges, float tolerance = 0.0f) {
			Vector3[] verts = unityMesh.vertices;
			Vector3[] normals = unityMesh.normals;
			Color[] vertColors = unityMesh.colors;
			BoneWeight[] boneWeights = unityMesh.boneWeights;
			Vector2[] uv1 = unityMesh.uv;
			Vector2[] uv2 = unityMesh.uv2;
			
			meshEdges.Clear();
			meshEdges.numMaterials = unityMesh.subMeshCount;
			meshEdges.equalityTolerance = tolerance;
			
			int numVerts = verts.Length;
			meshEdges.hasVertexColors = (vertColors.Length == numVerts);
			meshEdges.bindposes = unityMesh.bindposes;
			meshEdges.hasBoneWeights = (meshEdges.bindposes != null && boneWeights.Length == numVerts);
			meshEdges.hasUV1 = (uv1.Length == numVerts);
			meshEdges.hasUV2 = (uv2.Length == numVerts);
			
			int i;				
			for (i = 0; i < numVerts; ++i) {
				meshEdges.AddVertex(verts[i]);
			}
			if (meshEdges.hasVertexColors) {
				for (i = 0; i < numVerts; ++i) {
					meshEdges.vertices[i].color = vertColors[i];
				}
			}
			if (meshEdges.hasBoneWeights) {
				for (i = 0; i < numVerts; ++i) {
					meshEdges.vertices[i].boneWeight = boneWeights[i];
				}
			}
			
			bool quad = true;
			for (int m = 0; m < meshEdges.numMaterials; ++m) {
				if (unityMesh.GetTopology(m) != UnityEngine.MeshTopology.Quads) {
					quad = false;
					break;
				}
			}
				
			int v0, v1, v2, v3;
			for (int m = 0; m < meshEdges.numMaterials; ++m) {
				if (quad) {
					int[] indices = unityMesh.GetIndices(m);
					int num = indices.Length;
					for (i = 0; i < num;) {
						v0 = indices[i++]; v1 = indices[i++]; v2 = indices[i++]; v3 = indices[i++];
						Face f = new Face(v0, v1, v2, v3);
						meshEdges.AddFace(f);
						f.vertexNormal[0] = normals[v0]; f.vertexNormal[1] = normals[v1]; f.vertexNormal[2] = normals[v2]; f.vertexNormal[3] = normals[v3];
						if (meshEdges.hasUV1) {
							f.uv1[0] = uv1[v0]; f.uv1[1] = uv1[v1]; f.uv1[2] = uv1[v2]; f.uv1[3] = uv1[v3];
						}
						if (meshEdges.hasUV2) {
							f.uv2[0] = uv2[v0]; f.uv2[1] = uv2[v1]; f.uv2[2] = uv2[v2]; f.uv2[3] = uv2[v3];
						}
						f.material = m;
					}
				} else {
					int[] tris = unityMesh.GetTriangles(m);
					int num = tris.Length;
					for (i = 0; i < num;) {
						v0 = tris[i++]; v1 = tris[i++]; v2 = tris[i++];
						Face f = new Face(v0, v1, v2);
						meshEdges.AddFace(f);
						f.vertexNormal[0] = normals[v0]; f.vertexNormal[1] = normals[v1]; f.vertexNormal[2] = normals[v2];
						if (meshEdges.hasUV1) {
							f.uv1[0] = uv1[v0]; f.uv1[1] = uv1[v1]; f.uv1[2] = uv1[v2];
						}
						if (meshEdges.hasUV2) {
							f.uv2[0] = uv2[v0]; f.uv2[1] = uv2[v1]; f.uv2[2] = uv2[v2];
						}
						f.material = m;
					}
				}
			}
			
			 RemoveDoubleVertices(meshEdges);
			meshEdges.GenerateEdgeList();
			meshEdges.CalculateEdgeLinkedFaces();
			meshEdges.topology = quad ? MeshTopology.Quads : MeshTopology.Triangles;
			CreaseDetect.MarkCreasesFromFaceNormals(meshEdges);
		}
        private static void Triangulate(BaseMesh mesh)
        {
            if (mesh.topology == MeshTopology.Triangles) return;

            int faceIndex, cornerIndex;

            int numFaces = mesh.faceCount();
            Vector3[] coords = new Vector3[4];
            for (faceIndex = 0; faceIndex < numFaces; ++faceIndex)
            {
                Face f = mesh.faces[faceIndex];
                if (f.valid && f.cornerCount == 4)
                {
                    Face fnew = new Face(3);
                    // copy face parameters
                    fnew.material = f.material;
                    for (cornerIndex = 0; cornerIndex < 4; ++cornerIndex)
                    {
                        coords[cornerIndex] = mesh.vertices[f.v[cornerIndex]].coords;
                    }

                    Vector3 diag02 = coords[0] - coords[2];
                    Vector3 diag13 = coords[1] - coords[3];
                    if (diag02.sqrMagnitude < diag13.sqrMagnitude)
                    {
                        // Create triangles 201 and 023 .. use diag as first edge as a hint
                        fnew.CopyVertexInfoFromFace(f, 0, 0);
                        fnew.CopyVertexInfoFromFace(f, 2, 1);
                        fnew.CopyVertexInfoFromFace(f, 3, 2);
                    }
                    else
                    {
                        // Create triangles 312 and 013 .. use diag as first edge as a hint
                        fnew.CopyVertexInfoFromFace(f, 3, 0);
                        fnew.CopyVertexInfoFromFace(f, 1, 1);
                        fnew.CopyVertexInfoFromFace(f, 2, 2);
                        f.CopyVertexInfoFromFace(f, 3, 2);
                    }
                    mesh.AddFace(fnew);

                    f.cornerCount = 3;
                }
            }

            int numVerts = mesh.vertCount();
            for (int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
            {
                mesh.vertices[vertIndex].linkedFaces = new IndexList(18);
            }
            numFaces = mesh.faceCount();

            for (faceIndex = 0; faceIndex < numFaces; ++faceIndex)
            {
                Face f = mesh.faces[faceIndex];
                for (cornerIndex = 0; cornerIndex < f.cornerCount; ++cornerIndex)
                {
                    mesh.vertices[f.v[cornerIndex]].linkedFaces.Add(faceIndex);
                }
            }
            mesh.topology = MeshTopology.Triangles;
        }
        private static void RemoveDoubleVertices(BaseMesh mesh)
        {
            int numVerts = mesh.vertCount();

            List<int> vertexOrder = new List<int>(numVerts);
            for (int i = 0; i < numVerts; ++i) vertexOrder.Add(i);

            vertexOrder.Sort(delegate (int a, int b) {
                return mesh.CompareVertices(a, b, mesh.equalityTolerance);
            });

            int[] uniqueVertex = new int[numVerts];
            int unique = vertexOrder[0];
            uniqueVertex[unique] = unique;
            for (int i = 1; i < numVerts; ++i)
            {
                int vertexIndex = vertexOrder[i];
                if (mesh.CompareVertices(unique, vertexIndex, mesh.equalityTolerance) != 0)
                {
                    unique = vertexIndex;
                }
                uniqueVertex[vertexIndex] = unique;
            }
            // TODO: maybe use the center of all the almost equal vertices .. probably does not matter
            for (int i = 0; i < numVerts; ++i)
            {
                if (i != uniqueVertex[i])
                {
                    mesh.ReplaceVertex(i, uniqueVertex[i]);
                }
            }
            /* int num = */
            mesh.InvalidateUnconnectedVertices();
            //Debug.Log("RemoveDoubleVertices Invalidated " + num + " unconnected vertices.");
            /*num = */
            mesh.InvalidateDegenerateFaces();
            // Debug.Log("RemoveDoubleVertices Invalidated " + num + " degenerate faces.");
        }


        private static void MeshEdgesToUnityMesh(MeshEdges meshEdges, UnityEngine.Mesh unityMesh) {	
			meshEdges.InvalidateDegenerateFaces();
		//	Ops.TriangulateWithEdges(meshEdges);
			int numFaces = meshEdges.faceCount();
					
			List<UnityVertex> exVerts = new List<UnityVertex>(numFaces*3);
			
			List<Vector3> verts = new List<Vector3>();
			List<int>[] indices = new List<int>[meshEdges.numMaterials];	
			List<int> vertexTable = new List<int>();
			// Create a list of all vertices based on face corners (= lots of duplicates)
			for (int material = 0; material < meshEdges.numMaterials; ++material) {
				indices[material] = new List<int>();
				for (int faceIndex = 0; faceIndex < numFaces; ++faceIndex) {
					Face f = meshEdges.faces[faceIndex];
					if (f.valid && f.material == material) {
						int cornerCount = f.cornerCount;
						for (int cornerIndex = 0; cornerIndex < cornerCount; ++cornerIndex) {							
							UnityVertex ev = new UnityVertex();
							ev.vertIndex = f.v[cornerIndex];
							ev.cornerCount = cornerCount;
							ev.material = material;
							ev.coords = meshEdges.vertices[f.v[cornerIndex]].coords;
							ev.normal = f.vertexNormal[cornerIndex];
							ev.uv1 = f.uv1[cornerIndex];
							ev.uv2 = f.uv2[cornerIndex];
							
							vertexTable.Add(exVerts.Count);
							exVerts.Add(ev);
						}
					}
				}
			}
			
			vertexTable.Sort(delegate(int a, int b) {
				return UnityVertex.CompareEV(exVerts[a], exVerts[b]);
			});

			// Create index list to collapse list to the unique verts added to the final mesh
			// compile unique mesh arrays
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uv1 = new List<Vector2>();
			List<Vector2> uv2 = new List<Vector2>();
			List<Color> vertColors = new List<Color>();
			List<BoneWeight> boneWeights = new List<BoneWeight>();
			int[] uniqueTable = new int[exVerts.Count];
			int numUnique = 0;
			for (int i = 0; i < exVerts.Count; ++i) {
				int exIndex = vertexTable[i];
				if (i == 0 || UnityVertex.CompareEV(exVerts[vertexTable[i - 1]], exVerts[exIndex]) != 0) {
					verts.Add(exVerts[exIndex].coords);
					normals.Add(exVerts[exIndex].normal);
					uv1.Add(exVerts[exIndex].uv1);
					uv2.Add(exVerts[exIndex].uv2);
					
					Vertex v = meshEdges.vertices[exVerts[exIndex].vertIndex];
					vertColors.Add(v.color);
					boneWeights.Add(v.boneWeight);					
					numUnique++;				
				}
				uniqueTable[exIndex] = numUnique - 1;
			}
 
			{
				// Mixed tris/quads need to split quads
				int cornerCount, mat;
				int i0, i1, i2, i3;
				for (i0 = 0; i0 < exVerts.Count;) {
					i1 = i0 + 1;
					i2 = i0 + 2;
	  				cornerCount = exVerts[i0].cornerCount;
					mat = exVerts[i0].material;
					if (cornerCount == 3) {
						indices[mat].Add(uniqueTable[i0]);
						indices[mat].Add(uniqueTable[i1]);
						indices[mat].Add(uniqueTable[i2]);
						i0 += 3;
					} else { // Quad!
						i3 = i0 + 3;
						Vector3 diag02 = exVerts[i0].coords - exVerts[i2].coords;
						Vector3 diag13 = exVerts[i1].coords - exVerts[i3].coords;
						
						if (diag02.sqrMagnitude > diag13.sqrMagnitude) {
						// If 0-2 if shorter than 1-3
							indices[mat].Add(uniqueTable[i0]);
							indices[mat].Add(uniqueTable[i1]);
							indices[mat].Add(uniqueTable[i2]);
	
							indices[mat].Add(uniqueTable[i0]);
							indices[mat].Add(uniqueTable[i2]);
							indices[mat].Add(uniqueTable[i3]);
						} else {					
							indices[mat].Add(uniqueTable[i0]);
							indices[mat].Add(uniqueTable[i1]);
							indices[mat].Add(uniqueTable[i3]);
	
							indices[mat].Add(uniqueTable[i1]);
							indices[mat].Add(uniqueTable[i2]);
							indices[mat].Add(uniqueTable[i3]);
						}
						i0 += 4;					
					}
				}
			}
			
			unityMesh.Clear(false);
			unityMesh.name = "Mesh";
			
			if (verts.Count >= 65536 || exVerts.Count >= 65536*3) {
				Debug.Log("Cannot create Unity mesh from LodlMesh.MeshEdges. " +
					"The mesh is too large (>65k). " +
					"Vertices: " + verts.Count + " Triangles: " + exVerts.Count/3);
				return;
			}
			
			unityMesh.subMeshCount = meshEdges.numMaterials;
			unityMesh.vertices = verts.ToArray();
			unityMesh.normals = normals.ToArray();
			if (meshEdges.hasVertexColors) {
				unityMesh.colors = vertColors.ToArray();
			}
			if (meshEdges.hasUV1) {
				unityMesh.uv = uv1.ToArray();
			}
			if (meshEdges.hasUV2) {
				unityMesh.uv2 = uv2.ToArray();
			}
			if (meshEdges.hasBoneWeights) {
				unityMesh.bindposes = meshEdges.bindposes;
				unityMesh.boneWeights = boneWeights.ToArray();
			}
#if false
			if (meshEdges.topology == MeshTopology.Quads) {
				for (int mat = 0; mat < meshEdges.numMaterials; ++mat) {
					unityMesh.SetIndices(indices[mat].ToArray(), UnityEngine.MeshTopology.Quads, mat);
				}
			} else
#endif
			{
				for (int mat = 0; mat < meshEdges.numMaterials; ++mat) {
					unityMesh.SetTriangles(indices[mat].ToArray(), mat);
				}
			}
			if (meshEdges.hasUV1 && meshEdges.calculateTangents) {
				_calculateMeshTangents(unityMesh);
			}
		}	
		
		private static void _calculateMeshTangents(UnityEngine.Mesh mesh)
		{
    		int[] triangles = mesh.triangles;
    		Vector3[] vertices = mesh.vertices;
    		Vector2[] uv = mesh.uv;
    		Vector3[] normals = mesh.normals;
 
    		//variable definitions
    		int triangleCount = triangles.Length;
    		int vertexCount = vertices.Length;
 
    		Vector3[] tan1 = new Vector3[vertexCount];
    		Vector3[] tan2 = new Vector3[vertexCount];
    		Vector4[] tangents = new Vector4[vertexCount];
 			Vector3 sdir, tdir, e1, e2;
			Vector2 s, t;
			float div, r;
			int a, i1, i2, i3;
	   		for (a = 0; a < triangleCount; a += 3) {
		        i1 = triangles[a];
		        i2 = triangles[a + 1];
		        i3 = triangles[a + 2];
		 
				e1 = vertices[i2] - vertices[i1];
				e2 = vertices[i3] - vertices[i1];
		 
				s = uv[i2] - uv[i1]; // UV side vectors
				t = uv[i3] - uv[i1];
						 
				div = (s.x*t.y - s.y*t.x); // Cross product = 2xarea of uv triangle
		        r = (div != 0.0f) ? 1.0f/div : 0.0f;
		 		
				t *= r; 
				s *= r;
				
				sdir = t.y*e1 - s.y*e2;
				tdir = s.x*e2 - t.x*e1;
								
				tan1[i1] += sdir;
		        tan1[i2] += sdir;
		        tan1[i3] += sdir;
		 
		        tan2[i1] += tdir;
		        tan2[i2] += tdir;
		        tan2[i3] += tdir;
		    }
 			Vector3 n, tan;
			//float val1, val2;
			//int axis;
    		for (a = 0; a < vertexCount; ++a) {
        		n = normals[a];
       			tan = tan1[a];
 
		        Vector3.OrthoNormalize(ref n, ref tan);
				tangents[a] = tan;
		 
				 
	
		        tangents[a].w = (Vector3.Dot(Vector3.Cross(n, tan), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		    }
		 
		    mesh.tangents = tangents;
		}

	}
	
	class UnityVertex {
		public int vertIndex;
		public int material;
		public int cornerCount;
		public Vector3 coords;
		public Vector3 normal;
		public Vector2 uv1;
		public Vector2 uv2;
		public static int CompareEV(UnityVertex a, UnityVertex b) {
			int res = Vector3Compare(a.coords, b.coords);
			if (res != 0) return res;
			res = Vector3Compare(a.normal, b.normal);
			if (res != 0) return res;
			res =  Vector2Compare(a.uv1, b.uv1);
			if (res != 0) return res;
			res =  Vector2Compare(a.uv2, b.uv2);
			return res;
		}
        private static int Vector3Compare(Vector3 a, Vector3 b)
        {
            if (a.x < b.x) return -1;
            if (a.x > b.x) return 1;
            if (a.y < b.y) return -1;
            if (a.y > b.y) return 1;
            if (a.z < b.z) return -1;
            if (a.z > b.z) return 1;
            return 0;
        }
        private static int Vector2Compare(Vector2 a, Vector2 b)
        {
            if (a.x < b.x) return -1;
            if (a.x > b.x) return 1;
            if (a.y < b.y) return -1;
            if (a.y > b.y) return 1;
            return 0;
        }
    }
}