using System;

namespace TGC.MonoGame.Samples.Models;

public class ModelInfo : IDisposable
{
    internal ModelInfo(GeometryData[] geometryData)
    {
        GeometryData = geometryData;
    }

    public GeometryData[] GeometryData { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases every geometry of this model.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var geometryData in GeometryData)
            {
                geometryData.Geometry.Dispose();
            }
        }
    }
}
