using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public ZonesGenerator ZonesGenerator;
    public BackgroundDecorationGenerator BackgroundDecorationGenerator;
    public EntitiesGenerator EntitiesGenerator;

    public void Generate()
    {
        ZonesGenerator.Generate();
        BackgroundDecorationGenerator.Generate(ZonesGenerator.Bounds);
        EntitiesGenerator.Generate(ZonesGenerator.Zones);
    }

    public EntTamplier[] GetEntities()
    {
        return GetComponentsInChildren<EntTamplier>();
    }
}
