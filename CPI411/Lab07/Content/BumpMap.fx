float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float4 AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;

texture normalMap;

sampler tsampler1 = sampler_state {
	texture = <normalMap>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic 
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap; // Clamp, Mirror, MirrorOnce, Wrap, Border 
	AddressV = Wrap;
};

struct VertexShaderInput {
	float4 Position: POSITION;
	float2 TextureCoordinate: TEXCOORD;
	float4 Normal: NORMAL;
	float4 Tangent: TANGENT0;
	float4 Binormal: BINORMAL0;
};

struct VertexShaderOutput {
	float4 Position: POSITION0;
	float3 Normal: NORMAL0;
	float3 Tangent: TEXCOORD1;
	float3 Binormal: TEXCOORD2;
	float2 TextureCoordinate: TEXCOORD3;
	float3 WorldPosition : TEXCOORD4;

};

VertexShaderOutput BumpMapVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition.xyz;

	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose).xyz);
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose).xyz);
	output.TextureCoordinate = input.TextureCoordinate;
	return output;
}

float4 BumpMapPixelShader(VertexShaderOutput input) : COLOR0
{
	float3 N = input.Normal;
	float3 L = normalize(LightPosition - input.WorldPosition.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 R = reflect(-L, N);

	float3 T = input.Tangent;
	float3 B = input.Binormal;
	float3 H = normalize(L + V);

	float3 normalTexture = (tex2D(tsampler1, input.TextureCoordinate).xyz - float3(0.5, 0.5, 0.5)) * 2.0;

	// ** Lab 07
	//float3 bumpNormal = N + normalTexture.x * T + normalTexture.y * B;

	// ** Assignment3 Version
	float3x3 TangentToWorld;
	TangentToWorld[0] = T;
	TangentToWorld[1] = B;
	TangentToWorld[2] = N;
	float bumpNormal = mul(normalTexture, TangentToWorld);

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(bumpNormal, L));
	float4 specular = pow(max(0, dot(H, bumpNormal)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);
	color.a = 1;

	return color;
}

technique BumpMap
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 BumpMapVertexShader();
		PixelShader = compile ps_4_0 BumpMapPixelShader();
	}
}