﻿float4x4 MatrixTransform;
texture2D modelTexture;
float imageWidth;
float imageHeight;

sampler TextureSampler: register(s0) = sampler_state {
	texture = <modelTexture>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic 
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Clamp; // Clamp, Mirror, MirrorOnce, Wrap, Border 
	AddressV = Clamp;
};

sampler TextureSampler: register(s0) = sampler_state {
	texture = <modelTexture>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic 
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Clamp; // Clamp, Mirror, MirrorOnce, Wrap, Border 
	AddressV = Clamp;
};

struct VS_OUTPUT {
	float4 Position: POSITION;
	float2 UV0 : TEXCOORD0;
	float4 UV1 : TEXCOORD1;
};

VS_OUTPUT PostVertexShader(float4 inputPosition : POSITION, float2 inputTexture : TEXCOORD0)
{
	VS_OUTPUT output;
	output.Position = mul(inputPosition, MatrixTransform);
	output.UV0 = inputTexture;
	output.UV1 = float4(2 / imageWidth, 0, 0, 2 / imageHeight);

	return output;
}

float4 PostPixelShader(VS_OUTPUT input) : COLOR
{
	float4 outputTexture = tex2D(TextureSampler, input.UV0);
	// **** Main processing **** //
	outputTexture.rgb = ceil(outputTexture.rgb * 8) / 8;

	return outputTexture;
}

technique PostProcessing {
	pass Pass1
	{
		VertexShader = compile vs_4_0 PostVertexShader();
		PixelShader = compile ps_4_0 PostPixelShader();
	}
}