/// Lab 06 0 Reflection Per-Vertex Lighting Model
/// Not Assignment2 Per-Pixel Lighting
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;

texture decalMap;
texture environmentMap;

sampler tsampler1 = sampler_state {
	texture = <decalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyboxSampler = sampler_state
{
	texture = <environmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput {
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput {
	float4 Position : POSITION0;
	float3 Reflection : TEXCOORD0;
};

VertexShaderOutput ReflectionVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	float4 VertexPosition = mul(input.Position, World);
	float3 ViewDirection = CameraPosition - VertexPosition;

	float3 Normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.Reflection = reflect(-normalize(ViewDirection), normalize(Normal));

	return output;
}

float4 ReflectionPixelShader(VertexShaderOutput input) : COLOR0
{
	return texCUBE(SkyboxSampler, normalize(input.Reflection));

	/*float4 reflectedColor = texCUBE(SkyboxSampler, input.Reflection);
	float4 decalColor = tex2D(tsampler1, input.texCoord);
	return lerp(decalColor, reflectedColor, 0.5);*/
}

technique Reflection
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 ReflectionVertexShader();
		PixelShader = compile ps_4_0 ReflectionPixelShader();
	}
}