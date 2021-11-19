using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LCH
{
    partial class LchMeshPainter
    {
        private class RenderTextureTypeUndoRedo
        {
            public struct Records
            {
                public RenderTexture[] rt0;
            }

 
            int curPosition = -1;
            List<Records> records = new List<Records>();
            public void Add(Texture[] rt0)
            {
                while (records.Count > curPosition + 1 && records.Count > 0)
                {
                    Records _rs = records[records.Count - 1];
                    records.RemoveAt(records.Count - 1);
                    for (int i = 0; i < _rs.rt0.Length; i++)
                    {
                        if (_rs.rt0[i] != null)
                            GameObject.DestroyImmediate(_rs.rt0[i], true);
                    }
                }
                while (records.Count > UndoRedoMaxCount && records.Count > 0)
                {
                    Records _rs = records[0];
                    records.RemoveAt(0);
                    for (int i = 0; i < _rs.rt0.Length; i++)
                    {
                        if (_rs.rt0[i] != null)
                            GameObject.DestroyImmediate(_rs.rt0[i], true);
                    }
                }
                Records rs = new Records();
                rs.rt0 = new RenderTexture[rt0.Length];
                for (int i = 0; i < rt0.Length; i++)
                {
                    if (null != rt0[i])
                    {
                        RenderTexture rt = new RenderTexture(rt0[i].width, rt0[i].height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                        Graphics.Blit(rt0[i], rt);
                        rs.rt0[i] = rt;
                    }
                }
                records.Add(rs);
                curPosition = records.Count - 1;
            }
            public void Clear()
            {
                foreach (Records _rs in records)
                {
                    for (int i = 0; i < _rs.rt0.Length; i++)
                    {
                        if (_rs.rt0[i] != null)
                            GameObject.DestroyImmediate(_rs.rt0[i], true);
                    }
                }
                records.Clear();
                curPosition = records.Count - 1;
            }
            public bool CanUndo()
            {
                return curPosition >= 1;
            }
            public void UnDo(RenderTexture[] rt0)
            {
                if (CanUndo())
                {
                    curPosition--;
                    Records _rs = records[curPosition];
                    for (int i = 0; i < _rs.rt0.Length; i++)
                    {
                        if (null != rt0[i] && null != _rs.rt0[i])
                        {
                            Graphics.Blit(_rs.rt0[i], rt0[i]);
                        }
                    }

                }
            }
            public bool CanRedo()
            {
                return curPosition < records.Count - 1;
            }
            public void ReDo(RenderTexture[] rt0)
            {
                if (CanRedo())
                {
                    curPosition++;
                    Records _rs = records[curPosition];
                    for (int i = 0; i < _rs.rt0.Length; i++)
                    {
                        if (null != _rs.rt0[i] && null != _rs.rt0[i])
                        {
                            Graphics.Blit(_rs.rt0[i], rt0[i]);
                        }
                    }
                }
            }
        }
    }
        
}
