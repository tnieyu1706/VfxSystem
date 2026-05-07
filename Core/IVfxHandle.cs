using System;

namespace VfxSystem
{
    /// <summary>
    /// Interface representing a playing VFX.
    /// Inherits from IDisposable to support 'using' statement or manual cleanup.
    /// </summary>
    public interface IVfxHandle : IDisposable
    {
        /// <summary>
        /// Returns this VFX to the Pool. 
        /// Must be called when a Loop VFX is no longer needed.
        /// </summary>
        void Release();
    }
}