float4x4  World;
float4x4  View;
float4x4  Projection;
float4x4  WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float3 DiffuseLightDirection;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularIntensity = 1;
float Shininess;

float3 CameraPosition;
float3 LightPosition;

struct VertexShaderInput {
	float4 Position: POSITION;
	float4 Normal: NORMAL;
};

struct VertexShaderOutput {
	float4 Position : POSITION;
	float4 Color : COLOR;
	float4 Normal : TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;
};

VertexShaderOutput GouraudVertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = 0;
	output.Normal = 0;

	float3 N = normalize((mul(input.Normal, WorldInverseTranspose)).xyz);
	float3 L = normalize(LightPosition);
	float3 V = normalize(CameraPosition - worldPosition.xyz);
	float3 R = reflect(-L, N);

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	output.Color = saturate(ambient + diffuse + specular);

	return output;
}

float4 GouraudPixelShaderFunction(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

// Phong Shader - per pixel
VertexShaderOutput PhongVertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;
	output.Color = 0;

	return output;
}

float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 N = normalize((mul(input.Normal, WorldInverseTranspose)).xyz);
	float3 L = normalize(LightPosition);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 R = reflect(-L, N);

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);
	color.a = 1;
	return color;
}

// Toon Shader
VertexShaderOutput ToonVertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = input.Normal;

	return output;
}

float4 ToonPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 L = normalize(LightPosition);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 N = normalize((mul(input.Normal, WorldInverseTranspose)).xyz);
	float3 R = reflect(-L,N);
	float D = dot(V, R);

	if (D < -0.7)
	{
		return float4(0, 0, 0, 1);
	}
	else if (D < 0.2)
	{
		return float4(0.25, 0.25, 0.25, 1);
	}
	else if (D < 0.97)
	{
		return float4(0.5, 0.5, 0.5, 1);
	}
	else
	{
		return float4(1, 1, 1, 1);
	}
}

technique Gouraud
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 GouraudVertexShaderFunction();
		PixelShader = compile ps_4_0 GouraudPixelShaderFunction();
	}
}

technique Phong
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 PhongVertexShaderFunction();
		PixelShader = compile ps_4_0 PhongPixelShaderFunction();
	}
}

technique Toon
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 ToonVertexShaderFunction();
		PixelShader = compile ps_4_0 ToonPixelShaderFunction();
	}
}