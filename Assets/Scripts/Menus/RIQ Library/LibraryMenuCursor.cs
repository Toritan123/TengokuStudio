using System.Collections.Generic;
using System.Linq;
using HeavenStudio.InputSystem;
using HeavenStudio.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HeavenStudio
{
    public class LibraryMenuCursor : MonoBehaviour
    {
        public static LibraryMenuCursor instance;
        [SerializeField] public LibrarySelectable[] toolbarSelectables;
        [SerializeField] public bool canAct;
        [SerializeField] public bool editingText;
        [SerializeField] private RectTransform selectedDisplayRect;
        public LibrarySelectable defaultToolbarSelectable;

        [SerializeField] public LibrarySelectable gridSelectable;
        [SerializeField] public LibraryPopup currentPopup;
        [SerializeField] private List<LibrarySelectable> selectableStack;

        [SerializeField] public List<TMP_InputField> textInputs;


        public LibrarySelectable currentSelectable, lastSelectable;
        private RectTransform currentSelectableRect, lastSelectableRect;
        private float selectableLerpTimer;

        public static bool usingMouse;
        bool waitingForButtonUp;
        public bool levelsInGrid;

        private void Start()
        {
            instance = this;
            // LibraryMenuCursor.instance.SetSelectable(LibraryMenuCursor.instance.defaultToolbarSelectable);

        }

        void Update()
        {
            InputController controller = PlayerInput.GetInputController(1);
            if (!(LibraryManager.instance.exiting || GlobalGameManager.IsShowingDialog || waitingForButtonUp || !canAct))
            {

                UpdateSelectable(controller);

            }
            if (waitingForButtonUp)
            {
                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                {
                    if (controller.GetActionUp(PlayerInput.CurrentControlStyle, (int)InputController.ActionsPad.East, out _))
                    {
                        waitingForButtonUp = false;
                    }
                }
            }
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                usingMouse = true;
            }
        }

        void UpdateSelectable(InputController controller)
        {
            selectableLerpTimer += Time.deltaTime;

            if ((currentSelectableRect != null) && (lastSelectableRect != null))
            {
                if (selectableLerpTimer < 0.2f)
                {
                    float prog = Mathf.Clamp01(selectableLerpTimer / 0.2f);
                    prog = 1f - Mathf.Pow(1f - prog, 3);
                    selectedDisplayRect.position = Vector3.Lerp(lastSelectableRect.TransformPoint(lastSelectableRect.rect.center), currentSelectableRect.TransformPoint(currentSelectableRect.rect.center), prog);

                    Vector2 lastSize = lastSelectableRect.sizeDelta;
                    if (lastSelectable.adjustWithScale) { lastSize *= lastSelectableRect.localScale; }

                    Vector2 currentSize = currentSelectableRect.sizeDelta;
                    if (currentSelectable.adjustWithScale) { currentSize *= currentSelectableRect.localScale; }

                    selectedDisplayRect.sizeDelta = Vector2.Lerp(lastSize, currentSize, prog);
                }
                else
                {
                    selectedDisplayRect.position = currentSelectableRect.TransformPoint(currentSelectableRect.rect.center);

                    Vector2 currentSize = currentSelectableRect.sizeDelta;
                    if (currentSelectable.adjustWithScale) { currentSize *= currentSelectableRect.localScale; }

                    selectedDisplayRect.sizeDelta = currentSize;
                }
            }

            bool editingText = false;
            foreach (TMP_InputField field in textInputs)
            {
                if (field.isFocused)
                {
                    editingText = true;
                }
            }
            

            selectedDisplayRect.gameObject.SetActive(!usingMouse);

            if ((PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) && (!editingText))
            {
                switch (controller.GetLastActionDown())
                {
                    case (int)InputController.ActionsPad.East:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnEast();
                        }
                        break;
                    case (int)InputController.ActionsPad.South:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnSouth();
                        }
                        break;
                    case (int)InputController.ActionsPad.West:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnWest();
                        }
                        break;
                    case (int)InputController.ActionsPad.North:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnNorth();
                        }
                        break;
                    case (int)InputController.ActionsPad.Left:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnLeft();
                        }
                        break;
                    case (int)InputController.ActionsPad.Right:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnRight();
                        }
                        break;
                    case (int)InputController.ActionsPad.Up:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnUp();
                        }
                        break;
                    case (int)InputController.ActionsPad.Down:
                        if (currentSelectable != null)
                        {
                            DisableMouse();
                            currentSelectable.OnDown();
                        }
                        break;
                }
            }
            // foreach (var selectable in activeSelectables)
            // {
            //     // get button the mouse is hovering over
            //     if (RectTransformUtility.RectangleContainsScreenPoint(selectable.GetComponent<RectTransform>(), Input.mousePosition, Camera.main))
            //     {
            //         if (mouseSelectable != selectable)
            //         {
            //             mouseSelectable = selectable;
            //             usingMouse = true;
            //         }
            //         if (currentSelectable != selectable && usingMouse)
            //         {
            //             currentSelectable = selectable;
            //             SetSelectableRectTarget(currentSelectable);
            //             // SoundByte.PlayOneShot("ui/UIOption");
            //             break;
            //         }
            //     }
            // }

        }

        void SetSelectableRectTarget(LibrarySelectable selectable)
        {
            if (selectable == null)
            {
                selectedDisplayRect.gameObject.SetActive(false);
                return;
            }
            selectedDisplayRect.gameObject.SetActive(true);
            lastSelectableRect = currentSelectableRect;
            currentSelectableRect = selectable.GetComponent<RectTransform>();

            if (lastSelectableRect == null)
            {
                lastSelectableRect = currentSelectableRect;
            }
            selectedDisplayRect.position = lastSelectableRect.TransformPoint(lastSelectableRect.rect.center);

            if (lastSelectable.adjustWithScale)
            {
                selectedDisplayRect.sizeDelta = lastSelectableRect.sizeDelta * lastSelectableRect.localScale;
            }
            else
            {
                selectedDisplayRect.sizeDelta = lastSelectableRect.sizeDelta;
            }

            selectableLerpTimer = 0;
        }

        /// <summary>
        /// Move the rect
        /// </summary>
        /// <param name="selectable">Rect to move to</param>
        public void SetSelectable(LibrarySelectable selectable)
        {
            if (currentSelectable == null)
            {
                currentSelectable = selectable;
            }
            lastSelectable = currentSelectable;
            currentSelectable = selectable;

            lastSelectable.OnUnhover();
            currentSelectable.OnHover();

            selectedDisplayRect.gameObject.SetActive(true);
            canAct = true;
            SetSelectableRectTarget(currentSelectable);
        }

        /// <summary>
        /// jump to a new rect via button press
        /// </summary>
        /// <param name="selectable">the new rect</param>
        /// <param name="addPreviousToStack"></param>
        public void CursorSelect(LibrarySelectable selectable, bool addPreviousToStack)
        {
            if (addPreviousToStack)
            {
                selectableStack.Add(currentSelectable);
            }
            SetSelectable(selectable);
            DisableMouse();
        }

        public void CursorConfirm()
        {
            if (currentSelectable != null)
            {
                if (currentSelectable.GetComponent<Button>() != null)
                {
                    // currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnLeft().GetComponent<LibrarySelectable>();
                    currentSelectable.GetComponent<Button>().onClick.Invoke();
                }
                else if (currentSelectable.GetComponent<TMP_InputField>() != null)
                {
                    // currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnLeft().GetComponent<LibrarySelectable>();
                    currentSelectable.GetComponent<TMP_InputField>().ActivateInputField();
                }
                else
                {
                    // SoundByte.PlayOneShot("ui/UISelect");
                }
            }
        }

        public void CursorBack()
        {
            if (currentPopup != null)
            {
                currentPopup.HidePopup();
                currentPopup = null;
            }

            if (selectableStack.Count > 0)
            {
                SetSelectable(selectableStack[selectableStack.Count - 1]);
                selectableStack.Remove(currentSelectable);
                currentSelectable.OnUnhover();
            }
            SoundByte.PlayOneShot("ui/ExitSmall");
        }

        public void ToggleToolbar()
        {
            if (toolbarSelectables.Contains(currentSelectable) && levelsInGrid)
            {
                SetSelectable(gridSelectable);
            }
            else if (currentSelectable.TryGetComponent(out LibraryLevelIcon _) || currentSelectable.TryGetComponent(out LibraryFolderIcon _))
            {
                SetSelectable(defaultToolbarSelectable);
            }

        }

        public void MoveLeft()
        {
            if (currentSelectable != null)
            {
                if (currentSelectable.GetComponent<Selectable>()?.FindSelectableOnLeft() != null)
                {
                    currentSelectable.OnUnhover();
                    currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnLeft().GetComponent<LibrarySelectable>();
                    // currentSelectable.GetComponent<Selectable>()?.Select();
                    SetSelectableRectTarget(currentSelectable);
                    currentSelectable.OnHover();
                    SoundByte.PlayOneShot("ui/UIOption");
                }
                else
                {
                    // SoundByte.PlayOneShot("ui/UISelect");
                }
            }
        }

        public void MoveRight()
        {
            if (currentSelectable != null)
            {
                if (currentSelectable.GetComponent<Selectable>()?.FindSelectableOnRight() != null)
                {
                    currentSelectable.OnUnhover();
                    currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnRight().GetComponent<LibrarySelectable>();
                    // currentSelectable.GetComponent<Selectable>()?.Select();
                    SetSelectableRectTarget(currentSelectable);
                    currentSelectable.OnHover();
                    SoundByte.PlayOneShot("ui/UIOption");
                }
                else
                {
                    // SoundByte.PlayOneShot("ui/UISelect");
                }
            }
        }

        public void MoveUp()
        {
            if (currentSelectable != null)
            {
                if (currentSelectable.GetComponent<Selectable>()?.FindSelectableOnUp() != null)
                {
                    currentSelectable.OnUnhover();
                    currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnUp().GetComponent<LibrarySelectable>();
                    // currentSelectable.GetComponent<Selectable>()?.Select();
                    SetSelectableRectTarget(currentSelectable);
                    currentSelectable.OnHover();
                    SoundByte.PlayOneShot("ui/UIOption");
                }
                else
                {
                    // SoundByte.PlayOneShot("ui/UISelect");
                }
            }
        }

        public void MoveDown()
        {
            if (currentSelectable != null)
            {
                if (currentSelectable.GetComponent<Selectable>()?.FindSelectableOnDown() != null)
                {
                    currentSelectable.OnUnhover();
                    currentSelectable = currentSelectable.GetComponent<Selectable>()?.FindSelectableOnDown().GetComponent<LibrarySelectable>();
                    // currentSelectable.GetComponent<Selectable>()?.Select();
                    SetSelectableRectTarget(currentSelectable);
                    currentSelectable.OnHover();
                    SoundByte.PlayOneShot("ui/UIOption");
                }
                else
                {
                    // SoundByte.PlayOneShot("ui/UISelect");
                }
            }
        }

        public void HideCursor()
        {
            selectedDisplayRect.gameObject.SetActive(false);
            canAct = false;
        }

        public void ShowCursor()
        {
            selectedDisplayRect.gameObject.SetActive(true);
            canAct = true;
        }

        public void DisableMouse()
        {
            if (usingMouse)
            {
                SetSelectable(defaultToolbarSelectable);
                usingMouse = false;
            }

        }

    }
}