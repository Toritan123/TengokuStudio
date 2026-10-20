using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.PostProcessing;

using Jukebox;

namespace HeavenStudio.Games.Global
{
    /// <summary>
    /// Interface for VFXInstance<T>. As it's not generic, it's easier to work with.
    /// </summary>
    public interface IVFXInstance
    {
        string Datamodel { get; }
        List<RiqEntity> VfxEvents { get; }
        void ResetVfx();
        void AddEntity(RiqEntity e);
        bool UpdateVfx(RiqEntity e, float prog);
    }
    public class VFXInstance<T> : IVFXInstance where T : UnityEngine.Object
    {
        private readonly string datamodel;
        string IVFXInstance.Datamodel => datamodel;

        public readonly List<RiqEntity> vfxEvents = new();
        List<RiqEntity> IVFXInstance.VfxEvents => vfxEvents;

        public readonly T vfxObject;

        public delegate bool OnUpdateVfx(RiqEntity e, float prog, T obj);
        protected OnUpdateVfx onUpdateVfx;

        public virtual void ResetVfx()
        {
            vfxEvents.Clear();
        }

        public void AddEntity(RiqEntity e)
        {
            vfxEvents.Add(e);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="e">The current entity being iterated over.</param>
        /// <param name="prog">The unclamped progress through the entity.</param>
        public virtual bool UpdateVfx(RiqEntity e, float prog)
        {
            if (prog >= 0) {
                onUpdateVfx.Invoke(e, prog, vfxObject);
            }
            return false;
            // return onUpdateVfx.Invoke(e, prog, vfxObject);
        }

        /// <summary>
        /// VFXInstance
        /// </summary>
        /// <param name="datamodel">The datamodel of the RiqEntity to track.</param>
        /// <param name="vfxObject">The object to use to control vfx.</param>
        /// <param name="onUpdateVfx">A method, called to update vfx.</br>This is usually called inside another, overridden method.</param>
        public VFXInstance(string datamodel, T vfxObject, OnUpdateVfx onUpdateVfx)
        {
            this.datamodel = datamodel;
            this.vfxObject = vfxObject;
            this.onUpdateVfx = onUpdateVfx;
        }
    }
    public class VFXInstancePPE<T> : VFXInstance<T> where T : PostProcessEffectSettings
    {
        // don't seem to actually need the volume past initialization
        // public PostProcessVolume volume;
        public readonly bool enabledByDefault;

        public override void ResetVfx()
        {
            base.ResetVfx();
            vfxObject.enabled.Override(enabledByDefault);
            // this sets adjustments made in the editor to false, breaking some effects
            // vfxObject.SetAllOverridesTo(false);
        }

        // public override bool UpdateVfx(RiqEntity e, float prog)
        // {
        //     // vfxObject.enabled.Override(enabledByDefault);
        //     // vfxObject.SetAllOverridesTo(false);
        //     if (prog >= 0) {
        //         onUpdateVfx.Invoke(e, prog, vfxObject);
        //         Debug.Log("beat : " + e.beat);
        //         if (prog <= 1) return true;
        //     }
        //     return false;
        // }

        public VFXInstancePPE(string datamodel, PostProcessVolume volume, OnUpdateVfx onUpdateVfx, bool enabledByDefault = false)
            : base(datamodel, volume.profile.GetSetting<T>(), onUpdateVfx)
        {
            this.enabledByDefault = enabledByDefault;
        }
    }

    public class VFXInstanceObject<T> : VFXInstance<T> where T : MonoBehaviour
    {
        public readonly bool onByDefault;

        public override void ResetVfx()
        {
            vfxObject.gameObject.SetActive(onByDefault);
            base.ResetVfx();
        }

        public VFXInstanceObject(string datamodel, T vfxObject, OnUpdateVfx onUpdateVfx, bool onByDefault)
            : base(datamodel, vfxObject, onUpdateVfx)
        {
            this.onByDefault = onByDefault;
        }
    }

    public class VFXInstancePooled<T> : VFXInstance<T> where T : MonoBehaviour
    {
        public readonly Dictionary<int, T> vfxTrackedObjects = new();
        public readonly Transform objectParent;
        public readonly ObjectPool<T> vfxObjectPool;
        // public readonly Action<T> onPoolReleaseOrGet;

        private T OnPoolCreate()
        {
            return GameObject.Instantiate(vfxObject, objectParent);
        }

        public override bool UpdateVfx(RiqEntity e, float prog)
        {
            bool contains = vfxTrackedObjects.TryGetValue(e.uid, out T obj);
            if (obj == null)
            {
                vfxTrackedObjects.Remove(e.uid);
            }
            if (prog is >= 0f and <= 1f)
            {
                if (!contains) {
                    obj = vfxObjectPool.Get();
                    vfxTrackedObjects.Add(e.uid, obj);
                }
                return onUpdateVfx(e, prog, obj);
            }
            if (prog is > 1f or < 0f)
            {
                if (contains && obj != null) {
                    obj.gameObject.SetActive(false);
                    vfxTrackedObjects.Remove(e.uid);
                    vfxObjectPool.Release(obj);
                }
            }
            return false;
        }

        public VFXInstancePooled(string datamodel, T vfxObject, Transform objectParent, OnUpdateVfx onUpdateVfx, Action<T> onPoolReleaseOrGet = null)
            : base(datamodel, vfxObject, onUpdateVfx)
        {
            this.objectParent = objectParent;
            vfxObjectPool = new(OnPoolCreate, onPoolReleaseOrGet, onPoolReleaseOrGet);
        }
    }
}