using System;

namespace TGC.MonoGame.Samples.Models;

public class ModelInfo : IDisposable
{
    public GeometryData[] GeometryData { get; private set; }

    internal ModelInfo(GeometryData[] geometryData)
    {
        GeometryData = geometryData;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var geometryData in GeometryData)
        {
            geometryData.Geometry.Dispose();
        }
    }
}
