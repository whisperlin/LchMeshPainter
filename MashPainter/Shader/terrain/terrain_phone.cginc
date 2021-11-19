#ifndef _______________TERRAIN_PHONE__________ 
#define _______________TERRAIN_PHONE__________  1


#if   ! _META_PASS

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"

	#include "shadows.cginc"
 

#else
	#include "UnityCG.cginc"
	#include "UnityMetaPass.cginc"

#endif
			fixed4 _Color;

			half4 _UVScale0123;
			half4 _UVScale4567;
			half4 _Gloss0123;
			half4 _Gloss4567;
			half4 _SpPower0123;
			half4 _SpPower4567;
			half4 _SpeColor;

			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

			UNITY_DECLARE_TEX2DARRAY(_NormalTexArray);
			
			sampler2D _Control;
			sampler2D _Control2;
#if _NORMAL_MAP
			half4 _Normal0123;
			half4 _Normal4567;
 
#endif

		 
#if _WATER
			sampler2D _Sky;
			sampler2D _WaterNormal;
			sampler2D _Wave;
			half _SkyPower;
			half4 _ShallowColor;
			half4 _DeepColor;
			half4 _WaterParam;
			half4 _WaterParam2;
			half4 _WaterParam3;
#endif
			
			inline half2 ToRadialCoordsNetEase(half3 envRefDir)
			{

				half k = envRefDir.x / (length(envRefDir.xz) + 1E-06f);
				half2 normalY = { k, envRefDir.y };
				half2 latitude = acos(normalY) * 0.3183099f;
				half s = step(envRefDir.z, 0.0f);
				half u = s - ((s * 2.0f - 1.0f) * (latitude.x * 0.5f));
				return half2(u, latitude.y);
			}
			//这个是unity。
			inline float2 ToRadialCoords(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				return float2(0.5, 1.0) - sphereCoords;
			}

#if   ! _META_PASS

			
			 

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
			};

			struct v2f
			{
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#if _NORMAL_MAP
				half3 tspace0 : TEXCOORD2;
				half3 tspace1 : TEXCOORD3;
				half3 tspace2 : TEXCOORD4;

				#else
				half3 worldNormal : TEXCOORD2;
				#endif
				float3 posWorld : TEXCOORD5;

			

				//half4 ambientOrLightmapUV           : TEXCOORD2; // SH or Lightmap UV


				half4 ambientOrLightmapUV           : TEXCOORD6;
				SHADOW_COORDS(7)
 
			};

 
			half3 GetWorldNormal(v2f i,half3 n )
			{
#if _NORMAL_MAP
				n.xy = saturate(n.xy) * 2 - 1;
				n.xy *= saturate( n.z);
				n.z = sqrt(1 - saturate(dot(n.xy, n.xy)));
				n = normalize(n);
				half3 normal;
				normal.x = dot(i.tspace0, n);
				normal.y = dot(i.tspace1, n);
				normal.z = dot(i.tspace2, n);
				normal = normalize(normal);
				return normal;
#else
				return  normalize(i.worldNormal);
#endif
				
			}
			
			v2f vert(appdata v)
			{
				v2f o;
				o.uv = v.uv;
				float4 posWorld =  mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
				o.pos = mul(UNITY_MATRIX_VP, posWorld);
				o.posWorld = posWorld.xyz;
				half3 normal = UnityObjectToWorldNormal(v.normal);
				#if _NORMAL_MAP
				
				half3 tangent = UnityObjectToWorldDir(v.tangent.xyz);
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 bitangent = cross(normal, tangent) * tangentSign;

				o.tspace0 = half3(tangent.x, bitangent.x, normal.x);
				o.tspace1 = half3(tangent.y, bitangent.y, normal.y);
				o.tspace2 = half3(tangent.z, bitangent.z, normal.z);

				#else
				o.worldNormal =  normal;
				#endif
 
				o.ambientOrLightmapUV = VertexGIForward(v.uv1, posWorld, normal);
		 
				TRANSFER_SHADOW(o);


				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			
			fixed4 frag(v2f i) : SV_Target
			{
 
				 
				half4 ctrl = tex2D(_Control, i.uv);
				half4 ctrl2 = tex2D(_Control2, i.uv);
				 
				half4 albedo = 0;
				half3 n = 0;
#if _NORMAL_MAP
				 
#endif

#if _IF_ENABLE
				if (ctrl.r > 0)
#endif
				

				{

					half3 _auv = half3(i.uv.xy*_UVScale0123.rr,0);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.r;
 
					
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal0123.x)*ctrl.r;
					 
#endif

				}
				

#if _IF_ENABLE
				if (ctrl.g > 0)
#endif
				
				{
					half3 _auv = half3(i.uv.xy*_UVScale0123.gg, 1);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.g;
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal0123.y)*ctrl.g;
#endif
				}
				
