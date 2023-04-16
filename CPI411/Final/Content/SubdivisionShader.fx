float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput {
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput {
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	output.Color = input.Color;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique SubdivisionShader
{
	pass P0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
};

/*float4 main(float4 srcCoord, uniform samplerRECT srcTexMap : TEXUNIT0) : COLOR
{ 
	float3 wh, wv;      // weight values    
	float3   s1, s2, s3;    
	float2 f;      // calculate weights based on fractional part of      
	// source texture coordinate    
	//    
	// x == 0.0, y == 0.0 use face weights (1/4, 1/4, 0)    
	//                                     (1/4, 1.4, 0)    
	//                                     ( 0, 0, 0)    
	// x == 0.5, y == 0.0 use edge weights (horizontal)      
	//                                     (1/16, 6/16, 1/16)    
	//                                     (1/16, 6/16, 1/16)    
	//                                     ( 0, 0, 0)    
	// x == 0.0, y == 0.5 use edge weights (vertical)    
	//                                     (1/16, 1/16, 0)    
	//                                     (6/16, 6/16, 0)      
	//                                     (1/16, 1/16, 0)    
	// x == 0.5, y == 0.5 use valence 4 vertex weights    
	//                                     (1/64, 6/64, 1/64)    
	//                                     (6/64, 36/64, 6/64)    
	//                                     (1/64, 6/64, 1/64)    
	wh = float3 (1.0/8.0, 6.0/8.0, 1.0/8.0);    
	f = frac (srcCoord.xy + 0.001) < 0.25; // account for finite precision      
	if (f.x != 0.0) 
	{      
		wh = float3(0.5, 0.5, 0.0);      
		srcCoord.x += 0.5; // fraction was zero -- move to texel center    
	}    

	wv = float3 (1.0/8.0, 6.0/8.0, 1.0/8.0);    
	if (f.y != 0) 
	{      
		wv = float3 (0.5, 0.5, 0.0);     
		srcCoord.y += 0.5;  
		// fraction was zero -- need to move to texel center      
	}      

	// calculate the destination vertex position by using the weighted    
	// sum of the 9 vertex positions centered at srcCoord    
	s1 = texRECT (srcTexMap, srcCoord.xy + float2 (-1, -1)).xyz * wh.x + texRECT (srcTexMap, srcCoord.xy + float2 (0, -1)).xyz * wh.y + texRECT (srcTexMap, srcCoord.xy + float2 (1, -1)).xyz * wh.z;      
	s2 = texRECT (srcTexMap, srcCoord.xy + float2 (-1, 0)).xyz * wh.x + texRECT (srcTexMap, srcCoord.xy + float2 (0, 0)).xyz * wh.y + texRECT (srcTexMap, srcCoord.xy + float2 (1, 0)).xyz * wh.z;      
	s3 = texRECT (srcTexMap, srcCoord.xy + float2 (-1, 1)).xyz * wh.x + texRECT (srcTexMap, srcCoord.xy + float2 (0, 1)).xyz * wh.y + texRECT (srcTexMap, srcCoord.xy + float2 (1, 1)).xyz * wh.z;      
	return float4 (s1 * wv.x + s2 * wv.y + s3 * wv.z, 0);  
}*/