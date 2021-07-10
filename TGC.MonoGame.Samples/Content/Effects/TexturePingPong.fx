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

float sdRoundedBox(float2 p, float2 b, float4 r)
{
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    float2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}


float next(float2 coordinates)
{
    
    float2 movedCoordinates = coordinates + float2(ScreenSize.x, 0);
    float result = tex2D(textureSampler, movedCoordinates).r;
    
    movedCoordinates = coordinates - float2(ScreenSize.x, 0);
    result = max(result, tex2D(textureSampler, movedCoordinates).r);
    
    movedCoordinates = coordinates + float2(0, ScreenSize.y);
    result = max(result, tex2D(textureSampler, movedCoordinates).r);
    
    movedCoordinates = coordinates - float2(0, ScreenSize.y);
    result = max(result, tex2D(textureSampler, movedCoordinates).r);
    
    return result;    
}


float3 next2(float2 coordinates, out float2 resultValue)
{
    
    float2 movedCoordinates = coordinates + float2(ScreenSize.x, 0);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(0.0, movedCoordinates);
    
    movedCoordinates = coordinates - float2(ScreenSize.x, 0);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(1.0, movedCoordinates);
    
    
    movedCoordinates = coordinates + float2(0, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(2.0, movedCoordinates);
    
    
    movedCoordinates = coordinates - float2(0, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(3.0, movedCoordinates);
    
    movedCoordinates = coordinates - ScreenSize;
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(4.0, movedCoordinates);
    
    movedCoordinates = coordinates + ScreenSize;
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(5.0, movedCoordinates);
    
    movedCoordinates = coordinates + float2(ScreenSize.x, -ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(6.0, movedCoordinates);
    
    movedCoordinates = coordinates + float2(-ScreenSize.x, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
        return float3(7.0, movedCoordinates);
    
    resultValue = float2(0.0, 0.0);
    return float3(0.0, movedCoordinates);
}


float encode(float value)
{
    return 1.0 / (value + 0.1);
}

float decode(float value)
{
    return (1.0 / value) - 0.1;
}

float randomFloor(float2 coordinates, float max)
{
    float randomValue = random2(coordinates);
    randomValue *= max;
    randomValue = floor(randomValue);
    return randomValue;
}

float obtainRandomIndex(float2 coordinates, float avoid)
{
    float delta = 0.000001;
    float accumulator = 0.0;
    float randomValue = randomFloor(coordinates, 8.0);
    while (randomValue == avoid)
    {
        randomValue = randomFloor(coordinates + accumulator, 8.0);
        accumulator += delta;
    }
    return randomValue;
}


float4 nextUnique(float2 coordinates, out float index)
{
    bool found = false;
    index = 0.0;
    float2 foundCoordinates;
    float gValue = 0.0;
    
    float2 movedCoordinates = coordinates + float2(ScreenSize.x, 0);    
    float2 resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {                
        found = true;
        index = 1.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates - float2(ScreenSize.x, 0);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 1.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates + float2(0, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 1.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
        
    movedCoordinates = coordinates - float2(0, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 1.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates - ScreenSize;
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 2.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates + ScreenSize;
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 2.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates + float2(ScreenSize.x, -ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 0.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    movedCoordinates = coordinates + float2(-ScreenSize.x, ScreenSize.y);
    resultValue = tex2D(textureSampler, movedCoordinates).rg;
    if (resultValue.r > 0.0)
    {
        if (found)
            return float4(0, 0, 0, 0);
        found = true;
        index = 0.0;
        gValue = resultValue.g;
        foundCoordinates = movedCoordinates;
    }
    
    if (found)
        return float4(1.0, gValue, movedCoordinates);
    else
        return float4(0, 0, 0, 0);

}
float4 ProcessPS2(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinates);
    
    float2 coordinates = input.TextureCoordinates * 2.0 - 1.0;
    float distanceToBox = sdRoundedBox(coordinates, float2(0.01, 0.01), float4(0, 0, 0, 0));
    
    textureColor.r += step(distanceToBox, 0.0);
    
    
    float nextToThing = next(input.TextureCoordinates);
    
    if (textureColor.r == 0.0)
        textureColor.r += nextToThing * step(random2(input.TextureCoordinates + frac(Time)), 0.6) - 0.0005;
    
    return textureColor;
}

float4 ProcessPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinates);
    
    float2 coordinates = input.TextureCoordinates * 2.0 - 1.0;
    float distanceToBox = sdRoundedBox(coordinates, float2(0.005, 0.005), float4(0, 0, 0, 0));
    
    float boxy = step(distanceToBox, 0.0);
    textureColor.r += boxy;
    textureColor.g += boxy * random2(input.TextureCoordinates);
    
    
    float index = 0.0;
    float4 nextToThing = nextUnique(input.TextureCoordinates, index);
    
    if (textureColor.r == 0.0 && nextToThing.x > 0.0)
    {
        nextToThing.y *= 4.0;
        if (index == floor(nextToThing.y))
        {
            textureColor.r += nextToThing.x - 0.05;
            textureColor.g = random2(input.TextureCoordinates);
        }
    }
    else
        textureColor.g -= 0.01;
/*    else
        textureColor.b = 0.0;*/
    
    /*float p = input.TextureCoordinates.x * 256.0;
    p = floor(p);
    p /= 256.0;
    float n = 122.0;
    textureColor.b = step(p, (ScreenSize.x * n)) * step(ScreenSize.x * (n - 1), p);*/
    
    return textureColor;
}

float4 DebugPS(VertexShaderOutput input) : COLOR
{
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinates);
    if (Test > 0.0)
    {
        //float testValue = step(textureColor, Test + 0.01) * step(Test, textureColor.r);        

        float testValue = smoothstep(Test + 0.03, Test, textureColor.r) * (step(Test + 0.03, 1.0 - textureColor.r));
        
        return float4(0.0, 0.0, testValue, 1.0);
    }
    else
        return textureColor;
}

technique Process2
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ProcessPS2();
    }
};

technique Process
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ProcessPS();
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