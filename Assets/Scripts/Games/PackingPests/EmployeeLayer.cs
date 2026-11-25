using UnityEngine;
using System;
using System.Collections.Generic;

using HeavenStudio.Util;
using HeavenStudio;
using HeavenStudio.Games.Scripts_PackingPests;
using HeavenStudio.Games;
using Cysharp.Threading.Tasks.Triggers;

public class EmployeeLayer : MonoBehaviour
{
    public Material materialToSet; 

    [Header("GameObjects")]
    [SerializeField] public GameObject Candy;
    [SerializeField] public GameObject Spider;
    [SerializeField] public SpriteRenderer boxfront;
    [SerializeField] public SpriteRenderer lowerHandObj;
    [SerializeField] public GameObject Walls;
    [SerializeField] public GameObject Head;
    [SerializeField] public GameObject HeadNose;
    [SerializeField] private Transform fogParent;

    [Header("Background Objects")]

    [Header("Curves")]
    [SerializeField] SuperCurveObject.Path[] objectPaths;

    [Header("Animators")]
    public Animator moveHandAnim;
    public Animator handAnim;
    public Animator lowerHandAnim;
    public Animator upperHandAnim;
    public Animator spiderCrawlAnim;
    public Animator spiderAnim;
    public Animator moveHeadAnim;
    public Animator headAnim;

    [Header("Positions")]
    private Vector3 startPos;
    private Vector3 shiftPos;
    private float shiftPosX = -2f;
    private float shiftWallsX = 0.85f;
    private bool shifted = false;
    private bool isShifting = false;
    double movingStartBeat;


    public EmployeeLayer instance;
    public bool player = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ApplyMaterialRecursively(fogParent);

        startPos = transform.localPosition;
        float newX = startPos.z * shiftPosX * 0.975f;
        if (newX == 0) 
        {
            newX += shiftPosX;
        }
        shiftPos = new(newX, startPos.y, startPos.z);

        if (startPos.z != 0) shiftWallsX *= 1 / startPos.z;

        HeadNose.SetActive(!player);
    }

    void Update()
    {
        if (isShifting)
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, 1f);
            EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutCubic);
            float eased = func(0f, 1f, normalizedBeat);

            Vector3 newPos;
            float newWallX;
            string headAnim;
            if (!shifted)
            {
                newPos = Vector3.Lerp(shiftPos, startPos, eased);
                newWallX = Mathf.Lerp(shiftWallsX, 0f, eased);
                headAnim = "MoveHeadOut";
            }
            else {
                newPos = Vector3.Lerp(startPos, shiftPos, eased);
                newWallX = Mathf.Lerp(0f, shiftWallsX, eased);
                headAnim = "MoveHead";
            }
            transform.localPosition = newPos;
            Walls.transform.localPosition = new(newWallX, 0f, 0f);
            moveHeadAnim.DoNormalizedAnimation(headAnim, eased);

            if (normalizedBeat >= 1f) isShifting = false;
        }
    }

    public void ShiftCamera(double beat, float length, bool shiftBack)
    {
        if (!shifted)
        {
            movingStartBeat = beat;
            shifted = true;
            isShifting = true;
        }  

        if (shiftBack)
        {
            BeatAction.New(PackingPests.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { 
                    if (shifted)
                    {
                        movingStartBeat = beat + length;
                        shifted = false;
                        isShifting = true;
                    }
                    
                }),
            });
        }
        
    }

    public void OnDrawGizmos()
    {
        foreach (SuperCurveObject.Path path in objectPaths)
        {
            if (path.preview)
            {
                Candy.GetComponent<ThrowObject>().DrawEditorGizmo(path);
            }
        }
    }

    public SuperCurveObject.Path GetPath(string name)
    {
        foreach (SuperCurveObject.Path path in objectPaths)
        {
            if (path.name == name)
            {
                return path;
            }
        }
        return default(SuperCurveObject.Path);
    }

    void ApplyMaterialRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.material = materialToSet;
            }

            ApplyMaterialRecursively(child);
        }
    }

    public void ThrowCandy(double beat, int sprite)
    {
        handAnim.DoScaledAnimationAsync("Beat", 0.5f);
        var candy = Instantiate(Candy, transform).GetComponent<ThrowObject>();

        candy.layer = instance;
        candy.player = player;
        candy.spider = false;
        candy.sprite = sprite;

        candy.InitCandy(beat);

        if (player)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_BALL_OUT", beat),
            }, forcePlay: true);  
        }
    }

    public void ThrowSpider(double beat)
    {
        var spider = Instantiate(Spider, transform).GetComponent<ThrowObject>();

        spider.layer = instance;
        spider.player = player;
        spider.spider = true;

        spider.InitSpider(beat);

        if (player)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("packingPests/SE_SHIWAKE_EN_INSECT_OUT", beat),
            }, forcePlay: true);  
        }
    }

    public void TryRandomThrow(double beat)
    {
        if (player) return;
        if (PackingPests.instance.nextOpenCueBeat > beat) return;

        switch (UnityEngine.Random.Range(0, 75))
        {
            case 0:
                ThrowCandy(beat, 0);
                break;
            case 1:
                ThrowCandy(beat + 0.5, 0);
                break;
            case 2:
                ThrowSpider(beat);
                break;
            case 3:
                ThrowSpider(beat + 0.5);
                break;
            default:
                break;
        }
    }
}

