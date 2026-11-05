using System;
using UnityEngine;

namespace JSH.Core
{
    /// <summary>
    /// Added automatically by ComponentPool. Lets an object return itself to its pool,
    /// or be destroyed if it was not spawned from a pool.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoolReturn : MonoBehaviour
    {
        private Action _release;
        private IPoolable[] _poolables;

        public bool IsPooled => _release != null;

        internal void Bind(Action release)
        {
            _release = release;
            _poolables = GetComponentsInChildren<IPoolable>(true);
        }

        internal void NotifySpawned()
        {
            for (int i = 0; i < _poolables.Length; i++)
            {
                _poolables[i].OnSpawned();
            }
        }

        internal void NotifyDespawned()
        {
            for (int i = 0; i < _poolables.Length; i++)
            {
                _poolables[i].OnDespawned();
            }
        }

        public void Release()
        {
            if (!gameObject.activeSelf)
            {
                return; // already released
            }

            if (_release != null)
            {
                _release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void ReleaseOrDestroy(GameObject target)
        {
            if (target.TryGetComponent(out PoolReturn handle))
            {
                handle.Release();
            }
            else
            {
                Destroy(target);
            }
        }
    }
}
