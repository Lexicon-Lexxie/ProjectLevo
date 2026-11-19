using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Levo.Tests
{
    /// <summary>Creates test objects and removes them after every test.</summary>
    public abstract class PlayModeTestBase
    {
        readonly List<GameObject> created = new List<GameObject>();

        protected GameObject Create(string name)
        {
            var go = new GameObject(name);
            created.Add(go);
            return go;
        }

        protected GameObject CreateCube(string name, Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            created.Add(go);
            return go;
        }

        /// <summary>Sets a private [SerializeField] the way the Inspector would.</summary>
        protected static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        [TearDown]
        public void CleanUp()
        {
            foreach (var go in created)
                if (go != null) Object.Destroy(go);
            created.Clear();
        }
    }
}
