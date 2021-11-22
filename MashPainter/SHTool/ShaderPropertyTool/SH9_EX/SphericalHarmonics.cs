
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

#endif

[System.Serializable]
public class LchSH9
{
#if UNITY_EDITOR
    public Cubemap cubemap;
    public bool foldout = false;
#endif
    public Vector4[] coefficients = null;
    
#if UNITY_EDITOR
    public static void SaveCoefficients(SerializedProperty sp_curveList, Vector4[] coefficients)
    {
     
        sp_curveList.arraySize = coefficients.Length;

        for (int i = 0; i < coefficients.Length; i++)
        {
            var curveData = coefficients[i];
            SerializedProperty sp_CurveData = sp_curveList.GetArrayElementAtIndex(i);
            sp_CurveData.vector4Value = curveData;
             
        }
        sp_curveList.serializedObject.ApplyModifiedProperties();
    }

#endif
    #region SH9方法. 
    public static void sphericalHarmonicsFromCubemap9(Cubemap cubeTexture, ref Vector3[] output, bool GrammaSpace)
    {
        float[] resultR = new float[9];
        float[] resultG = new float[9];
        float[] resultB = new float[9];

        float fWt = 0.0f;
        for (uint i = 0; i < 9; i++)
        {
            resultR[i] = 0;
            resultG[i] = 0;
            resultB[i] = 0;
        }

        float[] shBuff = new float[9];
        float[] shBuffB = new float[9];

        // for each face of cube texture
        for (int face = 0; face < 6; face++)
        {
            // step between two texels for range [0, 1]
            float invWidth = 1.0f / cubeTexture.width;
            // initial negative bound for range [-1, 1]
            float negativeBound = -1.0f + invWidth;
            // step between two texels for range [-1, 1]
            float invWidthBy2 = 2.0f / cubeTexture.width;

            Color[] data = cubeTexture.GetPixels((CubemapFace)face);

            for (int y = 0; y < cubeTexture.width; y++)
            {
                // texture coordinate V in range [-1 to 1]
                float fV = negativeBound + y * invWidthBy2;

                for (int x = 0; x < cubeTexture.width; x++)
                {
                    // texture coordinate U in range [-1 to 1]
                    float fU = negativeBound + x * invWidthBy2;

                    // determine direction from center of cube texture to current texel
                    Vector3 dir;

                    switch ((CubemapFace)face)
                    {
                        case CubemapFace.PositiveX:
                            dir.x = 1.0f;
                            dir.y = 1.0f - (invWidthBy2 * y + invWidth);
                            dir.z = 1.0f - (invWidthBy2 * x + invWidth);
                            break;
                        case CubemapFace.NegativeX:
                            dir.x = -1.0f;
                            dir.y = 1.0f - (invWidthBy2 * y + invWidth);
                            dir.z = -1.0f + (invWidthBy2 * x + invWidth);
                            break;
                        case CubemapFace.PositiveY:
                            dir.x = -1.0f + (invWidthBy2 * x + invWidth);
                            dir.y = 1.0f;
                            dir.z = -1.0f + (invWidthBy2 * y + invWidth);
                            break;
                        case CubemapFace.NegativeY:
                            dir.x = -1.0f + (invWidthBy2 * x + invWidth);
                            dir.y = -1.0f;
                            dir.z = 1.0f - (invWidthBy2 * y + invWidth);
                            break;
                        case CubemapFace.PositiveZ:
                            dir.x = -1.0f + (invWidthBy2 * x + invWidth);
                            dir.y = 1.0f - (invWidthBy2 * y + invWidth);
                            dir.z = 1.0f;
                            break;
                        case CubemapFace.NegativeZ:
                            dir.x = 1.0f - (invWidthBy2 * x + invWidth);
                            dir.y = 1.0f - (invWidthBy2 * y + invWidth);
                            dir.z = -1.0f;
                            break;
                        default:
                            return;
                    }

                    // normalize direction
                    dir = dir.normalized;

                    // scale factor depending on distance from center of the face
                    float fDiffSolid = 4.0f / ((1.0f + fU * fU + fV * fV) * Mathf.Sqrt(1.0f + fU * fU + fV * fV));
                    fWt += fDiffSolid;

                    // calculate coefficients of spherical harmonics for current direction
                    sphericalHarmonicsEvaluateDirection9(ref shBuff, dir);
                    //XMSHEvalDirection(dir, ref shBuff);

                    // index of texel in texture
                    int pixOffsetIndex = x + y * cubeTexture.width;
                    // get color from texture and map to range [0, 1]
                    Vector3 clr = new Vector3(data[pixOffsetIndex].r, data[pixOffsetIndex].g, data[pixOffsetIndex].b);

                    if (GrammaSpace)
                    {
                        clr.x = Mathf.GammaToLinearSpace(clr.x);
                        clr.y = Mathf.GammaToLinearSpace(clr.y);
                        clr.z = Mathf.GammaToLinearSpace(clr.z);
                    }
                    // scale color and add to previously accumulated coefficients
                    sphericalHarmonicsScale9(ref shBuffB, shBuff, clr.x * fDiffSolid);
                    sphericalHarmonicsAdd9(ref resultR, resultR, shBuffB);
                    sphericalHarmonicsScale9(ref shBuffB, shBuff, clr.y * fDiffSolid);
                    sphericalHarmonicsAdd9(ref resultG, resultG, shBuffB);
                    sphericalHarmonicsScale9(ref shBuffB, shBuff, clr.z * fDiffSolid);
                    sphericalHarmonicsAdd9(ref resultB, resultB, shBuffB);
                }
            }
        }

        // final scale for coefficients
        float fNormProj = (4.0f * Mathf.PI) / fWt;
        sphericalHarmonicsScale9(ref resultR, resultR, fNormProj);
        sphericalHarmonicsScale9(ref resultG, resultG, fNormProj);
        sphericalHarmonicsScale9(ref resultB, resultB, fNormProj);

        // save result
        for (uint i = 0; i < 9; i++)
        {
            output[i].x = resultR[i];
            output[i].y = resultG[i];
            output[i].z = resultB[i];
        }
    }

