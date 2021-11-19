// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Lch Mesh Painter/Scene Common" 
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
		[Toggle(_NORMALMAP)] _NORMALMAP("Enable Normal", Float) = 0
		_Splat0 ("Layer 1", 2D) = "white" {}
		
        [NoScaleOffset]_BumpMap0("Normal 1[_NORMALMAP]", 2D) = "bump" {}
		_Spe0("_Spe 1",Color) = (0,0,0,1)
		_Splat1 ("Layer 2", 2D) = "black" {}
		[NoScaleOffset]_BumpMap1("Normal 2[_NORMALMAP]", 2D) = "bump" {}
		_Spe1("_Spe 2",Color) = (0,0,0,1)
		_Splat2 ("Layer 3", 2D) = "black" {}
		[NoScaleOffset]_BumpMap2("Normal 3[_NORMALMAP]", 2D) = "bump" {}
		_Spe2("_Spe 3",Color) = (0,0,0,1)
		_Splat3 ("Layer 4[_LMPLAY_L4]
		Layer 4[_LMPLAY_L5]
		Layer 4[_LMPLAY_L6]
		Layer 4[_LMPLAY_L7]
		Layer 4[_LMPLAY_L8]
		", 2D) = "black" {}
		[NoScaleOffset]_BumpMap3("Normal 4[_NORMALMAP&_LMPLAY_L4]
		Normal 4[_NORMALMAP&_LMPLAY_L5]
		Normal 4[_NORMALMAP&_LMPLAY_L6]
		Normal 4[_NORMALMAP&_LMPLAY_L7]
		", 2D) = "bump" {}
		_Spe3("_Spe 4[_LMPLAY_L4]
		_Spe 4[_LMPLAY_L5]
		_Spe 4[_LMPLAY_L6]
		_Spe 4[_LMPLAY_L7]",Color) = (0,0,0,1)
		_Splat4 ("Layer 5[_LMPLAY_L5]
		Layer 5[_LMPLAY_L6]
		Layer 5[_LMPLAY_L7]
		Layer 5[_LMPLAY_L8]
		", 2D) = "black" {}
		[NoScaleOffset]_BumpMap4("Normal 5[_NORMALMAP&_LMPLAY_L5]
		Normal 5[_NORMALMAP&_LMPLAY_L6]
		Normal 5[_NORMALMAP&_LMPLAY_L7]
		Normal 5[_NORMALMAP&_LMPLAY_L8]", 2D) = "bump" {}
		_Spe4("_Spe 5[_LMPLAY_L5]
		_Spe 5[_LMPLAY_L6]
		_Spe 5[_LMPLAY_L7]
		_Spe 5[_LMPLAY_L8]",Color) = (0,0,0,1)
		_Splat5 ("Layer 6[_LMPLAY_L6]
		Layer 6[_LMPLAY_L7]
		Layer 6[_LMPLAY_L8]", 2D) = "black" {}
		[NoScaleOffset]_BumpMap5("Normal 6[_NORMALMAP&_LMPLAY_L6]
		Normal 6[_NORMALMAP&_LMPLAY_L7]
		Normal 6[_NORMALMAP&_LMPLAY_L8]", 2D) = "bump" {}
		_Spe5("_Spe 6[_LMPLAY_L6]
		_Spe 6[_LMPLAY_L7]
		_Spe 6[_LMPLAY_L8]",Color) = (0,0,0,1)
		_Splat6 ("Layer 7[_LMPLAY_L7]
		Layer 7[_LMPLAY_L8]
		", 2D) = "black" {}
		//[NoScaleOffset]_BumpMap6("Normal 7", 2D) = "bump" {}
		_Spe6("_Spe 7[_LMPLAY_L7]
		_Spe 7[_LMPLAY_L8]",Color) = (0,0,0,1)
		_Splat7 ("Layer 8[_LMPLAY_L8]", 2D) = "black" {}
		//[NoScaleOffset]_BumpMap7("Normal 8", 2D) = "bump" {}
		_Spe7("_Spe 6[_LMPLAY_L8",Color) = (0,0,0,1)
		[KeywordEnum(L3,L4,L5,L6,L7,L8)] _LMPLAY("层数", Float) = 0
		[NoScaleOffset]_Control ("rgb:layer 1 2 3 4 ", 2D) = "red" {} 
		[NoScaleOffset]_Control2 ("rg:layer 5 [_LMPLAY_L5]
		rgb:layer 5 6 [_LMPLAY_L6]
		rgb:layer 5 6 7[_LMPLAY_L7]
		rgblayer 5 6 7 8[_LMPLAY_L8]"
		, 2D) = "black" {}
		[KeywordEnum(UV0, UV1, UV2)] _CHANNEL("UV Channel", Float) = 0

         

 

 
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Use same blending / depth states as Standard shader
            //Blend[_SrcBlend][_DstBlend]
            //ZWrite[_ZWrite]
            //Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
  
            //#pragma shader_feature _  _SPECULAR_COLOR
            //#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _LMPLAY _LMPLAY_L4 _LMPLAY_L5  _LMPLAY_L6 _LMPLAY_L7 _LMPLAY_L8

            #pragma multi_compile _ _RECEIVE_SHADOWS_OFF 

  
			#pragma multi_compile _CHANNEL_UV0 _CHANNEL_UV1  _CHANNEL_UV2

            // ----------------------------- --------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ LIGHTMAP_ON

            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1

            #ifndef UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED
			#define UNIVERSAL_SIMPLE_LIT_INPUT_INCLUDED

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			CBUFFER_START(UnityPerMaterial)

			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			float4 _Splat4_ST;
			float4 _Splat5_ST;
			float4 _Splat6_ST;
			float4 _Splat7_ST;
		 
			half4 _Spe0;
			half4 _Spe1;
			half4 _Spe2;
			half4 _Spe3;
			half4 _Spe4;
			half4 _Spe5;
			half4 _Spe6;
			half4 _Spe7;
		 
	 
			CBUFFER_END

			TEXTURE2D( _Control);
			TEXTURE2D( _Control2);
			TEXTURE2D( _Splat0);
			TEXTURE2D( _Splat1);
			TEXTURE2D( _Splat2);
			TEXTURE2D( _Splat3);
			TEXTURE2D( _Splat4);
			TEXTURE2D( _Splat5);
			TEXTURE2D( _Splat6);
			TEXTURE2D( _Splat7);
			 
			SAMPLER(sampler_Control);
			SAMPLER(sampler_Control2);
			SAMPLER(sampler_Splat0);
			SAMPLER(sampler_Splat1);
			SAMPLER(sampler_Splat2);
			SAMPLER(sampler_Splat3);
			SAMPLER(sampler_Splat4);
			SAMPLER(sampler_Splat5);
			SAMPLER(sampler_Splat6);
			SAMPLER(sampler_Splat7);


			TEXTURE2D( _BumpMap0);
			TEXTURE2D( _BumpMap1);
			TEXTURE2D( _BumpMap2);
			TEXTURE2D( _BumpMap3);
			TEXTURE2D( _BumpMap4);
			TEXTURE2D( _BumpMap5);
			//TEXTURE2D( _BumpMap6);
			//TEXTURE2D( _BumpMap7);
			

 
			SAMPLER(sampler_BumpMap0);
			SAMPLER(sampler_BumpMap1);
			SAMPLER(sampler_BumpMap2);
			SAMPLER(sampler_BumpMap3);
			SAMPLER(sampler_BumpMap4);
			SAMPLER(sampler_BumpMap5);
			//SAMPLER(sampler_BumpMap6);
			//SAMPLER(sampler_BumpMap7);
			 

			half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
			{
				half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
			#ifdef _SPECGLOSSMAP
				specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
			#elif defined(_SPECULAR_COLOR)
				specularSmoothness = specColor;
			#endif

			#ifdef _GLOSSINESS_FROM_BASE_ALPHA
				specularSmoothness.a = exp2(10 * alpha + 1);
			#else
				specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
			#endif

				return specularSmoothness;
			}

			#endif

						#ifndef UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED
			#define UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct Attributes
			{
				float4 positionOS    : POSITION;
				float3 normalOS      : NORMAL;
				float4 tangentOS     : TANGENT;
				float2 texcoord      : TEXCOORD0;
				float2 lightmapUV    : TEXCOORD1;

				#if _CHANNEL_UV2
				float2 uv2 :   TEXCOORD2;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float2 uv                       : TEXCOORD0;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

				float3 posWS                    : TEXCOORD2;    // xyz: posWS

			#ifdef _NORMALMAP
				float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
				float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
				float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
			#else
				float3  normal                  : TEXCOORD3;
				float3 viewDir                  : TEXCOORD4;
			#endif

				half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord              : TEXCOORD7;
			#endif

				#if _CHANNEL_UV1
				float2 uv1 :   TEXCOORD8;
				#endif

				#if _CHANNEL_UV2
				float2 uv2 :   TEXCOORD9;
				#endif
				float4 positionCS               : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
			{
				inputData.positionWS = input.posWS;

			#ifdef _NORMALMAP
				half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
				inputData.normalWS = TransformTangentToWorld(normalTS,
					half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
			#else
				half3 viewDirWS = input.viewDir;
				inputData.normalWS = input.normal;
			#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				viewDirWS = SafeNormalize(viewDirWS);

				inputData.viewDirectionWS = viewDirWS;

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				inputData.shadowCoord = input.shadowCoord;
			#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
			#else
				inputData.shadowCoord = float4(0, 0, 0, 0);
			#endif

				inputData.fogCoord = input.fogFactorAndVertexLight.x;
				inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
			}

			///////////////////////////////////////////////////////////////////////////////
			//                  Vertex and Fragment functions                            //
			///////////////////////////////////////////////////////////////////////////////

			// Used in Standard (Simple Lighting) shader
			Varyings LitPassVertexSimple(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
				half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
				half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
				half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				output.uv = input.texcoord;
				output.posWS.xyz = vertexInput.positionWS;
				output.positionCS = vertexInput.positionCS;

			#ifdef _NORMALMAP
				output.normal = half4(normalInput.normalWS, viewDirWS.x);
				output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
				output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
			#else
				output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
				output.viewDir = viewDirWS;
			#endif

				OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
				OUTPUT_SH(output.normal.xyz, output.vertexSH);

				output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif

				#if _CHANNEL_UV1
				output.uv1= i.lightmapUV;
				//oat2 uv1 : : TEXCOORD8
				#endif

				#if _CHANNEL_UV2
				output.uv2= i.uv2;
				//oat2 uv2 : : TEXCOORD29
				#endif

				return output;
			}
			half4 UniversalFragmentBlinnPhongDemo(InputData inputData, half3 diffuse, half4 specularGloss, half smoothness, half3 emission, half alpha)
			{
				Light mainLight = GetMainLight(inputData.shadowCoord);
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

				half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
				half3 diffuseColor = inputData.bakedGI + LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
				half3 specularColor = LightingSpecular(attenuatedLightColor, mainLight.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);

			#ifdef _ADDITIONAL_LIGHTS
				uint pixelLightCount = GetAdditionalLightsCount();
				for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
				{
					Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
					half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
					diffuseColor += LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
					specularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);
				}
			#endif

			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				diffuseColor += inputData.vertexLighting;
			#endif

				half3 finalColor = diffuseColor * diffuse + emission;

			//#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
				finalColor += specularColor;
			//#endif

				return half4(finalColor, alpha);
			}
			// Used for StandardSimpleLighting shader
			half4 LitPassFragmentSimple(Varyings i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
			 
				#if _CHANNEL_UV0
				float4 splat1 = SAMPLE_TEXTURE2D (_Control, sampler_Control,i.uv);
				float4 splat2 = SAMPLE_TEXTURE2D (_Control2, sampler_Control,i.uv);
 
				#endif
				#if _CHANNEL_UV1
				float4 splat1 = SAMPLE_TEXTURE2D (_Control, sampler_Control,i.uv1);
				float4 splat2 = SAMPLE_TEXTURE2D (_Control2, sampler_Control,i.uv1);
				#endif

				#if _CHANNEL_UV2
				float4 splat1 = SAMPLE_TEXTURE2D (_Control, sampler_Control,i.uv2);
				float4 splat2 = SAMPLE_TEXTURE2D (_Control2, sampler_Control,i.uv2);
				#endif
				
		

				float4 col  = splat1.r * SAMPLE_TEXTURE2D (_Splat0,sampler_Splat0, i.uv*_Splat0_ST.xy);
				col += splat1.g * SAMPLE_TEXTURE2D (_Splat1,sampler_Splat1, i.uv*_Splat1_ST.xy);
				col += splat1.b * SAMPLE_TEXTURE2D (_Splat2,sampler_Splat2, i.uv*_Splat2_ST.xy);

				half4 specular =  splat1.r * _Spe0 + splat1.g * _Spe1 + splat1.b * _Spe2;
				#if _NORMALMAP
				half3 normalTS = splat1.r * SampleNormal(i.uv*_Splat0_ST.xy, TEXTURE2D_ARGS(_BumpMap0, sampler_BumpMap0));
				normalTS      += splat1.g * SampleNormal(i.uv*_Splat1_ST.xy, TEXTURE2D_ARGS(_BumpMap1, sampler_BumpMap1));
				normalTS      += splat1.b * SampleNormal(i.uv*_Splat2_ST.xy, TEXTURE2D_ARGS(_BumpMap2, sampler_BumpMap2));
				#else
				half3 normalTS = half3(0,0,1);
				#endif

				#if _LMPLAY_L4  |_LMPLAY_L5 | _LMPLAY_L6 | _LMPLAY_L7 | _LMPLAY_L8
				col += splat1.a * SAMPLE_TEXTURE2D (_Splat3,sampler_Splat3, i.uv*_Splat3_ST.xy);

				specular +=  splat1.a * _Spe3;
				#if _NORMALMAP
	
				normalTS      += splat1.a * SampleNormal(i.uv*_Splat3_ST.xy, TEXTURE2D_ARGS(_BumpMap3, sampler_BumpMap3));
			 
				#endif
				
				#endif
 
				#if  _LMPLAY_L5 | _LMPLAY_L6 | _LMPLAY_L7 | _LMPLAY_L8
				col += splat2.r * SAMPLE_TEXTURE2D (_Splat4,sampler_Splat4, i.uv*_Splat4_ST.xy); 
				specular +=  splat2.r * _Spe4;
				#if _NORMALMAP
	
				normalTS      += splat2.r * SampleNormal(i.uv*_Splat4_ST.xy, TEXTURE2D_ARGS(_BumpMap4, sampler_BumpMap4));
			 
				#endif

				#endif

				#if    _LMPLAY_L6 | _LMPLAY_L7 | _LMPLAY_L8
				col += splat2.g * SAMPLE_TEXTURE2D (_Splat5,sampler_Splat5, i.uv*_Splat5_ST.xy);
				specular +=  splat2.g * _Spe5;
				#if _NORMALMAP
	
				normalTS      += splat2.g * SampleNormal(i.uv*_Splat5_ST.xy, TEXTURE2D_ARGS(_BumpMap5, sampler_BumpMap5));
			 
				#endif

				#endif


				#if     _LMPLAY_L7 | _LMPLAY_L8
				col += splat2.b * SAMPLE_TEXTURE2D (_Splat6,sampler_Splat6, i.uv*_Splat6_ST.xy);
				specular +=  splat2.b * _Spe6;
				/*#if _NORMALMAP
	
				normalTS      += splat2.b * SampleNormal(i.uv*_Splat6_ST.xy, TEXTURE2D_ARGS(_BumpMap6, sampler_BumpMap6));
			 
				#endif*/

				#endif


				#if      _LMPLAY_L8
				col += splat2.a * SAMPLE_TEXTURE2D (_Splat7,sampler_Splat7, i.uv*_Splat7_ST.xy);
				specular +=  splat2.a * _Spe4;
				/*#if _NORMALMAP
	
				normalTS      += splat2.a * SampleNormal(i.uv*_Splat7_ST.xy, TEXTURE2D_ARGS(_BumpMap7, sampler_BumpMap7));
			 
				#endif*/

				#endif

			 
				half3 diffuse = col.rgb ;

 

			 
				 
		 
				
				
		 
 
			 
				//return specular;
				
				half smoothness = col.a*255 * specular.a;

				InputData inputData;
				InitializeInputData(i, normalTS, inputData);

			  
				half4 color = UniversalFragmentBlinnPhongDemo(inputData, diffuse, specular, smoothness, 0, 1);
				color.rgb = MixFog(color.rgb, inputData.fogCoord);
				return color;
			};

			#endif

            ENDHLSL
        }

         
		UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
		UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
		//UsePass "Universal Render Pipeline/Simple Lit/Meta"
        UsePass "Universal Render Pipeline/Simple Lit/Universal2D"
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor   "LCHShaderGUIT4M"
}
