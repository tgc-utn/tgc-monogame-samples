float plot(float originalY, float functionResult) 
{    
    return smoothstep(0.02, 0.0, abs(functionResult - originalY));
}

float sdSegment(float2 initialPoint, float2 a, float2 b)
{
    float2 pa = initialPoint - a;
    float2 ba = b - a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}

float sdCircle(float2 initialPoint, float radius)
{
    return length(initialPoint) - radius;
}

float sdBox( in float2 p, in float2 b )
{
    float2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float2x2 rotation2D(float angle)
{
    float sinAngle = sin(angle);
    float cosAngle = cos(angle);
    
    return float2x2(cosAngle, -sinAngle,
                sinAngle, cosAngle);
}

float2 random2(float2 p) 
{
    return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
}

float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0 * j);
    j *= .125;
    r.x = frac(512.0 * j);
    j *= .125;
    r.y = frac(512.0 * j);
    return r;
}
