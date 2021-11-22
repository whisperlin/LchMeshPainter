Shader "Lit/New "
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
			#include "LchSH9.cginc"
			struct v2f
			{
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
			};

			 
			DEFINE_SH9(_New)

			 

			 
			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);

 
				GetSH9(_New, worldNormal, c);
			#   ifdef UNITY_COLORSPACE_GAMMA
				c = LinearToGammaSpace(c);
				o.diff.rgb = c.rgb;
			#   endif
 

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

 
