Shader "Hidden/Bushing"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
		_BrushIndexInMainTex("_BrushIndexInMainTex",Int) = 0
		_OperaingTex ("_OperaingTex", 2D) = "white" {}
		_BrushIndexInOperaTex("_BrushIndexInOperaTex",Int) = 0
		_BrushTex ("_BrushTex", 2D) = "white" {}
		_BrushStrong("_BrushStrong",range(0,5)) = 1
		_BrushMaxStrong("_BrushMaxStrong",range(0,1)) = 1
		

    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

			#pragma multi_compile _CHANNEL_UV0 _CHANNEL_UV1 _CHANNEL_UV2
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				#if _CHANNEL_UV1
					float2 uv : TEXCOORD2;
				#elif  _CHANNEL_UV2
					float2 uv : TEXCOORD1;
				#else
					float2 uv : TEXCOORD0;
				#endif
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _OperaingTex;
			sampler2D _BrushTex;
			half _BrushIndexInMainTex;
			half _BrushIndexInOperaTex;
			half _BrushStrong;
			half _BrushMaxStrong;
	 
            /*fixed4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
				half4 operaCol = tex2D(_OperaingTex, i.uv);
			
				
				half2 uvb = half2(i.uv.x,1-i.uv.y);
				half4 brush = tex2D(_BrushTex, uvb);
				half addValue = brush.r * _BrushStrong;
				// return brush;
				if(addValue == 0)
				{
					return col;
				}
				
				half srcOldVale = operaCol[_BrushIndexInOperaTex];
				half srcNewVale = saturate(  srcOldVale + addValue  );
				half sub = 1;
				if(srcNewVale < 1 )
				{
					sub =   (1-srcNewVale) /(1-srcOldVale);
				}

				for(int j = 0 ; j < 4 ; j++ )
				{
					if(j ==   _BrushIndexInMainTex)
					{
						col[j] = saturate(  col[j] + addValue  );
					}
					else
					{
						col[j] = saturate(  col[j] *sub  );
					}
				}
                return col;
            }*/


			fixed4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
				half4 operaCol = tex2D(_OperaingTex, i.uv);
			
				
				half2 uvb = half2(i.uv.x,1-i.uv.y);
				half4 brush = tex2D(_BrushTex, uvb);
				half addValue = brush.r * _BrushStrong;
 
				if(addValue == 0)
				{
					return col;
				}
				
				half srcOldVale = operaCol[_BrushIndexInOperaTex];
				half srcNewVale = saturate(  srcOldVale + addValue  );
				srcNewVale = min(_BrushMaxStrong, srcNewVale);
				if (srcNewVale < srcOldVale)
					return col;
				 
				half sub = 1;
				if(srcNewVale < 1 )
				{
					sub =   (srcNewVale-srcOldVale) /(1-srcOldVale);
				}
				half4 colorBrush = half4(0,0,0,0);
				for(int j = 0 ; j < 4 ; j++ )
				{
					if(j == _BrushIndexInMainTex)
					{
						colorBrush[j] = 1;
					}
				}
				col = lerp(col,colorBrush,sub);
                return col;
            }

			 
            ENDCG
        }
    }
}
