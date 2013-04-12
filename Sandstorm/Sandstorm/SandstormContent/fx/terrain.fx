//------------------------------------------------------
//--                                                  --
//--		   www.riemers.net                    --
//--   		    Basic shaders                     --
//--		Use/modify as you like                --
//--                                                  --
//------------------------------------------------------

struct VertexToPixel
{
    float4 Position   	: POSITION;
	float4 Color		: COLOR0;    
    float2 TexCoord		: TEXCOORD0;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 viewMatrix;
float4x4 projMatrix;
float4x4 worldMatrix;


//------- Texture Samplers --------

Texture heightMap;
sampler TextureSampler = sampler_state { texture = <heightMap>; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = Clamp; AddressV = Clamp;};

//------- Technique: Terrain --------

VertexToPixel TerrainVS( float4 inPos : POSITION, float2 inTexCoord: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;

	float4x4 viewProjection = mul (viewMatrix, projMatrix);
	float4x4 worldViewProjection = mul (worldMatrix, viewProjection);

	float4 pos = inPos;
	float height = tex2Dlod(TextureSampler, float4(inTexCoord, 0, 0)).r;
	pos.y = height * 100.0;

	Output.Position = mul(pos, worldViewProjection);
	Output.TexCoord = inTexCoord;
	Output.Color = float4(height, height, height, 1.0);
    
	return Output;    
}

PixelToFrame TerrainPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.Color;

	return Output;
}

technique Terrain
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TerrainVS();
		PixelShader  = compile ps_3_0 TerrainPS();
	}
}