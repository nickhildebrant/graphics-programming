float4 MyVertexShader(float4 position: POSITION) : POSITION
{
	return position;
}

float4 MyPixelShader() : COLOR
{
	return float4(1, 1, 1, 1);
}

technique MyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}