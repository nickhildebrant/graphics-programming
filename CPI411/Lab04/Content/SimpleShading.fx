float4x4  World;
float4x4  View;
float4x4  Projection;
float4x4  WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularIntensity = 1;
float Shininess;

float3 CameraPosition;
float3 LightPosition;

struct VertexSahderInput {
	float4 Position: POSITION;
	float4 Normal: NORMAL;
};

struct VertexShaderOutput {
	float4 Position : POSITION;
	float4 Color : COLOR;
	float4 Normal : TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;
};

VertexShaderOutput GourandVertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = 0;
	output.Normal = 0;

	float3 N = ;
	float3 V = ;
	float3 L = ;
	float3 R = ;
	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	output.Color = saturate(ambient + diffuse + specular);
	return output;
}

float4 GourandPixelShaderFunction(VertexShaderOutput input) : COLOR
{
	return saturate(input.Color + AmbientColor * AmbientIntensity);
}

technique MyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 GourandVertexShaderFunction();
		PixelShader = compile ps_4_0 GourandPixelShaderFunction();
	}
}