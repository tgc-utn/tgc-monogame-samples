#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 world;
float4x4 view;
float4x4 projection;

float2 screenSize;
float3 cameraPosition;

float radius;
float3 lightPosition;
float3 lightDiffuseColor;
float3 lightSpecularColor;
float KA;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};
texture colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
texture normalMap;
sampler normalMapSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
texture positionMap;
sampler positionMapSampler = sampler_state
{
    Texture = (positionMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
texture materialMap;
sampler materialMapSampler = sampler_state
{
    Texture = (materialMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

VertexShaderOutput PointLightVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);
    float4 screenPos = mul(viewPosition, projection);
    
    output.Position = screenPos;
    
    float2 ndc = screenPos.xy / screenPos.w;
    
    output.TextureCoordinates = ndc * 0.5 + 0.5;
    
    output.TextureCoordinates.y = 1.0 - output.TextureCoordinates.y;
    
    return output;
}
float sqr(float x)
{
    return x * x;
}
float attenuate_no_cusp(float distance, float radius, float max_intensity, float falloff)
{
    float s = distance / radius;

    if (s >= 1.0)
        return 0.0;

    float s2 = sqr(s);

    return max_intensity * sqr(1 - s2) / (1 + falloff * s2);
}

float3 getPixelColorNoAmbient(float3 worldPos, float3 normal, float KD, float KS, float shininess)
{
    float3 output;
    float3 lightDirection = normalize(lightPosition - worldPos);
    float3 viewDirection = normalize(cameraPosition - worldPos);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KD * lightDiffuseColor * NdotL;
    float NdotH = dot(normal, halfVector);
    float3 specularLight = sign(NdotL) * KS * lightSpecularColor * pow(saturate(NdotH), shininess);
    return diffuseLight + specularLight;
}
float4 PointLightPS(VertexShaderOutput input) : COLOR
{
    
    float2 sceneCoord = input.TextureCoordinates;
    
    float4 materialRaw = tex2D(materialMapSampler, sceneCoord);
    
    float KD = materialRaw.r;
    
    float4 normalRaw = tex2D(normalMapSampler, sceneCoord);
    
    if (KD == 0)
    {
        return float4(1, 0, 1, 1);
    }
    
    float KS = materialRaw.g;
    float4 worldRaw = tex2D(positionMapSampler, sceneCoord);
    float shininess = materialRaw.b * 60;
    
    float3 normal = normalize((normalRaw.rgb * 2.0) - 1);
    float3 worldPos = worldRaw.rgb;

    //light attenuation with distance
    float scaling = attenuate_no_cusp(distance(worldPos, lightPosition), radius, 3, 6);
    
    return float4(getPixelColorNoAmbient(worldPos, normal, KD, KS, shininess) * scaling, 1);
}

technique point_light
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PointLightVS();
        PixelShader = compile PS_SHADERMODEL PointLightPS();
    }
}

texture lightMap;
sampler lightMapSampler = sampler_state
{
    Texture = (lightMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

VertexShaderOutput PostVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
    
    return output;
}
float3 getPixelAmbient(float3 worldPos, float3 normal, float KD, float KS, float shininess)
{
    float3 output;
    float3 lightDirection = normalize(lightPosition - float3(0, 0, 0));
    float3 viewDirection = normalize(cameraPosition - worldPos);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KA * lightDiffuseColor + KD * lightDiffuseColor * NdotL;
    float NdotH = dot(normal, halfVector);
    float3 specularLight = sign(NdotL) * KS * lightSpecularColor * pow(saturate(NdotH), shininess);
    return diffuseLight + specularLight;
}

float4 AmbientPS(VertexShaderOutput input) : COLOR
{
    float4 materialRaw = tex2D(materialMapSampler, input.TextureCoordinates);
    
    float KD = materialRaw.r;
    
    float4 normalRaw = tex2D(normalMapSampler, input.TextureCoordinates);
    
    if (KD == 0)
    {
        return float4(1,1,1, 1);
    }
    
    float KS = materialRaw.g;
    float4 worldRaw = tex2D(positionMapSampler, input.TextureCoordinates);
    float shininess = materialRaw.b * 60;
    
    float3 normal = normalize((normalRaw.rgb * 2.0) - 1);
    float3 worldPos = worldRaw.rgb;
    
    return float4(getPixelAmbient(worldPos, normal, KD, KS, shininess), 1);
}

float4 IntegratePS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(colorSampler, input.TextureCoordinates);
    float4 light = tex2D(lightMapSampler, input.TextureCoordinates);
   
    return color * light;
}

technique ambient_light
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader = compile PS_SHADERMODEL AmbientPS();
    }
}
technique integrate
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader = compile PS_SHADERMODEL IntegratePS();
    }
}