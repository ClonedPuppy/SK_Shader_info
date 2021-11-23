#include "stereokit.hlsli"

//--name = app/ambient

//--tex_scale   = 1
//--diffuse     = white
//--AmbientUp	= 0,0,0
//--AmbientDown = 0,0,0

float4 AmbientUp;
float4 AmbientDown;
float tex_scale;
Texture2D diffuse : register(t0);
SamplerState diffuse_s : register(s0);

struct vsIn
{
	float4 pos : SV_Position;
	float3 norm : NORMAL0;
	float2 uv : TEXCOORD0;
};
struct psIn
{
	float4 pos : SV_Position;
	float2 uv : TEXCOORD0;
	float3 norm : TEXCOORD1;
	uint view_id : SV_RenderTargetArrayIndex;
};

psIn vs(vsIn input, uint id : SV_InstanceID)
{
	psIn o;
	o.view_id = id % sk_view_count;
	id = id / sk_view_count;

	float4 world = mul(input.pos, sk_inst[id].world);
	o.pos = mul(world, sk_viewproj[o.view_id]);

	o.uv = input.uv * tex_scale;
	o.norm = normalize(mul(input.norm, (float3x3) sk_inst[id].world));
	
	return o;
}

// Ambient calculation helper function
float3 CalcAmbient(float3 normal, float3 color)
{
	// Convert from [-1, 1] to [0, 1]
	float up = normal.y * 0.5 + 0.5;

	// Calculate the ambient value
	float3 ambient = AmbientDown.rgb + up * AmbientUp.rgb;

	// Apply the ambient value to the color
	return ambient.rgb * color;
}

float4 ps(psIn input) : SV_TARGET
{
	 // Sample the texture and convert to linear space
	float3 diffuseColor = diffuse.Sample(diffuse_s, input.uv).rgb;

	// Calculate the ambient color
	float3 AmbientColor = CalcAmbient(input.norm, diffuseColor);

	// Return the ambient color
	return float4(AmbientColor, 1.0);
}