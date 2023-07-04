texture tex;

sampler2D Texture = sampler_state
{
    Texture = <tex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};
float4x4 worldViewProj; 

struct a2v {
	float4 Position : POSITION;   // in object space
	float3 Normal   : NORMAL;
	float2 TexCoord : TEXCOORD0;
};

struct v2f {
	float4 Position  : POSITION;  // in clip space
	float2 TexCoord  : TEXCOORD0;
};

/*********** Vertex shader ******/

v2f MyFirstVS(a2v IN) 
{
	v2f OUT;
	OUT.Position = mul(IN.Position,worldViewProj);
	OUT.TexCoord = IN.TexCoord;
	return OUT;
}

/*********** Pixel shader ******/

float4 MyFirstPS(v2f IN) : COLOR 
{
	float4 color = tex2D(Texture, IN.TexCoord);
	return color;
}

technique TShader {
   pass p0 {		
      VertexShader = compile vs_2_0 MyFirstVS();    
      PixelShader  = compile ps_2_0 MyFirstPS();
    }    
}
