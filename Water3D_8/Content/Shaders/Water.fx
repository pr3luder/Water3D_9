float4x4 matWorldViewProj;	
float4x4 matProjTex;
float4x4 matWorld;
float4 eyePos;
texture tex0;
texture tex1;
texture tex2;
texture skycube;
texture Noise_Tex;
float waveSpeed = 0.05f;
float noiseSpeed = 0.05f; 
float fadeBias = 0.3f; 
float fadeExp = 6.0f; 
float4 waterColor = float4(0.2f, 0.7f, 0.7f, 1.0f);
float4 scale = float4(0.1f, 0.1f, 0.05f, 0.5f);
float time;
float random;

// data input from application
struct a2v {
     float4 Pos : SV_POSITION;
     float4 Normal : NORMAL;
     float2 texCoord0 : TEXCOORD0;
};

// vertex shader output to pixel shader
struct v2p {
    float4 Pos : SV_POSITION;
    float2 texCoord0 : TEXCOORD0;
    float3 scaledPos : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 lPos : TEXCOORD3;
    float4 vVec : TEXCOORD4;
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

sampler2D bumpMap = 
sampler_state
{
    Texture = <tex1>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    ADDRESSW = CLAMP;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler2D RefractMap = 
sampler_state
{
    Texture = <tex2>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    ADDRESSW = CLAMP;
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

sampler CubeTex = sampler_state
{
   Texture = (skycube);
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
v2p VS_projective(in a2v IN)
{   
	v2p OUT;
	// transform Position
    OUT.Pos = mul(IN.Pos, matWorldViewProj);
    OUT.lPos = mul(IN.Pos, matWorld);
    OUT.texCoord0 = IN.texCoord0;
    OUT.scaledPos = IN.Pos.xzy * scale;
    OUT.vVec = OUT.lPos - eyePos;
    OUT.Normal = IN.Normal;
    return OUT;
}

// -------------------------------------------------------------
// Pixel Shader
// -------------------------------------------------------------
p2a PS_projective(in v2p IN)
{
	p2a OUT;
	
	//CUBE MAPPING
	IN.scaledPos.x += waveSpeed * time;
    IN.scaledPos.z += noiseSpeed * time;
	float4 noisy = tex3D(Noise, IN.scaledPos);
	// Signed noise 
	float3 bump = 2 * noisy - 1;
	bump.xz *= 0.15;
	// Make sure the normal always points upwards
	bump.y = 0.8 * abs(bump.y) + 0.2;
	// Offset the surface normal with the bump
	bump = normalize(IN.Normal + bump);
	// the adjustable displacement factor
    float d = 30.0;
    // Find the reflection vector
    float3 reflVec = reflect(IN.vVec, bump);
	//float3 reflVec = reflect(IN.vVec, IN.Normal);
	float4 refl1 = texCUBE(CubeTex, reflVec.xyz);  // changed y,z ,yzx
    
    //PROJECTIVE MAPPING
    //temporary variable to hold the displaced vertex position
    float4 dPos;
    //displace the xz components of the vertex position
    dPos.xz = IN.lPos.xz + d * bump.xz;
    //the original y component is kept
    dPos.y = IN.lPos.y;
    // the w component is always one for a point
    dPos.w = 1.0;
	
	// Normalenvektor aus der Textur lesen
	float3 Normal = tex2D(bumpMap, IN.texCoord0);
	
	float4 outTexProj = mul(dPos, matProjTex);
	float4 refl2 = tex2Dproj(ReflectMap, outTexProj);
	
    float4 refl = lerp(refl1, refl2, 0.5);
    
    float4 refr = tex2Dproj(RefractMap, outTexProj);
	float lrp = 1.0 - max(dot(normalize(-IN.vVec), IN.Normal), 0);
    float fresnel = dot(normalize(-IN.vVec), IN.Normal);
    
    //refl = lerp(refr, refl, fresnel);
    //OUT.color = refl;
    OUT.color = lerp(waterColor, refl, saturate(fadeBias + pow(abs(lrp), fadeExp)));
    OUT.color = lerp(OUT.color, refr, fresnel);
    return OUT;
}


// -------------------------------------------------------------
// 
// -------------------------------------------------------------
technique TShader
{
    pass P0
    {
        Zenable = true;
        CullMode = None;
        //AlphaBlendEnable = true;
        //SrcBlend = SrcColor;
        //DestBlend = INVSRCCOLOR;
        
        // compile shaders
        VertexShader = compile vs_4_0_level_9_3 VS_projective();
        PixelShader = compile ps_4_0_level_9_3 PS_projective();
    }
}