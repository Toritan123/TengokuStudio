using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class ObstacleInput : MonoBehaviour
    {
        public AnimalAcrobat.AnimalType animalType;

        [Header("Components")]
        [SerializeField] private Animator _monkey;

        [Header("Shadows")]
        [SerializeField] private GameObject _gripShadow;
        [SerializeField] private GameObject _endShadow;

        [Header("Properties")]
        [SerializeField] private double _holdLength;
        [SerializeField] private ParticleSystem _holdParticle;
        [SerializeField] private ParticleSystem _sweatParticle;

        private AcrobatObstacle _mainScript;
        private AnimalAcrobat _game;
        
        private PlayerActionEvent _holdAction;
        private PlayerActionEvent _releaseAction;

        private AcrobatPlayer _playerScript;
        private Vector3 _endShadowDefaultScale;

        private bool _queueSounds = false;
        private double _initBeat;
        
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

            if (_endShadow != null)
            {
                _endShadowDefaultScale = _endShadow.transform.localScale;
            }
        }

        private void Update()
        {
            if (_queueSounds) { _queueSounds = false; ScheduleSounds(_initBeat); }

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

            _holding = false; 
            _recoveryChecked = false;
            _wasSkipped = false;
            _hasMissed = false;

            _holdAction = _game.ScheduleInput(beat - 1, 1, Minigame.InputAction_BasicPress, beforeGiraffe ? JustHoldGiraffe : JustHold, Miss, Empty);
            _releaseAction = _game.ScheduleInput(beat, _holdLength, Minigame.InputAction_FlickRelease, JustRelease, Miss, Empty);
            _monkey.gameObject.SetActive(false);

            _initBeat = beat;
            _queueSounds = true;
        }

        private void KillInputs()
        {
            if (_holdAction != null) { _holdAction.Disable(); _holdAction.QueueDeletion(); _holdAction = null; }
            if (_releaseAction != null) { _releaseAction.Disable(); _releaseAction.QueueDeletion(); _releaseAction = null; }
        }

        private void ScheduleSounds(double beat)
        {
            double eek1 = beat + _holdLength - 2;
            double eek2 = beat + _holdLength - 1;
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

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(0, delegate {}),
                new BeatAction.Action(caller.startBeat + caller.timer + (_holdLength * 0.5), delegate
                {
                    _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f / ((float)_holdLength * 0.5f));
                })
            });
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

             BeatAction.New(this, new List<BeatAction.Action>()
             {
                 new BeatAction.Action(0, delegate {}),
                 new BeatAction.Action(caller.startBeat + caller.timer + (_holdLength * 0.5), delegate
                 {
                     _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f);
                 })
             });
        }
        
        private void HandleShadowsOnGrab()
        {
            if (_endShadow != null) 
            {
                _endShadow.SetActive(true);
                ToggleMasks(_endShadow, true);
                StartCoroutine(PopInShadow(_endShadow.transform));
            }

            if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();
            if (_playerScript != null) _playerScript.ForceShadowDisable();
        }

        private IEnumerator PopInShadow(Transform shadowTrans)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            
            shadowTrans.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = 1f - (1f - t) * (1f - t); 
                
                shadowTrans.localScale = Vector3.Lerp(Vector3.zero, _endShadowDefaultScale, ease);
                yield return null;
            }

            shadowTrans.localScale = _endShadowDefaultScale;
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

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                if (_playerScript == null) _playerScript = FindObjectOfType<AcrobatPlayer>();
                if (_playerScript != null) _playerScript.muteNextReleaseParticle = true;
            }
            else
            {
                SoundByte.PlayOneShotGame("animalAcrobat/release");
            }
            
            bool isEnding = false;
            if (AnimalAcrobat.instance != null)
            {
                isEnding = AnimalAcrobat.instance.IsNextJumpToGorilla(caller.startBeat);
            }

            if (!isEnding)
            {
                SoundByte.PlayOneShotGame("animalAcrobat/turn", caller.startBeat + caller.timer + 1);
            }

            _monkey.gameObject.SetActive(false);

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

            _game.PlayerJump(caller.startBeat + caller.timer, animalType);
        }

        private void ToggleMasks(GameObject obj, bool state)
        {
            var masks = obj.GetComponentsInChildren<SpriteMask>(true);
            foreach (var m in masks) m.enabled = state;
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