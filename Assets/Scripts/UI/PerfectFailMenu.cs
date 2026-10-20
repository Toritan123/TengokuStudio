using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Common
{
    public class PerfectFailMenu : MonoBehaviour
    {
        public enum Options
        {
            StartOver,
            Settings,
            Quit
        }

        // TODO
        // MAKE OPTIONS ACCEPT MOUSE INPUT

        [SerializeField] SettingsDialog settingsDialog;
        [SerializeField] Animator failTextAnimator;
        [SerializeField] Animator doorsAnimator;
        [SerializeField] GameObject optionArrow;
        [SerializeField] GameObject optionHolder;
        [SerializeField] RawImage bg;
        [SerializeField] float bgXSpeed;

        public static bool IsPaused { get { return isPaused; } }

        private static bool isPaused = false;
        private double pauseBeat;
        private bool canPick = false;
        private bool isQuitting = false;
        private int optionSelected = 0;
        private bool canSkip = false;
        private bool skipInputLock = false;
        private bool playedJingle = false;

        Coroutine perfectCoroutine;

        int btPause, btUp, btDown, btConfirm;

        IEnumerator StartOptions()
        {
            yield return null;
            canPick = true;
            ChooseCurrentOption();
        }

        IEnumerator YouMissed()
        {
            Conductor.instance.Pause();

            yield return new WaitForSeconds(0.75f);

            GameManager.instance.CircleCursor.LockCursor(true);
            pauseBeat = Conductor.instance.songPositionInBeatsAsDouble;

            bool noSlam = false;
            if (!PersistentDataManager.gameSettings.perfectNoSlam)
            {
                doorsAnimator.Play("slam");
            }
            else
            {
                doorsAnimator.Play("nonSlam");
                noSlam = true;
            }
            
            yield return new WaitUntil(() => doorsAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !doorsAnimator.IsInTransition(0));

            yield return new WaitForSeconds(0.5f);

            canSkip = true;
            if (!noSlam) yield return new WaitForSeconds(1.5f);

            SoundByte.PlayOneShot("perfectFail");
            playedJingle = true;
            yield return new WaitForSeconds(0.136f);

            failTextAnimator.Play("reveal1");
            yield return new WaitForSeconds(0.45454f);

            failTextAnimator.Play("reveal2");
            yield return new WaitForSeconds(0.45454f);

            failTextAnimator.Play("reveal3");
            yield return new WaitForSeconds(2f);

            failTextAnimator.Play("move");
            yield return new WaitUntil(() => failTextAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !failTextAnimator.IsInTransition(0));

            yield return null;

            canSkip = false;
            isPaused = true;
            canPick = false;
            optionSelected = 0;
            StartCoroutine(StartOptions());
        }

        void UnPause(bool instant = false)
        {
            if ((!instant) && (!Conductor.instance.isPaused)) return;
            // GameManager.instance.CircleCursor.LockCursor(true);
            Conductor.instance.Play(pauseBeat);

            failTextAnimator.Play("idle");
            doorsAnimator.Play("doorIdle");



            isPaused = false;
            canPick = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            isPaused = false;
            isQuitting = false;
        }

        public void PerfectFail()
        {
            perfectCoroutine = StartCoroutine(YouMissed());
        }

        // Update is called once per frame
        void Update()
        {
            bg.uvRect = new Rect(bg.uvRect.position + (new Vector2(bgXSpeed, 0) * Time.deltaTime), bg.uvRect.size);
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

            if ((Input.GetKeyDown(KeyCode.Return) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btConfirm, out _)))
            {
                if (canSkip)
                {
                    skipInputLock = true;
                    canSkip = false;
                    StopCoroutine(perfectCoroutine);

                    if (!playedJingle) SoundByte.PlayOneShot("perfectFail");
                    playedJingle = false;

                    failTextAnimator.Play("ArrowAnim");
                    isPaused = true;
                    canPick = false;
                    optionSelected = 0;
                    StartCoroutine(StartOptions());
                }
            }
            else if (skipInputLock) skipInputLock = false;

            if (isPaused && canPick && !settingsDialog.IsOpen)
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
                else if ((Input.GetKeyDown(KeyCode.Return) || PlayerInput.GetInputController(1).GetActionDown(PlayerInput.CurrentControlStyle, btConfirm, out _)) && !skipInputLock)
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
                SoundByte.PlayOneShot("ui/UIOption");
        }

        void UseOption(Options option)
        {
            switch (option)
            {
                case Options.StartOver:
                    OnRestart();
                    break;
                case Options.Settings:
                    OnSettings();
                    SoundByte.PlayOneShot("ui/UISelect");
                    break;
                case Options.Quit:
                    OnQuit();
                    break;
            }
        }

        void OnRestart()
        {
            UnPause(true);
            GlobalGameManager.ForceFade(0, 0f, -1f);
            GameManager.instance.Stop(0, true, 1f);
            SoundByte.PlayOneShot("ui/UIEnter");
        }

        void OnQuit()
        {
            isQuitting = true;
            SoundByte.PlayOneShot("ui/PauseQuit");
            GameManager.instance.CircleCursor.LockCursor(false);

            GlobalGameManager.playbackSpeed = 1.0f;
            GlobalGameManager.autoplay = false;

            if (GlobalGameManager.returnToLibrary)
            {
                GlobalGameManager.transitioningToLibraryFromGameplay = true;
                GlobalGameManager.LoadScene("Library", 0, 0.35f, callback: GameManager.instance.DestroyGame);
            }
            else
            {
                GlobalGameManager.LoadScene("Title", 0, 0.35f, callback: GameManager.instance.DestroyGame);
            }


        }

        void OnSettings()
        {
            GameManager.instance.CircleCursor.LockCursor(false);
            settingsDialog.SwitchSettingsDialog();
        }
    }
}