#if _IF_ENABLE
				if (ctrl.b > 0)
#endif	
				
				{
					half3 _auv = float3(i.uv.xy*_UVScale0123.bb, 2);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.b;
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal0123.b) *ctrl.b;
#endif
				}
	 
#if _IF_ENABLE
				if (ctrl.a > 0)
#endif
				
				{
					half3 _auv = float3(i.uv.xy*_UVScale0123.aa, 3);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.a;
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal0123.a)*ctrl.a;
#endif
				}
				 
#if _IF_ENABLE
				if (ctrl2.r > 0)
#endif
				
				{
					half3 _auv = float3(i.uv.xy*_UVScale4567.x, 4);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.r;

#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv).xy, _Normal4567.r)*ctrl2.r;
#endif
				}
#if _IF_ENABLE
				if (ctrl2.g > 0)
#endif
				
				{
					
					half3 _auv = float3(i.uv.xy*_UVScale4567.y, 5);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.g;
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal4567.g)*ctrl2.g;
			 
#endif
				}
#if _IF_ENABLE
				if (ctrl2.b > 0)
#endif
				
				{
					half3 _auv = float3(i.uv.xy*_UVScale4567.z, 6);
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.b;
#if _NORMAL_MAP
					n.xyz += half3(UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal4567.b)*ctrl2.b;
#endif
				}
				half3 viewDir = normalize(UnityWorldSpaceViewDir(i.posWorld));
				half3 normal  ;

#if _WATER
				 
#endif
				
#if _IF_ENABLE
				if (ctrl2.a > 0)
#endif
				
				{
					 

#if _WATER
					half4 waterColor = lerp(_ShallowColor, _DeepColor, ctrl2.a);
#endif
					half3 _auv = float3(i.uv.xy*_UVScale4567.w, 7);

					#if _NORMAL_MAP
					
#if _WATER
					
					half2 _wc = 0;
					half _wt = 0;
					half _wn = 0;
					if (ctrl2.a < _WaterParam3.y && ctrl2.a > _WaterParam3.x)
					{
			 
						_wt = 1- ( (ctrl2.a - _WaterParam3.x) / (_WaterParam3.y - _WaterParam3.x));
						//return _wt;
						half2 _noizeUV = half2(_wt, frac(_Time.y));
						_wc.x = tex2D(_Wave, _noizeUV.yx).r;
					 
						//waterColor.a += _wt * 5;
						_wn = _wc.x * _WaterParam2.w;
					}

					half4 n1 = tex2D(_WaterNormal, (i.uv.xy +_Time.y*_WaterParam.xy+ _wn)*_WaterParam2.y);
					half4 n2 = tex2D(_WaterNormal, (i.uv.xy  +_Time.y*_WaterParam.zw+ _wn)*_WaterParam2.y);

					half2 n0 = (n1.xy + n2.xy)*0.5;
					n.xyz += half3(n0, _WaterParam2.z)*ctrl2.a;
			 

					
#else

					n.xyz += half3( UNITY_SAMPLE_TEX2DARRAY(_NormalTexArray, _auv ).xy, _Normal4567.a)*ctrl2.a;
					 
#endif
					
					//_auv.z = 11;
					


					#endif
					
					normal = GetWorldNormal(i, n);

#if _WATER

#if 1 //这个网易的，有点瑕疵，但是我贪它够简

					half3 _ref = reflect(viewDir, normal);
					half4 skyColor = tex2D(_Sky, ToRadialCoordsNetEase(_ref));
					
#else
					half3 _ref = reflect(-viewDir, normal);
					half4 skyColor = tex2D(_Sky, ToRadialCoords(_ref));
#endif
					
					 
					
					
					waterColor.rgb = lerp(waterColor.rgb, skyColor.rgb, saturate( _WaterParam2.x));
					
					albedo += waterColor * ctrl2.a;

					 
	 
					
	 

#else
					
					albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.a;
#endif
					
				

					
					
				}



#if _IF_ENABLE
				else
				{
					normal = GetWorldNormal(i, n);
				}
