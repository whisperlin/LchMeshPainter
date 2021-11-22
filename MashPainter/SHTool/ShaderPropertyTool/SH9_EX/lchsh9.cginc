#ifndef ___LCH_SH9___
#define ___LCH_SH9___ 1


#define DEFINE_SH9(name)\
uniform float4 name##_SHAr;\
uniform float4 name##_SHAg;\
uniform float4 name##_SHAb;\
uniform float4 name##_SHBr;\
uniform float4 name##_SHBg;\
uniform float4 name##_SHBb;\
uniform float4 name##_SHC;


#define GetSH9(name,normal,out_color)\
half3 out_color = 0;\
half4 name##_mid_result_normal4 = half4(normal,1);\
out_color.r = dot(name##_SHAr, name##_mid_result_normal4);\
out_color.g = dot(name##_SHAg, name##_mid_result_normal4);\
out_color.b = dot(name##_SHAb, name##_mid_result_normal4);\
half3 name##_mid_result_x1, name##_mid_result_x2;\
half4 name##_mid_result_vB = name##_mid_result_normal4.xyzz * name##_mid_result_normal4.yzzx;\
name##_mid_result_x1.r = dot(name##_SHBr, name##_mid_result_vB);\
name##_mid_result_x1.g = dot(name##_SHBg, name##_mid_result_vB);\
name##_mid_result_x1.b = dot(name##_SHBb, name##_mid_result_vB);\
half name##_mid_result_vC = name##_mid_result_normal4.x*name##_mid_result_normal4.x - name##_mid_result_normal4.y*name##_mid_result_normal4.y;\
name##_mid_result_x2 = name##_SHC.rgb * name##_mid_result_vC;\
out_color += name##_mid_result_x1 + name##_mid_result_x2;





#endif