﻿#if OPENGL
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
	float2 Texture : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 Texture : TEXCOORD0;
	float4 Color : COLOR0;
};

matrix MatView;
matrix MatWorld;
matrix MatProjection;

texture Tex;


sampler TexSamplerPixel = sampler_state
{
	Texture = <Tex>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler TexSampler = sampler_state
{
	Texture = <Tex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};



VertexShaderOutput VertexShader2D(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = input.Position;
	output.Texture = input.Texture;
	output.Color = input.Color;

	return output;
}

VertexShaderOutput VertexShader3D(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, MatWorld);
	float4 viewPosition = mul(worldPosition, MatView);
	output.Position = mul(viewPosition, MatProjection);
	output.Texture = input.Texture;
	output.Color = input.Color;

	return output;
}


float4 SmoothShaderFunction(VertexShaderOutput input) : COLOR
{
	float4 textureColor = tex2D(TexSampler, input.Texture);
	return input.Color * textureColor;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float4 textureColor = tex2D(TexSamplerPixel, input.Texture);
	return input.Color * textureColor;
}


technique BasicColorTexture
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader2D();
		PixelShader = compile PS_SHADERMODEL SmoothShaderFunction();
	}
}

technique TransformColorTexture
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader3D();
		PixelShader = compile PS_SHADERMODEL SmoothShaderFunction();
	}
}

technique BasicColorTexturePixel
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader2D();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}

technique TransformColorTexturePixel
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShader3D();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}