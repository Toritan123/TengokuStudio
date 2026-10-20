using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_BossaNova
{

    public class Shape : MonoBehaviour
    {
        [SerializeField] BezierCurve3D enterCurve;
        [SerializeField] BezierCurve3D hitCurve;
        [SerializeField] BezierCurve3D missCurve;
        [SerializeField] Transform shapeTransform;

        public GameObject Shadow;
        private GameObject ShadowInstance;
        private SpriteRenderer shadowRenderer;

        public double startBeat;

        public bool isEntering;
        public bool isHit;
        public bool isMiss;
        public bool isFalling;
        public bool forBossa;
        public bool isCube;
        public bool spin;
        public int voiceline;
        public int voiceVariant;

        private float floorHeight = -3f;

        public BossaNova game;

        void Awake()
        {
            

            ShadowInstance = Instantiate(Shadow, transform);
            shadowRenderer = ShadowInstance.GetComponent<SpriteRenderer>();
            ShadowInstance.SetActive(true);
            ShadowInstance.transform.position = new Vector3(transform.position.x, floorHeight - 0.5f, transform.position.z);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (forBossa)
            {
                game.ScheduleInput(startBeat, 1, Minigame.InputAction_BasicPress, Hit, Miss, null);
            }
            else
            {
                game.ScheduleUserInput(startBeat, 1, Minigame.InputAction_BasicPress, HeadBump, null, null);
                SoundByte.PlayOneShotGame(isCube ? "bossaNova/SE_BOSSA_EN_NUT_OTHER" : "bossaNova/SE_BOSSA_EN_BALL_OTHER", startBeat + 1);
                BeatAction.New(game, new List<BeatAction.Action>
                {
                    new(startBeat + 1, delegate {
                        isEntering = false;
                        isHit = true;
                        if (!game.novaAnim.IsPlayingAnimationNames("Head Bump")) game.novaAnim.DoScaledAnimationAsync("Hit", 0.5f);
                    }),
                });
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isEntering)
            {
                float flyPos = game.conductor.GetPositionFromBeat(startBeat, 1.15f);
                transform.position = enterCurve.GetPoint(flyPos);
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime));

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
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (360f * Time.deltaTime));
            }
            if (isHit)
            {
                float flyPos = game.conductor.GetPositionFromBeat(startBeat+1, 1.75f);
                transform.position = hitCurve.GetPoint(flyPos);

                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                    Destroy(ShadowInstance);
                }
            }
            if (isFalling)
            {
                float flyPos = game.conductor.GetPositionFromBeat(startBeat + 1, 1f);
                transform.position = missCurve.GetPoint(flyPos);

                if (flyPos > 1f)
                {
                    Destroy(gameObject);
                }
            }
            if (!isFalling) 
            {
                ShadowInstance.transform.position = new Vector3(transform.position.x, floorHeight - 0.5f, transform.position.z);
                ShadowInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                game.bossaAnim.DoScaledAnimationAsync("Barely", 0.5f);
                SoundByte.PlayOneShot("miss");

                isEntering = false;
                isFalling = true;
                Destroy(ShadowInstance);
            }
            else
            {
                isEntering = false;
                isHit = true;
                SoundByte.PlayOneShotGame(isCube ? "bossaNova/SE_BOSSA_EN_NUT" : "bossaNova/SE_BOSSA_EN_BALL");

                if (game.angerLevel <= 5)
                {
                    if (game.angerLevel > 0) VoiceAngry();
                    else
                    {
                        switch (game.emotion)
                        {
                            case 0:
                                VoiceAngry();
                                break;
                            case 1:
                                VoiceNormal();
                                break;
                            case 2:
                                VoicePlayful();
                                break;
                        }
                    }
                }

                game.bossaAnim.DoScaledAnimationAsync("Hit", 0.5f);
                if (game.angerLevel > 0 && game.angerLevel <= 5) game.angerLevel--;

                if (isCube)
                {
                    game.ringL.Play();
                }
                else
                {
                    game.ringR.Play();
                }
            }
        }

        private void HeadBump(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                game.bossaAnim.DoScaledAnimationAsync("Whiff", 0.5f);
                SoundByte.PlayOneShotGame(isCube ? "bossaNova/SE_BOSSA_EN_SWING_BALL" : "bossaNova/SE_BOSSA_EN_SWING_NUT");
            }
            else
            {
                SoundByte.PlayOneShotGame("bossaNova/SE_BOSSA_EN_MISS_HEAD");
                game.ScoreMiss();
                game.angerLevel = 5;

                game.bossaAnim.DoScaledAnimationAsync("Head Bump", 0.5f);
                game.novaAnim.DoScaledAnimationAsync("Head Bump", 0.5f);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            int randomVoice = UnityEngine.Random.Range(1, 2);
            if (isCube)
            {
                int randomCube = UnityEngine.Random.Range(1, 9);

                SoundByte.PlayOneShotGame("bossaNova/SE_BOSSA_EN_NUT_MISS_" + randomCube);
                SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_" + (70 + randomVoice), volume: 0.62f);

            }
            else
            {
                SoundByte.PlayOneShotGame("bossaNova/SE_BOSSA_EN_BALL_MISS");
                SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_" + (68 + randomVoice), volume: 1.4f);
            }
            game.bossaAnim.DoScaledAnimationAsync("Miss", 0.5f);

            isEntering = false;
            isHit = false;
            isFalling = true;
            Destroy(ShadowInstance);

            game.angerLevel = 6;
        }

        // voice stuff

        private void VoiceNormal()
        {
            bool alt = voiceVariant == 1;

            switch (voiceline)
            {
                case 0: // MaleBounce1
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new(alt ? "bossaNova/Bossa/SE_BOSSA_EN_4" : "bossaNova/Bossa/SE_BOSSA_EN_8", startBeat+1, volume: alt ? 1f : 0.89f),
                        new(alt ? "bossaNova/Bossa/SE_BOSSA_EN_5" : "bossaNova/Bossa/SE_BOSSA_EN_9", startBeat+2),
                    });
                    break;
                case 1: // MaleBounce3
                    SoundByte.PlayOneShotGame(alt ? "bossaNova/Bossa/SE_BOSSA_EN_6" : "bossaNova/Bossa/SE_BOSSA_EN_10");
                    break;
                case 2: // MaleBounce4
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new(alt ? "bossaNova/Bossa/SE_BOSSA_EN_7" : "bossaNova/Bossa/SE_BOSSA_EN_11", startBeat+1, volume: alt ? 1f : 0.89f),
                        new("bossaNova/Bossa/SE_BOSSA_EN_5", startBeat+2),
                    });
                    break;
                case 3: // MaleBounce6
                    SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_6");
                    break;
                case 4: // MaleSpin
                    SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_12", volume: 0.75f);
                    break;
                case 5: // FemaleBounce2
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new(alt ? (spin ? "bossaNova/Nova/SE_BOSSA_EN_30" : "bossaNova/Nova/SE_BOSSA_EN_31") : (spin ? "bossaNova/Nova/SE_BOSSA_EN_28" : "bossaNova/Nova/SE_BOSSA_EN_29"), startBeat+1, volume: alt ? (spin ? 0.62f : 0.55f) : (spin ? 0.89f : 0.73f)),
                        new("bossaNova/Nova/SE_BOSSA_EN_37", startBeat+2, volume: 0.55f),
                    });
                    break;
                case 6: // FemaleBounce4
                    SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_34", volume: alt ? 0.89f : 0.73f);
                    break;
                case 7: // FemaleSpin
                    SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_38", volume: 0.62f);
                    break;
            }
        }

        private void VoicePlayful()
        {
            switch (voiceVariant)
            {
                case 0:
                    switch (voiceline)
                    {
                        case 0: // MaleBounce1
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_13", startBeat+1),
                        new("bossaNova/Bossa/SE_BOSSA_EN_14", startBeat+2),
                    });
                            break;
                        case 1: // MaleBounce3
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_15");
                            break;
                        case 2: // MaleBounce4
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_16", startBeat+1, volume: 0.62f),
                        new("bossaNova/Bossa/SE_BOSSA_EN_22", startBeat+2),
                    });
                            break;
                        case 3: // MaleBounce6
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_15");
                            break;
                        case 4: // MaleSpin
                            game.bossaPlayfulSpinRandomization = (game.bossaPlayfulSpinRandomization + UnityEngine.Random.Range(1, 2)) % 3;
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_" + (25 + game.bossaPlayfulSpinRandomization));
                            break;
                        case 5: // FemaleBounce2
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Nova/SE_BOSSA_EN_39", startBeat+1, volume: 0.75f),
                        new("bossaNova/Nova/SE_BOSSA_EN_37", startBeat+2, volume: 0.47f),
                    });
                            break;
                        case 6: // FemaleBounce4
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_40", volume: 0.75f);
                            break;
                        case 7: // FemaleSpin
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_45", volume: 0.67f);
                            break;
                    }
                    break;
                case 1:
                    switch (voiceline)
                    {
                        case 0: // MaleBounce1
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_17", startBeat+1),
                        new("bossaNova/Bossa/SE_BOSSA_EN_18", startBeat+2),
                    });
                            break;
                        case 1: // MaleBounce3
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_19");
                            break;
                        case 2: // MaleBounce4
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_20", startBeat+1),
                        new("bossaNova/Bossa/SE_BOSSA_EN_14", startBeat+2),
                    });
                            break;
                        case 3: // MaleBounce6
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_15");
                            break;
                        case 4: // MaleSpin
                            game.bossaPlayfulSpinRandomization = (game.bossaPlayfulSpinRandomization + UnityEngine.Random.Range(1, 2)) % 3;
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_" + (25 + game.bossaPlayfulSpinRandomization));
                            break;
                        case 5: // FemaleBounce2
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Nova/SE_BOSSA_EN_41", startBeat+1, volume: 0.75f),
                        new("bossaNova/Nova/SE_BOSSA_EN_37", startBeat+2, volume: 0.47f),
                    });
                            break;
                        case 6: // FemaleBounce4
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_42", volume: 0.56f);
                            break;
                        case 7: // FemaleSpin
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_45", volume: 0.67f);
                            break;
                    }
                    break;
                case 2:
                    switch (voiceline)
                    {
                        case 0: // MaleBounce1
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_21", startBeat+1),
                        new("bossaNova/Bossa/SE_BOSSA_EN_22", startBeat+2),
                    });
                            break;
                        case 1: // MaleBounce3
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_23");
                            break;
                        case 2: // MaleBounce4
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_24", startBeat+1),
                        new("bossaNova/Bossa/SE_BOSSA_EN_14", startBeat+2),
                    });
                            break;
                        case 3: // MaleBounce6
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_15", volume: 0.75f);
                            break;
                        case 4: // MaleSpin
                            game.bossaPlayfulSpinRandomization = (game.bossaPlayfulSpinRandomization + UnityEngine.Random.Range(1, 2)) % 3;
                            SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_" + (25 + game.bossaPlayfulSpinRandomization));
                            break;
                        case 5: // FemaleBounce2
                            MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Nova/SE_BOSSA_EN_43", startBeat+1, volume: 0.75f),
                        new("bossaNova/Nova/SE_BOSSA_EN_37", startBeat+2, volume: 0.47f),
                    });
                            break;
                        case 6: // FemaleBounce4
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_44", volume: 0.75f);
                            break;
                        case 7: // FemaleSpin
                            SoundByte.PlayOneShotGame("bossaNova/Nova/SE_BOSSA_EN_45", volume: 0.67f);
                            break;
                    }
                    break;
            }
            
        }

        private void VoiceAngry()
        {
            bool alt = voiceVariant == 1;

            switch (voiceline)
            {
                case 0: // MaleBounce1
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_47", startBeat+1, volume: 0.75f),
                        new(alt && spin ? "bossaNova/Bossa/SE_BOSSA_EN_5" : "bossaNova/Bossa/SE_BOSSA_EN_9", startBeat+2, volume: 0.75f),
                    });
                    break;
                case 1: // MaleBounce3
                    SoundByte.PlayOneShotGame(alt && spin ? "bossaNova/Bossa/SE_BOSSA_EN_6" : "bossaNova/Bossa/SE_BOSSA_EN_10", volume: 0.75f);
                    break;
                case 2: // MaleBounce4
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Bossa/SE_BOSSA_EN_48", startBeat+1, volume: 0.75f),
                        new("bossaNova/Bossa/SE_BOSSA_EN_5", startBeat+2, volume: 0.75f),
                    });
                    break;
                case 3: // MaleBounce6
                    SoundByte.PlayOneShotGame("bossaNova/Bossa/SE_BOSSA_EN_6", volume: 0.75f);
                    break;
                case 4: // MaleSpin
                    SoundByte.PlayOneShotGame(alt ? "bossaNova/Bossa/SE_BOSSA_EN_49" : "bossaNova/Bossa/SE_BOSSA_EN_50", volume: 0.75f);
                    break;
                case 5: // FemaleBounce2
                    MultiSound.Play(new List<MultiSound.Sound>()
                    {
                        new("bossaNova/Nova/SE_BOSSA_EN_51", startBeat+1, volume: 1f),
                        new("bossaNova/Nova/SE_BOSSA_EN_37", startBeat+2, volume: 0.62f),
                    });
                    break;
                case 6: // FemaleBounce4
                    SoundByte.PlayOneShotGame(alt ? "bossaNova/Nova/SE_BOSSA_EN_52" : "bossaNova/Nova/SE_BOSSA_EN_53", volume: alt ? 0.75f : 0.89f);
                    break;
                case 7: // FemaleSpin
                    SoundByte.PlayOneShotGame(alt ? "bossaNova/Nova/SE_BOSSA_EN_54" : "bossaNova/Nova/SE_BOSSA_EN_55", volume: 1f);
                    break;
            }
        }
    }
}
