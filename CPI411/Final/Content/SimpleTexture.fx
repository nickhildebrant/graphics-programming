float3 offset;
float4x4 World;
float4x4 View;
float4x4 Projection;

sampler mySampler = sampler_state {
	Texture = <MyTexture>;
};

struct VertexPositionTexture {
	float4 position: POSITION;
	float2 textureCoordinate : TEXCOORD;
};

struct VertexPositionColor {
	float4 position: POSITION;
	float4 color: COLOR;
};

VertexPositionColor MyVertexShader(VertexPositionColor input) : POSITION
{
	return input;
}

float4 MyPixelShader(VertexPositionColor input : COLOR) : COLOR
{
	float4 color = input.color;
	if (color.r % 0.1 < 0.05f) return float4(1, 1, 1, 1);
	else return color;
}

VertexPositionTexture MyVertexShader2(VertexPositionTexture input)
{
	//input.position.xyz += offset; *** added for Lab2
	VertexPositionTexture output;
	float4 worldPos = mul(input.position, World);
	float4 viewPos = mul(worldPos, View);
	output.position = mul(viewPos, Projection);
	output.textureCoordinate = input.textureCoordinate;
	return output;
}

float4 MyPixelShader2(VertexPositionTexture input) : COLOR
{
	return tex2D(mySampler, input.textureCoordinate);
}

technique MyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader2();
		PixelShader = compile ps_4_0 MyPixelShader2();
	}
}