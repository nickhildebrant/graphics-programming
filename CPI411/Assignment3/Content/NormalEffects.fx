float4x4 World;
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

int MipMap;

float3 CameraPosition;

float3 UVScale;

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

sampler NormalMapSamplerNormalLinear = sampler_state {
	texture = <NormalMap>;
	Minfilter = LINEAR;
	Magfilter = LINEAR;
	Mipfilter = None;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyboxSampler = sampler_state {
	texture = <SkyboxTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexInput {
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent : TANGENT0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexOutput {
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT0;
	float3 WorldPosition : POSITION1;
	float2 TexCoord : TEXCOORD0;
};

VertexOutput VertexShaderFunction(VertexInput input)
{
	VertexOutput output;

	float4 worldpos = mul(input.Position, World);
	output.Position = mul(mul(worldpos, View), Projection);
	output.WorldPosition = worldpos.xyz;
	output.Normal = normalize(mul(float4(input.Normal.xyz, 0), World).xyz);
	output.Tangent = normalize(mul(float4(input.Tangent.xyz, 0), World).xyz);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 NormalMapPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	return float4(normalTex, 1.0);
}

float4 WorldSpacePixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb;}

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	return float4((worldNormal / 2.0) + 0.5, 1.0);
}

float4 TangentSpacePixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	float3 lightDistance = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(lightDistance, lightDistance);

	float3 L = normalize(lightDistance);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	color += AmbientIntensity * AmbientColor;
	color += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	color += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(color);
}

float4 ReflectionPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = reflect(-V, worldNormal);
	float3 color = texCUBE(SkyboxSampler, R).rgb;

	return float4(color, 1.0);
}

float4 RefractionPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));

	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 R = refract(-V, worldNormal, EtaRatio);
	float3 color = texCUBE(SkyboxSampler, R).rgb;

	return float4(color, 1.0);
}

float4 UTangentNormalizedPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = mul(normalize(normalTex), TBN);

	float4 color = float4(0.0, 0.0, 0.0, 1.0);

	float3 lightDistance = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(lightDistance, lightDistance);

	float3 L = normalize(lightDistance);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	color += AmbientIntensity * AmbientColor;
	color += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	color += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(color);
}

float4 UTangentUnnormalPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = input.Normal;
	float3 T = input.Tangent;
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = mul(normalTex, TBN);
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	float3 lightDistance = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(lightDistance, lightDistance);

	float3 L = normalize(lightDistance);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	color += AmbientIntensity * AmbientColor;
	color += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	color += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(color);
}

float4 NormalizedTangentUnnormalPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalTex, TBN));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	float3 lightDistance = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(lightDistance, lightDistance);

	float3 L = normalize(lightDistance);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	color += AmbientIntensity * AmbientColor;
	color += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	color += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(color);
}

float4 NormalizedTangentNormalizedPixelShader(VertexOutput input) : COLOR
{
	float3 normalTex;
	if (MipMap) { normalTex = tex2D(NormalMapSamplerNone, input.TexCoord * UVScale.xy).rgb; }
	else { normalTex = tex2D(NormalMapSamplerNormalLinear, input.TexCoord * UVScale.xy).rgb; }

	normalTex = lerp(float3(0.5, 0.5, 1), normalTex, UVScale.z);
	normalTex = (normalTex - 0.5) * 2.0;

	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(cross(N, T));
	float3x3 TBN = float3x3(T, B, N);

	float3 worldNormal = normalize(mul(normalize(normalTex), TBN));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	float3 lightDistance = LightPosition - input.WorldPosition;
	float oodist2 = 1.0 / dot(lightDistance, lightDistance);

	float3 L = normalize(lightDistance);
	float3 V = normalize(CameraPosition - input.WorldPosition);
	float3 H = normalize(L + V);

	color += AmbientIntensity * AmbientColor;
	color += DiffuseIntensity * DiffuseColor * saturate(dot(L, worldNormal)) * oodist2 * LightStrength * float4(SpecularColor, 1.0);
	color += SpecularIntensity * pow(saturate(dot(H, worldNormal)), 4 * Shininess) * oodist2 * LightStrength * float4(SpecularColor, 1.0);

	return saturate(color);
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

float4 FullscreenPS(FullscreenVertexOutput input) : COLOR
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
		PixelShader = compile ps_4_0 NormalMapPixelShader();
	}
};

technique WorldNormalShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 WorldSpacePixelShader();
	}
};

technique TangentSpaceShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 TangentSpacePixelShader();
	}
};

technique ReflectionShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 ReflectionPixelShader();
	}
};

technique RefractionShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 RefractionPixelShader();
	}
};

technique UnnormalTangentNormalized
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 UTangentNormalizedPixelShader();
	}
};

technique UnnormalTangentUnnormal
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 UTangentUnnormalPixelShader();
	}
};

technique NormalizeTangentUnnormal
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 NormalizedTangentUnnormalPixelShader();
	}
};

technique NormalizeTangentNormalized
{
	pass pass0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 NormalizedTangentNormalizedPixelShader();
	}
};

technique ImageShader
{
	pass pass0
	{
		VertexShader = compile vs_4_0 FullscreenVS();
		PixelShader = compile ps_4_0 FullscreenPS();
	}
};