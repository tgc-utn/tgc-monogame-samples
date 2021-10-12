#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float4x4 World;
float2 TexelSize;
float Time;
float3 Color;
float Threshold;
float Intensity;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture bloomTexture;
sampler2D bloomTextureSampler = sampler_state
{
    Texture = (bloomTexture);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
    


float3 hash(float3 p) // replace this by something better. really. do
{
    p = float3(dot(p, float3(127.1, 311.7, 74.7)),
			  dot(p, float3(269.5, 183.3, 246.1)),
			  dot(p, float3(113.5, 271.9, 124.6)));

    return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}



// Gradient Noise by Inigo Quilez
float noise(in float3 x)
{
    // grid
    float3 p = floor(x);
    float3 w = frac(x);
    
    // quintic interpolant
    float3 u = w * w * w * (w * (w * 6.0 - 15.0) + 10.0);

    
    // gradients
    float3 ga = hash(p + float3(0.0, 0.0, 0.0));
    float3 gb = hash(p + float3(1.0, 0.0, 0.0));
    float3 gc = hash(p + float3(0.0, 1.0, 0.0));
    float3 gd = hash(p + float3(1.0, 1.0, 0.0));
    float3 ge = hash(p + float3(0.0, 0.0, 1.0));
    float3 gf = hash(p + float3(1.0, 0.0, 1.0));
    float3 gg = hash(p + float3(0.0, 1.0, 1.0));
    float3 gh = hash(p + float3(1.0, 1.0, 1.0));
    
    // projections
    float va = dot(ga, w - float3(0.0, 0.0, 0.0));
    float vb = dot(gb, w - float3(1.0, 0.0, 0.0));
    float vc = dot(gc, w - float3(0.0, 1.0, 0.0));
    float vd = dot(gd, w - float3(1.0, 1.0, 0.0));
    float ve = dot(ge, w - float3(0.0, 0.0, 1.0));
    float vf = dot(gf, w - float3(1.0, 0.0, 1.0));
    float vg = dot(gg, w - float3(0.0, 1.0, 1.0));
    float vh = dot(gh, w - float3(1.0, 1.0, 1.0));
	
    // interpolation
    return va +
           u.x * (vb - va) +
           u.y * (vc - va) +
           u.z * (ve - va) +
           u.x * u.y * (va - vb - vc + vd) +
           u.y * u.z * (va - vc - ve + vg) +
           u.z * u.x * (va - vb - ve + vf) +
           u.x * u.y * u.z * (-va + vb + vc - vd + ve - vf - vg + vh);
}

//  Function from Inigo Quiles
//  https://www.shadertoy.com/view/MsS3Wc
float3 hsb2rgb(in float3 c)
{
    float3 rgb = clamp(abs((c.x * 6.0 + float3(0.0, 4.0, 2.0) %
                             6.0) - 3.0) - 1.0, 0.0, 1.0);
    rgb = rgb * rgb * (3.0 - 2.0 * rgb);
    return c.z * lerp(float3(1.0, 1.0, 1.0), rgb, c.y);
}


struct RandomEffectVertexShaderInput
{
    float4 Position : POSITION0;
};

struct RandomEffectVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD0;
};





