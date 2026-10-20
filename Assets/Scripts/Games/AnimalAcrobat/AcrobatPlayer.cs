using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using HeavenStudio.Common;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class AcrobatPlayer : SuperCurveObject
    {
        [Header("Components")]
        [SerializeField] private Transform _scroll;
        [SerializeField] private Transform _shadow;
        [SerializeField] private ParticleSystem _releaseParticle, _trailParticle, sweatParticle;
        
        [Header("Values")]
        [SerializeField] private float _jumpDistanceStart = 6f;
        [SerializeField] private float _jumpDistance = 8f;
        [SerializeField] private float _jumpHeight = 4f;
        [SerializeField] private float _jumpHeightInitial = 2f;
        [SerializeField] private float _jumpDistanceGiraffe = 16f;
        [SerializeField] private float _jumpHeightGiraffe = 8f;
        [SerializeField] private float _jumpStartAngle = 120f;
        
        private Animator _anim;
        private Path _path;
        private float _restY;
        private Vector3 _defaultShadowScale;
        
        private Coroutine _shadowRoutine;
        private Coroutine _jumpRoutine;
        private Coroutine _rotateRoutine;

        private ParticleSystem _currentTrailInstance;

        public bool muteNextReleaseParticle = false;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _path = new Path()
            {
                positions = new PathPos[2]
            };
            _restY = transform.localPosition.y;

            if (_shadow != null)
            {
                _defaultShadowScale = _shadow.localScale;
                ForceShadowEnable();
            }
        }

        private void LateUpdate()
        {
            if (_currentTrailInstance != null)
            {
                _currentTrailInstance.transform.position = transform.position;
                _currentTrailInstance.transform.rotation = Quaternion.identity; 
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            KillTrail();
        }

        private void KillTrail()
        {
            ParticleSystem trailToFade = _currentTrailInstance;
            _currentTrailInstance = null;

            if (trailToFade != null)
            {
                trailToFade.transform.SetParent(_scroll, true);
                trailToFade.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                
                ParticleSystem[] allSystems = trailToFade.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in allSystems)
                {
                    if (ps.particleCount > 0)
                    {
                        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
                        int count = ps.GetParticles(particles);

                        for (int i = 0; i < count; i++)
                        {
                            if (particles[i].remainingLifetime > 0.2f)
                            {
                                particles[i].remainingLifetime = 0.2f;
                            }
                        }

                        ps.SetParticles(particles, count);
                    }
                }

                Destroy(trailToFade.gameObject, 0.5f);
            }
        }

        private void SpawnTrail()
        {
            if (_trailParticle != null)
            {
                KillTrail(); 
                _currentTrailInstance = Instantiate(_trailParticle, _scroll);
                _currentTrailInstance.transform.position = transform.position;
                _currentTrailInstance.Play();
            }
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("PlayerBop", 0.5f);
        }

        public void PrepareRecovery(double landBeat, float targetXPos, AnimalAcrobat.AnimalType type)
        {
            gameObject.SetActive(true);
            StopAllCoroutines(); 
            KillTrail(); 

            double startBeat = landBeat - 2.0;

            float centerToGrabOffset = 0f;
            //offsets
            switch (type)
            {
                case AnimalAcrobat.AnimalType.Giraffe:
                    centerToGrabOffset = -9f;
                    break;
                case AnimalAcrobat.AnimalType.MonkeysLong:
                    centerToGrabOffset = -7f; 
                    break;
                case AnimalAcrobat.AnimalType.MonkeysShort:
                    centerToGrabOffset = -1.1f; 
                    break;
                default: 
                    centerToGrabOffset = 0f; 
                    break;
            }

            float finalTargetX = targetXPos + centerToGrabOffset;
            
            string randomThree = Random.Range(0, 2) == 0 ? "three1" : "three2";
            string randomGo = Random.Range(0, 2) == 0 ? "go1" : "go2";

            SoundByte.PlayOneShot("count-ins/" + randomThree, startBeat);
            SoundByte.PlayOneShotGame("animalAcrobat/return", startBeat);
            SoundByte.PlayOneShot("count-ins/" + randomGo, startBeat + 1.0);

            StartCoroutine(RecoveryCo(startBeat, landBeat, finalTargetX, type));
        }

        private IEnumerator RecoveryCo(double startBeat, double landBeat, float targetX, AnimalAcrobat.AnimalType type)
        {
            var cond = Conductor.instance;

            double unswungStart = cond.GetUnSwungBeat(startBeat);
            double unswungLand = cond.GetUnSwungBeat(landBeat);
            double unswungDuration = unswungLand - unswungStart;
            
            yield return new WaitUntil(() => cond.unswungSongPositionInBeatsAsDouble >= unswungStart);

            transform.localEulerAngles = Vector3.zero;
            _anim.Play("PlayerJump", 0, 0); 
            ForceShadowEnable();

            float jumpBackDist = _jumpDistance + 1f; 

            if (type == AnimalAcrobat.AnimalType.MonkeysShort) 
            {
                jumpBackDist = 5f; 
            }

            float startX = targetX - jumpBackDist; 
            float startY = _restY - 7f; 

            _path.positions[0] = new PathPos()
            {
                pos = new Vector3(startX, startY),
                height = 5, 
                duration = (float)unswungDuration 
            };
            _path.positions[1] = new PathPos()
            {
                pos = new Vector3(targetX, _restY),
            };

            while (true)
            {
                transform.localPosition = GetPathPositionFromBeat(_path, cond.unswungSongPositionInBeatsAsDouble, unswungStart);
                yield return null;
            }
        }

        public void ForceMissFall(Vector3 gripPos)
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            KillTrail(); 
            
            transform.position = gripPos;

            Vector3 local = transform.localPosition;
            transform.localPosition = new Vector3(local.x, local.y, 0);
            
            transform.localEulerAngles = Vector3.zero;
            
            _anim.Play("PlayerJump", 0, 0); 
            
            ForceShadowEnable();

            StartCoroutine(MissFall());
        }

        private IEnumerator MissFall()
        {
            float velocity = 13f;  
            float gravity = -166f;
            
            while(true)
            {
                velocity += gravity * Time.deltaTime;
                transform.localPosition += new Vector3(6f * Time.deltaTime, velocity * Time.deltaTime, 0);
                yield return null;
            }
        }

        public void JumpBetweenAnimals(double beat, float startPoint)
        {
            _anim.Play("PlayerAir", 0, 0);
            Jump(beat, 2, startPoint, _jumpDistance, _jumpHeight, _restY);
            RotateJump(beat, 2);
            HandleShadow(beat, 2, 1.4f);
            SpawnReleaseParticle();
        }

        public void JumpBetweenLong(double beat, float startPoint)
        {
            _anim.Play("PlayerAir", 0, 0);
            Jump(beat, 2, startPoint, 2.2f, 2.5f, _restY);
            RotateJump(beat, 2);
            HandleShadow(beat, 2, 1.4f);
            SpawnReleaseParticle();
        }
        
        public void JumpBetweenGiraffe(double beat, float startPoint)
        {
            _anim.Play("PlayerAir", 0, 0);

            if (!muteNextReleaseParticle) 
            {
                SpawnTrail();
            }

            Jump(beat, 4, startPoint, _jumpDistanceGiraffe, _jumpHeightGiraffe, _restY);
            RotateJump(beat, 4);
            HandleShadow(beat, 4, 2.2f);
            
            SpawnReleaseParticle();
        }

        public void JumpToLanding(double beat, float startPoint, float duration, Transform target)
        {
            _anim.Play("PlayerAir", 0, 0);
            Vector3 targetPos = target != null ? target.position : transform.parent.TransformPoint(new Vector3(startPoint + _jumpDistance, _restY));
            Vector3 localTarget = transform.parent.InverseTransformPoint(targetPos);
            float dist = localTarget.x - startPoint;
            float height = (duration >= 4) ? _jumpHeightGiraffe - 0.2f: _jumpHeight - 0.2f;

            if (duration >= 4) SpawnTrail();

            Jump(beat, duration, startPoint, dist, height, localTarget.y);
            
            if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
            _rotateRoutine = StartCoroutine(ArcRotate(beat, duration));
            
            HandleShadow(beat, duration, 1.4f);
            SpawnReleaseParticle();
            StartCoroutine(LandingSequence(beat, duration, target));
        }

        private IEnumerator ArcRotate(double beat, float duration)
        {
            var cond = Conductor.instance;
            float startAngle = _jumpStartAngle;
            float endAngle = 360f;
            while (cond.songPositionInBeatsAsDouble < beat + duration)
            {
                float normalized = cond.GetPositionFromBeat(beat, duration);
                float angle = Mathf.Lerp(startAngle, endAngle, normalized);
                transform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }
            transform.localEulerAngles = Vector3.zero;
        }

        private IEnumerator LandingSequence(double beat, float duration, Transform target)
        {
            var cond = Conductor.instance;
            double unswungTargetBeat = cond.GetUnSwungBeat(beat + duration);

            yield return new WaitUntil(() => cond.unswungSongPositionInBeatsAsDouble >= unswungTargetBeat);
            
            SoundByte.PlayOneShotGame("animalAcrobat/land", beat + duration);
            
            StopJump();
            if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
            transform.localEulerAngles = Vector3.zero;
            KillTrail();
            if (target != null) transform.position = target.position;
                    
            _anim.Play("PlayerLand", 0, 0);
            StartCoroutine(LandingShadowCo());
        }

        private IEnumerator LandingShadowCo()
        {
            ForceShadowEnable();
            if (_shadow == null) yield break;
            Vector3 startScale = _defaultShadowScale;
            Vector3 targetScale = _defaultShadowScale * 12;
            float duration = 0.5f; 
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = 1f - (1f - t) * (1f - t);
                _shadow.localScale = Vector3.Lerp(startScale, targetScale, ease);
                yield return null;
            }
            _shadow.localScale = targetScale;
        }

        private void StopJump()
        {
            if (_jumpRoutine != null) StopCoroutine(_jumpRoutine);
        }

        private void SpawnReleaseParticle()
        {
            if (muteNextReleaseParticle)
            {
                muteNextReleaseParticle = false; 
                return;
            }

            ParticleSystem spawnedParticle = Instantiate(_releaseParticle, _scroll);
            spawnedParticle.transform.position = transform.position;
            spawnedParticle.Play();
        }
        
        public void ForceShadowDisable()
        {
            if (_shadowRoutine != null) StopCoroutine(_shadowRoutine);
            if (_shadow != null)
            {
                _shadow.gameObject.SetActive(false);
                var masks = _shadow.GetComponentsInChildren<SpriteMask>(true);
                foreach (var mask in masks) mask.enabled = false;
            }
        }

        public void ForceShadowEnable()
        {
            if (_shadow != null)
            {
                _shadow.gameObject.SetActive(true);
                _shadow.localScale = _defaultShadowScale;
                var masks = _shadow.GetComponentsInChildren<SpriteMask>(true);
                foreach (var mask in masks) mask.enabled = true;
            }
        }

        public void InitialJump(double beat)
        {
            StartCoroutine(InitialJumpCo(beat));
        }

        private IEnumerator InitialJumpCo(double beat)
        {
            var cond = Conductor.instance;
            double unswungStart = cond.GetUnSwungBeat(beat);

            ForceShadowEnable();
            yield return new WaitUntil(() => cond.unswungSongPositionInBeatsAsDouble >= unswungStart);
            
            _anim.DoScaledAnimationAsync("PlayerJump", 0.5f);
            Jump(beat, 1, transform.localPosition.x, _jumpDistanceStart, _jumpHeightInitial, _restY);
        }

        private void HandleShadow(double beat, float duration, float startScaleMultiplier)
        {
            if (_shadow != null)
            {
                if (_shadowRoutine != null) StopCoroutine(_shadowRoutine);
                _shadowRoutine = StartCoroutine(ShadowCo(beat, duration, startScaleMultiplier));
            }
        }

        private IEnumerator ShadowCo(double beat, float duration, float startScaleMultiplier)
        {
            var cond = Conductor.instance;
            
            double unswungStart = cond.GetUnSwungBeat(beat);
            double unswungEnd = cond.GetUnSwungBeat(beat + duration);
            double unswungDuration = unswungEnd - unswungStart;
            double unswungSpinStart = cond.GetUnSwungBeat(beat + duration - 1.0);
            double unswungSpinDuration = unswungEnd - unswungSpinStart;

            ForceShadowEnable();
            Vector3 targetStartScale = _defaultShadowScale * startScaleMultiplier;
            _shadow.localScale = targetStartScale;
            
            while (cond.unswungSongPositionInBeatsAsDouble < unswungSpinStart) yield return null;

            float normalized = 0f;
            while (normalized < 1f)
            {
                float currentUnswung = (float)cond.unswungSongPositionInBeatsAsDouble;
                normalized = Mathf.Clamp01((float)((currentUnswung - unswungSpinStart) / unswungSpinDuration));
                
                float ease = 1f - (1f - normalized) * (1f - normalized);
                _shadow.localScale = Vector3.Lerp(targetStartScale, _defaultShadowScale, ease);
                yield return null;
            }
            _shadow.localScale = _defaultShadowScale;
        }

        private void Jump(double beat, float length, float startPoint, float distance, float height, float restY)
        {
            if (_jumpRoutine != null) StopCoroutine(_jumpRoutine);
            _jumpRoutine = StartCoroutine(JumpCo(beat, length, startPoint, distance, height, restY));
        }

        private IEnumerator JumpCo(double beat, float length, float startPoint, float distance, float height, float restY)
        {
            var cond = Conductor.instance;
            
            double unswungStart = cond.GetUnSwungBeat(beat);
            double unswungEnd = cond.GetUnSwungBeat(beat + length);
            float unswungDuration = (float)(unswungEnd - unswungStart);

            _path.positions[0] = new PathPos()
            {
                pos = new Vector3(startPoint, restY),
                height = height,
                duration = unswungDuration
            };
            _path.positions[1] = new PathPos()
            {
                pos = new Vector3(startPoint + distance, restY),
            };
            while (true)
            {
                transform.localPosition = GetPathPositionFromBeat(_path, cond.unswungSongPositionInBeatsAsDouble, unswungStart);
                yield return null;
            }
        }

        private void RotateJump(double beat, double length)
        {
            if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
            _rotateRoutine = StartCoroutine(RotateJumpCo(beat, length));
        }

        private IEnumerator RotateJumpCo(double beat, double length)
        {
            var cond = Conductor.instance;
            float normalized = 0;
            while (normalized <= 1)
            {
                normalized = cond.GetPositionFromBeat(beat, length - 1);
                float newAngle = Mathf.Lerp(_jumpStartAngle, 360, normalized);
                transform.localEulerAngles = new Vector3(0, 0, newAngle);
                yield return null;
            }
            normalized = 0;
            while (normalized <= 1)
            {
                normalized = cond.GetPositionFromBeat(beat + length - 1, 0.5);
                float newAngle = Mathf.Lerp(0, 720, normalized);
                transform.localEulerAngles = new Vector3(0, 0, newAngle);
                yield return null;
            }
        }
    }
}