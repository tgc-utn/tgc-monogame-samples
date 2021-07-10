#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float2 ScreenSize;
uniform float Time;
uniform float Test;

texture MainTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (MainTexture);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
    
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};

float2 random2(float2 p)
{
    return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
}


float encode(float value)
{
    return 1.0 / (value + 0.025);
}

float decode(float value)
{
    return (1.0 / value) - 0.025;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

void sampleIntegrate(float2 coordinates, inout float2 compareValue)
{
    float2 sample = tex2D(textureSampler, coordinates).rg;
    if (decode(sample.g) > decode(compareValue.g))
    {
        compareValue.g = sample.g;
        compareValue.r = sample.r;
    }
}

float playValue(float2 coordinates, float localValue)
{
    float sample = tex2D(textureSampler, coordinates).r;
    
    float2x2 payMatrix =
    {
        1.2, 2.7,
        0.0, 0.1
    };
    
    //return payMatrix[sign(sample), sign(localValue)];
    
    if(sample == localValue)
    {
        return payMatrix[sample][sample];
    } 
        
    return payMatrix[sample][1 - sample];
}

float newRed(float2 coordinates, float2 compareValue)
{
    sampleIntegrate(coordinates + ScreenSize, compareValue);
    sampleIntegrate(coordinates - ScreenSize, compareValue);
    sampleIntegrate(coordinates + float2(ScreenSize.x, 0.0), compareValue);
    sampleIntegrate(coordinates - float2(ScreenSize.x, 0.0), compareValue);
    sampleIntegrate(coordinates + float2(0.0, ScreenSize.y), compareValue);
    sampleIntegrate(coordinates + float2(0.0, -ScreenSize.y), compareValue);
    sampleIntegrate(coordinates + float2(ScreenSize.x, -ScreenSize.y), compareValue);
    sampleIntegrate(coordinates + float2(-ScreenSize.x, ScreenSize.y), compareValue);
    
    return compareValue.r;
}

float obtainGain(float2 coordinates, float value)
{
    float totalGain = 0.0;
    totalGain += playValue(coordinates + ScreenSize, value);
    totalGain += playValue(coordinates - ScreenSize, value);
    totalGain += playValue(coordinates + float2(ScreenSize.x, 0.0), value);
    totalGain += playValue(coordinates - float2(ScreenSize.x, 0.0), value);
    totalGain += playValue(coordinates + float2(0.0, ScreenSize.y), value);
    totalGain += playValue(coordinates + float2(0.0, -ScreenSize.y), value);
    totalGain += playValue(coordinates + float2(ScreenSize.x, -ScreenSize.y), value);
    totalGain += playValue(coordinates + float2(-ScreenSize.x, ScreenSize.y), value);
    return totalGain;
}

float4 ProcessPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinates);
    
    if (textureColor.g <= 0.0)
    {
        textureColor.g = encode(obtainGain(input.TextureCoordinates, textureColor.r));
    }
    else
    {
        // Play against near
        textureColor.r = newRed(input.TextureCoordinates, textureColor.rg);
        textureColor.g = 0.0;
    }
    
    return textureColor;
}
/*

    float sample = sign(tex2D(textureSampler, coordinates).r);
    
    float2x2 payMatrix =
    {
        1.2, 0,
        2.7, 0.1
    };
    
    return payMatrix[localValue, sample];
*/
float4 InitialPS(VertexShaderOutput input) : COLOR
{
    float midPixel = step(input.TextureCoordinates.x, 0.5 + ScreenSize.x) *
        step(0.5, input.TextureCoordinates.x);
    midPixel *= step(input.TextureCoordinates.y, 0.5 + ScreenSize.y) *
        step(0.5, input.TextureCoordinates.y);
    
    
    float random = step(random2(input.TextureCoordinates).x, 0.5);
    
    return float4(midPixel, 0, 0, 1);
}

float4 DebugPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinates).rrra;
    return textureColor;
}

technique Process
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ProcessPS();
    }
};

technique InitialConditions
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL InitialPS();
    }
};

technique Debug
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DebugPS();
    }
};