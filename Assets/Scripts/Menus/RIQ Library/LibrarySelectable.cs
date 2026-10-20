using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibrarySelectable : MonoBehaviour
    {
        public bool adjustWithScale = true;

        [Space]

        public UnityEvent onHover;
        public UnityEvent onUnhover;

        [Space]

        public UnityEvent onLeftOverride;
        public UnityEvent onRightOverride;
        public UnityEvent onUpOverride;
        public UnityEvent onDownOverride;

        [Space]

        public UnityEvent onEastOverride;
        public UnityEvent onSouthOverride;
        public UnityEvent onWestOverride;
        public UnityEvent onNorthOverride;


        public void OnHover()
        {
            onHover?.Invoke();
        }
        public void OnUnhover()
        {
            onUnhover?.Invoke();
        }

        public void OnLeft()
        {
            if ((onLeftOverride != null) && (onLeftOverride.GetPersistentEventCount() > 0))
            {
                onLeftOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.MoveLeft();
            }
        }

        public void OnRight()
        {
            if ((onRightOverride != null) && (onRightOverride.GetPersistentEventCount() > 0))
            {
                onRightOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.MoveRight();
            }
        }

        public void OnUp()
        {
            if ((onUpOverride != null) && (onUpOverride.GetPersistentEventCount() > 0))
            {
                onUpOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.MoveUp();
            }
        }

        public void OnDown()
        {
            if ((onDownOverride != null) && (onDownOverride.GetPersistentEventCount() > 0))
            {
                onDownOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.MoveDown();
            }
        }


        public void OnEast()
        {
            if ((onEastOverride != null) && (onEastOverride.GetPersistentEventCount() > 0))
            {
                onEastOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.CursorConfirm();
            }
        }

        public void OnSouth()
        {
            if ((onSouthOverride != null) && (onSouthOverride.GetPersistentEventCount() > 0))
            {
                onSouthOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.CursorBack();
            }
        }

        public void OnWest()
        {
            onWestOverride?.Invoke();
        }

        public void OnNorth()
        {
            if ((onNorthOverride != null) && (onNorthOverride.GetPersistentEventCount() > 0))
            {
                onNorthOverride.Invoke();
            }
            else
            {
                LibraryMenuCursor.instance.ToggleToolbar();
            }
        }

    }
}