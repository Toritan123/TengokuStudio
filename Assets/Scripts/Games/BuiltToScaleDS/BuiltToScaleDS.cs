using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;
using System;

using HeavenStudio.Util;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrFlickLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("builtToScaleDS", "Built to Scale", "1ad21a", false, false, new List<GameAction>()
            {
                new GameAction("spawn blocks", "Widget")
                {
                    function = delegate {var e = eventCaller.currentEntity; BuiltToScaleDS.instance.MultiplePiano(e.beat, e.length, e["silent"], e["note1"], e["note2"], e["note3"], e["note4"], e["note5"], e["note6"], e["staccato"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("silent", false, "Mute Notes", "Toggle if the piano notes in this event should be muted.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "note1", "note2", "note3", "note4", "note5", "note6"})
                        }, advanced: true),
                        new Param("note1", new EntityTypes.Note(0, 3, 4, "builtToScaleDS/Piano"), "1st note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note2", new EntityTypes.Note(2, 3, 4, "builtToScaleDS/Piano"), "2nd note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note3", new EntityTypes.Note(4, 3, 4, "builtToScaleDS/Piano"), "3rd note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note4", new EntityTypes.Note(5, 3, 4, "builtToScaleDS/Piano"), "4th note", "Set the number of semitones up or down this note should be pitched."),
                        new Param("note5", new EntityTypes.Note(7, 3, 4, "builtToScaleDS/Piano"), "5th note", "Set the number of semitones up or down this note should be pitched. This note plays together with the 6th note."),
                        new Param("note6", new EntityTypes.Note(12, 3, 4, "builtToScaleDS/Piano"), "6th note", "Set the number of semitones up or down this note should be pitched. This note plays together with the 5th note."),
                        new Param("staccato", false, "Staccato Note", "Toggle if this note should be shorter than the length of the block.")
                    }
                },
                new GameAction("play piano", "Play Note")
                {
                    function = delegate { BuiltToScaleDS.instance.PlayPiano(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["type"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", new EntityTypes.Note(0, 3, 4, "builtToScaleDS/Piano"), "Semitones", "Set the number of semitones up or down this note should be pitched.")
                    },
                    advanced = true
                },
                new GameAction("color", "Color Palette")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleDS.instance.UpdateMappingColors(e["object"], e["shooter"], e["bg"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("object", Color.white, "Widget Color", "Choose the color of the widgets & rods."),
                        new Param("shooter", Color.white, "Paddle Color", "Choose the color of the launch paddle."),
                        new Param("bg", new Color(0, 1, 0, 1), "Environment Color", "Choose the color of the environment.")
                    }
                },
                new GameAction("lights", "Lights")
                {
                    function = delegate { var e = eventCaller.currentEntity; BuiltToScaleDS.instance.Lights(e.beat, e.length, e["auto"], e["light"] && !e["auto"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("light", false, "Lights", "Toggle if the lights should activate for the duration of this event."),
                        new Param("auto", true, "Lights (Auto)", "Toggle if the lights should automatically activate until the another Lights event is reached.")
                    }
                },
                new GameAction("camera", "Camera Controls")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        // var rotation = new Vector3(0, e["valA"], 0);
                        BuiltToScaleDS.instance.ChangeCameraAngle(e.beat, e["valA"], e["valB"], e.length, (Util.EasingFunction.Ease)e["type"], e["additive"]);
                    },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>() {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Rotation", "Set the rotation of the camera around the rod."),
                        new Param("valB", new EntityTypes.Float(0.5f, 3f, 1), "Zoom", "Set the camera's level of zoom."),
                        new Param("type", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
                        new Param("additive", true, "Additive Rotation", "Toggle if the above rotation should be added to the current angle instead of setting the target angle to travel to.")
                    }
                }
            }, new List<string>() { "ntr", "normal" }, "ntrassembly", "en", new List<string>() { },
            chronologicalSortKey: 1,
            font: "Comet");
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BuiltToScaleDS;
    
    public class BuiltToScaleDS : Minigame
    {
        public enum BTSObject { HitPieces, MissPieces, FlyingRod }

        [Header("Camera")]
        [SerializeField] Transform camPos;
        [SerializeField] float cameraFoV;
        [SerializeField] Transform cameraPivot;

        [Header("References")]
        [SerializeField] SkinnedMeshRenderer environmentRenderer;
        [SerializeField] SkinnedMeshRenderer elevatorRenderer;
        public GameObject flyingRodBase;
        public GameObject movingBlocksBase;
        public GameObject hitPartsBase;
        public GameObject missPartsBase;
        public Transform partsHolder;
        public Transform blocksHolder;
        public Animator shooterAnim;
        public Animator elevatorAnim;

        [SerializeField] private Material shooterMaterial;
        [SerializeField] private Material objectMaterial;
        [SerializeField] private Material gridPlaneMaterial;
        [SerializeField] private Material elevatorMaterial;
        [SerializeField] private Material beltMaterial;
        [SerializeField] private Material[] firstPatternLights;
        [SerializeField] private Material[] secondPatternLights;

        [Header("Properties")]
        [SerializeField] float beltSpeed = 1f;

        private Material[] environmentMaterials;
        private Material[] elevatorMaterials;
        private float currentBeltOffset;
        private bool lighting = false;
        private bool autoLight = false;
        private bool firstLight = true;

        [NonSerialized] public bool shootingThisFrame;
        [NonSerialized] public bool lastShotOut = false;
        private static Color currentObjectColor = Color.white;
        private static Color currentShooterColor = Color.white;
        private static Color currentEnvironmentColor = new Color(0, 1, 0, 1);
        
        public static BuiltToScaleDS instance;

        private GameEvent lightBeat = new GameEvent();

        double cameraRotateBeat = double.MaxValue;
        double cameraRotateLength;
        Util.EasingFunction.Ease cameraRotateEase;
        float cameraRotateLast = 0, cameraScaleLast = 1;
        float cameraRotateNext = 0, cameraScaleNext = 1;

        private void Awake()
        {
            instance = this;

            GameCamera.AdditionalPosition = camPos.position + (Quaternion.Euler(camPos.eulerAngles) * Vector3.forward * 10f);
            GameCamera.AdditionalRotEuler = camPos.eulerAngles;
            GameCamera.AdditionalFoV = cameraFoV;

            // environmentMaterials = environmentRenderer.materials;
            // elevatorMaterials = elevatorRenderer.materials;
            // beltMaterial = Instantiate(environmentMaterials[8]);
            // environmentMaterials[8] = beltMaterial;
            // elevatorObjectMats = new Material[]
            // {
            //     Instantiate(elevatorMaterials[0]),
            //     Instantiate(elevatorMaterials[1]),
            //     Instantiate(elevatorMaterials[2]),
            // };
            // elevatorMaterials[0] = elevatorObjectMats[0];
            // elevatorMaterials[1] = elevatorObjectMats[1];
            // elevatorMaterials[2] = elevatorObjectMats[2];
            // elevatorMaterial = Instantiate(elevatorMaterials[3]);
            // elevatorMaterials[3] = elevatorMaterial;
            // gridMaterials = new Material[]
            // {
            //     Instantiate(environmentMaterials[9]),
            //     Instantiate(environmentMaterials[11]),
            //     Instantiate(environmentMaterials[12]),
            //     Instantiate(environmentMaterials[13]),
            //     Instantiate(environmentMaterials[14]),
            // };
            // environmentMaterials[9] = gridMaterials[0];
            // environmentMaterials[11] = gridMaterials[1];
            // environmentMaterials[12] = gridMaterials[2];
            // environmentMaterials[13] = gridMaterials[3];
            // environmentMaterials[14] = gridMaterials[4];

            // firstPatternLights = new Material[]
            // {
            //     Instantiate(environmentMaterials[1]),
            //     Instantiate(environmentMaterials[2]),
            //     Instantiate(environmentMaterials[4]),
            // };
            // environmentMaterials[1] = firstPatternLights[0];
            // environmentMaterials[2] = firstPatternLights[1];
            // environmentMaterials[4] = firstPatternLights[2];

            // secondPatternLights = new Material[]
            // {
            //     Instantiate(environmentMaterials[0]),
            //     Instantiate(environmentMaterials[3])
            // };
            // environmentMaterials[0] = secondPatternLights[0];
            // environmentMaterials[3] = secondPatternLights[1];

            elevatorAnim.Play("MakeRod", 0, 1f);
            UpdateColors();
        }

        private void OnDestroy()
        {
            currentObjectColor = Color.white;
            currentShooterColor = Color.white;
            currentEnvironmentColor = new Color(0, 1, 0, 1);
            UpdateColors();
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        public void UpdateMappingColors(Color objectColor, Color shooterColor, Color environmentColor)
        {
            currentObjectColor = objectColor;
            currentShooterColor = shooterColor;
            currentEnvironmentColor = environmentColor;
            UpdateColors();
        }

        private void PersistColors(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("builtToScaleDS", new string[] { "color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
                var lastEvent = allEventsBeforeBeat[^1];
                UpdateMappingColors(lastEvent["object"], lastEvent["shooter"], lastEvent["bg"]);
            }
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColors(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistColors(beat);
            EntityPreCheck(beat);
        }

        void EntityPreCheck(double beat)
        {
            cameraRotateBeat = double.MaxValue;
            cameraRotateLength = 0;
            cameraRotateEase = Util.EasingFunction.Ease.Linear;
            cameraRotateLast = 0; cameraScaleLast = 1;
            cameraRotateNext = 0; cameraScaleNext = 1;

            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.beat < beat && c.datamodel.Split(0) == "builtToScaleDS");
            RiqEntity lastGameSwitch = GameManager.instance.Beatmap.Entities.FindLast(c => c.beat <= beat && c.datamodel == "gameManager/switchGame/builtToScaleDS");

            if (lastGameSwitch == null) return;
            List<RiqEntity> cameraEntities = prevEntities.FindAll(c => c.beat >= lastGameSwitch.beat && c.datamodel == "builtToScaleDS/camera");

            foreach (var entity in cameraEntities)
            {
                ChangeCameraAngle(entity.beat, entity["valA"], entity["valB"], entity.length, (Util.EasingFunction.Ease)entity["type"], entity["additive"]);
            }

            UpdateCamera(beat);
        }

        void UpdateCamera(double beat)
        {
            if (beat >= cameraRotateBeat)
            {
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(cameraRotateEase);
                float rotProg = Conductor.instance.GetPositionFromBeat(cameraRotateBeat, cameraRotateLength, true);
                rotProg = Mathf.Clamp01(rotProg);
                float rot = func(cameraRotateLast, cameraRotateNext, rotProg);
                cameraPivot.rotation = Quaternion.Euler(0, rot, 0);
                cameraPivot.localScale = Vector3.one * func(cameraScaleLast, cameraScaleNext, rotProg);
            }

            GameCamera.AdditionalPosition = camPos.position + (Quaternion.Euler(camPos.rotation.eulerAngles) * Vector3.forward * 10f);
            GameCamera.AdditionalRotEuler = camPos.rotation.eulerAngles;
            GameCamera.AdditionalFoV = cameraFoV;
        }

        public void ChangeCameraAngle(double beat, float rotation, float camZoom, double length, Util.EasingFunction.Ease ease, bool additive = true)
        {
            cameraRotateBeat = beat;
            cameraRotateLength = length;
            cameraRotateEase = ease;
            cameraRotateLast = cameraRotateNext % 360f;
            cameraScaleLast = cameraScaleNext;
            cameraScaleNext = camZoom;
            if (additive)
            {
                cameraRotateNext = cameraRotateLast + rotation;
            }
            else
            {
                cameraRotateNext = rotation;
            }
        }

        private void UpdateColors()
        {
            objectMaterial.SetColor("_Color", currentObjectColor);
            shooterMaterial.SetColor("_Color", currentShooterColor);
            beltMaterial.SetColor("_Color", currentEnvironmentColor);
            gridPlaneMaterial.SetColor("_Color", currentEnvironmentColor);
            elevatorMaterial.SetColor("_Color", currentEnvironmentColor);
            if (!lighting)
            {
                foreach (var mat in firstPatternLights)
                {
                    mat.SetColor("_Color", currentEnvironmentColor);
                }
                foreach (var mat in secondPatternLights)
                {
                    mat.SetColor("_Color", currentEnvironmentColor);
                }
            }
        }

        List<RiqEntity> spawnedBlockEvents = new List<RiqEntity>();
        void Update()
        {
            shootingThisFrame = false;
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused)
                return;

            var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;

            var blockEvents = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel == "builtToScaleDS/spawn blocks");
            for (int i = 0; i < blockEvents.Count; i++)
            {
                var ev = blockEvents[i];
                if (spawnedBlockEvents.Contains(ev)) continue; // Don't spawn the same blocks multiple times.

                var spawnBeat = ev.beat - ev.length;
                if (currentBeat > spawnBeat && currentBeat < ev.beat + ev.length)
                {
                    SpawnBlocks(spawnBeat, ev.length, ev["note6"], ev["staccato"], ev["silent"]);
                    spawnedBlockEvents.Add(ev);
                    break;
                }
            }

            if (Conductor.instance.ReportBeat(ref lightBeat.lastReportedBeat, lightBeat.startBeat % 1) && autoLight)
            {
                HandleLights();
            }

            var shooterState = shooterAnim.GetCurrentAnimatorStateInfo(0);
            bool canShoot = (!shooterState.IsName("Shoot") || shooterAnim.IsAnimationNotPlaying()) && !shootingThisFrame;

            if (canShoot && lastShotOut)
                lastShotOut = false;

            if (canShoot && !lastShotOut && PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress.inputLockCategory))
            {
                lastShotOut = true;
                shootingThisFrame = true;
                Shoot();
                SpawnObject(BTSObject.FlyingRod);
                SoundByte.PlayOneShotGame("builtToScaleDS/Boing");
            }

            currentBeltOffset = (currentBeltOffset + Time.deltaTime * -beltSpeed) % 1f;
            beltMaterial.mainTextureOffset = new Vector2(0f, currentBeltOffset);
            // environmentRenderer.materials = environmentMaterials;
            // elevatorRenderer.materials = elevatorMaterials;

            if (PlayerInput.PlayerHasControl() && PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Touch)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress))
                {
                    shooterAnim.Play("Windup", 0, 0);
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !shootingThisFrame)
                {
                    shooterAnim.Play("WindDown", 0, 23 / 28f);
                }
            }
            else
            {
                if (!shootingThisFrame)
                {
                    if (blocksHolder.childCount == 0 && shooterState.IsName("Windup") && shooterAnim.IsAnimationNotPlaying())
                    {
                        shooterAnim.Play("WindDown", 0, 0);
                    }
                }
            }
            UpdateCamera(currentBeat);
        }

        void LateUpdate()
        {
        }

        public void Lights(double beat, float length, bool autoLights, bool shouldLights)
        {
            autoLight = autoLights;
            lighting = autoLights || shouldLights;
            if (shouldLights)
            {
                List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate { HandleLights(); }));
                }
                if (!autoLights)
                {
                    lighting = false;
                    actions.Add(new BeatAction.Action(beat + length, delegate
                    {
                        foreach (var lightMat in firstPatternLights)
                        {
                            lightMat.DOColor(currentEnvironmentColor, 0.2f);
                        }
                        foreach (var lightMat in secondPatternLights)
                        {
                            lightMat.DOColor(currentEnvironmentColor, 0.2f);
                        }
                    }));
                }
                BeatAction.New(instance, actions);
            }
            if (!autoLights && !shouldLights)
            {
                lighting = false;
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
            }
        }

        private void HandleLights()
        {
            if (firstLight)
            {
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(Color.white, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
            }
            else
            {
                foreach (var lightMat in firstPatternLights)
                {
                    lightMat.DOColor(currentEnvironmentColor, 0.2f);
                }
                foreach (var lightMat in secondPatternLights)
                {
                    lightMat.DOColor(Color.white, 0.2f);
                }
            }
            firstLight = !firstLight;
        }

        public void SpawnBlocks(double beat, float length, int hitNote, bool staccato, bool silent)
        {
            var newBlocks = GameObject.Instantiate(movingBlocksBase, blocksHolder).GetComponent<Blocks>();
            newBlocks.createBeat = beat;
            newBlocks.createLength = length;
            newBlocks.hitNote = hitNote;
            newBlocks.staccato = staccato;
            newBlocks.silent = silent;
            newBlocks.gameObject.SetActive(true);

            SetBlockTime(newBlocks, beat, length);
        }

        const int blockFramesPerSecond = 24;
        const int blockHitFrame = 39;
        const int blockTotalFrames = 80;
        const int spawnFrameOffset = -3;
        List<int> criticalFrames = new List<int> { 7, 15, 23, 31, 39, 47 };
        public void SetBlockTime(Blocks blocks, double spawnBeat, float length)
        {
            float spawnTimeOffset = (float)spawnFrameOffset / (float)blockFramesPerSecond;

            float secondsPerFrame = 1f / blockFramesPerSecond;
            float secondsToHitFrame = secondsPerFrame * blockHitFrame;

            float secondsPerBeat = Conductor.instance.secPerBeat;
            float secondsToHitBeat = secondsPerBeat * 5f * length + spawnTimeOffset;

            float speedMult = secondsToHitFrame / secondsToHitBeat;

            float secondsPastSpawnTime = secondsPerBeat * (Conductor.instance.songPositionInBeats - (float)spawnBeat) + spawnTimeOffset;
            float framesPastSpawnTime = blockFramesPerSecond * speedMult * secondsPastSpawnTime;
            
            // The only way I could deal with Unity's interpolation shenanigans without having a stroke.
            if (criticalFrames.Contains(Mathf.FloorToInt(framesPastSpawnTime)))
                framesPastSpawnTime = Mathf.CeilToInt(framesPastSpawnTime);

            blocks.anim.Play("Move", 0, framesPastSpawnTime / blockTotalFrames);
            blocks.anim.speed = speedMult;
        }

        public void SpawnObject(BTSObject btsObject)
        {
            GameObject prefabToUse = null;
            string animNameToUse = "";

            switch (btsObject)
            {
                case BTSObject.HitPieces:
                    prefabToUse = hitPartsBase;
                    animNameToUse = "PartsHit";
                    break;
                case BTSObject.MissPieces:
                    prefabToUse = missPartsBase;
                    animNameToUse = "PartsMiss";
                    break;
                case BTSObject.FlyingRod:
                    prefabToUse = flyingRodBase;
                    animNameToUse = "Fly";
                    break;
            }

            if (prefabToUse != null)
            {
                var newPiece = GameObject.Instantiate(prefabToUse, partsHolder).GetComponent<BTSPiece>();
                newPiece.gameObject.SetActive(true);
                newPiece.anim.Play(animNameToUse, 0, 0);
            }
        }

        public void Shoot()
        {
            shooterAnim.Play("Shoot", 0, 0);
            elevatorAnim.Play("MakeRod", 0, 0);
        }

        public void PlayPiano(double beat, float length, int semiTones)
        {
            var pianoPitch = SoundByte.GetPitchFromSemiTones(semiTones, true);
            var pianoSource = SoundByte.PlayOneShotGame("builtToScaleDS/Piano", beat, pianoPitch, 0.8f, true);

            pianoSource.SetLoopParams(beat + length, 0.1f);
        }

        public void MultiplePiano(double beat, float length, bool silent, int note1, int note2, int note3, int note4, int note5, int note6, bool staccato)
        {
            if (silent) return;
            float noteLength = length;
            float endNoteLength = 0.75f;

            if (staccato && length > 0.5f)
            {
                noteLength = 0.5f;
                endNoteLength = 0.5f;
            }
            PlayPiano(beat, noteLength, note1); 
            PlayPiano(beat + length, noteLength, note2);
            PlayPiano(beat + length * 2, noteLength, note3); 
            PlayPiano(beat + length * 3, noteLength, note4); 
            PlayPiano(beat + length * 4, endNoteLength, note5); 
        }
    }
}