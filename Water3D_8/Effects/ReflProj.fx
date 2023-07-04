float4x4 matWorldViewProj;	
float4x4 matProjTex;
float4x4 matWorld;
float4 eyePos;
texture tex0;
texture tex1;
texture Noise_Tex;
half waveSpeed = 0.05f;
half noiseSpeed = 0.05f; 
half fadeBias = 0.3f; 
half fadeExp = 6.0f; 
half4 waterColor = {0.2f, 0.7f, 0.7f, 1.0f};
float4 scale = {0.1f, 0.1f, 0.05f, 0.5f};
float time;

// data input from application
struct a2v {
     float4 Pos : POSITION;
     float2 Texcoord0 : TEXCOORD0;
     float4 Normal : NORMAL;
};

// vertex shader output to pixel shader
struct v2p {
    float4 Pos : POSITION;
    float3 scaledPos : TEXCOORD0;
    //float4 outTexProj : TEXCOORD1;
    float4 V : TEXCOORD2;
    float4 Normal : TEXCOORD3;
    float4 lPos : TEXCOORD4;
};   

struct p2a {
    float4 color : COLOR0;
};

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler2D ReflectMap = 
sampler_state
{
    Texture = <tex0>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    ADDRESSW = CLAMP;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
samplerCUBE CubeMap = 
sampler_state
{
    Texture = <tex1>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
sampler3D Noise = sampler_state
{
   Texture = (Noise_Tex);
   ADDRESSU = WRAP;
   ADDRESSV = WRAP;
   ADDRESSW = WRAP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
void VS_projective(in a2v IN, out v2p OUT, uniform float4x4 matWorldViewProj, uniform float4x4 matWorld)
{   
	// transform Position
    OUT.Pos = mul(IN.Pos, matWorldViewProj); 
    OUT.scaledPos = IN.Pos.xzy * scale;


    // transform vertex position by combined texture matrix and
    // copy result into homogenous texture coordinate set 0
    //OUT.outTexProj = mul(IN.Pos, matProjTex);
    
    // create ray from camera position to the vertex in world space
    float4 V = IN.Pos - eyePos;	 
   
    // copy the result to homogenous texture coordinate set 0
    //OUT.outTexCube = reflect(V, IN.Normal);
    OUT.V = V;
    OUT.Normal = IN.Normal;
    OUT.lPos = IN.Pos;
}

// -------------------------------------------------------------
// Pixel Shader
// -------------------------------------------------------------
void PS_projective(in v2p IN, out p2a OUT, uniform sampler2D ReflectMap,  uniform samplerCUBE CubeMap, uniform sampler3D Noise,
	uniform half waveSpeed,
	uniform half noiseSpeed,
	uniform half fadeBias,
	uniform half fadeExp,
	uniform half4 waterColor,
	uniform float4x4 matProjTex) {
	
	IN.scaledPos.x += waveSpeed  * time;
    IN.scaledPos.z += noiseSpeed * time;
	
	float4 noisy = tex3D(Noise, IN.scaledPos);

	// Signed noise 
	float3 bump = 2 * noisy - 1;
	bump.xz *= 0.15;
   
	// Make sure the normal always points upwards
	bump.y = 0.8 * abs(bump.y) + 0.2;
	// Offset the surface normal with the bump
	bump = normalize(IN.Normal + bump);
	float3 outTexCube = reflect(IN.V, bump);

	// the adjustable displacement factor
    float d = 30.0;
    //temporary variable to hold the displaced vertex position
    float4 dPos;
    //displace the xz components of the vertex position
    dPos.xz = IN.lPos.xz + d * bump.xz;
    //the original y component is kept
    dPos.y = IN.lPos.y;
    // the w component is always one for a point
    dPos.w = 1.0;
	
	float4 outTexProj = mul(dPos, matProjTex);

	//projectively sample the 2D reflection texture
	float3 texcoords = outTexProj.xyz / outTexProj.w;
	float4 colorEnv = tex2D(ReflectMap, texcoords.xy);
	colorEnv = tex2D(ReflectMap, texcoords);
	float4 colorSky = texCUBE(CubeMap, outTexCube);
    
    float4 refl = lerp(colorSky, colorEnv, 0.5f);
    float lrp = 1 - dot(-normalize(IN.V), bump);
    OUT.color = lerp(waterColor, refl, saturate(fadeBias + pow(lrp, fadeExp)));
}


// -------------------------------------------------------------
// 
// -------------------------------------------------------------
technique TShader
{
    pass P0
    {
        ZENABLE = TRUE;
        CULLMODE = NONE;
        // compile shaders
        VertexShader = compile vs_2_0 VS_projective(matWorldViewProj, matWorld);
        PixelShader  = compile ps_2_0 PS_projective(ReflectMap, CubeMap, Noise, waveSpeed, noiseSpeed, fadeBias, fadeExp, waterColor, matProjTex);
    }
}