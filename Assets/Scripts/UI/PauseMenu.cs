using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using UnityEngine.UI;

namespace HeavenStudio.Common
{
    public class PauseMenu : MonoBehaviour
    {
        public enum Options
        {
            Continue,
            StartOver,
            Settings,
            Quit
        }

        // TODO
        // MAKE OPTIONS ACCEPT MOUSE INPUT

        [SerializeField] float patternSpeed = 1f;
        [SerializeField] SettingsDialog settingsDialog;
        [SerializeField] Animator animator;
        [SerializeField] TMP_Text chartTitleText;
        [SerializeField] TMP_Text chartArtistText;
        [SerializeField] TMP_Text chartCurrentBpm;
        [SerializeField] TMP_Text chartTempoXX;
        [SerializeField] GameObject optionArrow;
        [SerializeField] GameObject optionHolder;
        [SerializeField] GameObject tempoXXHolder;
        [SerializeField] Image tempoXXGraphic;

        [SerializeField] RectTransform patternL;
        [SerializeField] RectTransform patternR;

        public static bool IsPaused { get { return isPaused; } }

        private static bool isPaused = false;
        private double pauseBeat;
        private bool canPick = false;
        private bool isQuitting = false;
        private int optionSelected = 0;

        private Color tempoUpColor = "#EC7479".Hex2RGB();
        private Color tempoDownColor = "#7484FB".Hex2RGB();

        int btPause, btUp, btDown, btConfirm;

        void Pause()
        {
            if (GlobalGameManager.IsShowingDialog) return;
            if (!Conductor.instance.isPlaying) return;
            GameManager.instance.CircleCursor.LockCursor(true);
            GameManager.instance.Pause();
            pauseBeat = Conductor.instance.songPositionInBeatsAsDouble;
            chartTitleText.text = GameManager.instance.Beatmap["remixtitle"].ToString();
            chartArtistText.text = GameManager.instance.Beatmap["remixauthor"].ToString();
            chartCurrentBpm.text = $"\u266A={Conductor.instance.songBpm.ToString()}";

            tempoXXGraphic.color = GlobalGameManager.playbackSpeed > 1f ? tempoUpColor : tempoDownColor;
            chartTempoXX.text = Mathf.RoundToInt(GlobalGameManager.playbackSpeed * 100).ToString() + "%";
            tempoXXHolder.gameObject.SetActive(GlobalGameManager.playbackSpeed != 1f);

            animator.Play("PauseShow");
            SoundByte.PlayOneShot("ui/PauseIn", ui: true);

            isPaused = true;
            canPick = false;
            optionSelected = 0;
            ChooseCurrentOption();
        }

        void UnPause(bool instant = false)
        {
            if ((!instant) && (!Conductor.instance.isPaused)) return;
            StartCoroutine(UnPauseCo(instant));
        }

        IEnumerator UnPauseCo(bool instant = false)
        {
            yield return null; // wait a frame so the unpause input doesn't also input in-game
            
            // GameManager.instance.CircleCursor.LockCursor(true);
            if (instant)
            {
                animator.Play("NoPose");
            }
            else
            {
                animator.Play("PauseHide");
                SoundByte.PlayOneShot("ui/PauseOut", ui: true);
                GameManager.instance.Play(pauseBeat);
            }

            isPaused = false;
            canPick = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            isPaused = false;
            isQuitting = false;
        }

        // Update is called once per frame
        void Update()
        {
            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    btPause = (int)InputController.ActionsTouch.Pause;
                    btConfirm = (int)InputController.ActionsTouch.Tap;
                    btUp = -1;
                    btDown = -1;
                    break;
                case InputController.ControlStyles.Baton:
                    btPause = (int)InputController.ActionsBaton.Pause;
                    btUp = (int)InputController.ActionsBaton.Up;
                    btDown = (int)InputController.ActionsBaton.Down;
                    btConfirm = (int)InputController.ActionsBaton.Face;
                    break;
                default:
                    btPause = (int)InputController.ActionsPad.Pause;
                    btUp = (int)InputController.ActionsPad.Up;
                    btDown = (int)InputController.ActionsPad.Down;
                    btConfirm = (int)InputController.ActionsPad.East;
                    break;
            }

