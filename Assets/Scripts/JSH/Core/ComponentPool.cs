using UnityEngine;
using UnityEngine.Pool;

namespace JSH.Core
{
    /// <summary>Optional callbacks for pooled objects (any component in the prefab hierarchy).</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    /// <summary>
    /// ObjectPool&lt;T&gt; wrapper for prefabs. Spawned objects can return themselves
    /// via PoolReturn.Release() / PoolReturn.ReleaseOrDestroy(gameObject).
    /// Usage:
    ///   _bulletPool = new ComponentPool&lt;Bullet&gt;(_bulletPrefab, transform, prewarm: 20);
    ///   Bullet b = _bulletPool.Get(pos, rot);
    /// </summary>
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly ObjectPool<T> _pool;

        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public ComponentPool(T prefab, Transform parent = null, int defaultCapacity = 16, int maxSize = 256, int prewarm = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<T>(Create, null, OnRelease, OnDestroyItem, true, defaultCapacity, maxSize);

            if (prewarm > 0)
            {
                T[] temp = new T[prewarm];
                for (int i = 0; i < prewarm; i++)
                {
                    temp[i] = _pool.Get();
                }

                for (int i = 0; i < prewarm; i++)
                {
                    _pool.Release(temp[i]);
                }
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = _pool.Get();
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            if (item.TryGetComponent(out PoolReturn handle))
            {
                handle.NotifySpawned();
            }

            return item;
        }

        public T Get(Vector3 position) => Get(position, Quaternion.identity);

        public void Release(T item) => _pool.Release(item);

        public void Clear() => _pool.Clear();

        private T Create()
        {
            T item = Object.Instantiate(_prefab, _parent);
            item.gameObject.SetActive(false);
            if (!item.TryGetComponent(out PoolReturn handle))
            {
                handle = item.gameObject.AddComponent<PoolReturn>();
            }

            handle.Bind(() => _pool.Release(item));
            return item;
        }

        private void OnRelease(T item)
        {
            if (item.TryGetComponent(out PoolReturn handle))
            {
                handle.NotifyDespawned();
            }

            item.gameObject.SetActive(false);
            if (item.transform.parent != _parent)
            {
                item.transform.SetParent(_parent, false);
            }
        }

        private static void OnDestroyItem(T item)
        {
            if (item != null)
            {
                Object.Destroy(item.gameObject);
            }
        }
    }
}
