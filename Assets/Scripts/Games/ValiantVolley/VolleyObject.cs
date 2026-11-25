using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_ValiantVolley
{
    public class VolleyObject : MonoBehaviour
    {
        [SerializeField] BezierCurve3D enterCurve;
        [SerializeField] BezierCurve3D bounceCurve1;
        [SerializeField] BezierCurve3D bounceCurve2;
        [SerializeField] BezierCurve3D hitCurve;
        [SerializeField] BezierCurve3D barelyCurve;

        [SerializeField] Transform objectTransform;

        public Animator anim;

        private SpriteRenderer shadowRenderer;

        public SpriteRenderer objectSprite;
        public Sprite fruitSprite;
        public GameObject missImpact;

        public double startBeat;
        public double distance;

        public bool isEntering;
        public bool isHit;
        public bool missed;
        public bool barely;
        public int objectType; // 0 => Dirt || 1 => Fruit
        public int currentAnt;

        public bool beingJuggled;
        public double lastJuggle;
        public double lastJuggleLength;
        private float hitPitch = 1f;
        private double thisBeat;
        private int currentJuggle = 0;

        private List<BeatAction.Action> queuedActions = new();
        private List<PlayerActionEvent> queuedJuggles = new();
        public List<double> intervalInputList = new();
        public List<float> juggleLengths = new();

        public ValiantVolley game;

        void Awake()
        {
            currentAnt = 0;
            if (objectType == 0)
            {
                game.ScheduleInput(startBeat + distance, distance * 2, Minigame.InputAction_BasicPress, Hit, Miss, null);
            }
            else
            {
                objectSprite.sprite = fruitSprite;
                game.ScheduleInput(startBeat + distance, distance * 2, ValiantVolley.InputAction_AltPress, Hit, Miss, null);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

            if (objectType == 1) transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-600f * Time.deltaTime));
            else transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-120f * Time.deltaTime));
            
            if (anim.IsPlayingAnimationNames("ObjectJuggle")) transform.rotation = Quaternion.Euler(0, 0, 0);

            if (beingJuggled)
            {
                if (currentJuggle > juggleLengths.Count) currentJuggle = 0;

                if (objectType == 0)
                {
                    if (intervalInputList.Count > 0)
                    {
                        var queuedInputs = new List<double>();
                        foreach (var input in intervalInputList)
                        {
                            if (input > startBeat) queuedInputs.Add(input);
                        }
                        foreach (var input in queuedInputs)
                        {

                            if (input == lastJuggle)
                            {
                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 1f; }));

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle = 0; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle = 0; }));
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", input, 1f, 2.3f, false, false, 0f, false, false, -0.2f);
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", input + distance, 1f, 2f);
                            }
                            else
                            {
                                
                                queuedActions.Add(new BeatAction.Action(input, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1/ juggleLengths[currentJuggle] * 0.5f * Conductor.instance.SongPitch); }));
                                queuedActions.Add(new BeatAction.Action(input + distance, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1/ juggleLengths[currentJuggle] * 0.5f * Conductor.instance.SongPitch); }));

                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 0.8f; }));

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle += 1; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle += 1; }));
                                
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", input, 0.7f, 2.3f, false, false, 0f, false, false, -0.2f);
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", input + distance, 0.7f, 2f);
                            }

                            queuedActions.Add(new BeatAction.Action(input, delegate { thisBeat = input; }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { thisBeat = input + distance; }));
                            queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { thisBeat = input + distance * 2; }));
                            queuedJuggles.Add(game.ScheduleInput(input, distance * 2, Minigame.InputAction_BasicPress, Hit, Miss, null));
                            queuedActions.Add(new BeatAction.Action(input, delegate { game.ants[0].AntAction("dirtHit", hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { game.ants[1].AntAction("dirtHit", hitPitch); }));
                            intervalInputList.Remove(input);
                        }
                    }
                }
                else
                {
                    if (intervalInputList.Count > 0)
                    {
                        var queuedInputs = new List<double>();
                        foreach (var input in intervalInputList)
                        {
                            queuedInputs.Add(input);
                        }
                        foreach (var input in queuedInputs)
                        {
                            
                            if (input == lastJuggle)
                            {
                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 1f; }));
                                SoundByte.PlayOneShotGame("valiantVolley/fruitVolley", input, 1, 3f, false, false, 0f, false, false, -0.2f);
                                SoundByte.PlayOneShotGame("valiantVolley/fruitVolley", input + distance, 1, 2f);

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle = 0; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle = 0; }));
                            }
                            else
                            {
                                queuedActions.Add(new BeatAction.Action(input, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f); }));
                                queuedActions.Add(new BeatAction.Action(input + distance, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f); }));

                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 0.8f; }));

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle += 1; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle += 1; }));

                                SoundByte.PlayOneShotGame("valiantVolley/fruitJuggle", input, 1f, 3f, false, false, 0f, false, false, -0.2f);
                                SoundByte.PlayOneShotGame("valiantVolley/fruitJuggle", input + distance, 1f, 2f);
                            }

                            queuedActions.Add(new BeatAction.Action(input, delegate { thisBeat = input; }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { thisBeat = input + distance; }));
                            queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { thisBeat = input + distance * 2; }));
                            queuedJuggles.Add(game.ScheduleInput(input, distance * 2, ValiantVolley.InputAction_AltPress, Hit, Miss, null));
                            queuedActions.Add(new BeatAction.Action(input, delegate { game.ants[0].AntAction("fruitHit", hitPitch); }));
                            
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { game.ants[1].AntAction("fruitHit", hitPitch); }));
                            
                            intervalInputList.Remove(input);
                        }
                    }
                }
            }

            if (isEntering)
            {

                float flyPos = 0;
                if (flyPos < 1f) flyPos = game.conductor.GetPositionFromBeat(startBeat, distance, true, false);

                if (flyPos < 1f && currentAnt == 0)
                {
                    if (queuedActions.Count > 0) game.hitPitch = 0.8f;
                    transform.position = enterCurve.GetPoint(flyPos);
                }
                else if (currentAnt != 2)
                {
                    if (currentAnt == 0)
                    {
                        queuedActions.Sort((s1, s2) => s1.beat.CompareTo(s2.beat));
                        BeatAction.New(this, queuedActions);

                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        if ((thisBeat == lastJuggle && thisBeat != 0) || queuedActions.Count == 0)
                        {
                            hitPitch = 1f;
                            anim.DoScaledAnimationAsync("ObjectHit", 0.5f);
                            currentJuggle = 0;
                        }
                        else
                        {
                            hitPitch = 0.8f;
                            anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f * Conductor.instance.SongPitch);
                            
                            currentJuggle += 1;
                        }
                        if (objectType == 0)
                        {
                            game.ants[0].AntAction("dirtHit", hitPitch);
                            if ((thisBeat == lastJuggle && thisBeat != 0) || queuedActions.Count == 0)
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 1f, 2.3f, false, false, 0f, false, false, -0.2f);
                            }
                            else
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 0.7f, 2.3f, false, false, 0f, false, false, -0.2f);
                            }
                        }
                        else
                        {
                            game.ants[0].AntAction("fruitHit", hitPitch);
                            if ((thisBeat == lastJuggle && thisBeat != 0) || queuedActions.Count == 0)
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/fruitVolley", -1, 1, 2.6f, false, false, 0f, false, false, -0.2f);
                            }
                            else
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/fruitJuggle", -1, 1, 2.6f, false, false, 0f, false, false, -0.2f);
                            }
                            
                        }
                    }
                    if (flyPos >= 1) currentAnt = 1;

                    flyPos = game.conductor.GetPositionFromBeat(startBeat + distance, distance, true, false);

                    if (thisBeat == lastJuggle && thisBeat != 0)
                    {
                        flyPos = game.conductor.GetPositionFromBeat(lastJuggle, lastJuggleLength, true, false);
                        transform.position = bounceCurve1.GetPoint(flyPos);
                    }
                    else if (queuedActions.Count == 0)
                    {
                        transform.position = bounceCurve1.GetPoint(flyPos);
                    }

                    if (flyPos > 1f)
                    {
                        if (queuedActions.Count > 0) hitPitch = 0.8f;
                        if (objectType == 0)
                        {
                            game.ants[1].AntAction("dirtHit", hitPitch);
                            if ((thisBeat == lastJuggle + distance && thisBeat != 0) || queuedActions.Count == 0)
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 1f, 1.2f); 
                            }
                            else SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 0.7f, 1.2f);
                        }
                        else
                        {
                            game.ants[1].AntAction("fruitHit", hitPitch);
                            if ((thisBeat == lastJuggle + distance && thisBeat != 0) || queuedActions.Count == 0)
                            {
                                SoundByte.PlayOneShotGame("valiantVolley/fruitVolley", -1, 1, 1.5f); 
                            }
                            else SoundByte.PlayOneShotGame("valiantVolley/fruitJuggle", -1, 1, 1.5f);
                        }
                        currentAnt = 2;
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        if ((thisBeat == lastJuggle + distance && thisBeat != 0) || queuedActions.Count == 0)
                        {
                            anim.DoScaledAnimationAsync("ObjectHit", 0.5f);
                            currentJuggle = 0;
                        }
                        else
                        {
                            currentJuggle = 0;
                            anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f * Conductor.instance.SongPitch);
                            
                            currentJuggle += 1;
                        }


                    }
                }
                else
                {
                    flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 2, distance, true, false);
                    if (thisBeat == lastJuggle + distance && thisBeat != 0)
                    {
                        flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance, lastJuggleLength, true, false);
                        if (flyPos < 1f) transform.position = bounceCurve2.GetPoint(flyPos);
                    }
                    else if (queuedActions.Count == 0)
                    {
                        if (flyPos < 1f) transform.position = bounceCurve2.GetPoint(flyPos);
                    }

                    if (flyPos > 1f)
                    {

                    }
                }
            }
            else
            {
                
            }
            if (isHit && !missed)
            {
                barely = false;
                game.bopStatus = 1;
                float flyPos;
                if (thisBeat == lastJuggle + distance * 2 && thisBeat != 0) flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance * 2, lastJuggleLength, true, false);
                else flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 3, distance, true, false);

                if ((thisBeat == lastJuggle + distance * 2 && thisBeat != 0) || queuedActions.Count == 0) transform.position = hitCurve.GetPoint(flyPos);

                isEntering = false;


                if (flyPos <= 0.1f)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    if ((thisBeat == lastJuggle + distance * 2 && thisBeat != 0) || queuedActions.Count == 0)
                    {
                        anim.DoScaledAnimationAsync("ObjectHit", 0.5f);

                    }
                }

                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                }
            }
            if (barely)
            {
                isHit = false;
                game.bopStatus = 2;
                float flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 3, distance, true, false);
                if (thisBeat == lastJuggle + distance * 2 && thisBeat != 0)
                {
                    flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance * 2, lastJuggleLength, true, false);
                    transform.position = barelyCurve.GetPoint(flyPos);
                }
                else if (queuedActions.Count == 0)
                {
                    transform.position = barelyCurve.GetPoint(flyPos);
                }

                if (anim.IsAnimationNotPlaying() && flyPos <= 0.1f)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    anim.DoScaledAnimationAsync("ObjectBarely", 0.5f);
                }

                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                }
            }
            if (missed)
            {
                foreach (var i in queuedJuggles) i.CanHit(false);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                Destroy(gameObject);
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (caller.startBeat + distance * 2 != lastJuggle + distance * 2 && thisBeat != 0)
            {
                hitPitch = 0.8f;
                anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f * Conductor.instance.SongPitch);
                currentJuggle += 1;
            }
            else hitPitch = 1f;

            if (state >= 1f || state <= -1f)
            {
                isEntering = false;
                barely = true;
                SoundByte.PlayOneShot("nearMiss");
            }
            else
            {
                isEntering = false;
                isHit = true;

                if (objectType == 0)
                {
                    game.VolleyHit(caller, state, hitPitch);
                    if (caller.startBeat + distance * 2 != lastJuggle + distance * 2 && thisBeat != 0)
                    {
                        SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 0.7f, 2.3f, false, false, 0f, false, false, 0.2f);
                    }
                    else
                    {
                        SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, 1f, 2.3f, false, false, 0f, false, false, 0.2f);
                    }
                }
                else
                {
                    game.FruitHit(caller, state, hitPitch);
                    if (caller.startBeat + distance * 2 != lastJuggle + distance * 2 && thisBeat != 0)
                    {
                        SoundByte.PlayOneShotGame("valiantVolley/fruitJuggle", -1, 1, 2.6f, false, false, 0f, false, false, 0.2f);
                    }
                    else
                    {
                        SoundByte.PlayOneShotGame("valiantVolley/fruitVolley", -1, 1, 2.6f, false, false, 0f, false, false, 0.2f);
                    }
                }
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            missImpact.SetActive(true);
            missImpact.transform.parent = game.transform;
            Destroy(missImpact, 0.08f);
            objectSprite.enabled = false;
            if (objectType == 0)
            {
                SoundByte.PlayOneShotGame("valiantVolley/dirtMiss", -1, 1, 1.5f, false, false, 0f, false, false, 0.2f);
            }
            else
            {
                SoundByte.PlayOneShotGame("valiantVolley/fruitMiss", -1, 1, 1.5f, false, false, 0f, false, false, 0.2f);
            }

            game.bopStatus = 2;
            game.ants[2].cantBop = false;
            game.ants[2].isPreparing = false;
            isEntering = false;
            isHit = false;
            missed = true;
        }
    }
}
