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

        //public GameObject Shadow;
        //private GameObject ShadowInstance;
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

        //private float floorHeight = -3f;

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

            if (beingJuggled) game.spawnedObjects.Add(this);
            if (beingJuggled) foreach (var i in intervalInputList) print("Input list:" + i);
            //print("dist:" + distance);
            //print("Distance1: " + distance.ToString() + ", beat: " + startBeat.ToString());
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //print("The last juggle: " + lastJuggle);
            //print("Jugge lengths count:" + juggleLengths.Count);

            //bool hasDoneQueued = false;
            /*if (!barely)
            {
                if (objectType == 1) transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-600f * Time.deltaTime));
                else transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-120f * Time.deltaTime));
            }*/

            if (objectType == 1) transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-600f * Time.deltaTime));
            else transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-120f * Time.deltaTime));
            
            if (anim.IsPlayingAnimationNames("ObjectJuggle")) transform.rotation = Quaternion.Euler(0, 0, 0);

            if (beingJuggled)
            {
                if (currentJuggle > juggleLengths.Count) currentJuggle = 0;

                if (objectType == 0)
                {
                    //print("qtos inputos: " + game.intervalInputList0.Count);
                    if (intervalInputList.Count > 0)
                    {
                        var queuedInputs = new List<double>();
                        foreach (var input in intervalInputList)
                        {
                            //Debug.Log("I1: " + input);
                            if (input > startBeat) queuedInputs.Add(input);
                        }
                        foreach (var input in queuedInputs)
                        {
                            //print("Distance2: " + distance.ToString() + ", beat: " + startBeat.ToString());
                            //print("da input:" + input);

                            if (input == lastJuggle)
                            {
                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 1f; }));

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle = 0; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle = 0; }));
                                //print("sim" + input);
                            }
                            else
                            {
                                queuedActions.Add(new BeatAction.Action(input, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1/juggleLengths[currentJuggle] * 0.5f); }));
                                queuedActions.Add(new BeatAction.Action(input + distance, delegate { anim.DoScaledAnimationAsync("ObjectJuggle", 1/juggleLengths[currentJuggle] * 0.5f); }));

                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 0.8f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 0.8f; }));

                                queuedActions.Add(new BeatAction.Action(input + 0.0001f, delegate { currentJuggle += 1; }));
                                queuedActions.Add(new BeatAction.Action(input + distance + 0.0001f, delegate { currentJuggle += 1; }));

                                //print("não");
                            }

                            queuedActions.Add(new BeatAction.Action(input, delegate { thisBeat = input; }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { thisBeat = input + distance; }));
                            queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { thisBeat = input + distance * 2; }));
                            queuedJuggles.Add(game.ScheduleInput(input, distance * 2, Minigame.InputAction_BasicPress, Hit, Miss, null));
                            //float thisHitPitch = hitPitch;
                            queuedActions.Add(new BeatAction.Action(input, delegate { game.ants[0].AntAction("dirtHit", hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input, delegate { SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { game.ants[1].AntAction("dirtHit", hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, hitPitch); }));
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
                            //Debug.Log("I1: " + input);
                            queuedInputs.Add(input);
                        }
                        foreach (var input in queuedInputs)
                        {
                            
                            //queuedActions.Add(new BeatAction.Action(input, delegate { game.ants[0].AntAction("fruitHit"); }));
                            if (input == lastJuggle)
                            {
                                queuedActions.Add(new BeatAction.Action(input - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance - 0.0001f, delegate { hitPitch = 1f; }));
                                queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { hitPitch = 1f; }));

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
                            }

                            queuedActions.Add(new BeatAction.Action(input, delegate { thisBeat = input; }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { thisBeat = input + distance; }));
                            queuedActions.Add(new BeatAction.Action(input + distance * 2 - 0.0001f, delegate { thisBeat = input + distance * 2; }));
                            queuedJuggles.Add(game.ScheduleInput(input, distance * 2, ValiantVolley.InputAction_AltPress, Hit, Miss, null));
                            //float thisHitPitch = hitPitch;
                            queuedActions.Add(new BeatAction.Action(input, delegate { game.ants[0].AntAction("fruitHit", hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input, delegate { SoundByte.PlayOneShotGame("valiantVolley/fruitHit", -1, hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { game.ants[1].AntAction("fruitHit", hitPitch); }));
                            queuedActions.Add(new BeatAction.Action(input + distance, delegate { SoundByte.PlayOneShotGame("valiantVolley/fruitHit", -1, hitPitch); }));
                            intervalInputList.Remove(input);
                        }
                    }
                }
            }

            //anim.DoScaledAnimationAsync("ObjectHit", 0.5f);
            if (isEntering)
            {
                
                float flyPos = game.conductor.GetPositionFromBeat(startBeat, distance); 

                //Debug.Log("FlyPos: " + flyPos);
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
                        //if (objectType == 0) game.ants[0].AntAction("dirtHit");
                        //else game.ants[0].AntAction("fruitHit");

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
                            anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f);
                            
                            currentJuggle += 1;
                        }
                        if (objectType == 0)
                        {
                            game.ants[0].AntAction("dirtHit", hitPitch);
                            SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, hitPitch);
                        }
                        else
                        {
                            game.ants[0].AntAction("fruitHit", hitPitch);
                            SoundByte.PlayOneShotGame("valiantVolley/fruitHit", -1, hitPitch);
                        }
                    }
                    if (flyPos >= 1) currentAnt = 1;

                    flyPos = game.conductor.GetPositionFromBeat(startBeat + distance, distance);

                    if (thisBeat == lastJuggle && thisBeat != 0)
                    {
                        flyPos = game.conductor.GetPositionFromBeat(lastJuggle, lastJuggleLength);
                        //print("Last juggle length: " + lastJuggleLength);
                        transform.position = bounceCurve1.GetPoint(flyPos);
                    }
                    else if (queuedActions.Count == 0)
                    {
                        transform.position = bounceCurve1.GetPoint(flyPos);
                    }

                    if (flyPos > 1f)
                    {
                        if (queuedActions.Count > 0) hitPitch = 0.8f;
                        //Debug.Log("CurAnim:" + anim.GetCurrentAnimatorClipInfo(0));
                        if (objectType == 0)
                        {
                            game.ants[1].AntAction("dirtHit", hitPitch);
                            SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, hitPitch);
                        }
                        else
                        {
                            game.ants[1].AntAction("fruitHit", hitPitch);
                            SoundByte.PlayOneShotGame("valiantVolley/fruitHit", -1, hitPitch);
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
                            //print("Current juggle: " + currentJuggle);
                            currentJuggle = 0;
                            anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f);
                            
                            currentJuggle += 1;
                        }


                        //foreach (var i in queuedActions) Debug.Log("QActions: " + i);
                    }
                }
                else
                {
                    flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 2, distance);
                    if (thisBeat == lastJuggle + distance && thisBeat != 0)
                    {
                        flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance, lastJuggleLength); // + startBeat + 2 - game.multiIntervalStartBeat
                        transform.position = bounceCurve2.GetPoint(flyPos);
                    }
                    else if (queuedActions.Count == 0)
                    {
                        transform.position = bounceCurve2.GetPoint(flyPos);
                    }

                    if (flyPos > 1f)
                    {
                        //transform.rotation = Quaternion.Euler(0, 0, 0);
                        /*if (!barely && !missed)
                            anim.DoScaledAnimationAsync("ObjectHit", 0.5f);*/
                        //isEntering = false;
                        //isHit = true;
                    }
                }

                //Debug.Log("Anim:" + anim.IsAnimationNotPlaying());

                //if (flyPos == 1);

                /*
                if (flyPos > 1f)
                {
                    isEntering = false;
                    isHit = true;
                }
                */
            }
            else
            {
                //transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * Time.deltaTime));
            }
            if (isHit && !missed)
            {
                barely = false;
                game.bopStatus = 1;
                float flyPos;
                if (thisBeat == lastJuggle + distance * 2 && thisBeat != 0) flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance * 2, lastJuggleLength);
                else flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 3, distance);

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
                //Destroy(gameObject);
                float flyPos = game.conductor.GetPositionFromBeat(startBeat + distance * 3, distance);
                if (thisBeat == lastJuggle + distance * 2 && thisBeat != 0)
                {
                    flyPos = game.conductor.GetPositionFromBeat(lastJuggle + distance * 2, lastJuggleLength);
                    transform.position = barelyCurve.GetPoint(flyPos);
                }
                else if (queuedActions.Count == 0)
                {
                    transform.position = barelyCurve.GetPoint(flyPos);
                }
                //transform.rotation = Quaternion.Euler(0, 0, 0);

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
            //print("Animation playing: " + anim.IsPlayingAnimationNames("ObjectJuggle"));
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (caller.startBeat + distance * 2 != lastJuggle + distance * 2 && thisBeat != 0)
            {
                hitPitch = 0.8f;
                anim.DoScaledAnimationAsync("ObjectJuggle", 1 / juggleLengths[currentJuggle] * 0.5f);
                currentJuggle += 1;
            }
            else hitPitch = 1f;

            //print("hitpItch:" + hitPitch);
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
                    SoundByte.PlayOneShotGame("valiantVolley/dirtHit", -1, hitPitch);
                }
                else
                {
                    game.FruitHit(caller, state, hitPitch);
                    SoundByte.PlayOneShotGame("valiantVolley/fruitHit", -1, hitPitch);
                }
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            missImpact.SetActive(true);
            missImpact.transform.parent = game.transform;
            Destroy(missImpact, 0.08f);
            objectSprite.enabled = false;
            //print("tchau");
            if (objectType == 0)
            {
                SoundByte.PlayOneShotGame("valiantVolley/dirtMiss");
            }
            else
            {
                SoundByte.PlayOneShotGame("valiantVolley/fruitMiss");
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
