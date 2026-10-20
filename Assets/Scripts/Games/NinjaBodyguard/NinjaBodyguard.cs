using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ninjaBodyguard", "Ninja Bodyguard", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("start interval", "Start Interval")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out NinjaBodyguard instance)) {
                            instance.StartInterval(e.beat, e.length, e["auto"], e["flash"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("flash", true, "Auto Black Flash", "Toggle if the flash should be handled automatically."),
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval."),
                    },
                    defaultLength = 8f,
                    resizable = true,
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out NinjaBodyguard instance)) {
                            instance.PassTurn(e.beat, flash: e["flash"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("flash", true, "Auto Black Flash", "Toggle if the flash should be handled automatically."),
                    },
                    defaultLength = 1f,
                    //resizable = true,
                },
                new GameAction("arrow", "Arrow")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("prepare", "Prepare Shot")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("cameraPos", "Set Camera Position")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out NinjaBodyguard instance)) {
                            instance.SetCameraPos(e.beat, e["pos"], e["flash"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("pos", NinjaBodyguard.CameraPos.Enemies, "Position", "Set position of the camera."),
                        new Param("flash", true, "Black Flash", "Toggle if the flash should be handled automatically."),
                    },
                    defaultLength = 0.5f,
                },
            },
            new List<string>() { "agb", "repeat" },
            "agbninja", "en",
            new List<string>() { },
            chronologicalSortKey: 26,
            font: "KanTeiRyu"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_NinjaBodyguard;

    public class NinjaBodyguard : Minigame
    {
        [SerializeField] Animator PlayerAnim;
        [SerializeField] Animator GuideAnim;
        [SerializeField] Animator LordAnim;
        [SerializeField] EnemyNinja FirstNinja;
        [SerializeField] Arrow NinjaArrow;
        [SerializeField] GameObject LeftSceneObj;
        [SerializeField] GameObject Blackout;
        [SerializeField] ParticleSystem HitParticle;
        [SerializeField] float xDistanceEnemy;
        [SerializeField] float yDistanceEnemy;

        private bool dPad = false;
        private bool canWhiff = true;

        private bool cameraOnEnemy = false;

        private List<RiqEntity> callEntities;
        private List<EnemyNinja> Enemies = new();
        private List<Arrow> MissArrows = new();

        private float enemiesStartX;
        private float enemiesStartY;

        public enum CameraPos
        {
            Enemies,
            Cutscene,
            Player,
        }

        private List<PlayerActionEvent> nrmEvents = new();

        protected static bool IA_PadDPadPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        protected static bool IA_PadEastPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_TouchPress(out double dt)
        {
            return PlayerInput.GetSlide(out dt);
        }

        public static PlayerInput.InputAction InputAction_Nrm =
            new("AgbNinjaEast", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadEastPress, IA_TouchPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Pad =
            new("AgbNinjaPad", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadDPadPress, IA_TouchPress, IA_BatonBasicPress);


        private void Awake()
        {
            enemiesStartX = FirstNinja.transform.localPosition.x;
            enemiesStartY = FirstNinja.transform.localPosition.y;

            FirstNinja.gameObject.SetActive(false);
            Blackout.SetActive(false);

            LordAnim.DoScaledAnimationAsync("Stay", 0.5f);

            double beat = Conductor.instance.songPositionInBeatsAsDouble; 
            var closeIntervals = EventCaller.GetAllInGameManagerList("ninjaBodyguard", new string[] { "start interval" }).FindAll(x => x.beat >= beat && x.beat < beat + 1.5);
            if (closeIntervals.Count > 0)
            {
                cameraOnEnemy = true;
                GameCamera.AdditionalPosition = new Vector3(-50f, 0, 0);
            }
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm) && !dPad && canWhiff)
            {
                SetInputs(true);
                PlayerAnim.DoScaledAnimationAsync("NinjaSwingR", 0.5f);
                GuideAnim.DoScaledAnimationAsync("Right", 0.5f);
                SoundByte.PlayOneShotGame("ninjaBodyguard/SE_AGB_TONO_EN_FURU", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 101), false));
            }
            if (PlayerInput.GetIsAction(InputAction_Pad) && !IsExpectingInputNow(InputAction_Pad) && dPad && canWhiff)
            {
                SetInputs(false);
                PlayerAnim.DoScaledAnimationAsync("NinjaSwingL", 0.5f);
                GuideAnim.DoScaledAnimationAsync("Left", 0.5f);
                SoundByte.PlayOneShotGame("ninjaBodyguard/SE_AGB_TONO_EN_FURU", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-200, 101), false));
            }
        }

        public override void OnGameSwitch(double beat)
        {
            StoreAllCallEntities();
            DoInactiveStartInterval(beat, false);
        }

        public override void OnPlay(double beat)
        {
            StoreAllCallEntities();
            DoInactiveStartInterval(beat, true);
        }

        private void StoreAllCallEntities()
        {
            callEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "ninjaBodyguard/arrow");
        }

        private void DoInactiveStartInterval(double beat, bool fromPlay)
        {
            RiqEntity startIntervalEntity = gameManager.Beatmap.Entities.FindLast(e => (fromPlay ? e.beat : e.beat - 2) < beat && e.beat + e.length >= beat && e.datamodel == "ninjaBodyguard/start interval");
            if (startIntervalEntity != null)
            {
                RiqEntity e = startIntervalEntity;
                StartInterval(e.beat, e.length, e["auto"], e["flash"]);
            }
        }

        public void StartInterval(double beat, float length, bool autoPass, bool flash)
        {
            List<RiqEntity> neededCalls = GetNeededCalls(beat, length);
            if (neededCalls.Count <= 0) return;

            if (Enemies.Count > 0)
            {
                foreach (var ninja in Enemies)
                {
                    Destroy(ninja.gameObject);
                }
            }

            Enemies = SpawnNinjas(neededCalls.Count);

            List<RiqEntity> preparesEntities = gameManager.Beatmap.Entities.FindAll(e => e.datamodel == "ninjaBodyguard/prepare" && e.beat >= beat && e.beat < beat + length);

            List<MultiSound.Sound> sounds = new();
            List<BeatAction.Action> actions = new();

            actions.Add(new(beat, () => {
                if (flash) Flash(beat, true);
                GameCamera.AdditionalPosition = new Vector3(-50f, 0, 0);
                cameraOnEnemy = true;
                canWhiff = false;
            }));

            foreach (RiqEntity prepare in preparesEntities)
            {
                actions.Add(new(prepare.beat, () =>
                {
                    foreach (var ninja in Enemies)
                    {
                        ninja.PrepareArrow();
                    }
                }));
            }

            int ninjaIndex = 0;

            foreach (RiqEntity call in neededCalls)
            {
                int currentNinja = ninjaIndex;
                float pitch = SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-100, 1), false);

                sounds.Add(new("ninjaBodyguard/SE_AGB_TONO_EN_YUMI", call.beat));
                sounds.Add(new("ninjaBodyguard/SE_AGB_TONO_EN_YUMI_PITCH", call.beat, pitch: pitch));
                actions.Add(new(call.beat, () => { Enemies[currentNinja].ShootArrow(); }));

                ninjaIndex++;
            }

            if (autoPass)
            {
                actions.Add(new(beat + length, delegate {
                    PassTurn(beat + length, beat, length, neededCalls, flash);
                }));
            }

            _ = MultiSound.Play(sounds);
            _ = BeatAction.New(this, actions);
        }

        public void PassTurn(double beat, double startIntervalBeat = double.NaN, float startIntervalLength = float.NaN, List<RiqEntity> neededCalls = null, bool flash = true)
        {
            if (double.IsNaN(startIntervalBeat) || double.IsNaN(startIntervalLength))
            {
                RiqEntity startInterval = gameManager.Beatmap.Entities.FindLast(e => e.datamodel == "ninjaBodyguard/start interval" && e.beat + e.length < beat);
                if (startInterval == null) return;
                startIntervalBeat = startInterval.beat;
                startIntervalLength = startInterval.length;
            }
            neededCalls ??= GetNeededCalls(startIntervalBeat, startIntervalLength);
            if (neededCalls.Count <= 0) return;

            PlayerAnim.DoScaledAnimationAsync("Idle", 0.5f);
            GuideAnim.DoScaledAnimationAsync("Left", 0.5f);
            SetInputs(false);

            LordAnim.DoScaledAnimationAsync("Stay", 0.5f);

            if (MissArrows.Count > 0)
            {
                foreach (var arrow in MissArrows)
                {
                    Destroy(arrow.gameObject);
                }
            }

            if (flash) Flash(beat, false);
            GameCamera.AdditionalPosition = new Vector3(0, 0, 0);
            cameraOnEnemy = false;
            canWhiff = true;

            foreach (RiqEntity call in neededCalls)
            {
                double relativeBeat = call.beat - startIntervalBeat;
                nrmEvents.Add(ScheduleInput(beat, relativeBeat, InputAction_Nrm, OnHit, OnMiss, null));
            }
        }

        private void Flash(double beat, bool cameraEnemy)
        {
            if (cameraEnemy == cameraOnEnemy) return;

            Blackout.SetActive(true);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate {
                    Blackout.SetActive(false);
                }),
            });
        }

        public void SetCameraPos(double beat, int pos, bool flash)
        {
            switch (pos)
            {
                case (int)CameraPos.Enemies:
                    var camPos = new Vector3(-50f, 0, 0);
                    if (flash) Flash(beat, true);
                    GameCamera.AdditionalPosition = camPos;
                    cameraOnEnemy = true;
                    canWhiff = false;
                    return;
                case (int)CameraPos.Cutscene:
                    camPos = new Vector3(-25f, 0, 0);
                    if (flash) Flash(beat, true);
                    GameCamera.AdditionalPosition = camPos;
                    cameraOnEnemy = false;
                    canWhiff = false;
                    return;
                case (int)CameraPos.Player:
                    camPos = new Vector3(0f, 0, 0);
                    if (flash) Flash(beat, true);
                    GameCamera.AdditionalPosition = camPos;
                    cameraOnEnemy = false;
                    canWhiff = true;
                    return;
                default:
                    return;
            }
        }

        private List<RiqEntity> GetNeededCalls(double beat, float length)
        {
            return callEntities.FindAll(e => e.beat >= beat && e.beat <= beat + length);
        }

        private List<EnemyNinja> SpawnNinjas(int count) 
        {
            List<EnemyNinja> ninjas = new();

            float currentX = xDistanceEnemy;
            float currentY = yDistanceEnemy;
            if (count > 6)
            {
                float divRatio = 6f / count;
                currentX = xDistanceEnemy * divRatio;
                currentY = yDistanceEnemy * divRatio;
            }

            Vector3 firstNinjaPos = new Vector3(enemiesStartX + (currentX / 2) - (currentX / 2 * count), enemiesStartY + (currentY / 2) - (currentY / 2 * count));

            for (int i = 0; i < count; i++)
            {
                EnemyNinja newNinja = Instantiate(FirstNinja, transform);
                newNinja.transform.SetParent(LeftSceneObj.transform);
                newNinja.transform.localPosition = new Vector3(firstNinjaPos.x + currentX * i, firstNinjaPos.y + currentY * i);
                newNinja.SetOrder(i);
                newNinja.gameObject.SetActive(true);
                ninjas.Add(newNinja);
            }

            return ninjas;
        }

        private void SetInputs(bool setDPad)
        {
            dPad = setDPad;
            if (nrmEvents.Count == 0) return;
            if (setDPad)
            {
                foreach (var input in nrmEvents)
                {
                    input.SetInputAction(InputAction_Pad);
                }
            }
            else
            {
                foreach (var input in nrmEvents)
                {
                    input.SetInputAction(InputAction_Nrm);
                }
            }
        }

        private void OnHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                PlayerAnim.DoScaledAnimationAsync(dPad ? "NinjaSwingL" : "NinjaSwingR", 0.5f);
                GuideAnim.DoScaledAnimationAsync(dPad ? "Left" : "Right", 0.5f);
                SoundByte.PlayOneShotGame("ninjaBodyguard/SE_AGB_TONO_EN_KIN");

                Arrow newArrow = Instantiate(NinjaArrow, transform);
                newArrow.currentState = dPad ? Arrow.ArrowState.DivertL : Arrow.ArrowState.DivertR;
                newArrow.gameObject.SetActive(true);
            }
            else
            {
                PlayerAnim.DoScaledAnimationAsync(dPad ? "NinjaCutL" : "NinjaCutR", 0.5f);
                GuideAnim.DoScaledAnimationAsync(dPad ? "Left" : "Right", 0.5f);
                SoundByte.PlayOneShotGame("ninjaBodyguard/SE_AGB_TONO_EN_HIT");
                HitParticle.Play();

                Arrow newArrow = Instantiate(NinjaArrow, transform);
                newArrow.currentState = Arrow.ArrowState.Destroy;
                newArrow.gameObject.SetActive(true);
            }

            SetInputs(!dPad);
        }

        private void OnMiss(PlayerActionEvent caller)
        {
            LordAnim.DoScaledAnimationAsync("Shock", 0.5f);
            SoundByte.PlayOneShotGame("ninjaBodyguard/SE_AGB_TONO_EN_MISS");

            Arrow newArrow = Instantiate(NinjaArrow, transform);
            newArrow.currentState = Arrow.ArrowState.Hit;
            newArrow.gameObject.SetActive(true);

            MissArrows.Add(newArrow);
        }
    }
}