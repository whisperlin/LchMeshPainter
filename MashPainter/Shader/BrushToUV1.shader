// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/BrushToUV1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color",Color) = (1.0,1.0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+2000" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Offset -1, -1
		Cull Off
        LOD 100

        Pass
        {
			
            CGPROGRAM

			#pragma multi_compile _CHANNEL_UV0 _CHANNEL_UV1 _CHANNEL_UV2
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			float4x4 worldToBrush;
            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
                #if _CHANNEL_UV0
					float2 uv : TEXCOORD0;
				#endif
				#if _CHANNEL_UV1
					float2 uv : TEXCOORD1;
				#endif
				#if _CHANNEL_UV2
					float2 uv : TEXCOORD2;

				#endif
				 
		 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 local : TEXCOORD1;
				float brushNormalZ: TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			half4 _Color;
            v2f vert (appdata v)
            {
                v2f o;
               
				half4 worldPos =   mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) ;
				//o.worldPos = worldPos;
				o.local = mul(worldToBrush,worldPos);

#if UNITY_UV_STARTS_AT_TOP

#else
				v.uv.y = 1- v.uv.y;
#endif
				float2 uv0 = v.uv;


                o.uv = uv0;

				float3 worldNormal = UnityObjectToWorldNormal(v.normal);

 
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				o.brushNormalZ = dot(worldViewDir ,worldNormal);

			 
				o.vertex.xy = uv0 * 2 - float2(1, 1);
				o.vertex.z = 0.5;
				o.vertex.w = 1;
				

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				
				clip(i.brushNormalZ);
				
				 
				float r =  length(i.local.xy);
				if(abs(i.local.x)>1 ||abs(i.local.y)>1)
				{
					return half4(0,0,0,0);
				}
				fixed4 col = tex2D(_MainTex, (i.local.xy+1)*0.5);
				return float4(col.aaa,1);
			 
            }
            ENDCG
        }
    }
}
