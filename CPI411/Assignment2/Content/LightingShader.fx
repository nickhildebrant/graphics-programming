float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 DiffuseLightDirection;

float4 AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;

float EtaRatio = 0.68f;
float Reflectivity;
float Shininess;
float4 SpecularColor;
float SpecularIntensity = 1;

Texture decalMap;
Texture environmentMap;

float3 FresnelEtaRatio;
float FresnelPower;
float FresnelScale;
float FresnelBias;

sampler tsampler1 = sampler_state
{
	texture = <decalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyBoxSampler = sampler_state
{
	texture = <environmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
	float4 Position: SV_Position0;
	float4 normal: NORMAL0;
	float2 TextureCoordinate: TEXCOORD0;
};


struct VertexShaderOutput
{
	float4 Position: POSITION0;
	float2 TextureCoordinate: TEXCOORD0;
	float3 Reflection: TEXCOORD1;
};

struct FresnelVertexShaderOutput
{
	float4 Position: POSITION0;
	float reflectionFactor : COLOR;
	float3 Reflection: TEXCOORD0;
	float3 Red: TEXCOORD1;
	float3 Green: TEXCOORD2;
	float3 Blue: TEXCOORD3;
};

VertexShaderOutput ReflectionVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projectionPosition = mul(viewPosition, Projection);
	output.Position = projectionPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.Reflection = reflect(I, N);
	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 ReflectionPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.Reflection);
	float4 decalColor = tex2D(tsampler1, input.TextureCoordinate);
	return lerp(decalColor, reflectedColor, Reflectivity);
}

VertexShaderOutput RefractionVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projectionPosition = mul(viewPosition, Projection);
	output.Position = projectionPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.Reflection = refract(I, N, EtaRatio);
	output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 RefractionPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 refractedColor = texCUBE(SkyBoxSampler, input.Reflection);
	float4 decalColor = tex2D(tsampler1, input.TextureCoordinate);
	return lerp(decalColor, refractedColor, Reflectivity);
}

FresnelVertexShaderOutput DispersionVertexShader(VertexShaderInput input)
{
	FresnelVertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projectionPosition = mul(viewPosition, Projection);
	output.Position = projectionPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);

	output.Reflection = 0;
	output.Red = refract(I, N, FresnelEtaRatio.x);
	output.Green = refract(I, N, FresnelEtaRatio.y);
	output.Blue = refract(I, N, FresnelEtaRatio.z);
	output.reflectionFactor = Reflectivity;

	return output;
}

float4 DispersionPixelShader(FresnelVertexShaderOutput input) : COLOR0
{
	float4 refractedColor;
	refractedColor.r = texCUBE(SkyBoxSampler, input.Red).r;
	refractedColor.g = texCUBE(SkyBoxSampler, input.Green).g;
	refractedColor.b = texCUBE(SkyBoxSampler, input.Blue).b;
	refractedColor.a = 1;

	return refractedColor;
}

FresnelVertexShaderOutput FresnelVertexShader(VertexShaderInput input)
{
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projectionPosition = mul(viewPosition, Projection);

	FresnelVertexShaderOutput output;
	output.Position = projectionPosition;

	float3 N = normalize(mul(input.normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.Reflection = reflect(I, N);

	output.Red = refract(I, N, FresnelEtaRatio.x);
	output.Green = refract(I, N, FresnelEtaRatio.y);
	output.Blue = refract(I, N, FresnelEtaRatio.z);
	output.reflectionFactor = FresnelBias + FresnelScale * pow(1 + dot(I, N), FresnelPower);

	return output;
}

float4 FresnelPixelShader(FresnelVertexShaderOutput input) : COLOR0
{
	float4 refractedColor;
	refractedColor.r = texCUBE(SkyBoxSampler, input.Red).r;
	refractedColor.g = texCUBE(SkyBoxSampler, input.Green).g;
	refractedColor.b = texCUBE(SkyBoxSampler, input.Blue).b;
	refractedColor.a = 1;

	float4 reflectedColor = texCUBE(SkyBoxSampler, input.Reflection);

	return lerp(refractedColor, reflectedColor, input.reflectionFactor);
}

technique Reflection
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 ReflectionVertexShader();
		PixelShader = compile ps_4_0 ReflectionPixelShader();
	}
}

technique Refraction
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 RefractionVertexShader();
		PixelShader = compile ps_4_0 RefractionPixelShader();
	}
}

technique Dispersion
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 DispersionVertexShader();
		PixelShader = compile ps_4_0 DispersionPixelShader();
	}
}

technique Fresnel
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 FresnelVertexShader();
		PixelShader = compile ps_4_0 FresnelPixelShader();
	}
}