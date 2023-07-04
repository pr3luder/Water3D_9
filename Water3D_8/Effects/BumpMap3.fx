//**************************************************************//
//  Effect File exported by RenderMonkey 1.6
//
//  - Although many improvements were made to RenderMonkey FX  
//    file export, there are still situations that may cause   
//    compilation problems once the file is exported, such as  
//    occasional naming conflicts for methods, since FX format 
//    does not support any notions of name spaces. You need to 
//    try to create workspaces in such a way as to minimize    
//    potential naming conflicts on export.                    
//    
//  - Note that to minimize resulting name collisions in the FX 
//    file, RenderMonkey will mangle names for passes, shaders  
//    and function names as necessary to reduce name conflicts. 
//**************************************************************//

//--------------------------------------------------------------//
// Ocean
//--------------------------------------------------------------//

//--------------------------------------------------------------//
// Water
//--------------------------------------------------------------//

float4x4 Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_view_proj_matrix : ViewProjection;
float4 Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_view_position : ViewPosition;
float time_0_X : Time0_X;
float4x4 matProjTex;
texture tex0;

float4 scale
<
   string UIName = "scale";
   string UIWidget = "Direction";
   bool UIVisible = true;
   float4 UIMin = ( 0.00, 0.00, 0.00, 0.00 );
   float4 UIMax = ( 0.01, 0.01, 0.01, 0.01 );
   bool Normalize = false;
> = { 0.1f, 0.1f, 0.05f, 0.5f };

struct Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_VS_OUTPUT {
   float4 Pos:    POSITION;
   float3 normal: TEXCOORD1;
   float3 pos:    TEXCOORD0;
   float3 vVec:   TEXCOORD2;
   float4 outTexProj : TEXCOORD3;
};

Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_VS_OUTPUT Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_main(float4 Pos: POSITION,  float3 normal: TEXCOORD1, float3 pos: TEXCOORD0, uniform float4 scale){
   Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_VS_OUTPUT Out;

   // Get some size on the water
   //Pos.xz *= 1000;  // changed y,z
   //Pos.y = -30;     // changed y,z

   Out.Pos = mul( Pos, Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_view_proj_matrix);
   Out.pos = Pos.xzy * scale; // changed y,z
  
   // the adjustable displacement factor
   float d = 2.0;
   //temporary variable to hold the displaced vertex position
   float4 dPos;
   //displace the xz components of the vertex porsition
   dPos.xz = Pos.xz + d * normal.xz;
   //the original y component is kept
   dPos.y = Pos.y;
   // the w component is always one for a point
   dPos.w = 1.0;
    
   // transform vertex position by combined texture matrix and
   // copy result into homogenous texture coordinate set 0
   Out.outTexProj = mul(dPos, matProjTex);	
    
   //Out.vVec = Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_view_position;
   Out.vVec = Pos - Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_view_position;
   Out.normal = normal;

   return Out;
}

float waveSpeed
<
   string UIName = "waveSpeed";
   string UIWidget = "Numeric";
   bool UIVisible = true;
   float UIMin = 0.00;
   float UIMax = 1.00;
> = 0.1f;

float noiseSpeed
<
   string UIName = "noiseSpeed";
   string UIWidget = "Numeric";
   bool UIVisible = true;
   float UIMin = 0.00;
   float UIMax = 1.00;
> = 0.1f;

float fadeBias
<
   string UIName = "fadeBias";
   string UIWidget = "Numeric";
   bool UIVisible = true;
   float UIMin = 0.00;
   float UIMax = 1.00;
> = 0.3f;

float fadeExp
<
   string UIName = "fadeExp";
   string UIWidget = "Numeric";
   bool UIVisible = true;
   float UIMin = 0.00;
   float UIMax = 8.00;
> = 6.0f;

float4 waterColor : Diffuse
<
   string UIName = "waterColor";
   string UIWidget = "ColorPicker";
   bool UIVisible = true;
> = { 0.2f, 0.7f, 0.7f, 1.0f };

texture Noise_Tex
<
   string ResourceName = "NoiseVolume.dds";
>;

sampler Noise = sampler_state
{
   Texture = (Noise_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   ADDRESSW = WRAP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};
sampler2D ReflectMap = sampler_state
{
    Texture = <tex0>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


texture skyBox_Tex
<
   string ResourceName = "test.dds";
>;

sampler Reflection_Refraction_and_Water_Effects_Ocean_Water_Pixel_Shader_skyBox = sampler_state
{
   Texture = (skyBox_Tex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

float4 Reflection_Refraction_and_Water_Effects_Ocean_Water_Pixel_Shader_main(float3 pos: TEXCOORD0, float3 normal: TEXCOORD1, float3 vVec: TEXCOORD2, float4 outTexProj: TEXCOORD3,
	uniform half waveSpeed,
	uniform half noiseSpeed,
	uniform half fadeBias,
	uniform half fadeExp,
	uniform half4 waterColor) : COLOR {
	
   pos.x += waveSpeed  * time_0_X;
   pos.z += noiseSpeed * time_0_X;  

   float4 noisy = tex3D(Noise, pos);

   // Signed noise 
   float3 bump = 2 * noisy - 1;
   bump.xz *= 0.15; // changed y,z
   
   // Make sure the normal always points upwards
   bump.y = 0.8 * abs(bump.y) + 0.2;//changed y,z  
   
   // Offset the surface normal with the bump
   bump = normalize(normal + bump);
   
   // Find the reflection vector
   float3 reflVec = reflect(vVec, bump);
   float4 refl = texCUBE(Reflection_Refraction_and_Water_Effects_Ocean_Water_Pixel_Shader_skyBox, reflVec.xyz);  // changed y,z ,yzx
   float4 colorEnv = tex2Dproj(ReflectMap, outTexProj);
   float4 refl2 = lerp(refl, colorEnv, 0.5);	
   float lrp = 1 - dot(-normalize(vVec), bump);
   //half4 color = {0.5,0.2,0.6,1.0};
   // Interpolate between the water color and reflection
   return lerp(waterColor, refl2, saturate(fadeBias + pow(lrp, fadeExp)));
   //return waterColor;
}

//--------------------------------------------------------------//
// Technique Section for Effect Workspace.Reflection Refraction and Water Effects.Ocean
//--------------------------------------------------------------//
technique TShader
{
   pass p0
   {
	  ZENABLE = TRUE;
      CULLMODE = NONE;

      VertexShader = compile vs_2_0 Reflection_Refraction_and_Water_Effects_Ocean_Water_Vertex_Shader_main(scale);
      PixelShader = compile ps_2_0 Reflection_Refraction_and_Water_Effects_Ocean_Water_Pixel_Shader_main(waveSpeed, noiseSpeed, fadeBias, fadeExp, waterColor);
   }

}