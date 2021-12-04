#include "stereokit.hlsli"

//--name = app/direct

//--diffuse     = white
//--AmbientUp	= 0,0,0
//--AmbientDown = 0,0,0
//--PointLightPosition = 1,1,1,0
//--LightRangeRcp = 1
//--LightColor = 1,1,1
//--DirToLight = 1,1,1,0
//--specExp = 1
//--specIntensity = 1
//--SpotLightPos = 1,1,1,0
//--SpotDirToLight = 0,1,0,0
//--SpotCosOuterCone = 1.5
//--SpotCosInnerConeRcp = 1
//--tex_scale   = 1

float4 AmbientUp;
float4 AmbientDown;
float LightRangeRcp;
float4 LightColor;
float specExp;
float specIntensity;
float4 DirToLight;
float4 PointLightPosition;
float4 SpotLightPos;
float4 SpotDirToLight;
float SpotCosOuterCone;
float SpotCosInnerConeRcp;
float tex_scale;

Texture2D diffuse : register(t0);
SamplerState diffuse_s : register(s0);

/////////////////////////////////////////////////////////////////////////////
// shader input/output structure
/////////////////////////////////////////////////////////////////////////////
struct vsIn
{
	float4 pos	: SV_POSITION;
	float3 norm : NORMAL0;
	float2 uv	: TEXCOORD0;
};
struct psIn
{
	float4 pos		: SV_POSITION;
	float2 uv		: TEXCOORD0;
	float3 normal	: TEXCOORD1;
	float3 world	: TEXCOORD2;
	float3 view_dir : TEXCOORD3;
	uint view_id	: SV_RenderTargetArrayIndex;
};

/////////////////////////////////////////////////////////////////////////////
// Vertex shader
/////////////////////////////////////////////////////////////////////////////
psIn vs(vsIn input, uint id : SV_InstanceID)
{
	psIn o;
	o.view_id = id % sk_view_count;
	id = id / sk_view_count;
	
	o.world = mul(float4(input.pos.xyz, 1), sk_inst[id].world).xyz;  // multiply by local matrix
	o.pos = mul(float4(o.world, 1), sk_viewproj[o.view_id]); // this is mvp output

	o.uv = input.uv * tex_scale;
	o.normal = mul(input.norm, (float3x3)sk_inst[id].world);
	
	o.view_dir = sk_camera_pos[o.view_id].xyz - o.world;
	
	return o;
}

/////////////////////////////////////////////////////////////////////////////
// Pixel shaders
/////////////////////////////////////////////////////////////////////////////

// Ambient light calculation helper function
float3 CalcAmbient(float3 normal, float3 color)
{
	// Convert from [-1, 1] to [0, 1]
	float up = normal.y * 0.5 + 0.5;

	// Calculate the ambient value
	float3 ambient = AmbientDown.rgb + up * AmbientUp.rgb;

	// Apply the ambient value to the color
	return ambient * color;
}

// Material preparation
struct Material
{
	float3 normal;
	float4 diffuseColor;
	float specExp;
	float specIntensity;
	float3 eyeDir;
};

Material PrepareMaterial(float3 normal, float2 UV, float3 viewDir)
{
	Material material;

	// Normalize the interpolated vertex normal
	material.normal = normalize(normal);
	
	material.eyeDir = normalize(viewDir);

	// Sample the texture and convert to linear space
	material.diffuseColor = diffuse.Sample(diffuse_s, UV);
	//material.diffuseColor.rgb *= material.diffuseColor.rgb;

	// Copy the specular values from the constant buffer
	material.specExp = specExp;
	material.specIntensity = specIntensity;

	return material;
}

// Directional light calculation helper function
float3 CalcDirectional(float3 position, Material material)
{
	// Phong diffuse
	float NDotL = dot(DirToLight.xyz, material.normal);
	float3 finalColor = LightColor.rgb * saturate(NDotL);

	// Blinn specular
	float3 ToEye = normalize(material.eyeDir).xyz - position;
	ToEye = normalize(ToEye);
	float3 HalfWay = normalize(ToEye + DirToLight.xyz);
	float NDotH = saturate(dot(HalfWay, material.normal));
	finalColor = LightColor.rgb * pow(NDotH, material.specExp) * material.specIntensity;

	// Apply the final light color to the pixels diffuse color
	return finalColor * material.diffuseColor.rgb;
}

float4 ps(psIn input) : SV_TARGET
{
	// Prepare the material structure
	Material material = PrepareMaterial(input.normal, input.uv, input.view_dir);

	// Calculate the ambient color
	float3 finalColor = CalcAmbient(material.normal, material.diffuseColor.rgb);
   
	// Calculate the directional light
	finalColor += CalcDirectional(input.world.xyz, material);

	// Return the final color
	return float4(finalColor, 1.0);
}