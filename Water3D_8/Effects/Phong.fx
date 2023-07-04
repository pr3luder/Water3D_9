string description = "Per-pixel Phong lighting";


/************* NON-TWEAKABLES **************/
float4x4 worldView     : WorldView                  < string UIWidget="None"; >;
float4x4 worldViewIT   : WorldViewInverseTranspose  < string UIWidget="None"; >;
float4x4 worldViewProj : WorldViewProjection        < string UIWidget="None"; >;
float4x4 view          : View                       < string UIWidget="None"; >;


/************* TWEAKABLES **************/
float4 lightPosition : Position
<
    string UIName = "Light position";
    string Object = "PointLight";
    string Space = "World";
> = {1.0f, -3.0f, 1.0f, 1.0f};

float4 lightColor : Diffuse
<
    string UIName = "Light Color";
    string Object = "PointLight";
> = { 1.0f, 1.0f, 1.0f, 1.0f };

float4 ambientLight : Ambient
<
    string UIName = "Ambient Light Color";
    string Space = "material";
> = {0.0f, 0.0f, 0.5f, 1.0f};

float4 materialDiffuse : Diffuse
<
    string UIName = "Material Color";
    string Space = "material";
> = {0.0f, 0.0f, 1.0f, 1.0f};

float4 materialSpecular : Specular
<
    string UIName = "Material Specular";
    string Space = "material";	
> = {1.0f, 1.0f, 1.0f, 1.0f};


float shininess : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Shininess";
> = 60.0;


/*********** Functions implementing the Phong lighting model ******/

float4 ambientReflection(float4 surfaceColor,
                    	 float4 lightColor) {
  return lightColor * surfaceColor;
}

float4 diffuseReflection(float4 surfaceColor,
           				 float3 surfaceNormal,
                    	 float4 lightColor,
                    	 float3 lightDirection) {
  float diffuseFactor = max(0, dot(lightDirection, surfaceNormal));
  return lightColor * surfaceColor * diffuseFactor;
}

float4 specularReflection(float4 surfaceColor,
                          float  surfaceShininess,
               		  float3 surfaceNormal,
                    	  float4 lightColor,
                    	  float3 halfAngle) {
  float specularFactor = pow(max(0, dot(halfAngle, surfaceNormal)), surfaceShininess);
  return lightColor * surfaceColor * specularFactor;       
}

float4 phongReflection(float4 ambientSurfaceColor,
                       float4 ambientLightColor,
                       float4 diffuseSurfaceColor,
                       float4 specularSurfaceColor,
                       float  surfaceShininess,
                       float3 surfaceNormal,
                       float3 halfAngle,
                       float3 lightDirection,
                       float4 lightColor) {
                       
  float4 ambient = ambientReflection(ambientSurfaceColor, ambientLightColor);

  float4 diffuse = diffuseReflection(diffuseSurfaceColor, surfaceNormal,
                                     lightColor, lightDirection);
                                     
  float4 specular = specularReflection(specularSurfaceColor, surfaceShininess, surfaceNormal,
                                       lightColor, halfAngle);

  if (dot(lightDirection, surfaceNormal) <= 0) specular = float4(0,0,0,0);
    
  return diffuse + specular + ambient;
}



/*********** Vertex shader ******/

void phongVS(float4 position   : POSITION,
               float4 normal     : NORMAL,
		     
           out float4 clipPosition : POSITION,
           out float3 eyeSurfaceNormal : TEXCOORD0,
           out float3 eyeViewerDirection : TEXCOORD1,
           out float3 eyeLightDirection : TEXCOORD2) {

  // compute standard clip position
  clipPosition = mul(position, worldViewProj);

  // compute the necessary positions and directions in eye space  
  float3 eyeSurfacePosition = mul(position, worldView).xyz;
  float3 eyeLightPosition   = mul(lightPosition, view).xyz;

  eyeViewerDirection = normalize(-eyeSurfacePosition);
  eyeSurfaceNormal   = mul(normal, worldViewIT).xyz;
  eyeLightDirection  = eyeLightPosition - eyeSurfacePosition;
}

/* Pixel shader */
void phongPS(float3 eyeSurfaceNormal : TEXCOORD0,
			 float3 eyeViewerDirection : TEXCOORD1,
			 float3 eyeLightDirection : TEXCOORD2,
			 out float4 color : COLOR) {
	float3 Nn = normalize(eyeSurfaceNormal);
	float3 Vn = normalize(eyeViewerDirection);
	float3 Ln = normalize(eyeLightDirection);
	float3 Hn = normalize(Vn + Ln);
	color = phongReflection(materialDiffuse, ambientLight,
                        	materialDiffuse, materialSpecular, shininess,
                          	Nn, Hn, Ln, lightColor);
}
technique phong {
    pass p0 {		
	VertexShader = compile vs_2_0 phongVS();
	PixelShader = compile ps_2_0 phongPS();
    }    
}
