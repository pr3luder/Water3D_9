float4x4 matWorldViewProj;	
float4x4 matWorld;
float3 xLightDirection;
float xAmbient;


// data input from application
struct a2v {
     float4 Pos : POSITION0;
     float3 Normal : NORMAL0;
     float4 color : COLOR0;
};

// vertex shader output to pixel shader
struct v2p {
    float4 Pos : POSITION0;
    float4 color : COLOR0;
    float LightingFactor : TEXCOORD0;
};   

struct p2a {
    float4 color : COLOR0;
};


v2p VS_multitext(in a2v IN, uniform float4x4 matWorldViewProj, uniform float4x4 matWorld, uniform float xLightDirection)
{   
	//, uniform float3 xLightDirection, uniform float xAmbient
	v2p OUT;
	// transform Position
    OUT.Pos = mul(IN.Pos, matWorldViewProj);
    OUT.color = IN.color;
    xLightDirection = normalize(xLightDirection);
    float3 Normal = normalize(mul(IN.Normal, matWorld));	
	OUT.LightingFactor = 1;
	OUT.LightingFactor = saturate(dot(Normal, -xLightDirection));
    return OUT;
}

p2a PS_multitext(in v2p IN) 
{
	p2a OUT = (p2a)0;
	OUT.color = IN.color;
	OUT.color.a = 1.0;
	OUT.color.rgb *= saturate(IN.LightingFactor + xAmbient);
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
        //AlphaBlendEnable = false;
        //SrcBlend = SrcColor;
        //DestBlend = INVSRCCOLOR;
        
        // compile shaders
        VertexShader = compile vs_4_0_level_9_1 VS_multitext();
        PixelShader = compile ps_4_0_level_9_1 PS_multitext();
    }
}
