using UnityEngine;
using UnityEngine.Pool;

namespace VfxSystem
{
    /// <summary>
    /// This component is attached to VFX Prefabs.
    /// It manages the Particle System and implements IVfxHandle to interact with the Pool.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class VfxInstance : MonoBehaviour, IVfxHandle
    {
        private ParticleSystem _particleSystem;
        private IObjectPool<VfxInstance> _pool;
        private bool _isReleased = false;

        private void Awake()
        {
            _particleSystem ??= GetComponent<ParticleSystem>();
            
            // Ensure Stop Action is set to Callback so One-shot works correctly
            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        /// <summary>
        /// Initializes the instance with its managing pool.
        /// </summary>
        public void Init(IObjectPool<VfxInstance> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Starts playing the VFX at a specific position.
        /// </summary>
        public void Play(Vector3 position)
        {
            _isReleased = false;
            transform.position = position;
            gameObject.SetActive(true);
            _particleSystem.Play(true);
        }

        /// <summary>
        /// Automatically called by Unity when the Particle System stops (requires StopAction = Callback).
        /// Very useful for One-shot VFX.
        /// </summary>
        private void OnParticleSystemStopped()
        {
            // Auto return to pool when finished (for One-shot)
            if (!_isReleased && this != null)
            {
                Release();
            }
        }

        /// <summary>
        /// Called by the user or VfxManager to return the VFX to the pool (for Loop).
        /// </summary>
        public void Release()
        {
            // Prevent releasing if already released, pool is missing, or object is being destroyed
            if (_isReleased || _pool == null || this == null) return;
            
            _isReleased = true;
            
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            
            // Return to pool. (Note: gameObject.SetActive(false) is handled by ObjectPool)
            _pool.Release(this);
        }

        /// <summary>
        /// Implements IDisposable.
        /// Allows usage: using (var vfx = VfxManager.PlayLoop(...)) { ... }
        /// </summary>
        public void Dispose()
        {
            Release();
        }
    }
}