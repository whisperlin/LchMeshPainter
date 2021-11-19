Shader "Lch/Terrain Phone(Shadow Mask)"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTexArray("图层图集", 2DArray) = "" {}
		_UVScale0123("一二三四层缩放",Vector) = (20,20,20,20)
		_UVScale4567("五六七八层缩放",Vector) = (20,20,20,20)

		[Toggle(_NORMAL_MAP)]_NORMAL_MAP("法线",Int) = 1
		_NormalTexArray("[_NORMAL_MAP]法线图集", 2DArray) = "" {}

		[LCHVector]_Normal0123("[_NORMAL_MAP]法线强度1(0,1)  法线强度2(0.0,1)  法线强度3(0,1)  法线强度4(0,1)",Vector) = (0.5,0.5,0.5,0.5)
		[LCHVector]_Normal4567("[_NORMAL_MAP]法线强度5(0,1)  法线强度6(0.0,1)  法线强度7(0,1)  法线强度8(0,1)",Vector) = (0.5,0.5,0.5,0.5)

		_Control("_Control", 2D) = "red" {}
		_Control2("_Control2", 2D) = "black" {}

 
		[Toggle(_SPEC_ENABLE)]_SPEC_ENABLE("高光",Int) = 1
 
		[LCHVector]_Gloss0123("[_SPEC_ENABLE]粗糙1(0.005,1)  粗糙2(0.005,1)  粗糙3(0.005,1)  粗糙4(0.005,1)",Vector) = (0.5,0.5,0.5,0.5)
		[LCHVector]_Gloss4567("[_SPEC_ENABLE]粗糙5(0.005,1)  粗糙6(0.005,1)  粗糙7(0.005,1)  粗糙8(0.005,1)",Vector) = (0.5,0.5,0.5,0.5)

	 
		[HDR]_SpeColor("高光颜色[_SPEC_ENABLE]",Color) = (1,1,1,1)
		[LCHVector]_SpPower0123("[_SPEC_ENABLE]高光强度1(0.005,1)  高光强度2(0.005,1)  高光强度3(0.005,1)  高光强度4(0.005,1)",Vector) = (1,1,1,1)
		[LCHVector]_SpPower4567("[_SPEC_ENABLE]高光强度5(0.005,1)  高光强度6(0.005,1)  高光强度7(0.005,1)  高光强度8(0.005,1)",Vector) = (1,1,1,1)

		[Toggle(_WATER)]_WATER("水",Int) = 1
		_Sky("[_WATER]天空鱼眼图", 2D) = "black" {}
		_WaterNormal("[_WATER]法线图",2D ) = "gray" {}
		_Wave("[_WATER]水波动画",2D) = "black" {}
		 
		[LCHVector]_WaterParam("[_WATER]水移动速度U1(-0.1,0.1)  水移动速度V1(-0.1,0.1)  水移动速度U2(-0.1,0.1)  水移动速度V2(-0.1,0.1)",Vector) = (0.03,0.02,-0.01,-0.009)
		[LCHVector]_WaterParam2("[_WATER]天空强度(0,1)  水法线缩放(0,20)  法线强度(0,1)  波浪强度(0,1)",Vector) = (0.5,10,1,0.2)
		[LCHVector]_WaterParam3("[_WATER]<minmax>海岸线开始结束距离(0.02,0.4) ",Vector) = (0.05,0.1,1,0.2)
		_ShallowColor("[_WATER]浅水色(a,高光)",Color) = (0.06808472,0.7811196,0.9622642,0)
		_DeepColor ("[_WATER]深水色(a,高光)",Color) = (0.06808472,0.7811196,0.9622642,1)

	}

	SubShader
	{
		Pass
		{
			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define FORWARD_BASE_PASS
 
 

			#pragma multi_compile  LIGHTPROBE_SH
			#pragma multi_compile  DIRECTIONAL

			/*#pragma multi_compile  LIGHTPROBE_SH
			#pragma multi_compile  DIRECTIONAL*/
			
			#pragma multi_compile  _ LIGHTMAP_SHADOW_MIXING
			//#define LIGHTMAP_SHADOW_MIXING 1
			//#pragma multi_compile LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile  _ LIGHTMAP_OFF LIGHTMAP_ON   
			#pragma multi_compile  _ SHADOWS_SCREEN 
			#pragma multi_compile  _ SHADOWS_SHADOWMASK
			#pragma multi_compile   _NORMAL_MAP _ 
			#pragma multi_compile   _SPEC_ENABLE _ 
			#pragma multi_compile   _WATER _ 

			#define _IF_ENABLE 1
			//#pragma multi_compile   _IF_ENABLE _ 

			#include "terrain_phone.cginc"
			ENDCG
		}

		
		UsePass "Mobile/VertexLit/ShadowCaster"
		UsePass "Hidden/Terrain PhoneMeta/Meta"
 
	
		
		 
	
	}
			FallBack "Legacy Shaders/Transparent/Diffuse"
	 CustomEditor "LCHShaderGUIBase" 
}
