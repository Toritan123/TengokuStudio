using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class GiraffeInput : MonoBehaviour
    {
        public AnimalAcrobat.AnimalType animalType; 

        [Header("Components")]
        [SerializeField] private Animator _monkey;
        [SerializeField] private ParticleSystem _holdParticle;
        [SerializeField] private ParticleSystem _sweatParticle;

        [Header("Shadows")]
        [SerializeField] private GameObject _gripShadow; 
        [SerializeField] private GameObject _endShadow;  

        [Header("FireHoop")]
        [SerializeField] private Animator _fireHoopAnim;     
        [SerializeField] private ParticleSystem _fireHoopSparkle; 

        private AcrobatObstacle _mainScript;
        private AnimalAcrobat _game;
        private Sound _drumRollSound;
        
        private PlayerActionEvent _holdAction;
        private PlayerActionEvent _releaseAction;
        
        private AcrobatPlayer _playerScript;
        private Vector3 _endShadowDefaultScale;

        private bool _queueSounds = false;
        private double _initBeat;
        private int _framesPassed = 0;

        private bool _holding = false;
        private bool _recoveryChecked = false;
        private bool _wasSkipped = false;
        private bool _hasMissed = false;

        private void Awake()
        {
            _game = AnimalAcrobat.instance;
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
            _playerScript = FindObjectOfType<AcrobatPlayer>();
            if (_endShadow != null) _endShadowDefaultScale = _endShadow.transform.localScale;
        }

        private void OnDestroy()
        {
            if (_drumRollSound != null) _drumRollSound.KillLoop(0.587);
        }

        private void Update()
        {
            if (_queueSounds)
            {
                if (_framesPassed < 2)
                {
                    _framesPassed++;
                    return;
                }

                _queueSounds = false;
                ScheduleSounds(_initBeat);
            }

            if (!_wasSkipped && !_holding)
            {
                if (_game != null && _game.MonkeyMissed && _recoveryChecked)
                {
                    _wasSkipped = true;
                    KillInputs();
                    return;
                }
            }

            if (!_recoveryChecked && Conductor.instance.songPositionInBeatsAsDouble >= _initBeat - 2.0)
            {
                _recoveryChecked = true;
                
                if (_game != null && _game.MonkeyMissed)
                {
                    double beatsAvailable = _initBeat - _game.LastMissBeat;
                    
                    //3 beat recovery window
                    if (beatsAvailable < 3.0)
                    {
                        _wasSkipped = true; 
                        KillInputs();
                        return; 
                    }

                    if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();
                    if (_playerScript != null)
                    {
                        _playerScript.PrepareRecovery(_initBeat, transform.localPosition.x, animalType);
                    }
                    
                    _game.RecoverStartBeat = _initBeat;
                    _game.MonkeyMissed = false;
                }
            }

            if (_holding && !PlayerInput.GetIsAction(Minigame.InputAction_BasicPressing) && !GameManager.instance.Autoplay)
            {
                Miss(null);
            }
        }

        public void Init(double beat, bool beforeGiraffe)
        {
            if (_endShadow != null) { _endShadow.SetActive(false); ToggleMasks(_endShadow, false); }
            if (_gripShadow != null) { _gripShadow.SetActive(true); ToggleMasks(_gripShadow, true); }
            if (_fireHoopAnim != null) _fireHoopAnim.Play("FireIdle", -1, 0f); 
            if (_fireHoopSparkle != null) _fireHoopSparkle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _holding = false;
            _recoveryChecked = false;
            _wasSkipped = false;
            _hasMissed = false; 

            _holdAction = _game.ScheduleInput(beat - 1, 1, Minigame.InputAction_BasicPress, beforeGiraffe ? JustHoldGiraffe : JustHold, Miss, Empty);
            _releaseAction = _game.ScheduleInput(beat, 4, Minigame.InputAction_FlickRelease, JustRelease, Miss, Empty);
            
            _monkey.gameObject.SetActive(false);

            _initBeat = beat;
            _queueSounds = true;
            _framesPassed = 0;
        }

        private void KillInputs()
        {
            if (_holdAction != null) { _holdAction.Disable(); _holdAction.QueueDeletion(); _holdAction = null; }
            if (_releaseAction != null) { _releaseAction.Disable(); _releaseAction.QueueDeletion(); _releaseAction = null; }
        }
        
        private void ScheduleSounds(double beat)
        {
            double eek1 = beat + 2;
            double eek2 = beat + 3;
            double currentSongTime = Conductor.instance.songPositionInBeatsAsDouble;

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            if (eek1 >= currentSongTime) 
                sounds.Add(new MultiSound.Sound("animalAcrobat/eek", eek1));
            else if (eek1 >= currentSongTime - 1.0) 
                SoundByte.PlayOneShotGame("animalAcrobat/eek", forcePlay: true);

            if (eek2 >= currentSongTime)
                sounds.Add(new MultiSound.Sound("animalAcrobat/eek", eek2));
            else if (eek2 >= currentSongTime - 1.0)
                SoundByte.PlayOneShotGame("animalAcrobat/eek", forcePlay: true);

            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray());
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            if (_game.MonkeyMissed || _wasSkipped) return;

            _holding = true;
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 1f, 0.4f);
            HandleShadowsOnGrab(); 
            _game.PlayerSetActive(false);

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                 SpawnSweatParticle();
            }
            else
            {
                SoundByte.PlayOneShotGame("animalAcrobat/catch");
                SpawnParticle();
            }

            BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(0, delegate {}), new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate { _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f); }) });
        }

        private void JustHoldGiraffe(PlayerActionEvent caller, float state)
        {
            if (_game.MonkeyMissed || _wasSkipped) return;

            _holding = true;
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 1f, 0.4f);
            HandleShadowsOnGrab(); 
            _game.PlayerSetActive(false);

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                 SpawnSweatParticle();
            }
            else
            {
                SoundByte.PlayOneShotGame("animalAcrobat/giraffeCatch");
                SpawnParticle();
            }

            BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(0, delegate {}), new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate { _monkey.DoScaledAnimationAsync("PlayerHanging", 0.25f); }) });
        }

        private void HandleShadowsOnGrab()
        {
            if (_endShadow != null) { _endShadow.SetActive(true); ToggleMasks(_endShadow, true); StartCoroutine(PopInShadow(_endShadow.transform)); }
            if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();
            if (_playerScript != null) _playerScript.ForceShadowDisable();
        }

        private IEnumerator PopInShadow(Transform shadowTrans)
        {
            float duration = 0.2f; float elapsed = 0f; shadowTrans.localScale = Vector3.zero;
            while (elapsed < duration) { elapsed += Time.deltaTime; float t = elapsed / duration; float ease = 1f - (1f - t) * (1f - t); shadowTrans.localScale = Vector3.Lerp(Vector3.zero, _endShadowDefaultScale, ease); yield return null; }
            shadowTrans.localScale = _endShadowDefaultScale;
        }

        private void ToggleMasks(GameObject obj, bool state)
        {
            var masks = obj.GetComponentsInChildren<SpriteMask>(true); foreach (var m in masks) m.enabled = state;
        }

        private void SpawnParticle()
        {
            ParticleSystem spawnedParticle = Instantiate(_holdParticle, transform);
            spawnedParticle.transform.position = _holdParticle.transform.position;
            spawnedParticle.Play();
        }

        private void SpawnSweatParticle()
        {
            ParticleSystem spawnedParticle = Instantiate(_sweatParticle, transform);
            spawnedParticle.transform.position = _sweatParticle.transform.position;
            spawnedParticle.Play();
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            if (!_holding) return;

            _holding = false;
            bool isBarely = state >= 1f || state <= -1f;

            if (isBarely)
            {
                SoundByte.PlayOneShot("miss");
                if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();
                if (_playerScript != null) _playerScript.muteNextReleaseParticle = true;
            }
            else
            {
                SoundByte.PlayOneShotGame("animalAcrobat/giraffeJump");
            }

            _monkey.gameObject.SetActive(false);
            
            if (_gripShadow != null) { _gripShadow.SetActive(false); ToggleMasks(_gripShadow, false); }
            if (_endShadow != null) { StopAllCoroutines(); _endShadow.SetActive(false); ToggleMasks(_endShadow, false); }

            _game.PlayerJump(caller.startBeat + caller.timer, animalType);

            double beat = caller.startBeat + caller.timer;

            if (!isBarely)
            {
                _drumRollSound = SoundByte.PlayOneShotGame("animalAcrobat/giraffeDrumroll", -1, 1f, 1f, true);
                
                GiraffeDrumRollLoop(beat);
            }

            bool isEnding = false;
            if (AnimalAcrobat.instance != null) isEnding = AnimalAcrobat.instance.IsNextJumpToGorilla(caller.startBeat);

            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();

            if (!isBarely)
            {
                sounds.Add(new MultiSound.Sound("animalAcrobat/giraffeCymbal", beat + 2));
                sounds.Add(new MultiSound.Sound("animalAcrobat/applause", beat + 2.25));
            }
            
            if (!isEnding) sounds.Add(new MultiSound.Sound("animalAcrobat/turn", beat + 3)); 
            
            if (sounds.Count > 0) MultiSound.Play(sounds.ToArray());

            if (!isBarely)
            {
                BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(beat + 2, delegate { StartCoroutine(PlayFireHoopSequence()); }) });
            }
        }

        private IEnumerator PlayFireHoopSequence()
        {
            if (_fireHoopAnim != null) { _fireHoopAnim.Play("FireClose", -1, 0f); yield return null; yield return new WaitForSeconds(_fireHoopAnim.GetCurrentAnimatorStateInfo(0).length); if (_fireHoopSparkle != null) _fireHoopSparkle.Play(); }
        }

        private void GiraffeDrumRollLoop(double beat)
        {
            _drumRollSound.LerpVolume(beat, 1, 1, 0.125f);
            BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(beat + 1, delegate { _drumRollSound.LerpVolume(beat + 1, 3, 0.125f, 1); }), new BeatAction.Action(beat + 4, delegate { _drumRollSound.KillLoop(0.587); }) });
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (_wasSkipped) return;
            if (_initBeat < _game.RecoverStartBeat) return;
            if (_game.MonkeyMissed) return;
            if (_hasMissed) return;
            _hasMissed = true;

            _game.LastMissBeat = Conductor.instance.songPositionInBeatsAsDouble;

            bool wasHolding = _holding;
            _holding = false;
            _monkey.gameObject.SetActive(false);

            SoundByte.PlayOneShotGame("animalAcrobat/miss");

            if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();

            if (wasHolding)
            {
                if (_playerScript != null) _playerScript.ForceMissFall(_monkey.transform.position);

                if (_gripShadow != null) 
                {
                    _gripShadow.SetActive(false);
                    ToggleMasks(_gripShadow, false);
                }
                if (_endShadow != null) 
                {
                    StopAllCoroutines();
                    _endShadow.SetActive(false);
                    ToggleMasks(_endShadow, false);
                }
            }

            _releaseAction?.Disable(); 
            _releaseAction?.QueueDeletion(); 
            _holdAction?.Disable();
            _holdAction?.QueueDeletion();

            _game.MonkeyMissed = true;
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}