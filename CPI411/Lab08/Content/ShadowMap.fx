float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;

float3 CameraPosition;
float3 LightPosition;

float AmbientColor = 0.8f;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position2D : TEXCOORD2;
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
	float4 projTexCoord = input.Position2D / input.Position2D.w;	// Remember how to get the world position, Step 1
	projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5);		// Step 3 [-1,1] -> [0,1]
	projTexCoord.y = 1.0 - projTexCoord.y;							// Step 4
	float depth = 1.0 - projTexCoord.z;								// Step 5, invert z
	float4 color = (depth > 0) ? depth : 0;							// Step 6, culling
	color.a = 1;

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
