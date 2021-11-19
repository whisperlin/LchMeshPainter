using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LCH
{
    partial class LchMeshPainter
    {
        public static int UndoRedoMaxCount = 30;
        private class VertexUndoRedo
        {
            public struct Records
            {
                public Color [] colors;
            }
            int curPosition = -1;
            List<Records> records = new List<Records>();
            public void Add(Color[] colors)
            {
                while (records.Count > curPosition + 1 && records.Count > 0)
                {
                    Records _rs = records[records.Count - 1];
                    records.RemoveAt(records.Count - 1);
                }
                while (records.Count > UndoRedoMaxCount && records.Count > 0)
                {
                    Records _rs = records[0];
                    records.RemoveAt(0);
                }
                Records rs = new Records();
                if (null == colors)
                {
                    rs.colors = null;
                }
                else
                {
                    rs.colors = new Color[colors.Length];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        rs.colors[i] = colors[i];
                    }
                }
                records.Add(rs);
                curPosition = records.Count - 1;
            }
            public void Clear()
            {
                records.Clear();
                curPosition = records.Count - 1;
            }
            public bool CanUndo()
            {
                return curPosition >= 1;
            }
            public void UnDo(Mesh mesh)
            {
                if (null == mesh)
                    return;
                if (CanUndo())
                {
                    curPosition--;
                    Records _rs = records[curPosition];
                    mesh.colors = _rs.colors;

                }
            }
            public bool CanRedo()
            {
                return curPosition < records.Count - 1;
            }
            public void ReDo(Mesh mesh)
            {
                if (null == mesh)
                    return;
                if (CanRedo())
                {
                    curPosition++;
                    Records _rs = records[curPosition];
                    mesh.colors = _rs.colors;
                }
            }
        }
    }
        
}
    
