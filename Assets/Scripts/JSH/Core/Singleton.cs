using UnityEngine;

namespace JSH.Core
{
    /// <summary>
    /// Scene-placed singleton base. Duplicates destroy their whole GameObject,
    /// so put all managers on one "[Managers]" prefab and drop it in every scene.
    /// Override OnSingletonAwake instead of Awake.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [SerializeField] private bool _dontDestroyOnLoad = true;

        public static T Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                OnDuplicateInstance();
                return;
            }

            Instance = (T)this;
            if (_dontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake() { }

        // Standalone services can override this when a duplicate shares an object
        // with other, non-duplicate managers.
        protected virtual void OnDuplicateInstance() => Destroy(gameObject);

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
