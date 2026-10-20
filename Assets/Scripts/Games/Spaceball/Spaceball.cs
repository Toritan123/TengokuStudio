using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbBatterLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceball", "Spaceball", "000073", false, false, new List<GameAction>()
            {
                new GameAction("shoot", "Pitch Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, false, false, false, eventCaller.currentEntity["type"]); },
                    preFunction = delegate { Spaceball.instance.PrepareDispenser(); },
                    preFunctionLength = 1,
                    defaultLength = 2, 
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "Set the object to shoot.") 
                    } 
                },
                new GameAction("shootFast", "Pitch Fast Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, false, true, false, eventCaller.currentEntity["type"]); },
                    preFunction = delegate { Spaceball.instance.PrepareDispenser(); },
                    preFunctionLength = 1,
                    defaultLength = 1, 
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "Set the object to shoot.") 
                    } 
                },
				new GameAction("shootHigh", "Pitch High Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, true, false, false, eventCaller.currentEntity["type"]); },
                    preFunction = delegate { Spaceball.instance.PrepareDispenser(); },
                    preFunctionLength = 1,
                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "Set the object to shoot.") 
                    } 
                },
				new GameAction("shootOff", "Pitch Offbeat Ball")
                {
                    function = delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, false, false, true, eventCaller.currentEntity["type"]); },
                    preFunction = delegate { Spaceball.instance.PrepareDispenser(); },
                    preFunctionLength = 1,
                    defaultLength = 2f,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", Spaceball.BallType.Baseball, "Type", "Set the object to shoot.") 
                    } 
                },
				new GameAction("costume", "Batter Costume")
                {
                    function = delegate { Spaceball.instance.Costume(eventCaller.currentEntity["type"], eventCaller.currentEntity["Mask"], eventCaller.currentEntity["SkinColor"], eventCaller.currentEntity["ClothesColor"], eventCaller.currentEntity["ClothesColor2"], eventCaller.currentEntity["BatColor"]); },
                    parameters = new List<Param>() 
                    {
                        new Param("type", Spaceball.CostumeType.Standard, "Type", "Set the costume for the batter to wear.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)Spaceball.CostumeType.Custom, new string[]{ "Mask", "SkinColor", "ClothesColor", "ClothesColor2", "BatColor"})
                        }),
                        new Param("Mask", Spaceball.MaskType.None, "Mask", "Set the batter's mask."),
                        new Param("SkinColor", new Color(0.3882353f, 0.9058824f, 0), "Skin", "Set the Space Batter's skin color."),
                        new Param("ClothesColor", new Color(1, 1, 1), "Clothes", "Set the Space Batter's clothes color."),
                        new Param("ClothesColor2", new Color(0, 0, 0), "Clothes 2", "Set the Space Batter's clothes color."),
                        new Param("BatColor", new Color(0.8078431f, 0.7411765f, 0), "Bat", "Set the Space Batter's bat color.")                    } 
                },
                new GameAction("alien", "Space Umpire Animation")
                {
                    function = delegate { Spaceball.instance.alien.Show(eventCaller.currentEntity.beat, eventCaller.currentEntity["hide"]); },
                    parameters = new List<Param>()
                    {
                        new Param("hide", false, "Hide", "Toggle if Space Umpire should be hidden from the scene.")
                    }
                },
                new GameAction("camera", "Zoom Camera")
                {
                    defaultLength = 4, 
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(1, 320, 10), "Zoom", "Set the level to zoom to."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.") 
                    } 
                },
                new GameAction("prepare dispenser", "Dispenser Prepare")
                {
                    function = delegate { Spaceball.instance.PrepareDispenser(); }, 
                },
                new GameAction("fade background", "Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; Spaceball.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["startRoomColor"], e["endRoomColor"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Spaceball.defaultBGColor, "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Spaceball.defaultBGColor, "End Color", "Set the color at the end of the event."),
                        new Param("startRoomColor", Spaceball.defaultRoomColor, "Start Room Color", "Set the color of the room at the start of the event."),
                        new Param("endRoomColor", Spaceball.defaultRoomColor, "End Room Color", "Set the color of the room at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
            },
            new List<string>() {"agb", "normal"},
            "agbbatter", "en",
            new List<string>() {},
            chronologicalSortKey: 4
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_Spaceball;

    public class Spaceball : Minigame
    {
		public enum BallType {
            Baseball = 0,
            Onigiri = 1,
            Alien = 2,
            Apple = 4,
            Star = 5,
            Tacobell = 3,
        }
		
        public enum CostumeType {
            Standard,
            Bunny,
            SphereHead,
            StandardCyan,
            OrangeAlien,
            SquareHead,
            Custom,
        }

        public enum MaskType {
            None = 0,
            Bunny = 1,
            SphereHead = 2,
            OrangeAlien = 4,
            SquareHead = 5
        }


        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer square;
        [SerializeField] SpriteRenderer room, hole, shadow, shadow2;
        [SerializeField] Sprite Placeholder;

        [SerializeField] GameObject Ball;
        [SerializeField] GameObject BallsHolder;

        [SerializeField] GameObject Dispenser;
        public GameObject Dust;

        private float lastCamDistance;
        private float currentZoomCamBeat;
        private float currentZoomCamLength;
        private float currentZoomCamDistance;

        private int currentZoomIndex;

        public static Color defaultBGColor = new Color(0, 0f, 0.4509804f);

        public static Color defaultRoomColor = new Color(0, 0.8588235f, 0.07058824f);

        [SerializeField] Sprite[] BallSprites;
        [SerializeField] Material[] CostumeColors;

        private List<RiqEntity> _allCameraEvents = new List<RiqEntity>();

        public Alien alien;

        private Util.EasingFunction.Ease lastEase;

        public static Spaceball instance { get; set; }

        public override void OnGameSwitch(double beat)
        {
            for (int i = 1; i < BallsHolder.transform.childCount; i++)
                Destroy(BallsHolder.transform.GetChild(i).gameObject);
            PersistColor(beat);
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnTimeChange()
        {
            UpdateCameraZoom();
        }

        private void Awake()
        {
            instance = this;
            var camEvents = EventCaller.GetAllInGameManagerList("spaceball", new string[] { "camera" });
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            _allCameraEvents = tempEvents;

            currentZoomCamDistance = -10;
        }

        private void Update()
        {
            BackgroundColorUpdate();
            if (_allCameraEvents.Count > 0)
            {
                if (currentZoomIndex < _allCameraEvents.Count && currentZoomIndex >= 0)
                {
                    if (Conductor.instance.songPositionInBeatsAsDouble >= _allCameraEvents[currentZoomIndex].beat)
                    {
                        UpdateCameraZoom();
                        currentZoomIndex++;
                    }
                }

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(currentZoomCamBeat, currentZoomCamLength);

                if (normalizedBeat >= 0)
                {
                    if (normalizedBeat > 1)
                    {
                        GameCamera.AdditionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                    }
                    else
                    {
                        if (currentZoomCamLength < 0)
                        {
                            GameCamera.AdditionalPosition = new Vector3(0, 0, currentZoomCamDistance + 10);
                        }
                        else
                        {
                            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);

                            float newPosZ = func(lastCamDistance + 10, currentZoomCamDistance + 10, normalizedBeat);
                            GameCamera.AdditionalPosition = new Vector3(0, 0, newPosZ);
                        }
                    }
                }
                else
                {
                    // ?
                    GameCamera.AdditionalPosition = new Vector3(0, 0, 0);
                }
            }
        }

        private void UpdateCameraZoom()
        {
            if (_allCameraEvents.Count == 0)
                currentZoomCamDistance = -10;

            if (currentZoomIndex < _allCameraEvents.Count && currentZoomIndex >= 0)
            {
                if (currentZoomIndex - 1 >= 0)
                    lastCamDistance = _allCameraEvents[currentZoomIndex - 1]["valA"] * -1;
                else
                {
                    if (currentZoomIndex == 0)
                        lastCamDistance = -10;
                    else
                        lastCamDistance = _allCameraEvents[0]["valA"] * -1;
                }

                currentZoomCamBeat = (float)_allCameraEvents[currentZoomIndex].beat;
                currentZoomCamLength = _allCameraEvents[currentZoomIndex].length;

                float dist = _allCameraEvents[currentZoomIndex]["valA"] * -1;

                if (dist > 0)
                    currentZoomCamDistance = 0;
                else
                    currentZoomCamDistance = dist;

                lastEase = (Util.EasingFunction.Ease) _allCameraEvents[currentZoomIndex]["ease"];
            }
        }

        public void Shoot(double beat, bool high, bool quick, bool offbeat, int type)
        {
            GameObject ball = Instantiate(Ball);
            ball.transform.parent = Ball.transform.parent;
            ball.SetActive(true);
            ball.GetComponent<SpaceballBall>().startBeat = beat;

            if (high)
            {
                ball.GetComponent<SpaceballBall>().high = true;
                SoundByte.PlayOneShotGame("spaceball/longShoot");
            }
            else if (offbeat)
            {
                ball.GetComponent<SpaceballBall>().offbeat = true;
                SoundByte.PlayOneShotGame("spaceball/longShoot", -1, 0.5f);
            }
            if (quick)
            {
                ball.GetComponent<SpaceballBall>().quick = true;
                SoundByte.PlayOneShotGame("spaceball/longShoot" , -1, 1.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("spaceball/shoot");
            }

            ball.GetComponent<SpaceballBall>().Sprite.sprite = BallSprites[type];
            switch(type)
            {
                case (int)BallType.Baseball:
                    break;
                case (int)BallType.Onigiri:
                    ball.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case (int)BallType.Alien:
                    break;
                case (int)BallType.Tacobell:
                    ball.transform.localScale = new Vector3(2f, 2f, 1);
                    ball.GetComponent<SpaceballBall>().isTacobell = true;
                    break;
                case (int)BallType.Apple:
                    ball.transform.localScale = new Vector3(5f, 5f, 1);
                    break;
                case (int)BallType.Star:
                    ball.transform.localScale = new Vector3(6f, 6f, 1);
                    break;
            }

            Dispenser.GetComponent<Animator>().Play("DispenserShoot", 0, 0);
        }

        public void PrepareDispenser()
        {
            Dispenser.GetComponent<Animator>().Play("DispenserPrepare", 0, 0);
        }

        public void Costume(int type, int mask, Color skin, Color clothes, Color clothes2, Color bat)
        {
            CostumeColors[6].SetColor("_ColorAlpha", clothes2);
            CostumeColors[6].SetColor("_ColorBravo", skin);
            CostumeColors[6].SetColor("_ColorDelta", clothes);
            SpaceballPlayer.instance.BatColors[6] = bat;

            SpaceballPlayer.instance.SetCostume(CostumeColors[type], type, mask);
        }

        //color stuff

        private ColorEase bgColorEase = new(defaultBGColor);
        private ColorEase bgColorRoomEase = new(defaultRoomColor);

        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, Color startRoomColor, Color endRoomColor, int ease)
        {
            bgColorEase = new(beat, length, startColor, endColor, ease);
            bgColorRoomEase = new(beat, length, startRoomColor, endRoomColor, ease);
        }

                private void BackgroundColorUpdate()
        {
            bg.color = bgColorEase.GetColor();
            square.color = bgColorEase.GetColor();
            room.color = bgColorRoomEase.GetColor();
            hole.color = bgColorRoomEase.GetColor();
            shadow.color = bgColorRoomEase.GetColor();
            shadow2.color = bgColorRoomEase.GetColor();
        }

        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("spaceball", new string[] { "fade background" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"],  lastEvent["startRoomColor"], lastEvent["endRoomColor"], lastEvent["ease"]);
            }
        }
    }
}