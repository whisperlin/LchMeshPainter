// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/Terrain PhoneMeta" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
	_MainTexArray("Texture Array", 2DArray) = "" {}
	 
   
}

SubShader {
    LOD 100
    Tags { "RenderType"="Opaque" }

    Pass
    {
        Name "META"
        Tags { "LightMode" = "Meta" }
        CGPROGRAM
        #pragma vertex vertMeta
        #pragma fragment fragMeta
        #pragma target 2.0
		#define _META_PASS 1
       #include "terrain_phone.cginc"
        ENDCG
    }
}

 
}

