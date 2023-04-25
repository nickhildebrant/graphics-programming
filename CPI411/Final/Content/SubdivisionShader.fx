float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float SubdivisionIteration;
float TesselationFactor;

texture DisplacementTexture;
float DisplacementHeight;

float GeometryGeneration;

bool HeightMapColors;
bool ShowTexture;

SamplerState DisplacementSampler = sampler_state {
	Texture = <DisplacementTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput {
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
	//float4 Tangent : TANGENT0;
	//float4 Binormal : BINORMAL0;
};

struct VertexShaderOutput {
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal: TEXCOORD1;
	float3 Tangent : TEXCOORD2;
	float3 Binormal : TEXCOORD3;
	float3 WorldPosition: TEXCCOORD4;
};

struct HullShaderOutput {
	float4 Position : BEZIERPOS;
};

struct TrianglePatchOutput {
	float Edges[3] : SV_TessFactor;
	float InsideFactor : SV_InsideTessFactor;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float3 normalTexture = tex2Dlod(DisplacementSampler, float4(input.TextureCoordinate.xy, 0, 0));
	float4 normalColor = float4(normalTexture, 1);
	normalTexture = 2 * (normalTexture - float3(0.5, 0.5, 0.5));

	float3x3 TangentToWorld;
	float3x3 RotationMatrix = { 1, 0, 0, 0, cos(90), -sin(90), 0, sin(90), cos(90) };
	TangentToWorld[0] = mul(input.Normal, RotationMatrix);
	TangentToWorld[1] = cross(mul(input.Normal, RotationMatrix), input.Normal);
	TangentToWorld[2] = input.Normal;
	float3 displaceNormal = mul(normalTexture, TangentToWorld);

	input.Position.xyz -= (DisplacementHeight * (displaceNormal.z - 1)) * input.Normal;

	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.WorldPosition = worldPos;
	output.Position = mul(viewPos, Projection);
	output.TextureCoordinate = input.TextureCoordinate;
	output.Normal = mul(TangentToWorld[0], WorldInverseTranspose).xyz;
	output.Tangent = mul(TangentToWorld[1], WorldInverseTranspose).xyz;
	output.Binormal = mul(TangentToWorld[2], WorldInverseTranspose).xyz;

	output.Color = HeightMapColors ? normalColor : input.Color;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	return ShowTexture ? tex2D(DisplacementSampler, input.TextureCoordinate) : input.Color;
}

/*TrianglePatchOutput TrianglePatchFunction(InputPatch<VertexShaderOutput, 3> inputPatch, uint patchID : SV_PrimitiveID)
{
	TrianglePatchOutput output;
	output.Edges[0] = TesselationFactor;
	output.Edges[1] = TesselationFactor;
	output.Edges[2] = TesselationFactor;
	output.InsideFactor = TesselationFactor;

	return output;
}

[domain("tri")]						// tri  quad  isoline
[partitioning("fractional_even")]	// fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]		// triangle_cw  triangle_ccw  line
[outputcontrolpoints(3)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(50.0)]
HullShaderOutput HullShaderFunction(InputPatch<VertexShaderOutput, 3> inputPatch, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
	HullShaderOutput output;
	output.Position = inputPatch[i].TextureCoordinate;
	return output;
}

[domain("tri")]
VertexShaderOutput DomainShaderFunction(const OutputPatch<HullShaderOutput, 3> outputPatch, float3 barycentric : SV_DomainLocation, TrianglePatchOutput patchConst)
{
	VertexShaderOutput output;

	float4 position = outputPatch[0].Position * barycentric.x + outputPatch[1].Position * barycentric.y + outputPatch[2].Position * barycentric.z;
	float distance = length(position.xyz);

	position.z = distance * distance;
	position.z += TextureDisplacement * Texture.SampleLevel(TextureSampler, position.xy * 2, 0).x;

	output.Position = mul(position, World * View * Projection);
	output.TextureCoordinate = position;
	return output;
}

[maxvertexcount(100)]
void GeometryShaderFunction(triangle in VertexShaderOutput vertex[3], inout TriangleStream<VertexShaderOutput> triangleStream)
{
	float3 vertex0 = vertex[0].TextureCoordinate.xyz;
	float3 vertex1 = vertex[1].TextureCoordinate.xyz;
	float3 vertex2 = vertex[2].TextureCoordinate.xyz;

	float size = 1 / GeometryGeneration;

	// Calculate parametric functions for the model's smooth curves
	for (float s = 0; s < 3; s += size)
	{
		float t = frac(s);
		float3 origin = s < 1 ? lerp(vertex0, vertex1, t) : s < 2 ? lerp(vertex1, vertex2, t) : lerp(vertex2, vertex0, t);
		origin.z += TextureDisplacement * Texture.SampleLevel(TextureSampler, origin.xy, 0).x;

		vertex[0].Position = mul(float4(origin + vertex0 * size, 1), World * View * Projection);
		vertex[1].Position = mul(float4(origin + vertex1 * size, 1), World * View * Projection);
		vertex[2].Position = mul(float4(origin + vertex2 * size, 1), World * View * Projection);

		triangleStream.Append(vertex[0]);
		triangleStream.Append(vertex[1]);
		triangleStream.Append(vertex[2]);

		triangleStream.RestartStrip();
	}
}*/

technique SubdivisionShader
{
	pass P0
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
		//HullShader = compile hs_5_0 HullShaderFunction();
		//DomainShader = compile ds_5_0 DomainShaderFunction();
		//GeometryShader = compile gs_4_0 GeometryShaderFunction();
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