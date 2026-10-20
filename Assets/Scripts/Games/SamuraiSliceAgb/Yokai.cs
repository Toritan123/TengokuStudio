using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_SamuraiSliceAgb
{
    public class Yokai : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] SamuraiSliceAgb game;
        [SerializeField] BezierCurve3D enterCurve;
        [SerializeField] BezierCurve3D missCurve;
        [SerializeField] Animator samuraiAnim;
        [SerializeField] GameObject shadow;
        [SerializeField] GameObject bigShadowObject;
        [SerializeField] ParticleSystem sliceEffect;
        [SerializeField] ParticleSystem sliceribbonEffect;
        [SerializeField] ParticleSystem slicemediumEffect;
        [SerializeField] ParticleSystem sliceflyingEffect;
        [SerializeField] ParticleSystem slicebigEffect;
        [SerializeField] ParticleSystem barelyEffect;
        [SerializeField] ParticleSystem barelyRibbonEffect;
        [SerializeField] ParticleSystem barelyMediumEffect;
        [SerializeField] ParticleSystem barelyFlyingEffect;
        [SerializeField] ParticleSystem barelyBigEffect;
        [SerializeField] SpriteRenderer fireSprite;
        [SerializeField] Animator fireAnim;
        [SerializeField] Animator flashAnim;
        [SerializeField] BezierCurve3D flyCurve;
        [SerializeField] BezierCurve3D walkCurve;
        [SerializeField] List<BezierCurve3D> enterCurves = new List<BezierCurve3D>();
        [SerializeField] List<BezierCurve3D> flyingCurves = new List<BezierCurve3D>();
        [SerializeField] Vector3 bigYokaiStartPosition;
        [SerializeField] float bigYokaiXDistance;
        [SerializeField] float bigYokaiYDistance;
        [SerializeField] float shadowOffsetX;
        public int type;
        int curveIndex;
        GameObject shadowInstance;
        bool isEntering = true;
        bool isMiss;
        public bool slowDown;
        public double startBeat;
        float floorHeight = 1.9f;
        // Start is called before the first frame update
        void Start()
        {   
            shadowInstance = Instantiate(type == 4 ? bigShadowObject : shadow, transform);
            shadowInstance.SetActive(true);
            shadowInstance.transform.position = new Vector3(transform.position.x+(type == 4 ? shadowOffsetX : 0f), floorHeight);
            game.ScheduleInput(startBeat, 6, Minigame.InputAction_FlickPress, Hit, Miss, null);
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
                for (int i = 0; i < 6; i++)
                {
                    actions.Add(new BeatAction.Action(startBeat + i, delegate
                    {
                        switch (type)
                        {
                            case 0:
                            anim.DoScaledAnimationAsync("Jump", 0.5f);
                            break;
                            case 1:
                            anim.DoScaledAnimationAsync("Flying", 0.5f);
                            break;
                            case 2:
                            anim.DoScaledAnimationAsync("Jump1", 0.5f);
                            break;
                            case 3:
                            anim.DoScaledAnimationAsync("Flying1", 0.5f);
                            break;
                            case 4:
                            anim.DoScaledAnimationAsync("YokaiWalk", 0.5f);
                            break;
                        }
                    }));
                }
                BeatAction.New(game, actions);
        }
        private void Hit(PlayerActionEvent caller, float state)
        {   
            game.Slice();
            Destroy(gameObject);
            if (state is >= 1f or <= (-1f))
            {   
                fireAnim.gameObject.SetActive(false);
                game.fireState = 0;
                SoundByte.PlayOneShotGame("agbSamuraiSlice/barely");
                switch (type)
                {
                    case 0:
                    barelyEffect.Play();
                    break;
                    case 1:
                    barelyRibbonEffect.Play();
                    break;
                    case 2:
                    barelyMediumEffect.Play();
                    break;
                    case 3:
                    barelyFlyingEffect.Play();
                    break;
                    case 4:
                    barelyBigEffect.Play();
                    break;
                }
                game.samuraiState = 0;
                game.sliceState = 0;
            }
            else
            {   
                if (game.sliceState < 5) game.sliceState++;
                switch (game.sliceState)
                {
                    case 0:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicelowslow" : "agbSamuraiSlice/sliceLow");
                    break;
                    case 1:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicelowslow" : "agbSamuraiSlice/sliceLow");
                    break;
                    case 2:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicelowslow" : "agbSamuraiSlice/sliceLow");
                    break;
                    case 3:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicemediumslow" : "agbSamuraiSlice/slicemedium");
                    break;
                    case 4:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicemediumslow" : "agbSamuraiSlice/slicemedium");
                    break;
                    case 5:
                    SoundByte.PlayOneShotGame(slowDown ? "agbSamuraiSlice/slicehighslow" : "agbSamuraiSlice/slicehigh");
                    break;
                }
                switch (type)
                {
                    case 0:
                    sliceEffect.Play();
                    break;
                    case 1:
                    sliceribbonEffect.Play();
                    break;
                    case 2:
                    slicemediumEffect.Play();
                    break;
                    case 3:
                    sliceflyingEffect.Play();
                    break;
                    case 4:
                    slicebigEffect.Play();
                    break;
                }
                if (game.samuraiState < 2) game.samuraiState++;
                if (game.fireState < 8) game.fireState++;
                if (game.fireState > 2)
                {   
                    fireAnim.gameObject.SetActive(true);
                    fireAnim.DoScaledAnimationAsync("Fire"+(game.fireState - 2), 0.5f);
                }
                if (slowDown) game.conductor.SetMinigamePitch(0.5f);
                BeatAction.New(game, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(caller.startBeat + caller.timer + 1f, delegate 
                    { 
                        if (slowDown) game.conductor.SetMinigamePitch(1f);
                    })
                });
                if (slowDown) flashAnim.DoScaledAnimationAsync("Flash", 0.5f);

                /*
                if (game.fireState > 2)
                {   
                    var fireState = game.fireState;
                    if (game.fireColorNumberSix) fireSprite.color = game.fireColors[fireState - 2];
                    else
                    {
                        var fireStart = game.fireColorStart;
                        var fireEnd = game.fireColorEnd;
                        fireSprite.color = new Color(fireStart.r + (fireEnd.r - fireStart.r) / 6 * fireState - 2, fireStart.g + (fireEnd.g - fireStart.g) / 6 * fireState - 2, fireStart.b + (fireEnd.b - fireStart.b) / 6 * fireState - 2, 1f );
                    }
                } */
            }
            game.Slice();
            game.FogSlice(state is >= 1f or <= (-1f));
        }
        private void Miss(PlayerActionEvent caller)
        {
            isEntering = false;
            isMiss = true;
            SoundByte.PlayOneShotGame("agbSamuraiSlice/miss");
            samuraiAnim.DoScaledAnimationAsync("Miss", 0.5f);
            game.samuraiState = 0;
            fireAnim.gameObject.SetActive(false);
            game.fireState = 0;
            game.sliceState = 0;
            game.FogSlice(true);
        }
        // Update is called once per frame
        void Update()
        {   
            float shadowPos = 0f;
            if (game.conductor.unswungSongPositionInBeatsAsDouble < startBeat + 5) shadowPos = game.conductor.GetPositionFromBeat(startBeat, 5f) * -2 + floorHeight;
            else shadowPos = -4.43f;
            if (isEntering)
            {   
                BezierCurve3D curve = enterCurves[curveIndex];
                switch (type)
                {
                    case 0:
                    curve = enterCurves[curveIndex];
                    break;
                    case 1:
                    curve = flyingCurves[curveIndex];
                    break;
                    case 2:
                    curve = enterCurves[curveIndex];
                    break;
                    case 3:
                    curve = flyingCurves[curveIndex];
                    break;
                    case 4:
                    curve = enterCurves[curveIndex];
                    break;
                }
                if (type != 4 || curveIndex > 4)
                {
                    float flyPos = game.conductor.GetPositionFromBeat(startBeat + curveIndex, 1f);
                    transform.position = curve.GetPoint(flyPos);
                    if (flyPos > 1f) curveIndex++;
                }
                else
                {
                    float flyPos = game.conductor.GetPositionFromBeat(startBeat, 5f);
                    transform.position = new Vector3(bigYokaiStartPosition.x + bigYokaiXDistance * flyPos, bigYokaiStartPosition.y + bigYokaiYDistance * flyPos);
                    if (flyPos > 1f) curveIndex = 5;
                }
            }
            if (isMiss)
            {
                float flyPos = game.conductor.GetPositionFromBeat(startBeat+6, 1f);
                transform.position = missCurve.GetPoint(flyPos);
                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                }
            }
            shadowInstance.transform.position = new Vector3(transform.position.x+(type == 4 ? shadowOffsetX : 0f), shadowPos);
        }
        void Awake()
        {   
        }
    }
}