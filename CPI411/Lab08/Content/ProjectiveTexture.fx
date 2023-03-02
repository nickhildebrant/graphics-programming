﻿float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;

float3 CameraPosition;
float3 LightPosition;

float AmbientColor;

texture ProjectiveTexture;

sampler TextureSampler = sampler_state
{
	Texture = < ProjectiveTexture >;
	MinFilter = none;
	MagFilter = none;
	MipFilter = none;
	AddressU = border;
	AddressV = border;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal	: TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

VertexShaderOutput VSFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPosition = mul(worldPos, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPos.xyz;
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 PSFunction(VertexShaderOutput input) : COLOR0
{
	float4 projTexCoord = ? ? ? ; // Remember how to get the world position, Step 1
	projTexCoord = projTexCoord / projTexCoord.w;	// Step 2
	projTexCoord.xy = ? ? ? ;						// Step 3
	projTexCoord.y = 1.0 - projTexCoord.y;			// Step 4
	float depth = 1.0 - projTexCoord.z;				// Step 5
	float4 color = ? ? ? ;							// Step 6

	// Step 7
	if (color.x == 0 && color.y == 1 && color.z == 1)
		color.xyz = float3(0, 0, 0);

	// Step 8 - optional
	float3 N = normalize(input.Normal);
	float3 L = normalize(LightPosition - input.WorldPosition);
	if (dot(L, N) < 0) color = 0;

	return float4(AmbientColor, AmbientColor, AmbientColor, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VSFunction();
		PixelShader = compile ps_4_0 PSFunction();
	}
}
