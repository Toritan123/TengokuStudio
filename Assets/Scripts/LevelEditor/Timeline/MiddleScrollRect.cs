using UnityEngine.EventSystems;
using UnityEngine.UI;
using Riten.Native.Cursors;

namespace HeavenStudio.Editor.Track
{
    public class MiddleScrollRect : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;

            NativeCursor.SetCursor(NTCursors.ResizeAll);
            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;

            NativeCursor.ResetCursor();
            base.OnEndDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (Conductor.instance.isPlaying) return;
            if (eventData.button != PointerEventData.InputButton.Middle) return;
            eventData.button = PointerEventData.InputButton.Left;

            NativeCursor.SetCursor(NTCursors.ResizeAll);
            base.OnDrag(eventData);
        }
    }
}