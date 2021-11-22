Shader "Lit/Unity SH9"
{
	SubShader
	{
		// very simple lighting pass, that only does non-textured ambient
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f
			{
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				// only evaluate ambient
				o.diff.rgb = ShadeSH9(half4(worldNormal,1));
				o.diff.a = 1;
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				return i.diff;
			}
			ENDCG
		}

		 
	}
}