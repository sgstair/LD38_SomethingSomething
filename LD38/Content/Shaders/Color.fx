#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

matrix MatView;
matrix MatWorld;
matrix MatProjection;

VertexShaderOutput VertexShader2D(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = input.Position;
	output.Color = input.Color;

	return output;
}

VertexShaderOutput VertexShader3D(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, MatWorld);
	float4 viewPosition = mul(worldPosition, MatView);
	output.Position = mul(viewPosition, MatProjection);
	output.Color = input.Color;

	return output;
}



float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return input.Color;
}

technique BasicColor
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader2D();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}

technique TransformColor
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader3D();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}