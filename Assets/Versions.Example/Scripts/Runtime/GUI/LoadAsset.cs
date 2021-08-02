using System.Collections.Generic;
using UnityEngine;

namespace Versions.Example
{
    public class LoadAsset : MonoBehaviour
    {
        private readonly List<InstantiateObject> objects = new List<InstantiateObject>();

        private void Start()
        {
            objects.Add(InstantiateObject.InstantiateAsync(Res.GetPrefab("Children")));
            objects.Add(InstantiateObject.InstantiateAsync(Res.GetPrefab("Children2")));
        }

        private void OnDestroy()
        {
            foreach (var instantiateObject in objects) instantiateObject.Destroy();

            objects.Clear();
        }
    }
}