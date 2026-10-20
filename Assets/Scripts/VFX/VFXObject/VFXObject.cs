using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Jukebox;

namespace HeavenStudio.VFX
{
    public class VFXObject : MonoBehaviour
    {
        public enum TextboxAnchor
        {
            TopLeft,
            TopMiddle,
            TopRight,
            Left,
            Middle,
            Right,
            BottomLeft,
            BottomMiddle,
            BottomRight,
            Custom,
        }

        public enum ClosedCaptionsAnchor
        {
            Top,
            Bottom,
        }

        // heartbeat variable, if not set to true in a frame the object will destroy itself 
        protected bool updatedThisFrame = false;
        protected float XAnchor = 3f;
        protected float YAnchor = 3.5f;

        public virtual void UpdateObject(RiqEntity e, float prog = 0f)
        {
            updatedThisFrame = true;
        }

        private void LateUpdate()
        {
            if (!updatedThisFrame) {
                Destroy(gameObject);
            }
            updatedThisFrame = false;
        }

        protected Vector3 GetAnchorPosFromEntity(RiqEntity entity)
        {
            return (TextboxAnchor)entity["type"] switch
            {
                TextboxAnchor.TopLeft =>      new Vector3(-XAnchor, YAnchor),
                TextboxAnchor.TopMiddle =>    new Vector3(0, YAnchor),
                TextboxAnchor.TopRight =>     new Vector3(XAnchor, YAnchor),
                TextboxAnchor.Left =>         new Vector3(-XAnchor, 0),
                TextboxAnchor.Middle =>       new Vector3(0, 0),
                TextboxAnchor.Right =>        new Vector3(XAnchor, 0),
                TextboxAnchor.BottomLeft =>   new Vector3(-XAnchor, -YAnchor),
                TextboxAnchor.BottomMiddle => new Vector3(0, -YAnchor),
                TextboxAnchor.BottomRight =>  new Vector3(XAnchor, -YAnchor),
                TextboxAnchor.Custom =>       new Vector3(entity["x"], entity["y"]),
                _ => new Vector3(0, 0),
            };
        }
    }
}