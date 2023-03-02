float4x4 MatrixTransform;
texture2D modelTexture;
texture2D filterTexture;
float imageWidth;
float imageHeight;

float3x3 RGB2YCbCr = 
{
	{ 0.2989f, 0.5866f, 0.1154f },
	{ -0.1687f, -0.3312f, 0.5000f },
	{ 0.5000f, -0.4183f, -0.0816f }
};

sampler TextureSampler: register(s0) = sampler_state {
	texture = <modelTexture>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic 
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Clamp; // Clamp, Mirror, MirrorOnce, Wrap, Border 
	AddressV = Clamp;
};

sampler FilterSampler: register(s1) = sampler_state {
	texture = <filterTexture>;
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
	// outputTexture.rgb = ceil(outputTexture.rgb * 8) / 8;

	//float offset = 0;
	//outputTexture.r = tex2D(FilterSampler, float2(outputTexture.r + offset, 0)).r;
	//outputTexture.g = tex2D(FilterSampler, float2(outputTexture.g + offset, 0)).g;
	//outputTexture.b = tex2D(FilterSampler, float2(outputTexture.b + offset, 0)).b;

	//float ycbr = mul(RGB2YCbCr, outputTexture.rgb);
	//outputTexture.rgb = ycbr.r;

	// Blur
	float4 tex1 = tex2D(TextureSampler, input.UV0 + input.UV1.xy * 5);
	float4 tex2 = tex2D(TextureSampler, input.UV0 - input.UV1.xy * 5);
	float4 tex3 = tex2D(TextureSampler, input.UV0 + input.UV1.zw * 5);
	float4 tex4 = tex2D(TextureSampler, input.UV0 - input.UV1.zw * 5);
	//outputTexture = (outputTexture + tex1 + tex2 + tex3 + tex4) / 5;
	outputTexture = outputTexture * 4 (tex1 + tex2 + tex3 + tex4);

	return outputTexture;
}

technique PostProcessing {
	pass Pass1
	{
		VertexShader = compile vs_4_0 PostVertexShader();
		PixelShader = compile ps_4_0 PostPixelShader();
	}
}