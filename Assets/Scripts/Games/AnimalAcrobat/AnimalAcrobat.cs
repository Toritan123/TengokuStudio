using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrAnimalAcrobatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("animalAcrobat", "Animal Acrobat", "738e63", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        AnimalAcrobat.instance.Bop(e.beat, e.length);
                    },
                    resizable = true
                },
                new GameAction("elephant", "Elephant") { defaultLength = 4, defaultVersion = 1 },
                new GameAction("giraffe", "Giraffe") { defaultLength = 8, defaultVersion = 1 },
                new GameAction("monkeys", "Monkeys") { defaultLength = 8, defaultVersion = 1 },
                new GameAction("monkeyLong", "Monkey Line Standalone") { defaultLength = 5, defaultVersion = 1 },
                new GameAction("monkeyShort", "Monkey Standalone") { defaultLength = 3, defaultVersion = 1 },
                new GameAction("toggleSpotlight", "Toggle Spotlight") 
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        AnimalAcrobat.instance.SetSpotlight(e["active"]); 
                    },
                    defaultLength = 1,
                    parameters = new List<Param>()
                    {
                        new Param("active", true, "Active", "Turn the spotlight on or off.")
                    }
                },
                new GameAction("confetti", "Confetti")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        AnimalAcrobat.instance.TriggerConfetti(e.beat);
                    },
                    defaultLength = 1
                },
                new GameAction("bgcolor", "Background Appearance")
                {
                    function = delegate
                    {
                        var e = eventCaller.currentEntity;
                        AnimalAcrobat.instance.BackgroundColor(e.beat, e.length, e["colorAStart"], e["colorAEnd"], e["colorBStart"], e["colorBEnd"], e["ease"]);
                    },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorAStart", AnimalAcrobat._defaultColorA, "Start Color A", "Set the color at the start of the event."),
                        new Param("colorAEnd", AnimalAcrobat._defaultColorA, "End Color A", "Set the color at the end of the event."),
                        new Param("colorBStart", AnimalAcrobat._defaultColorB, "Start Color B", "Set the secondary color at the start of the event."),
                        new Param("colorBEnd", AnimalAcrobat._defaultColorB, "End Color B", "Set the secondary color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "colorAStart", "colorBStart" }),
                        }),
                    }
                }
            },
            new List<string>() { "ctr", "normal" },
            "ctrblanco", "en",
            new List<string>() { },
            chronologicalSortKey: 9,
            font: "Comet");
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_AnimalAcrobat;

    public class AnimalAcrobat : Minigame
    {
        private static readonly int POOL_NUMBER = 5; 
        private static readonly int EXPIRATION_TIMES = 4;

        [Header("Animal Prefabs")]
        [SerializeField] private AcrobatObstacle _elephant;
        [SerializeField] private AcrobatObstacle _giraffe, _monkeysLong, _monkeysShort;
        [SerializeField] private AcrobatObstacle _gorilla; 

        [Header("Components")]
        [SerializeField] private Transform _scroll;
        [SerializeField] private AcrobatPlayer _playerMonkey;
        [SerializeField] private GameObject _spotlightMain;
        [SerializeField] private Material _bgMat;

        [Header("Confetti Components")]
        [SerializeField] private GameObject _partyPoppers; 
        private List<Animator> _poppersAnimators = new List<Animator>();
        private List<ParticleSystem> _poppersParticles = new List<ParticleSystem>();

        [Header("Values")]
        [SerializeField] private float _jumpDistance = 8f;
        [SerializeField] private float _jumpDistanceGiraffe = 16f;
        [SerializeField] private float _jumpStartCameraDistance = 4f;
        [SerializeField] private float _jumpStartDistance = -1f;
        [SerializeField] private float _giraffeCameraZoom = 5f;
        [SerializeField] private float _cameraSmoothSpeed = 10f; 
        [SerializeField] public static Color _defaultColorA =  "ADCE96".Hex2RGB();
        [SerializeField] public static Color _defaultColorB = "596948".Hex2RGB();

        private List<AcrobatObstacle> _pooledElephants = new(), _pooledGiraffes = new(), _pooledMonkeysLong = new(), _pooledMonkeysShort = new();
        private List<AcrobatObstacle> _pooledGorillas = new(); 

        private int _idxElephant, _idxGiraffe, _idxMonkeyLong, _idxMonkeyShort, _idxGorilla;
        
        private Transform _currentGorillaLandTarget;

        public enum AnimalType
        {
            Elephant,
            Giraffe,
            MonkeysLong,
            MonkeysShort,
            Gorilla
        }

        private struct QueuedAnimal
        {
            public AnimalType type;
            public double startBeat;
            public double length;
            public float rotationDistance;
            public float rotationHeight;
            public Util.EasingFunction.Ease ease;

            public double holdPadding;
            public double holdPaddingStart;

            public double GetHoldLengthFromType()
            {
                return type switch
                {
                    AnimalType.Elephant => 2,
                    AnimalType.Giraffe => 4,
                    AnimalType.MonkeysLong => 3,
                    AnimalType.MonkeysShort => 1,
                    AnimalType.Gorilla => 4,
                    _ => throw new System.NotImplementedException(),
                };
            }
        }

        private List<QueuedAnimal> _queuedAnimals = new();
        private int _animalPoolIndex = 0;
        private float _animalSummatedDistance = 0;
        private bool _lastAnimalWasGiraffe = false;
        
        private Util.EasingFunction.Function _funcEaseOut;
        private Util.EasingFunction.Function _funcEaseIn; 

        private double _jumpBeat = double.MaxValue;

        public bool MonkeyMissed = false;
        
        public double LastMissBeat = double.MinValue; 
        public double RecoverStartBeat = double.MinValue;
        private double _nextGameSwitchBeat = double.MaxValue; 

        public static AnimalAcrobat instance;

        private bool _shouldPlayStartSound = false;
        private double _cachedStartBeat;
        private double _cachedPlayHeadBeat;
        private int _startSoundFramesPassed = 0;

        private void Awake()
        {
            instance = this;
            _funcEaseOut = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseOutQuad);
            _funcEaseIn = Util.EasingFunction.GetEasingFunction(Util.EasingFunction.Ease.EaseInQuad); 

            SetupConfetti();
        }

        private void SetupConfetti()
        {
            if (_partyPoppers == null) return;

            _partyPoppers.SetActive(false);
            _poppersAnimators.Clear();
            _poppersParticles.Clear();

            foreach (Transform child in _partyPoppers.transform)
            {
                var anim = child.GetComponent<Animator>();
                if (anim != null) _poppersAnimators.Add(anim);

                var spriteTransform = child.Find("Sprite");
                if (spriteTransform != null)
                {
                    var particleTransform = spriteTransform.Find("Confetti");
                    if (particleTransform != null)
                    {
                        var ps = particleTransform.GetComponent<ParticleSystem>();
                        if (ps != null) _poppersParticles.Add(ps);
                    }
                }
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;
            
            if (_shouldPlayStartSound)
            {
                if (_startSoundFramesPassed < 2)
                {
                    _startSoundFramesPassed++;
                }
                else
                {
                    _shouldPlayStartSound = false;
                    
                    if (_cachedStartBeat >= _cachedPlayHeadBeat - 0.01) 
                    {
                        SoundByte.PlayOneShotGame("animalAcrobat/start", _cachedStartBeat, forcePlay: true);
                    }
                }
            }

            BackgroundColorUpdate();
            AnimalPoolUpdate(cond);
            CameraUpdate(cond);
        }

        public void TriggerConfetti(double beat)
        {
            if (_partyPoppers == null || MonkeyMissed) return;
            
            StopCoroutine(nameof(CoConfettiSequence));
            StartCoroutine(CoConfettiSequence(beat));
        }

        private IEnumerator CoConfettiSequence(double beat)
        {
            _partyPoppers.SetActive(true);
            
            yield return null; 
            foreach (var anim in _poppersAnimators)
            {
                anim.Play("PopIntro", -1, 0f);
            }

            float waitTime = 0f;
            if (_poppersAnimators.Count > 0)
            {
                yield return null; 

                var stateInfo = _poppersAnimators[0].GetCurrentAnimatorStateInfo(0);
                waitTime = stateInfo.length;
            }

            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            foreach (var ps in _poppersParticles)
            {
                ps.Play();
            }

            SoundByte.PlayOneShotGame("animalAcrobat/cracker");
        }

        public void SetSpotlight(bool active)
        {
            if (_spotlightMain != null)
            {
                _spotlightMain.SetActive(active);
            }
        }

        public void PlayerSetActive(bool active)
        {
            _playerMonkey.gameObject.SetActive(active);
        }

        public bool IsNextJumpToGorilla(double beat)
        {
            int currentIndex = _queuedAnimals.FindIndex(x => x.startBeat <= beat && x.startBeat + x.length > beat);
            
            if (currentIndex != -1 && currentIndex + 1 < _queuedAnimals.Count)
            {
                return _queuedAnimals[currentIndex + 1].type == AnimalType.Gorilla;
            }
            return true;
        }

        public void PlayerJump(double beat, AnimalType type)
        {
            float startPoint = GetDistanceAtBeat(beat) - (type == AnimalType.Giraffe ? _jumpDistanceGiraffe : _jumpDistance);
            PlayerSetActive(true);
            int currentIndex = _queuedAnimals.FindIndex(x => x.startBeat <= beat && x.startBeat + x.length > beat);
            
            if (currentIndex != -1 && currentIndex + 1 < _queuedAnimals.Count)
            {
                if (_queuedAnimals[currentIndex + 1].type == AnimalType.Gorilla)
                {
                    float duration = (type == AnimalType.Giraffe) ? 4f : 2f;
                    _playerMonkey.JumpToLanding(beat, startPoint, duration, _currentGorillaLandTarget);
                    return;
                }
            }

            if (type == AnimalType.Giraffe)
            {
                _playerMonkey.JumpBetweenGiraffe(beat, startPoint);
            }
            else if (type == AnimalType.MonkeysLong)
            {
                _playerMonkey.JumpBetweenLong(beat, startPoint);
            }
            else
            {
                _playerMonkey.JumpBetweenAnimals(beat, startPoint);
            }
        }

        private float GetDistanceAtBeat(double beat)
        {
            var animals = _queuedAnimals.FindAll(x => x.startBeat <= beat);

            float result = (animals.Count == 0) ? 0 : _jumpStartCameraDistance;
            for (int i = 0; i < animals.Count; i++)
            {
                result += animals[i].rotationDistance;
                result += (animals[i].type == AnimalType.Giraffe) ? _jumpDistanceGiraffe : _jumpDistance;
            }
            return result;
        }

        public void Bop(double beat, float length)
        {
            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++)
            {
                if (beat + i >= _jumpBeat) break;
                actions.Add(new BeatAction.Action(beat + i, delegate
                {
                    _playerMonkey.Bop();
                }));
            }

            if (actions.Count > 0) BeatAction.New(this, actions);
        }

        private int _animalCameraIndex = 0;
        private float _lastCameraX = 0;
        private double _cameraHoldTime = 1;
        private bool _lastCameraAnimalWasGiraffe = false;

        private void CameraUpdate(Conductor cond)
        {
            if (_animalCameraIndex >= _queuedAnimals.Count || cond.songPositionInBeatsAsDouble < _queuedAnimals[0].startBeat - 1 - _queuedAnimals[0].holdPaddingStart) return;

            var currentAnimal = _queuedAnimals[_animalCameraIndex];
            double releaseBeat = currentAnimal.startBeat + currentAnimal.GetHoldLengthFromType();
            double fullMovementDuration = currentAnimal.GetHoldLengthFromType() + currentAnimal.holdPadding + currentAnimal.holdPaddingStart;
            double movementStartBeat = currentAnimal.startBeat - currentAnimal.holdPaddingStart;

            float distance = (_animalCameraIndex == 0) ? _jumpStartCameraDistance : (_lastCameraAnimalWasGiraffe ? _jumpDistanceGiraffe : _jumpDistance);
            float normalizedHold = cond.GetPositionFromBeat(movementStartBeat, fullMovementDuration);

            Vector3 targetPos = GameCamera.AdditionalPosition;
            bool targetFound = false;

            if (currentAnimal.type == AnimalType.Gorilla)
            {
                float normalizedTravel = cond.GetPositionFromBeat(movementStartBeat - _cameraHoldTime, _cameraHoldTime);
                if (normalizedTravel <= 1)
                {
                    float newX = Mathf.Lerp(_lastCameraX, _lastCameraX + distance, normalizedTravel);
                    targetPos = new Vector3(newX, 0, 0);
                    targetFound = true;
                }
                else
                {
                    targetPos = new Vector3((_lastCameraX + distance), 0, 0);
                    targetFound = true;
                }
            }
            else if (normalizedHold < 0)
            {
                float normalizedTravel = cond.GetPositionFromBeat(movementStartBeat - _cameraHoldTime, _cameraHoldTime);
                float newX = Mathf.Lerp(_lastCameraX, _lastCameraX + distance, normalizedTravel);
                
                float newZ = 0;
                if (_lastCameraAnimalWasGiraffe) newZ = 0; 
                
                targetPos = new Vector3(newX, 0, -newZ);
                targetFound = true;
            }
            else if (cond.songPositionInBeatsAsDouble >= releaseBeat)
            {
                float releaseProgress = (float)((releaseBeat - movementStartBeat) / fullMovementDuration);
                var func = Util.EasingFunction.GetEasingFunction(currentAnimal.ease);

                float currentStartRotX = _lastCameraX + distance;
                float currentEndRotX = currentStartRotX + currentAnimal.rotationDistance;
                float cutX = func(currentStartRotX, currentEndRotX, releaseProgress);

                float gapToNext = (currentAnimal.type == AnimalType.Giraffe) ? _jumpDistanceGiraffe : _jumpDistance;
                float targetX = currentEndRotX + gapToNext;

                double arrivalBeat = releaseBeat + 2; 
                if (_animalCameraIndex + 1 < _queuedAnimals.Count)
                {
                    var nextAnimal = _queuedAnimals[_animalCameraIndex + 1];
                    arrivalBeat = nextAnimal.startBeat - nextAnimal.holdPaddingStart;
                }

                float jumpProgress = cond.GetPositionFromBeat(releaseBeat, arrivalBeat - releaseBeat);
                float clampedJump = Mathf.Clamp01(jumpProgress);
                
                float camX = Mathf.Lerp(cutX, targetX, clampedJump);
                float camZ = 0;

                if (currentAnimal.type == AnimalType.Giraffe && !MonkeyMissed)
                {
                    float zoomOutDuration = 0.2f; 
                    float zoomInStartPoint = 0.85f; 

                    if (clampedJump < zoomOutDuration)
                    {
                        float t = clampedJump / zoomOutDuration;
                        camZ = _funcEaseIn(0, _giraffeCameraZoom, t); 
                    }
                    else if (clampedJump < zoomInStartPoint)
                    {
                        camZ = _giraffeCameraZoom;
                    }
                    else
                    {
                        float t = (clampedJump - zoomInStartPoint) / (1f - zoomInStartPoint);
                        float easeOutT = t * t * t; 
                        camZ = Mathf.Lerp(_giraffeCameraZoom, 0, easeOutT);
                    }
                }

                targetPos = new Vector3(camX, 0, -camZ);
                targetFound = true;

                if (cond.songPositionInBeatsAsDouble >= arrivalBeat)
                {
                    AdvanceAnimalIndex(currentAnimal, distance);
                }
            }
            else if (normalizedHold >= 0 && normalizedHold <= 1)
            {
                var func = Util.EasingFunction.GetEasingFunction(currentAnimal.ease);
                float newX = func(_lastCameraX + distance, _lastCameraX + distance + currentAnimal.rotationDistance, normalizedHold);
                float angle = func(0, 180, normalizedHold);
                float newY = Mathf.Sin(angle * Mathf.Deg2Rad) * currentAnimal.rotationHeight;

                targetPos = new Vector3(newX, newY, 0);
                targetFound = true;
            }

            if (targetFound)
            {
                GameCamera.AdditionalPosition = Vector3.Lerp(GameCamera.AdditionalPosition, targetPos, Time.deltaTime * _cameraSmoothSpeed);
            }
        }

        private void AdvanceAnimalIndex(QueuedAnimal currentAnimal, float distance)
        {
            _cameraHoldTime = (currentAnimal.type == AnimalType.Giraffe) ? 4 : 2;
            _lastCameraX += distance + currentAnimal.rotationDistance;
            _animalCameraIndex++;
            _lastCameraAnimalWasGiraffe = currentAnimal.type == AnimalType.Giraffe;
        }

        private void AnimalPoolUpdate(Conductor cond)
        {
            if (_animalPoolIndex >= _queuedAnimals.Count) return;

            double bakeBeat = _queuedAnimals[_animalPoolIndex - EXPIRATION_TIMES].startBeat;
            if (cond.songPositionInBeatsAsDouble >= bakeBeat)
            {
                BakeNextAvailableAnimal();
            }
        }

        private void BakeNextAvailableAnimal()
        {
            if (_animalPoolIndex >= _queuedAnimals.Count) return;
            var currentAnimal = _queuedAnimals[_animalPoolIndex];
            
            AcrobatObstacle animal = null;

            switch (currentAnimal.type)
            {
                case AnimalType.Elephant:
                    animal = _pooledElephants[_idxElephant];
                    _idxElephant = (_idxElephant + 1) % POOL_NUMBER;
                    break;
                case AnimalType.Giraffe:
                    animal = _pooledGiraffes[_idxGiraffe];
                    _idxGiraffe = (_idxGiraffe + 1) % POOL_NUMBER;
                    break;
                case AnimalType.MonkeysLong:
                    animal = _pooledMonkeysLong[_idxMonkeyLong];
                    _idxMonkeyLong = (_idxMonkeyLong + 1) % POOL_NUMBER;
                    break;
                case AnimalType.MonkeysShort:
                    animal = _pooledMonkeysShort[_idxMonkeyShort];
                    _idxMonkeyShort = (_idxMonkeyShort + 1) % POOL_NUMBER;
                    break;
                case AnimalType.Gorilla:
                    animal = _pooledGorillas[_idxGorilla];
                    _idxGorilla = (_idxGorilla + 1) % 1; 
                    break;
            }

            if (_animalPoolIndex == 0) _animalSummatedDistance = _jumpStartDistance;
            
            animal.gameObject.SetActive(true);

            if (currentAnimal.type == AnimalType.Gorilla)
            {
                float coordinateOffset = _jumpStartCameraDistance - _jumpStartDistance; 
                
                animal.transform.localPosition = new Vector3(_animalSummatedDistance + coordinateOffset, 0, 0);
                
                _currentGorillaLandTarget = animal.transform.Find("Land");

                // hide gorilla on gameswitch 
                if (currentAnimal.startBeat >= _nextGameSwitchBeat)
                {
                    var renderers = animal.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var r in renderers) r.enabled = false;
                }
                else
                {
                    var renderers = animal.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var r in renderers) r.enabled = true;
                }

                Animator gorillaAnim = animal.GetComponent<Animator>(); 

                BeatAction.New(this, new List<BeatAction.Action>() 
                { 
                    new BeatAction.Action(currentAnimal.startBeat, delegate 
                    { 
                        if (MonkeyMissed && gorillaAnim != null)
                        {
                            gorillaAnim.DoScaledAnimationAsync("GorillaMiss", 0.5f);
                        }
                    }) 
                });
            }
            else
            {
                double expBeat = currentAnimal.startBeat + currentAnimal.length;
                for (int i = 0; i < EXPIRATION_TIMES; i++)
                {
                    if (_animalPoolIndex + i + 1 >= _queuedAnimals.Count)
                    {
                        expBeat = double.MaxValue;
                        break;
                    }
                    var seekedAnimal = _queuedAnimals[_animalPoolIndex + i + 1];
                    expBeat += seekedAnimal.length;
                }

                animal.Init(currentAnimal.startBeat, expBeat, _lastAnimalWasGiraffe);

                _animalSummatedDistance += (animal.GetRotationDistance() * 0.5f);
                
                animal.transform.localPosition = new Vector3(_animalSummatedDistance, 0, 0);

                _animalSummatedDistance += (animal.GetRotationDistance() * 0.5f) + ((currentAnimal.type == AnimalType.Giraffe) ? _jumpDistanceGiraffe : _jumpDistance);
                _lastAnimalWasGiraffe = currentAnimal.type == AnimalType.Giraffe;
            }

            _animalPoolIndex++;
        }

        public override void OnGameSwitch(double beat) 
        { 
            var firstCue = GameManager.instance.Beatmap.Entities.Find(e => e.datamodel is "animalAcrobat/elephant" or "animalAcrobat/giraffe" or "animalAcrobat/monkeys" or "animalAcrobat/monkeyLong" or "animalAcrobat/monkeyShort");
            var spot = GameManager.instance.Beatmap.Entities.Find(e => e.datamodel is "animalAcrobat/toggleSpotlight");
            if (firstCue != null && spot == null && firstCue.version == 0)
            {
                SetSpotlight(false);
            }

            PersistColor(beat);
            
            GetAnimals(beat); 
        }
        public override void OnPlay(double beat) { OnGameSwitch(beat); }

        private void GetAnimals(double beat)
        {
            _animalPoolIndex = 0;
            _animalSummatedDistance = 0;
            _lastAnimalWasGiraffe = false;
            _animalCameraIndex = 0;
            _lastCameraX = 0;
            
            MonkeyMissed = false;
            LastMissBeat = double.MinValue; 
            RecoverStartBeat = double.MinValue; 
            _nextGameSwitchBeat = double.MaxValue; 

            if (_partyPoppers != null) _partyPoppers.SetActive(false);

            _idxElephant = 0; _idxGiraffe = 0; _idxMonkeyLong = 0; _idxMonkeyShort = 0; _idxGorilla = 0;

            foreach (var obj in _pooledElephants) if (obj != null) Destroy(obj.gameObject);
            foreach (var obj in _pooledGiraffes) if (obj != null) Destroy(obj.gameObject);
            foreach (var obj in _pooledMonkeysLong) if (obj != null) Destroy(obj.gameObject);
            foreach (var obj in _pooledMonkeysShort) if (obj != null) Destroy(obj.gameObject);
            foreach (var obj in _pooledGorillas) if (obj != null) Destroy(obj.gameObject);

            _pooledElephants.Clear(); _pooledGiraffes.Clear();
            _pooledMonkeysLong.Clear(); _pooledMonkeysShort.Clear();
            _pooledGorillas.Clear(); _queuedAnimals.Clear();

            for (int i = 0; i < POOL_NUMBER; i++)
            {
                _pooledElephants.Add(Instantiate(_elephant, _scroll));
                _pooledGiraffes.Add(Instantiate(_giraffe, _scroll));
                _pooledMonkeysLong.Add(Instantiate(_monkeysLong, _scroll));
                _pooledMonkeysShort.Add(Instantiate(_monkeysShort, _scroll));
            }
            if (_gorilla != null) _pooledGorillas.Add(Instantiate(_gorilla, _scroll));

            var allGameSwitches = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame" }).FindAll(x => x.beat > beat);
            if (allGameSwitches.Count > 0) _nextGameSwitchBeat = allGameSwitches[0].beat;

            var allAnimals = EventCaller.GetAllInGameManagerList("animalAcrobat", new string[]
            {
                "elephant", "giraffe", "monkeys", "monkeyLong", "monkeyShort"
            }).FindAll(x => x.beat >= beat && x.beat < _nextGameSwitchBeat);
            
            if (allAnimals.Count > 0)
            {
                allAnimals.Sort((x, y) => x.beat.CompareTo(y.beat));

                double goodBeat = allAnimals[0].beat;
                for (int i = 0; i < allAnimals.Count; i++)
                {
                    var animal = allAnimals[i];
                    if (animal.beat != goodBeat) continue;
                    goodBeat += animal.length;

                    if (animal.datamodel == "animalAcrobat/monkeys")
                    {
                        _queuedAnimals.Add(new QueuedAnimal() { startBeat = animal.beat, length = 5, type = AnimalType.MonkeysLong, rotationDistance = _monkeysLong.GetRotationDistance(), rotationHeight = _monkeysLong.GetRotationHeight(), ease = _monkeysLong.Ease, holdPadding = _monkeysLong.HoldPadding, holdPaddingStart = _monkeysLong.HoldPaddingStart });
                        _queuedAnimals.Add(new QueuedAnimal() { startBeat = animal.beat + 5, length = 3, type = AnimalType.MonkeysShort, rotationDistance = _monkeysShort.GetRotationDistance(), rotationHeight = _monkeysShort.GetRotationHeight(), ease = _monkeysShort.Ease, holdPadding = _monkeysShort.HoldPadding, holdPaddingStart = _monkeysShort.HoldPaddingStart });
                        continue;
                    }

                    AcrobatObstacle prefab = animal.datamodel switch
                    {
                        "animalAcrobat/elephant" => _elephant,
                        "animalAcrobat/giraffe" => _giraffe,
                        "animalAcrobat/monkeyLong" => _monkeysLong,
                        "animalAcrobat/monkeyShort" => _monkeysShort,
                        _ => null
                    };
                    if (prefab == null) continue;

                    _queuedAnimals.Add(new QueuedAnimal()
                    {
                        startBeat = animal.beat,
                        length = animal.datamodel switch
                        {
                            "animalAcrobat/elephant" => 4,
                            "animalAcrobat/giraffe" => 8,
                            "animalAcrobat/monkeyLong" => 5,
                            "animalAcrobat/monkeyShort" => 3,
                            _ => 0
                        },
                        type = animal.datamodel switch
                        {
                            "animalAcrobat/elephant" => AnimalType.Elephant,
                            "animalAcrobat/giraffe" => AnimalType.Giraffe,
                            "animalAcrobat/monkeyLong" => AnimalType.MonkeysLong,
                            "animalAcrobat/monkeyShort" => AnimalType.MonkeysShort,
                            _ => AnimalType.Elephant
                        },
                        rotationDistance = prefab.GetRotationDistance(),
                        rotationHeight = prefab.GetRotationHeight(),
                        ease = prefab.Ease,
                        holdPadding = prefab.HoldPadding,           
                        holdPaddingStart = prefab.HoldPaddingStart  
                    });
                }

                if (_gorilla != null && _queuedAnimals.Count > 0)
                {
                    var lastAnimal = _queuedAnimals[_queuedAnimals.Count - 1];
                    _queuedAnimals.Add(new QueuedAnimal()
                    {
                        startBeat = lastAnimal.startBeat + lastAnimal.length,
                        length = 4,
                        type = AnimalType.Gorilla,
                        rotationDistance = 0,
                        rotationHeight = 0,
                        ease = Util.EasingFunction.Ease.Linear,
                        holdPadding = 0,
                        holdPaddingStart = 0
                    });
                }

                if (_queuedAnimals.Count > 0) 
                {
                    for (int i = 0; i < POOL_NUMBER - 1; i++) { BakeNextAvailableAnimal(); }
                    _jumpBeat = _queuedAnimals[0].startBeat - 1;

                    _cachedStartBeat = _jumpBeat;
                    _cachedPlayHeadBeat = beat;
                    _shouldPlayStartSound = true;
                    _startSoundFramesPassed = 0;

                    _playerMonkey.InitialJump(_jumpBeat);
                }
            }
        }

        private ColorEase _colorAEase = new(_defaultColorA);
        private ColorEase _colorBEase = new(_defaultColorB);
        //call this in update
        private void BackgroundColorUpdate()
        {
            _bgMat.SetColor("_ColorAlpha", _colorAEase.GetColor()); //Red
            _bgMat.SetColor("_ColorBravo", _colorBEase.GetColor()); //Green
        }

        public void BackgroundColor(double beat, float length, Color colorAStartSet, Color colorAEndSet, Color colorBStartSet, Color colorBEndSet, int ease)
        {
            _colorAEase = new ColorEase(beat, length, colorAStartSet, colorAEndSet, ease);
            _colorBEase = new ColorEase(beat, length, colorBStartSet, colorBEndSet, ease);
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("animalAcrobat", new string[] { "bgcolor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorAStart"], lastEvent["colorAEnd"], lastEvent["colorBStart"], lastEvent["colorBEnd"], lastEvent["ease"]);
            }
        }
    }
}