float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseCamera; //Inverse Camera Matrix
texture2D Texture;

bool usingTexture;

sampler ParticleSampler = sampler_state {
	texture = <Texture>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
};

struct VertexShaderInput {
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD0;
	float4 ParticlePosition: POSITION1;
	float4 ParticleParamater: POSITION2; // x: Scale x/y: Color
};

struct VertexShaderOutput {
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD0;
	float4 Color: COLOR0;
};

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
	if (usingTexture)
	{
		VertexShaderOutput output;
		float4 worldPosition = mul(input.Position, InverseCamera);
		worldPosition.xyz = worldPosition.xyz * sqrt(input.ParticleParamater.x);
		worldPosition += input.ParticlePosition;
		output.Position = mul(mul(mul(worldPosition, World), View), Projection);
		output.TexCoord = input.TexCoord;
		output.Color = 1 - input.ParticleParamater.x / input.ParticleParamater.y;
		return output;
	}

	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, InverseCamera);
	worldPosition.xyz = worldPosition.xyz * sqrt(input.ParticleParamater.x);
	worldPosition += input.ParticlePosition;
	float4 viewPos = mul(worldPosition, View);
	output.Position = mul(viewPos, Projection);

	//output.Color = saturate(float4(0.1, 0.1, 0.1, 1));
	output.Color = 1 - input.ParticleParamater.x / input.ParticleParamater.y;
	return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : COLOR
{
	if (usingTexture)
	{
		float4 color = tex2D(ParticleSampler, input.TexCoord);
		color *= input.Color;
		return color;
	}

	return float4(0.1, 0.1, 0.1, 1) * input.Color;
}

technique ParticleShader {
	pass Pass0
	{
		VertexShader = compile vs_4_0 ParticleVertexShader();
		PixelShader = compile ps_4_0 ParticlePixelShader();
	}
}