    public void Commint(string name, string keyWorld)
    {
        if (coefficients.Length > 0)
        {
            Shader.SetGlobalVector(name + "_SHAr", coefficients[0]);
            Shader.SetGlobalVector(name + "_SHAg", coefficients[1]);
            Shader.SetGlobalVector(name + "_SHAb", coefficients[2]);
            Shader.SetGlobalVector(name + "_SHBr", coefficients[3]);
            Shader.SetGlobalVector(name + "_SHBg", coefficients[4]);
            Shader.SetGlobalVector(name + "_SHBb", coefficients[5]);
            Shader.SetGlobalVector(name + "_SHC", coefficients[6]);
            if(keyWorld.Length>0)
                Shader.EnableKeyword(keyWorld);
        }
        else
        {
            if (keyWorld.Length > 0)
                Shader.DisableKeyword(keyWorld);
        }
    }
    public void Clear(string keyWorld)
    {
        if (keyWorld.Length > 0)
            Shader.DisableKeyword(keyWorld);
    }

    private static Vector3 DecodeHDR(Color clr)
    {
        return new Vector3(clr.r, clr.g, clr.b) * clr.a;// * Mathf.Pow(clr.a, 2);// * (Mathf.Pow(clr.a, 0.1f) * 1);
    }

    private static void sphericalHarmonicsEvaluateDirection9(ref float[] outsh, Vector3 dir)
    {
        // 86 clocks
        // Make sure all constants are never computed at runtime
        const float kInv2SqrtPI = 0.28209479177387814347403972578039f; // 1 / (2*sqrt(kPI))
        const float kSqrt3Div2SqrtPI = 0.48860251190291992158638462283835f; // sqrt(3) / (2*sqrt(kPI))
        const float kSqrt15Div2SqrtPI = 1.0925484305920790705433857058027f; // sqrt(15) / (2*sqrt(kPI))
        const float k3Sqrt5Div4SqrtPI = 0.94617469575756001809268107088713f; // 3 * sqrtf(5) / (4*sqrt(kPI))
        const float kSqrt15Div4SqrtPI = 0.54627421529603953527169285290135f; // sqrt(15) / (4*sqrt(kPI))
        const float kOneThird = 0.3333333333333333333333f; // 1.0/3.0
        outsh[0] = kInv2SqrtPI;
        outsh[1] = -dir.y * kSqrt3Div2SqrtPI;
        outsh[2] = dir.z * kSqrt3Div2SqrtPI;
        outsh[3] = -dir.x * kSqrt3Div2SqrtPI;
        outsh[4] = dir.x * dir.y * kSqrt15Div2SqrtPI;
        outsh[5] = -dir.y * dir.z * kSqrt15Div2SqrtPI;
        outsh[6] = (dir.z * dir.z - kOneThird) * k3Sqrt5Div4SqrtPI;
        outsh[7] = -dir.x * dir.z * kSqrt15Div2SqrtPI;
        outsh[8] = (dir.x * dir.x - dir.y * dir.y) * kSqrt15Div4SqrtPI;
    }

