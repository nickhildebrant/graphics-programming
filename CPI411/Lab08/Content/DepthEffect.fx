float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;

float3 CameraPosition;
float3 LightPosition;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position2D: TEXCOORD0;
};

VertexShaderOutput VSFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(mul(input.Position, LightViewMatrix), LightProjectionMatrix);

	output.Position2D = output.Position;
	return output;
}

float4 PSFunction(VertexShaderOutput input) : COLOR0
{
	float4 projTexCoord = mul(mul(float4(input.Position2D.xyz, 1.0), LightViewMatrix), LightProjectionMatrix);
	projTexCoord = projTexCoord / projTexCoord.w;
	projTexCoord.y *= -1.0;
	projTexCoord.xy = projTexCoord.xy * 0.5 + 0.5;
	float depth = 1.0 - projTexCoord.z;
	float4 color = (depth > 0) ? depth : 0;
	return color;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VSFunction();
		PixelShader = compile ps_4_0 PSFunction();
	}
}