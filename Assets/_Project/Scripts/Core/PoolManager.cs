using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    public class PoolManager<T> where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new Stack<T>();

        public PoolManager(T prefab, int prewarmCount = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarmCount; i++)
            {
                T instance = CreateNew();
                instance.gameObject.SetActive(false);
                _inactive.Push(instance);
            }
        }

        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();
            instance.gameObject.SetActive(true);
            instance.OnSpawned();
            return instance;
        }

        public void Return(T instance)
        {
            instance.OnDespawned();
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }

        private T CreateNew()
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }
}
