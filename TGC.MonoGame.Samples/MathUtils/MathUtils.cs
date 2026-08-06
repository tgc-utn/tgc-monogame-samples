
using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.MathUtils;

public static class MathUtils
{
    public static float InverseLerp(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    public static float Remap(float minOldRange, float maxOldRange, float minNewRange, float maxNewRange, float value)
    {
        float percentInOldRange = InverseLerp(minOldRange, maxOldRange, value);
        return MathHelper.Lerp(minNewRange, maxNewRange, percentInOldRange);
    }
    
    public static float RemapClamped(float minOldRange, float maxOldRange, float minNewRange, float maxNewRange, float value)
    {
        float percentInOldRange = Math.Clamp(InverseLerp(minOldRange, maxOldRange, value), 0f, 1f);
        return MathHelper.Lerp(minNewRange, maxNewRange, percentInOldRange);
    }
}