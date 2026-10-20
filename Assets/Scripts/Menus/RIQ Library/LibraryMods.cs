using System;
using System.Collections;
using System.Collections.Generic;
using HeavenStudio.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HeavenStudio.Common;

namespace HeavenStudio
{
    public class LibraryMods : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image tempoMultImage;
        [SerializeField] private TMP_Text tempoMultText;
        [SerializeField] private RectTransform tempoControlHandle;
        [SerializeField] private RectTransform[] tempoControlInteractArea;
        [SerializeField] private float tempoControlRadius;
        [SerializeField] private Image tempoModGraphic;
        [SerializeField] private TMP_Text tempoModText;
        [SerializeField] private Image autoplayIcon;
        [SerializeField] private GameObject autoplayMod;
        [SerializeField] LibraryPopup popup;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] LibrarySelectable tempoSelectable;

        [Header("Assets")]
        [SerializeField] private Sprite tempoNeutralSprite;
        [SerializeField] private Sprite tempoExtremeSprite;
        [SerializeField] private Color tempoUpColor;
        [SerializeField] private Color tempoNeutralColor;
        [SerializeField] private Color tempoDownColor;
        [SerializeField] private Sprite autoplayEnable;
        [SerializeField] private Sprite autoplayDisable;

        [Header("Vars")]
        [SerializeField] private int scrollIndex;
        [SerializeField] public int tempoMult;
        [SerializeField] private bool adjustingTempo;
        [SerializeField] private Vector3 lastMousePos;
        [SerializeField] public bool autoplayOn;

        public void SetTempoModIcon()
        {
            if (tempoMult < 100)
            {
                tempoModGraphic.color = tempoDownColor;
                tempoModText.text = Localization.T("TEMPO DOWN!");
                tempoModGraphic.transform.parent.gameObject.SetActive(true);

                tempoMultImage.sprite = tempoExtremeSprite;
                tempoMultImage.color = tempoDownColor;
            }
            else if (tempoMult > 100)
            {
                tempoModGraphic.color = tempoUpColor;
                tempoModText.text = Localization.T("TEMPO UP!");
                tempoModGraphic.transform.parent.gameObject.SetActive(true);

                tempoMultImage.sprite = tempoExtremeSprite;
                tempoMultImage.color = tempoUpColor;
            }
            else
            {
                tempoModGraphic.transform.parent.gameObject.SetActive(false);

                tempoMultImage.sprite = tempoNeutralSprite;
                tempoMultImage.color = tempoNeutralColor;
            }
        }

        private void SetTempoMult()
        {
            tempoMult = (5 * scrollIndex) + 50;
            tempoMultText.text = tempoMult.ToString() + "%";

            float targetAngle;

            if (scrollIndex <= 10)
            {
                tempoControlHandle.transform.rotation = Quaternion.Euler(0, 0, -((14 * scrollIndex) - 140));
                targetAngle = 230 - (14 * scrollIndex);
            }
            else
            {
                tempoControlHandle.transform.rotation = Quaternion.Euler(0, 0, -(((14f / 3f) * scrollIndex) - (233333f / 5000f)));
                targetAngle = -((140f / 30f) * scrollIndex) + (1400f / 30f) + 90;
            }

            tempoControlHandle.localPosition = new Vector3(
                tempoControlRadius * Mathf.Cos(targetAngle * Mathf.Deg2Rad),
                tempoControlRadius * Mathf.Sin(targetAngle * Mathf.Deg2Rad),
                tempoControlHandle.localPosition.z);

            // -140 to +140

            LibraryManager.instance.bgmSource.pitch = tempoMult / 100f;

            SetTempoModIcon();
        }

        public void ShowPopup()
        {
            popup.ShowPopup();
            LibraryMenuCursor.instance.CursorSelect(tempoSelectable, true);
            LibraryMenuCursor.instance.currentPopup = popup;
        }

        void Start()
        {
            SetTempoMult();
        }

        public void ToggleAutoplay()
        {
            if (autoplayOn)
            {
                autoplayOn = false;
                autoplayIcon.sprite = autoplayDisable;
                autoplayMod.SetActive(false);
            }
            else
            {
                autoplayOn = true;
                autoplayIcon.sprite = autoplayEnable;
                autoplayMod.SetActive(true);
            }
        }

        void Update()
        {
            if (!adjustingTempo && UiUtils.MouseInAnyRectTransform(tempoControlInteractArea, LibraryManager.instance.cam))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    adjustingTempo = true;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    scrollIndex = 10;
                    SetTempoMult();
                }
            }

            if (adjustingTempo)
            {
                if (Input.GetMouseButton(0))
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentCanvas.transform as RectTransform,
                        Input.mousePosition, parentCanvas.worldCamera,
                        out Vector2 cursorPos);

                    Vector3 mousePos = parentCanvas.transform.TransformPoint(cursorPos) - tempoMultImage.transform.position;

                    if (mousePos != lastMousePos)
                    {
                        lastMousePos = mousePos;

                        float angle = -(Mathf.Atan2(mousePos.x, mousePos.y) * Mathf.Rad2Deg);
                        // print($"cursor x: {mousePos.x}     cursor y: {mousePos.y}     angle:{angle}");

                        angle = Mathf.Clamp(angle, -141, 141);

                        if (angle >= 0)
                        {
                            scrollIndex = (int)Math.Round((-0.0714286f * angle) + 10f, 0, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            scrollIndex = (int)Math.Round((-0.214286 * angle) + 10f, 0, MidpointRounding.AwayFromZero);
                        }

                        SetTempoMult();
                    }
                }
                else
                {
                    adjustingTempo = false;
                }
            }
        }

        public void ScrollLeft()
        {
            if (scrollIndex > 0) scrollIndex--;
            SetTempoMult();
        }

        public void ScrollRight()
        {
            if (scrollIndex < 40) scrollIndex++;
            SetTempoMult();
        }
    }
}
