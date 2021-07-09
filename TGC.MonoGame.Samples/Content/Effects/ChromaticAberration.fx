#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float2 ScreenDelta;
uniform float Displacement;

uniform texture BaseTexture;
uniform sampler2D textureSampler = sampler_state
{
    Texture = (BaseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
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


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

float4 BasicChromaticPS(VertexShaderOutput input) : COLOR
{
    // Sample the base color in the Render Target
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
    
    // Set our delta as a displacement in X, a given set of pixels
    // ScreenDelta is a vector containing (1.0 / ScreenWidth, 1.0 / ScreenHeight)
    float2 delta = float2(ScreenDelta.x * Displacement, 0.0);
    
    // Sample from our point but N pixels to the Right
    float4 sampleRight = tex2D(textureSampler, input.TextureCoordinates + delta);
    
    // Sample from our point but N pixels to the Left
    float4 sampleLeft = tex2D(textureSampler, input.TextureCoordinates - delta);
    
    // Use the Red Channel from the displaced Right sample
    // Use the Green Channel from the Render Target sample
    // Use the Blue Channel from the displaced Left sample
    float3 color = float3(sampleRight.r, baseColor.g, sampleLeft.b);
    
    return float4(color, 1.0);
}

float4 CenteredChromaticPS(VertexShaderOutput input) : COLOR
{
    // Sample the base color in the Render Target
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
    
    // Get the distance from our coordinates to the center of the screen
    float distanceToCenter = distance(input.TextureCoordinates, float2(0.5, 0.5));
    
    // Set our delta as a displacement in X, a given set of pixels
    // ScreenDelta is a vector containing (1.0 / ScreenWidth, 1.0 / ScreenHeight)
    // Also, scale it by the distance to the center of the screen
    float2 delta = float2(ScreenDelta.x * Displacement * distanceToCenter, 0.0);
    
    // Sample from our point but N pixels to the Right
    float4 sampleRight = tex2D(textureSampler, input.TextureCoordinates + delta);
    
    // Sample from our point but N pixels to the Left
    float4 sampleLeft = tex2D(textureSampler, input.TextureCoordinates - delta);
    
    // Use the Red Channel from the displaced Right sample
    // Use the Green Channel from the Render Target sample
    // Use the Blue Channel from the displaced Left sample
    float3 color = float3(sampleRight.r, baseColor.g, sampleLeft.b);
    
    return float4(color, 1.0);
}

float4 CenteredDirectionalChromaticPS(VertexShaderOutput input) : COLOR
{
    // Sample the base color in the Render Target
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
    
    // Get coordinates where (0, 0) is the center of the screen
    // This is to use the coordinates as vector for intensity and direction of Chromatic samples
    float2 centeredCoordinates = input.TextureCoordinates * 2.0 - 1.0;
    
    // Set our delta as a displacement in X, a given set of UNITS
    // As our vector is a circular direction and is not bound to Width and Height, 
    // We use the minimum between Height and Width to scale, as the coordinate direction is way too strong
    float2 delta = centeredCoordinates * Displacement * min(ScreenDelta.x, ScreenDelta.y);
    
    // Sample from our point but N units away from the center of the screen
    float4 sampleAway = tex2D(textureSampler, input.TextureCoordinates + delta);
    
    // Sample from our point but N units towards to the center of the screen
    float4 sampleTowards = tex2D(textureSampler, input.TextureCoordinates - delta);
    
    // Use the Red Channel from the displaced Away sample
    // Use the Green Channel from the Render Target sample
    // Use the Blue Channel from the displaced Towards sample
    float3 color = float3(sampleAway.r, baseColor.g, sampleTowards.b);
    
    return float4(color, 1.0);
}


technique BasicChromaticAberration
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BasicChromaticPS();
    }
};


technique CenteredChromaticAberration
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL CenteredChromaticPS();
    }
};


technique CenteredDirectionalChromaticAberration
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL CenteredDirectionalChromaticPS();
    }
};



