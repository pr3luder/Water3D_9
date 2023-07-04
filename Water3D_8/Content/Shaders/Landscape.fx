float4x4 matWorldViewProj;	
float4x4 matWorld;
texture baseTex;
texture detailTex;
texture lightTex;
float3 xLightDirection;
float xAmbient;


// data input from application
struct a2v {
     float4 Pos : POSITION;
     float3 Normal : NORMAL;
     float2 TexCoord0 : TEXCOORD0;
     float2 TexCoord1 : TEXCOORD1;
};

// vertex shader output to pixel shader
struct v2p {
    float4 Pos : POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord0 : TEXCOORD1;
    float2 TexCoord1 : TEXCOORD2;
    float LightingFactor : TEXCOORD3;
};   

struct p2a {
    float4 Color : COLOR;
};

sampler2D Base = sampler_state
{
   Texture = (baseTex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   ADDRESSW = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

sampler2D Detail = sampler_state
{
   Texture = (detailTex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   ADDRESSW = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

sampler2D Light = sampler_state
{
   Texture = (lightTex);
   ADDRESSU = CLAMP;
   ADDRESSV = CLAMP;
   ADDRESSW = CLAMP;
   MAGFILTER = LINEAR;
   MINFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

v2p VS_multitext(in a2v IN, uniform float4x4 matWorldViewProj, uniform float4x4 matWorld, uniform float3 xLightDirection)
{   
	v2p OUT;
	// transform Position
    OUT.Pos = mul(IN.Pos, matWorldViewProj);
    OUT.Normal = IN.Normal;
    OUT.TexCoord0 = IN.TexCoord0;
    OUT.TexCoord1 = IN.TexCoord1;
    xLightDirection = normalize(xLightDirection);
    float3 Normal = normalize(mul(IN.Normal, matWorld));	
	OUT.LightingFactor = 1;
	OUT.LightingFactor = saturate(dot(Normal, -xLightDirection));
    return OUT;
}

p2a PS_multitext(v2p IN, uniform sampler2D baseTex, uniform sampler2D lightTex)
{
	p2a OUT = (p2a)0;
	float4 color1 = tex2D(Base, IN.TexCoord0);
	float4 color2 = tex2D(Light, IN.TexCoord0);
	float4 color3 = tex2D(Detail, IN.TexCoord1);
	float4 color4 = lerp(color1, color2, 0.5);
	OUT.Color = lerp(color4, color3, 0.5);
	//OUT.Color.rgb *= saturate(IN.LightingFactor + xAmbient);
    return OUT;
}


// -------------------------------------------------------------
// 
// -------------------------------------------------------------
technique TShader2
{
    pass P0
    {
        Zenable = true;
        CullMode = None;
        AlphaBlendEnable = false;
        //SrcBlend = SrcColor;
        //DestBlend = INVSRCCOLOR;
        
        // compile shaders
        VertexShader = compile vs_4_1 VS_multitext();
        PixelShader = compile ps_4_1 PS_multitext();
    }
}