            if (isQuitting) return;

            if (PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btPause, out _) && !settingsDialog.IsOpen)
            {
                if (isPaused)
                {
                    UnPause();
                }
                else
                {
                    Pause();
                }
            }
            else if (isPaused && canPick && !settingsDialog.IsOpen)
            {
                if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                {
                    foreach (Transform t in optionHolder.transform)
                    {
                        if (t.TryGetComponent<Collider2D>(out Collider2D c) && c.OverlapPoint(PlayerInput.GetInputController(1).GetPointer()))
                        {
                            int idx = t.GetSiblingIndex();
                            ChooseOption((Options)idx, idx != optionSelected);
                            optionSelected = idx;
                            break;
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btUp, out _))
                {
                    optionSelected--;
                    if (optionSelected < 0)
                    {
                        optionSelected = optionHolder.transform.childCount - 1;
                    }
                    ChooseOption((Options)optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btDown, out _))
                {
                    optionSelected++;
                    if (optionSelected > optionHolder.transform.childCount - 1)
                    {
                        optionSelected = 0;
                    }
                    ChooseOption((Options)optionSelected);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btConfirm, out _))
                {
                    if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                    {
                        if (optionHolder.transform.GetChild(optionSelected).GetComponent<Collider2D>().OverlapPoint(PlayerInput.GetInputController(1).GetPointer()))
                        {
                            UseOption((Options)optionSelected);
                        }
                    }
                    else
                    {
                        UseOption((Options)optionSelected);
                    }
                }
            }

            if (isPaused)
            {
                patternL.anchoredPosition = new Vector2((Time.realtimeSinceStartup * patternSpeed) % 13, patternL.anchoredPosition.y);
                patternR.anchoredPosition = new Vector2(-(Time.realtimeSinceStartup * patternSpeed) % 13, patternR.anchoredPosition.y);
            }
        }

        public void ChooseCurrentOption()
        {
            ChooseOption((Options)optionSelected, false);
            canPick = true;
        }

        public void ChooseOption(Options option, bool sound = true)
        {
            optionArrow.transform.position = new Vector3(optionArrow.transform.position.x, optionHolder.transform.GetChild((int)option).position.y, optionArrow.transform.position.z);
            foreach (Transform child in optionHolder.transform)
            {
                child.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            optionHolder.transform.GetChild((int)option).transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            if (sound)
                SoundByte.PlayOneShot("ui/UIOption", ui: true);
        }

        void UseOption(Options option)
        {
            switch (option)
            {
                case Options.Continue:
                    OnContinue();
                    break;
                case Options.StartOver:
                    OnRestart();
                    break;
                case Options.Settings:
                    OnSettings();
                    SoundByte.PlayOneShot("ui/UISelect", ui: true);
                    break;
                case Options.Quit:
                    OnQuit();
                    break;
            }
        }

        void OnContinue()
        {
            UnPause();
        }

        void OnRestart()
        {
            GlobalGameManager.ForceFade(0, 0f, -1f);
            GameManager.instance.Stop(0, true, 1f);
            SoundByte.PlayOneShot("ui/UIEnter", ui: true);
            UnPause(true);
        }

        void OnQuit()
        {
            isQuitting = true;
            SoundByte.PlayOneShot("ui/PauseQuit", ui: true);
            GameManager.instance.CircleCursor.LockCursor(false);

            GlobalGameManager.playbackSpeed = 1.0f;
            GlobalGameManager.autoplay = false;

            if (GlobalGameManager.returnToLibrary)
            {
                GlobalGameManager.transitioningToLibraryFromGameplay = true;
                GlobalGameManager.LoadScene("Library", 0, 0.35f, callback: GameManager.instance.EndRemixEvents);
            }
            else
            {
                GlobalGameManager.LoadScene("Title", 0, 0.35f, callback: GameManager.instance.EndRemixEvents);
            }


        }

        void OnSettings()
        {
            GameManager.instance.CircleCursor.LockCursor(false);
            settingsDialog.SwitchSettingsDialog();
        }
    }
}
