using UnityEngine;

namespace BC.Core
{
    /// <summary>
    /// Scene-placed singleton base. Derived classes override <see cref="OnSingletonAwake"/> instead of Awake.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        [SerializeField] private bool _persistAcrossScenes = true;

        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;

            if (_persistAcrossScenes)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
