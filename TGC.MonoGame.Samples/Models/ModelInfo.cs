

namespace TGC.MonoGame.Samples.Models;

public class ModelInfo
{
    public GeometryData[] GeometryData { get; private set; }

    internal ModelInfo(GeometryData[] geometryData)
    {
        GeometryData = geometryData;
    }
}