RandomEffectVertexShaderOutput RandomEffectVS(in RandomEffectVertexShaderInput input)
{
    RandomEffectVertexShaderOutput output = (RandomEffectVertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = mul(input.Position, World);
    
    return output;
}


float4 RandomEffectPS(RandomEffectVertexShaderOutput input) : COLOR
{
    //float3 hsb = float3(frac(Time * 0.1), frac(Time * 0.1 + 564.0) * 0.5 + 0.5, 1.0);
    float3 color = Color * saturate(noise(input.WorldPosition.xyz * 0.1 - float3(0.0, 2.0 * (cos(Time) + Time), 0.0))) * 5.0;
    return float4(color, 1.0);
}




// Better, temporally stable box filtering
// [Jimenez14] http://goo.gl/eomGso
// . . . . . . .
// . A . B . C .
// . . D . E . .
// . F . G . H .
// . . I . J . .
// . K . L . M .
// . . . . . . .
float3 DownsampleBox13Tap(float2 uv)
{
    // Sample half a texel for the inner samples. Let the bilinear filtering interpolate the result
    float3 topLeft = tex2D(textureSampler, uv + TexelSize * float2(-1.0, -1.0)).rgb;
    float3 top = tex2D(textureSampler, uv + TexelSize * float2(0.0, -1.0)).rgb;
    float3 topRight = tex2D(textureSampler, uv + TexelSize * float2(1.0, -1.0)).rgb;
    float3 innerTopLeft = tex2D(textureSampler, uv + TexelSize * float2(-0.5, -0.5)).rgb;
    float3 innerTopRight = tex2D(textureSampler, uv + TexelSize * float2(0.5, -0.5)).rgb;
    float3 left = tex2D(textureSampler, uv + TexelSize * float2(-1.0, 0.0)).rgb;
    float3 center = tex2D(textureSampler, uv).rgb;
    float3 right = tex2D(textureSampler, uv + TexelSize * float2(1.0, 0.0)).rgb;
    float3 innerBottomLeft = tex2D(textureSampler, uv + TexelSize * float2(-0.5, 0.5)).rgb;
    float3 innerBottomRight = tex2D(textureSampler, uv + TexelSize * float2(0.5, 0.5)).rgb;
    float3 bottomLeft = tex2D(textureSampler, uv + TexelSize * float2(-1.0, 1.0)).rgb;
    float3 bottom = tex2D(textureSampler, uv + TexelSize * float2(0.0, 1.0)).rgb;
    float3 bottomRight = tex2D(textureSampler, uv + TexelSize * float2(1.0, 1.0)).rgb;

    half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

    float3 blurredResult = (innerTopLeft + innerTopRight + innerBottomLeft + innerBottomRight) * div.x;
    blurredResult += (topLeft + top + center + left) * div.y;
    blurredResult += (top + topRight + right + center) * div.y;
    blurredResult += (left + center + bottom + bottomLeft) * div.y;
    blurredResult += (center + right + bottomRight + bottom) * div.y;

    return blurredResult;
}


void ApplyThresholdIntensity(inout float3 color)
{
    color *= step(Threshold, color) * Intensity;
}


// Better, temporally stable box filtering
// [Jimenez14] http://goo.gl/eomGso
// . . . . . . .
// . A . B . C .
// . . D . E . .
// . F . G . H .
// . . I . J . .
// . K . L . M .
// . . . . . . .
float3 DownsampleBox13TapFilter(float2 uv)
{
    // Sample half a texel for the inner samples. Let the bilinear filtering interpolate the result
    float3 topLeft = tex2D(textureSampler, uv + TexelSize * float2(-1.0, -1.0)).rgb;
    ApplyThresholdIntensity(topLeft);
    float3 top = tex2D(textureSampler, uv + TexelSize * float2(0.0, -1.0)).rgb;
    ApplyThresholdIntensity(top);
    float3 topRight = tex2D(textureSampler, uv + TexelSize * float2(1.0, -1.0)).rgb;
    ApplyThresholdIntensity(topRight);
    float3 innerTopLeft = tex2D(textureSampler, uv + TexelSize * float2(-0.5, -0.5)).rgb;
    ApplyThresholdIntensity(innerTopLeft);
    float3 innerTopRight = tex2D(textureSampler, uv + TexelSize * float2(0.5, -0.5)).rgb;
    ApplyThresholdIntensity(innerTopRight);
    float3 left = tex2D(textureSampler, uv + TexelSize * float2(-1.0, 0.0)).rgb;
    ApplyThresholdIntensity(left);
    float3 center = tex2D(textureSampler, uv);
    ApplyThresholdIntensity(center);
    float3 right = tex2D(textureSampler, uv + TexelSize * float2(1.0, 0.0)).rgb;
    ApplyThresholdIntensity(right);
    float3 innerBottomLeft = tex2D(textureSampler, uv + TexelSize * float2(-0.5, 0.5)).rgb;
    ApplyThresholdIntensity(innerBottomLeft);
    float3 innerBottomRight = tex2D(textureSampler, uv + TexelSize * float2(0.5, 0.5)).rgb;
    ApplyThresholdIntensity(innerBottomRight);
    float3 bottomLeft = tex2D(textureSampler, uv + TexelSize * float2(-1.0, 1.0)).rgb;
    ApplyThresholdIntensity(bottomLeft);
    float3 bottom = tex2D(textureSampler, uv + TexelSize * float2(0.0, 1.0)).rgb;
    ApplyThresholdIntensity(bottom);
    float3 bottomRight = tex2D(textureSampler, uv + TexelSize * float2(1.0, 1.0)).rgb;
    ApplyThresholdIntensity(bottomRight);

    
    
    
    half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

    float3 blurredResult = (innerTopLeft + innerTopRight + innerBottomLeft + innerBottomRight) * div.x;
    blurredResult += (topLeft + top + center + left) * div.y;
    blurredResult += (top + topRight + right + center) * div.y;
    blurredResult += (left + center + bottom + bottomLeft) * div.y;
    blurredResult += (center + right + bottomRight + bottom) * div.y;

    return blurredResult;
}

#define sampleScale 1.0

// 9-tap bilinear upsampler (tent filter)
float3 UpsampleTent(float2 uv)
{
    float4 distanceToTexel = TexelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0);

    half3 blurredResult;
    blurredResult = tex2D(textureSampler, uv - distanceToTexel.xy).rgb;
    blurredResult += tex2D(textureSampler, uv - distanceToTexel.wy).rgb * 2.0;
    blurredResult += tex2D(textureSampler, uv - distanceToTexel.zy).rgb;

    blurredResult += tex2D(textureSampler, uv + distanceToTexel.zw).rgb * 2.0;
    blurredResult += tex2D(textureSampler, uv       ).rgb * 4.0;
    blurredResult += tex2D(textureSampler, uv + distanceToTexel.xw).rgb * 2.0;

    blurredResult += tex2D(textureSampler, uv + distanceToTexel.zy).rgb;
    blurredResult += tex2D(textureSampler, uv + distanceToTexel.wy).rgb * 2.0;
    blurredResult += tex2D(textureSampler, uv + distanceToTexel.xy).rgb;

    return blurredResult * (1.0 / 16.0);
}

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




