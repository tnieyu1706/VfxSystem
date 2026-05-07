using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace VfxSystem
{
    /// <summary>
    /// Manager handling the pooling and lifecycle of VFX instances.
    /// Uses the prefab's InstanceID to retrieve and play VFX, ensuring optimal lazy loading.
    /// </summary>
    public class VfxManager : MonoBehaviour
    {
        // Dictionary storing pools, key is the InstanceID of the VFX prefab
        protected Dictionary<int, IObjectPool<VfxInstance>> _pools = new Dictionary<int, IObjectPool<VfxInstance>>();

        /// <summary>
        /// Gets or creates a pool for the specified VFX prefab.
        /// </summary>
        protected virtual IObjectPool<VfxInstance> GetPool(VfxInstance prefab)
        {
            int prefabId = prefab.gameObject.GetInstanceID();

            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                pool = new ObjectPool<VfxInstance>(
                    createFunc: () =>
                    {
                        var instance = Instantiate(prefab);
                        // Note: Removed SetParent so instances stay in the active scene and are destroyed on scene unload.
                        return instance;
                    },
                    actionOnGet: (instance) =>
                    {
                        if (instance != null) instance.gameObject.SetActive(true);
                    },
                    actionOnRelease: (instance) =>
                    {
                        if (instance != null) instance.gameObject.SetActive(false);
                    },
                    actionOnDestroy: (instance) =>
                    {
                        if (instance != null) Destroy(instance.gameObject);
                    },
                    collectionCheck: false,
                    defaultCapacity: 10,
                    maxSize: 100
                );
                _pools.Add(prefabId, pool);
            }

            return pool;
        }

        /// <summary>
        /// Plays a looping VFX. Returns an IVfxHandle that must be released by the caller.
        /// </summary>
        public virtual IVfxHandle PlayLoop(VfxInstance prefab, Vector3 position)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[VfxManager] PlayLoop called with a null prefab!");
                return null;
            }

            var pool = GetPool(prefab);
            var vfxInstance = pool.Get();

            // Handle case where pool returns a destroyed object (e.g., after scene unload)
            if (vfxInstance == null) return null;

            vfxInstance.Init(pool);
            vfxInstance.Play(position);

            return vfxInstance;
        }

        /// <summary>
        /// Plays a one-shot VFX. It will automatically return to the pool when finished.
        /// </summary>
        public virtual void PlayOneshot(VfxInstance prefab, Vector3 position)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[VfxManager] PlayOneshot called with a null prefab!");
                return;
            }

            var pool = GetPool(prefab);
            var vfxInstance = pool.Get();

            // Handle case where pool returns a destroyed object
            if (vfxInstance == null) return;

            vfxInstance.Init(pool);
            vfxInstance.Play(position);
        }
    }
}