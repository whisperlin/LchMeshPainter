Shader "Hidden/BrushBlend"
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
        LOD 100

        Pass
        {
			Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			float4x4 worldToBrush;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 local : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			half4 _Color;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				half4 worldPos =   mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)) ;
				o.local = mul(worldToBrush,worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.y = 1- o.uv.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
                return col;
            }
            ENDCG
        }
    }
}
