using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_HoleInOne
{
    using HeavenStudio.Util;
    public class Ball : MonoBehaviour
    {
        [System.NonSerialized] public double startBeat;
        private double currentBeat;
        [SerializeField] private float shadowStartY = 12f;
        [SerializeField] private float shadowEndY = 15f;
        [SerializeField] private float floorY = 12f;
        [SerializeField] private float shadowYOffset = -0.2f;
        [SerializeField] private float shadowMinSize = 0.4f;

        private Transform shadow;

        public BezierCurve3D[] curve;
        private BezierCurve3D currentCurve;
        [SerializeField] SpriteRenderer ballSR;
        [SerializeField] SpriteRenderer shadowSR;
        [SerializeField] SpriteRenderer bigBallSR;
        [SerializeField] SpriteRenderer bigShadowSR;

        private HoleInOne game;

        public void Init(bool monkey)
        {
            game = HoleInOne.instance;
            shadow = transform.Find("Shadow");

            if (bigBallSR != null) bigBallSR.enabled = false;
            if (bigShadowSR != null) bigShadowSR.enabled = false;

            if (monkey)
            {
                currentBeat = startBeat + 1;
                game.ScheduleInput(startBeat, 2f, HoleInOne.InputAction_FlickPress, MonkeySuccess, MonkeyMiss, Empty);
                currentCurve = curve[0];
            }
            else 
            {
                currentBeat = startBeat + 2;
                game.ScheduleInput(startBeat + 1, 2f, HoleInOne.InputAction_FlickPress, MandrillSuccess, MandrillMiss, Empty);
                currentCurve = curve[0];

                ballSR.enabled = false;
                shadowSR.enabled = false;
            }
        }
        void Update()
        {
            var cond = Conductor.instance;

            if (currentCurve is not null)
    {
                // monkey throw
                if (currentCurve == curve[0])
                {
                    float curveProg = cond.GetPositionFromBeat(currentBeat, 3);
                    if (curveProg > 0.55f) curveProg = (curveProg - 0.55f) / 4 + 0.55f;

                    transform.position = currentCurve.GetPoint(curveProg);

                    if (shadow != null)
                    {
                        float shadowY = Mathf.Lerp(shadowStartY, shadowEndY, curveProg);
                        shadow.position = new Vector3(transform.position.x, shadowY, transform.position.z);
                        shadow.localScale = Vector3.one;
                    }
                }
                // player hit
                else if (currentCurve == curve[1])
                {
                    float curveProg = cond.GetPositionFromBeat(currentBeat, 2);
                    transform.position = currentCurve.GetPoint(curveProg);

                    if (shadow != null)
                    {
                        float startY = floorY;
                        float endY = currentCurve.GetPoint(1f).y;

                        if (curveProg > 0.23f)
                        {
                            if (shadowSR != null)
                            {
                                shadowSR.sortingOrder = -101;
                                ballSR.sortingOrder = -99;
                                bigBallSR.sortingOrder = -99;
                                bigShadowSR.sortingOrder = -101;
                            }
                        }

                        float shadowY = Mathf.Lerp(startY, endY, curveProg);

                        shadow.position = new Vector3(transform.position.x, shadowY, transform.position.z);
                        float newSize = Mathf.Lerp(1f, shadowMinSize, curveProg);
                        shadow.localScale = new Vector3(newSize, newSize, 1f);
                    }
                }
            }
        }

        public void MonkeySuccess(PlayerActionEvent caller, float state)
        {
            double beat = caller.startBeat + caller.timer;

            if (state >= 0.8f || state <= -0.8f)
            {
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_SHOT_LEFT");
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WATER_IN_LEFT", beat + 1f);

                game.MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyMissHead", 1f);
                game.GolferAnim.Play("GolferMiss", 0, 0);
                Destroy(gameObject);
                return;
            }
            int randomSuccess = UnityEngine.Random.Range(1, 4);
            currentCurve = curve[1];
            currentBeat = beat;
            SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_SHOT", beat);

            if (game.isWhale)
            {
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WHALE", beat + 2f);
            }
            else
            {
                switch (randomSuccess)
                {
                    case 1:
                        SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_CUP_IN", beat + 2f);
                        break;
                    case 2:
                        SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_CUP_IN_ROLL", beat + 2f);
                        break;
                    case 3:
                        MultiSound.Play(new MultiSound.Sound[] {
                            new MultiSound.Sound("holeInOne/backspin1", beat + 2f),
                            new MultiSound.Sound("holeInOne/backspin2", beat + 3f)
                        });
                        break;
                    default:
                        SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_CUP_IN_ROLL", beat + 2f);
                        break;
                }
            }
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,          delegate { game.MonkeyHeadAnim.DoScaledAnimationAsync("MonkeyJustHead", 1f); }),
                new BeatAction.Action(beat,          delegate { game.GolferAnim.Play("GolferJust", 0, 0); }),
                new BeatAction.Action(beat + 1.5f,   delegate { game.Hole.SetActive(true); }),
                new BeatAction.Action(beat + 2f,     delegate {
                    game.HoleAnim.DoScaledAnimationAsync("ZoomSmall" + randomSuccess, 1f);
                    Destroy(gameObject);
                }),
            });
        }

        public void MandrillSuccess(PlayerActionEvent caller, float state)
        {
            double beat = caller.startBeat + caller.timer;
    
            if (bigBallSR != null) bigBallSR.enabled = true;
            if (bigShadowSR != null) bigShadowSR.enabled = true;

            if (state >= 0.8f || state <= -0.8f)
            {
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_SHOT_LEFT");
                SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_WATER_IN_LEFT", beat + 1f);

                game.BallEffectAnim.DoScaledAnimationAsync("BallEffectJust", 1f);
                game.GolferAnim.DoScaledAnimationAsync("GolferJust", 1f);
                Destroy(gameObject);
                return;
            }

            currentCurve = curve[1];
            currentBeat = beat;

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("holeInOne/SE_GOLF_SHOT_GORILLA_BIG_BALL", beat),
                new MultiSound.Sound((game.isWhale) ? "holeInOne/SE_GOLF_WHALE" : ("holeInOne/SE_GOLF_CUP_IN_GORILLA"), beat + 2f)
            });

            game.BallEffectAnim.DoScaledAnimationAsync("BallEffectJust", 1f);

            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat,       delegate { game.GolferAnim.Play("GolferJust", 0, 0);}),
                new BeatAction.Action(beat + 1.5f, delegate { game.Hole.SetActive(true);}),
                new BeatAction.Action(beat + 2f,   delegate {
                    game.HoleAnim.DoScaledAnimationAsync("ZoomBig", 1f);
                    Destroy(gameObject);
                }),
            });
        }

        public void MandrillMiss(PlayerActionEvent caller)
        {
            ballSR.enabled = true;
            shadowSR.enabled = true;
            SoundByte.PlayOneShotGame("holeInOne/SE_GOLF_MISS_BALL_ATTACK_BIG_GORI");
            SoundByte.PlayOneShotGame($"holeInOne/SE_GOLF_MISS_BALL_ATTACK_BIG_VOICE{UnityEngine.Random.Range(1, 6)}");

            game.MonkeyAnim.DoScaledAnimationAsync("MonkeySpin", 1f);
            game.BallEffectAnim.DoScaledAnimationAsync("BallEffectThrough", 1f);
            game.GolferAnim.DoScaledAnimationAsync("GolferThroughMandrill", 1f);

            game.canBop = false;
            BeatAction.New(game, new List<BeatAction.Action>() {
        new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
            game.canBop = true;
            Destroy(gameObject);
        })
    });
        }

        public void MonkeyMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame($"holeInOne/SE_GOLF_MISS_THROUGH{UnityEngine.Random.Range(1, 4)}");
            game.MonkeyHeadAnim.DoScaledAnimationAsync("MonkeySadHead", 1f);

            game.canBop = false;
            BeatAction.New(game, new List<BeatAction.Action>() {
        new BeatAction.Action(caller.startBeat + caller.timer + 2f, delegate {
            game.canBop = true;
            Destroy(gameObject);
        })
    });
        }

        public void Empty(PlayerActionEvent caller) {}
    }
}