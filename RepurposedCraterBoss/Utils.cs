using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RepurposedCraterBoss
{
    public static class Utils
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent(out T component))
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
    }
}