#endif
				

				 
				
				 
				
				
				half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 lightColor = _LightColor0;

				

	 

				

	#if defined(LIGHTMAP_ON)  
 

				half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.ambientOrLightmapUV.xy);
				half3 ambient  = DecodeLightmap(bakedColorTex);
	#else
				half3 ambient = i.ambientOrLightmapUV.rgb;

		 

	#endif 
			
				 
			
				
				half shadowMaskAttenuation = UnitySampleBakedOcclusion(i.ambientOrLightmapUV, 0);
				//return shadowMaskAttenuation;
				half realtimeShadowAttenuation = SHADOW_ATTENUATION(i);

				float zDist = dot(_WorldSpaceCameraPos - i.posWorld, UNITY_MATRIX_V[2].xyz);
				float fadeDist = UnityComputeShadowFadeDistance(i.posWorld, zDist);


				half atten = UnityMixRealtimeAndBakedShadows(realtimeShadowAttenuation, shadowMaskAttenuation, UnityComputeShadowFade(fadeDist));
			
			 

				half nl = saturate(dot(normal, lightDir));
 
				

				half3 diffuse = (ambient + lightColor * nl *atten)* saturate( albedo.rgb);
				half4 c = 1;
				c.rgb = diffuse   ;
				#if _SPEC_ENABLE
				half nv = saturate(dot(normal, viewDir));
				half  _Gloss = dot(ctrl.rgba,  _Gloss0123.rgba ) + dot(ctrl2.rgba,  _Gloss4567  );
				half  _SpPower = dot(ctrl.rgba,  _SpPower0123.rgba ) + dot(ctrl2.rgba,  _SpPower4567 );
			 
				 
				half nDotH =  saturate ( dot(normal, normalize(viewDir + lightDir)) );
				
				half3 directSpe = pow( nDotH, (_Gloss    * 256))* _SpeColor  * max(0, albedo.a)*_SpPower;
 
				c.rgb += directSpe  ;
				#endif
			
				

 

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}





#else


	  
        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
 
 
            UNITY_VERTEX_OUTPUT_STEREO
        };

        //float4 _MainTex_ST;
        //float4 _Illum_ST;

        v2f vertMeta (appdata_full v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
            o.uv = v.texcoord;
   
        #ifdef EDITOR_VISUALIZATION
            o.vizUV = 0;
            o.lightCoord = 0;
            if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
                o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
            else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
            {
                o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
            }
        #endif
            return o;
        }

 
 

        half4 fragMeta (v2f i) : SV_Target
        {
            UnityMetaInput metaIN;
            UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);

			half4 ctrl = tex2D(_Control, i.uv);
			half4 ctrl2 = tex2D(_Control2, i.uv);

			half4 albedo = 0;
 

#if _IF_ENABLE
			if (ctrl.r > 0)
#endif


			{

				half3 _auv = half3(i.uv.xy*_UVScale0123.rr, 0);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.r;

 

			}
#if _IF_ENABLE
			if (ctrl.g > 0)
#endif

			{
				half3 _auv = half3(i.uv.xy*_UVScale0123.gg, 1);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.g;
 
			}
#if _IF_ENABLE
			if (ctrl.b > 0)
#endif	

			{
				half3 _auv = float3(i.uv.xy*_UVScale0123.bb, 2);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.b;
 
			}
#if _IF_ENABLE
			if (ctrl.a > 0)
#endif

			{
				half3 _auv = float3(i.uv.xy*_UVScale0123.aa, 3);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl.a;
 
			}

#if _IF_ENABLE
			if (ctrl2.r > 0)
#endif

			{
				half3 _auv = float3(i.uv.xy*_UVScale4567.x, 4);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.r;
 
			}
#if _IF_ENABLE
			if (ctrl2.g > 0)
#endif

			{

				half3 _auv = float3(i.uv.xy*_UVScale4567.y, 5);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.g;
 
			}
#if _IF_ENABLE
			if (ctrl2.b > 0)
#endif

			{
				half3 _auv = float3(i.uv.xy*_UVScale4567.z, 6);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.b;
 
			}
#if _IF_ENABLE
			if (ctrl2.a > 0)
#endif

			{
				half3 _auv = float3(i.uv.xy*_UVScale4567.w, 7);
				albedo += UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, _auv)*ctrl2.a;
 
			}
            fixed4 c = albedo * _Color;
            metaIN.Albedo = c.rgb;


            return UnityMetaFragment(metaIN);
        }

#endif

#endif
