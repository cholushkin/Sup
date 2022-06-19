using System.Collections.Generic;
using UnityEngine;

public class EntitiesGenerator : MonoBehaviour
{
    public GameObject PrefabEntity;

    public void Generate(List<Rect> rects)
    {
        foreach (var rect in rects)
        {
            CreateEntity(PrefabEntity.name, rect, transform);
        }
    }

    private EntTamplier CreateEntity(string entName, Rect rect, Transform parent)
    {
        var entityObject = Instantiate(PrefabEntity, rect.center, Quaternion.identity);
        var entity = entityObject.GetComponent<EntTamplier>();
        entityObject.name = entName;
        entityObject.GetComponent<EntTamplier>().LivingRect = rect;
        entityObject.transform.SetParent(parent);
        return entity;
    }
}
