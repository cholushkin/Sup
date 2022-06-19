using System.Collections.Generic;
using BSPTreeRect;
using GameLib.Random;
using UnityEngine;

public class ZonesGenerator : MonoBehaviour
{
    public int Width;
    public int Height;
    public Range MinRectHeight;
    public Range MinRectWidth;

    public List<Rect> Zones { get; private set; }
    public Rect Bounds { get; private set; }

    private BspTree _tree;


    public List<Rect> Generate()
    {
        Width = Progression.Instance.FieldWidth;
        Height = Progression.Instance.FieldHeight;

        Bounds = new Rect(
            new Vector2(transform.position.x - Width * 0.5f,
            transform.position.y - Height * 0.5f),
            new Vector2(Width, Height));

        var rootNode = new BspTree.Node { Rect = Bounds };

        var treeGeneratorParams = new BspTreeHelper.BspTreeGeneratorParams { MinNodeHeight = MinRectHeight, MinNodeWidth = MinRectWidth };
        _tree = BspTreeHelper.GenerateBspTree(rootNode, treeGeneratorParams);
        Zones = _tree.GetTopNodes();
        return Zones;
    }

    private void OnDrawGizmos()
    {
        void DrawRect(Rect rect, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(rect.center, new Vector3(rect.width, rect.height, 2));
        }

        if (_tree == null)
            return;

        DrawRect(Bounds, Color.green);
        foreach (var zone in Zones)
            DrawRect(zone, Color.white);
    }
}