﻿float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 LightPosition;
float LightStrength;
float3 SpecularColor;
float4 DiffuseColor;
float4 AmbientColor;
float AmbientIntensity;
float DiffuseIntensity;
float SpecularIntensity;
float Shininess;
float EtaRatio;

float NormalMapRepeatU;
float NormalMapRepeatV;
int SelfShadow;
float BumpHeight;
int NormalizeTangentFrame;
int NormalizeNormalMap;

int MipMap;

float3 CameraPosition;

float3 UvwScale;

texture NormalMap;
texture SkyboxTexture;

sampler NormalMapSamplerNone = sampler_state
{
	texture = <NormalMap>;
	MinFilter = none;
	MagFilter = none;
	MipFilter = none;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler NormalMapSamplerNormalLinear = sampler_state
{
	texture = <NormalMap>;
	Minfilter = LINEAR;
	Magfilter = LINEAR;
	Mipfilter = None;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyboxSampler = sampler_state
{
	texture = <SkyboxTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct StandardVertexInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent : TANGENT0;
	float2 TexCoord : TEXCOORD0;
};

struct StandardVertexOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
	float3 WorldPosition : POSITION1;
	float2 TexCoord : TEXCOORD0;
};

StandardVertexOutput VertexShaderFunction(in StandardVertexInput input)
{
	StandardVertexOutput output;
	float4 worldpos = mul(input.Position, World);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(float4(input.Normal.xyz, 0), World).xyz);
	output.Tangent = normalize(mul(float4(input.Tangent.xyz, 0), World).xyz);
	output.TexCoord = input.TexCoord;
	return output;
}

float4 NormalPS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	return float4(normalTex, 1.0);
}

float4 WorldNormalPS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	return float4((worldNormal / 2.0) + 0.5, 1.0);
}

float4 BlinnMappedStandardPS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	float4 col = float4(0.0, 0.0, 0.0, 1.0);
	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(col);
}

float4 ReflectPS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = reflect(-V, worldNormal);
	float3 col = texCUBE(SkyboxSampler, R).rgb;

	return float4(col, 1.0);
}

float4 RefractPS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = refract(-V, worldNormal, EtaRatio);
	float3 col = texCUBE(SkyboxSampler, R).rgb;

	return float4(col, 1.0);
}

float4 BlinnMappedNotNormalizeTangentFramePS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = mul(normalize(normalTex), TBN);

	float4 col = float4(0.0, 0.0, 0.0, 1.0);

	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(col);
}

float4 BlinnMappedNotNormalizeTangentFrameNoNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);

	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = mul(normalTex, TBN);
	float4 col = float4(0.0, 0.0, 0.0, 1.0);
	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(col);
}

float4 BlinnMappedNormalizeTangentNoNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));
	float4 col = float4(0.0, 0.0, 0.0, 1.0);
	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(col);
}

float4 BlinnMappedNormalizeTangentNormalizeSamplePS(in StandardVertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap)
	{
		normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UvwScale.xy).rgb;
	}
	else
	{
		normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UvwScale.xy).rgb;
	}
	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UvwScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	float4 col = float4(0.0, 0.0, 0.0, 1.0);
	float3 LD = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(LD, LD);

	float3 L = normalize(LD);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	col += AmbientIntensity * AmbientColor;
	col += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	col += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(col);
}

struct FullscreenVertexOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD;
};

FullscreenVertexOutput FullscreenVS(uint id: SV_VertexID)
{
	FullscreenVertexOutput output;

	output.TexCoord = float2((id << 1) & 2, id & 2);
	output.Position = float4(output.TexCoord * float2(2, -2) + float2(-1, 1), 0, 1);

	return output;
}

float4 FullscreenPS(in FullscreenVertexOutput input) : COLOR
{
	float3 normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord).rgb;
	return float4(normalTex, 1.0);
}

// * * * * * Techniques Section * * * * * //
technique NormalMapShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 NormalPS();
	}
};

technique WorldNormalShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 WorldNormalPS();
	}
};

technique TangentSpaceShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 BlinnMappedStandardPS();
	}
};

technique ReflectionShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 ReflectPS();
	}
};

technique RefractionShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 RefractPS();
	}
};

technique BlinnMappedNotNormalizeTangentFrame
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 BlinnMappedNotNormalizeTangentFramePS();
	}
};

technique BlinnMappedNotNormalizeTangentFrameNoNormalizeSamplePS
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 BlinnMappedNotNormalizeTangentFrameNoNormalizeSamplePS();
	}
};

technique BlinnMappedNormalizeTangentNoNormalizeSample
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 BlinnMappedNormalizeTangentNoNormalizeSamplePS();
	}
};

technique BlinnMappedNormalizeTangentNoNormalizeSample
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 BlinnMappedNormalizeTangentNormalizeSamplePS();
	}
};

technique Fullscreen
{
	pass P0
	{
		VertexShader = compile vs_4_0 FullscreenVS();
		PixelShader = compile ps_4_0 FullscreenPS();
	}
};