    private static void sphericalHarmonicsAdd9(ref float[] result, float[] inputA, float[] inputB)
    {
        for (int i = 0; i < 9; i++)
        {
            result[i] = inputA[i] + inputB[i];
        }
    }

    private static void sphericalHarmonicsScale9(ref float[] result, float[] input, float scale)
    {
        for (int i = 0; i < 9; i++)
        {
            result[i] = input[i] * scale;
        }
    }

    public static readonly float s_fSqrtPI = Mathf.Sqrt(Mathf.PI);
    public static readonly float fC0 = 1.0f / (2.0f * s_fSqrtPI);
    public static readonly float fC1 = Mathf.Sqrt(3.0f) / (3.0f * s_fSqrtPI);
    public static readonly float fC2 = Mathf.Sqrt(15.0f) / (8.0f * s_fSqrtPI);
    public static readonly float fC3 = Mathf.Sqrt(5.0f) / (16.0f * s_fSqrtPI);
    public static readonly float fC4 = 0.5f * fC2;
    public static void ConvertSHConstants(Vector3[] sh, ref Vector4[] SHArBrC)
    {
        int iC;
        for (iC = 0; iC < 3; iC++)
        {
            SHArBrC[iC].x = -fC1 * sh[3][iC];
            SHArBrC[iC].y = -fC1 * sh[1][iC];
            SHArBrC[iC].z = fC1 * sh[2][iC];
            SHArBrC[iC].w = fC0 * sh[0][iC] - fC3 * sh[6][iC];
        }

        for (iC = 0; iC < 3; iC++)
        {
            SHArBrC[iC + 3].x = fC2 * sh[4][iC];
            SHArBrC[iC + 3].y = -fC2 * sh[5][iC];
            SHArBrC[iC + 3].z = 3.0f * fC3 * sh[6][iC];
            SHArBrC[iC + 3].w = -fC2 * sh[7][iC];
        }

        SHArBrC[6].x = fC4 * sh[8][0];
        SHArBrC[6].y = fC4 * sh[8][1];
        SHArBrC[6].z = fC4 * sh[8][2];
        SHArBrC[6].w = 1.0f;
    }
    #endregion
}

public class LchSH9Attribte : LabelAttribute
{
    public LchSH9Attribte(string label, string conditionalSourceField) : base(label, conditionalSourceField)
    {
    }
    public LchSH9Attribte(string label) : base(label)
    {
    }
}

[CustomPropertyDrawer(typeof(LchSH9Attribte))]
public class LchSH9AttribteDrawer : ConditionalHidePropertyDrawer
{