VertexShaderOutput DownsampleVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

float4 ExtractDownsamplePS(VertexShaderOutput input) : COLOR
{
    float3 result = DownsampleBox13TapFilter(input.TextureCoordinates);
    
    return float4(result, 1.0);
}

float4 DownsamplePS(VertexShaderOutput input) : COLOR
{
    float3 result = DownsampleBox13Tap(input.TextureCoordinates);
    
    return float4(result, 1.0);
}

float4 UpsampleCombinePS(VertexShaderOutput input) : COLOR
{
    float3 result = UpsampleTent(input.TextureCoordinates);
    
    result += tex2D(bloomTextureSampler, input.TextureCoordinates).rgb;
    
    return float4(result, 1.0);
}

float4 UpsampleCombineTonemappingPS(VertexShaderOutput input) : COLOR
{
    float3 result = UpsampleTent(input.TextureCoordinates);
    
    result += tex2D(bloomTextureSampler, input.TextureCoordinates).rgb;
    
    return float4(result.xyz, 1.0);
}






technique RandomEffect
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL RandomEffectVS();
        PixelShader = compile PS_SHADERMODEL RandomEffectPS();
    }
};

technique ExtractDownsample
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DownsampleVS();
        PixelShader = compile PS_SHADERMODEL ExtractDownsamplePS();
    }
};

technique Downsample
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DownsampleVS();
        PixelShader = compile PS_SHADERMODEL DownsamplePS();
    }
};

technique UpsampleCombine
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DownsampleVS();
        PixelShader = compile PS_SHADERMODEL UpsampleCombinePS();
    }
};


technique UpsampleCombineTonemapping
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DownsampleVS();
        PixelShader = compile PS_SHADERMODEL UpsampleCombineTonemappingPS();
    }
};
