using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public abstract class Object
    {
        public static void Destroy(Object obj)
        {
            if (obj is GameObject go) go.MarkDestroyed();
        }

        public static T Instantiate<T>(T original, Transform parent = null) where T : Component
        {
            var go = new GameObject(original.gameObject != null ? original.gameObject.name : typeof(T).Name);
            return go.AddComponent<T>();
        }
    }

    public abstract class Component : Object
    {
        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject?.transform;

        internal void AttachTo(GameObject go) => gameObject = go;

        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>();
    }

    public abstract class MonoBehaviour : Component
    {
        public bool enabled = true;
    }

    public abstract class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    public class GameObject : Object
    {
        public string name;
        public string tag = "Untagged";
        public Transform transform { get; }
        public bool ActiveSelf { get; private set; } = true;
        public bool IsDestroyed { get; private set; }

        private readonly List<Component> _components = new List<Component>();

        public GameObject() : this("GameObject") { }

        public GameObject(string name)
        {
            this.name = name;
            transform = new Transform();
            transform.AttachTo(this);
            _components.Add(transform);
        }

        internal void MarkDestroyed() => IsDestroyed = true;

        public void SetActive(bool value)
        {
            bool wasActive = ActiveSelf;
            ActiveSelf = value;
            if (wasActive == value) return;

            foreach (Component c in _components)
            {
                if (c is MonoBehaviour) InvokeLifecycle(c, value ? "OnEnable" : "OnDisable");
            }
        }

        public T AddComponent<T>() where T : Component
        {
            var instance = System.Activator.CreateInstance<T>();
            instance.AttachTo(this);
            _components.Add(instance);

            if (instance is Camera cam && tag == "MainCamera") Camera.main = cam;

            if (instance is MonoBehaviour)
            {
                InvokeLifecycle(instance, "Awake");
                if (ActiveSelf) InvokeLifecycle(instance, "OnEnable");
            }

            return instance;
        }

        public T GetComponent<T>() where T : class
        {
            foreach (Component c in _components)
            {
                if (c is T t) return t;
            }

            return null;
        }

        internal static void InvokeLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            method?.Invoke(target, null);
        }
    }

    public class Transform : Component
    {
        public Vector3 position;

        public void LookAt(Transform target) { /* no-op for shim; rotation is not unit-tested */ }
    }
}