    public override float BaseGetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty _coeficients = property.FindPropertyRelative("coefficients");
        SerializedProperty _foldout = property.FindPropertyRelative("foldout");
        if (_coeficients.arraySize == 0)
            return 55;
        if (_foldout.boolValue)
            return 55 + _coeficients.arraySize * 20 + 20;
        else
            return 55 + 40;
    }
    public override void DrawPropertyItem(LabelAttribute condHAtt, Rect position, SerializedProperty property, GUIContent label)
    {
        Rect _rect = position;
        _rect.height = 50;
        EditorGUI.LabelField(_rect, label.text);
        SerializedProperty _cubemap = property.FindPropertyRelative("cubemap");
        SerializedProperty _coeficients = property.FindPropertyRelative("coefficients");
        SerializedProperty _foldout = property.FindPropertyRelative("foldout");

        EditorGUI.BeginChangeCheck();
        Rect _rect2 = _rect;
        _rect2.x = _rect.width - 50;
        _rect2.width = 50;
        Cubemap cubemap = (Cubemap)_cubemap.objectReferenceValue;
        cubemap = (Cubemap)EditorGUI.ObjectField(_rect2, cubemap, typeof(Cubemap), true);
        if (EditorGUI.EndChangeCheck())
        {
            _rect.y += 50;
            _rect.height = 30;
            if (null == cubemap)
            {
                _coeficients.arraySize = 0;
            }
            else
            {
                Vector4[] coefficients = new Vector4[7];
                CheckAndConvertEnvMap(ref cubemap, ref coefficients);
                LchSH9.SaveCoefficients(_coeficients, coefficients);
            }
        }
        _cubemap.objectReferenceValue = cubemap;
        _rect.y += 55;
        _rect.height = 20;
        property.serializedObject.ApplyModifiedProperties();
        _rect.x += 20;
        _rect.width -= 20;
        if (_coeficients.arraySize == 0)
            return;
        _foldout.boolValue = EditorGUI.Foldout(_rect, _foldout.boolValue, "sh9");
        _rect.x -= 20;
        _rect.width += 20;
        _rect.y += 20;
         
        if (_foldout.boolValue)
        {
            for (int i = 0; i < _coeficients.arraySize; i++)
            {
                SerializedProperty sp_cf = _coeficients.GetArrayElementAtIndex(i);

                EditorGUI.Vector4Field(_rect, "  ", sp_cf.vector4Value);
                _rect.y += 20;
            }
        }
    }
    private static void CheckAndConvertEnvMap(ref Cubemap envMap, ref Vector4[] sh_out)
    {
        if (!envMap) return;

        string map_path = AssetDatabase.GetAssetPath(envMap);

        if (string.IsNullOrEmpty(map_path)) return;

        TextureImporter ti = AssetImporter.GetAtPath(map_path) as TextureImporter;
        if (!ti) return;

        bool need_reimport = false;
        if (!ti.isReadable)
        {
            ti.isReadable = true;
            need_reimport = true;
        }
        if (!ti.mipmapEnabled)
        {
            ti.mipmapEnabled = true;
            need_reimport = true;
        }
         

        TextureImporterSettings tis = new TextureImporterSettings();
        ti.ReadTextureSettings(tis);
        if (tis.cubemapConvolution != TextureImporterCubemapConvolution.Specular)
        {
            tis.cubemapConvolution = TextureImporterCubemapConvolution.Specular;
            ti.SetTextureSettings(tis);
            need_reimport = true;
        }
        if (need_reimport)
        {
            ti.SaveAndReimport();
            envMap = AssetDatabase.LoadAssetAtPath<Cubemap>(map_path);
            if (!envMap) return;
        }
        Vector3[] sh = new Vector3[9];
        LchSH9.sphericalHarmonicsFromCubemap9((Cubemap)envMap, ref sh, (PlayerSettings.colorSpace == ColorSpace.Gamma));
        LchSH9.ConvertSHConstants(sh, ref sh_out);
    }
   
}
