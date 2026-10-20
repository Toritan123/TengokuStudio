using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrGuiroLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("loveLizards", "Love Lizards", "f8c840", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out LoveLizards instance)) {
                            instance.Bop(e.beat, e.length, e["toggle"], e["auto"]);
                        }
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", LoveLizards.WhoBops.Both, "Bop", "Set the characters to bop for the duration of this event."),
                        new Param("auto", LoveLizards.WhoBops.None, "Bop (Auto)", "Set the characters to automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("dance", "Mating Dance")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out LoveLizards instance)) {
                            instance.Dance(e.beat, e["toggle"], e["react"]);
                        }
                    },
                    defaultLength = 4f,
                    preFunctionLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Triplet Rhythm", "Toggle if this cue should use the triplet rhythm from Remix 3 (DS)."),
                        new Param("react", true, "Auto React", "Toggle if the male lizard should automatically react to your performance."),
                    }
                },
                new GameAction("React", "Success/Fail Reaction")
                {
                    function = delegate {
                        if (eventCaller.gameManager.TryGetMinigame(out LoveLizards instance)) {
                            instance.MaleReact();
                        }
                    },
                    defaultLength = 0.5f
                },
                new GameAction("toggle female", "Toggle Female Lizard")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.TryGetMinigame(out LoveLizards instance)) {
                            instance.ToggleFemale(e["toggle"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Female Lizard", "Toggle if the Female Lizard should be active."),
                    }
                },
                new GameAction("changeBG", "Background Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        LoveLizards.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["start2"], e["end2"], e["start3"], e["end3"], e["shadowStart"], e["shadowEnd"], e["ease"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", LoveLizards.defaultColor1, "Start Color (Back)", "Set the color of the back background layer at the start."),
                        new Param("end", LoveLizards.defaultColor1, "End Color (Back)", "Set the color of the back background layer at the end."),
                        new Param("start2", LoveLizards.defaultColor2, "Start Color (Middle)", "Set the color of the middle background layer at the start."),
                        new Param("end2", LoveLizards.defaultColor2, "End Color (Middle)", "Set the color of the middle background layer at the end."),
                        new Param("start3", LoveLizards.defaultColor3, "Start Color (Front)", "Set the color of the front background layer at the start."),
                        new Param("end3", LoveLizards.defaultColor3, "End Color (Front)", "Set the color of the front background layer at the end."),
                        new Param("shadowStart", LoveLizards.defaultShadowColor, "Start Color (Shadow)", "Set the shadow color at the start."),
                        new Param("shadowEnd", LoveLizards.defaultShadowColor, "End Color (Shadow)", "Set the shadow color at the end."),

                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                }
            },
            new List<string>() { "ntr", "normal" },
            "ntrlizards", "en",
            new List<string>() { },
            chronologicalSortKey: 11,
            font: "FOT-SlumpStd-DB SDF"
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class LoveLizards : Minigame
    {
        public enum WhoBops
        {
            Both,
            FemaleLizard,
            MaleLizard,
            None,
        }
        [SerializeField] Animator MaleLizard;
        [SerializeField] Animator FemaleLizard;
        [SerializeField] Animator Guide;

        public SpriteRenderer background1;
        public SpriteRenderer background2;
        public SpriteRenderer background3;

        public Material shadowMaterial;

        public static Color defaultColor1 = new Color(155 / 255f, 91 / 255f, 0 / 255f);   
        public static Color defaultColor2 = new Color(202 / 255f, 138 / 255f, 28 / 255f); 
        public static Color defaultColor3 = new Color(254 / 255f, 198 / 255f, 56 / 255f);
        public static Color defaultShadowColor = new Color(171 / 255f, 100 / 255f, 4 / 255f); 

        private ColorEase colorEase1 = new (defaultColor1);
        private ColorEase colorEase2 = new (defaultColor2);
        private ColorEase colorEase3 = new (defaultColor3);
        private ColorEase shadowColorEase = new (defaultShadowColor);

        private Color _cachedOriginalShadowColor;

        private bool dPad = false;
        private bool hasMissed = false;
        private bool maleBop = true;
        private bool femaleBop = true;
        private bool bopL = false;
        private bool femaleToggle = true;
        private bool canBop = true;
        private bool doingCue = false;
        private bool guideActive = true;
        private double cueBeat = double.MinValue;

        private List<PlayerActionEvent> nrmEvents = new ();

        public static LoveLizards instance;

        protected static bool IA_PadDPadPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        protected static bool IA_PadEastPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_TouchPress(out double dt)
        {
            return PlayerInput.GetSlide(out dt);
        }

        public static PlayerInput.InputAction InputAction_Nrm =
            new ("NtrGuiroEast", new int[] { IAPressCat, IAPressCat, IAPressCat
    },
            IA_PadEastPress, IA_TouchPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Pad =
            new ("NtrGuiroPad", new int[] { IAPressCat, IAPressCat, IAPressCat
},
            IA_PadDPadPress, IA_TouchPress, IA_BatonBasicPress);

void Awake()
{
    instance = this;
    guideActive = PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch;
    Guide.gameObject.SetActive(guideActive);

    if (shadowMaterial != null)
    {
        _cachedOriginalShadowColor = shadowMaterial.GetColor("_AddColor");
    }
}

private void OnDestroy()
{
    if (shadowMaterial != null)
    {
        shadowMaterial.SetColor("_AddColor", _cachedOriginalShadowColor);
    }
}

public void Update()
{
    BackgroundColorUpdate();

    if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm) && !dPad && femaleToggle && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
    {
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardSlideUp", 0.5f, animLayer: 1);
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardMouthUp", 0.5f, animLayer: 3);
        if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) FemaleLizard.DoScaledAnimationAsync("FemaleLizardHold", 0.5f, animLayer: 2);
        if (guideActive) Guide.DoScaledAnimationAsync("GuideUp");
        SoundByte.PlayOneShotGame("loveLizards/female" + UnityEngine.Random.Range(1, 6));
        SetInputs(true);
        hasMissed = true;
    }
    if (PlayerInput.GetIsAction(InputAction_Pad) && !IsExpectingInputNow(InputAction_Pad) && dPad && femaleToggle && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
    {
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardSlideDown", 0.5f, animLayer: 1);
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardMouthDown", 0.5f, animLayer: 3);
        if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) FemaleLizard.DoScaledAnimationAsync("FemaleLizardHold", 0.5f, animLayer: 2);
        if (guideActive) Guide.DoScaledAnimationAsync("GuideDown");
        SoundByte.PlayOneShotGame("loveLizards/female" + UnityEngine.Random.Range(1, 6));
        SetInputs(false);
        hasMissed = true;
    }
    if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Pad) && femaleToggle && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
    {
        FemaleLizard.DoScaledAnimationAsync(dPad ? "FemaleLizardSlideDown" : "FemaleLizardSlideUp", 0.5f, animLayer: 1);
        FemaleLizard.DoScaledAnimationAsync(dPad ? "FemaleLizardMouthDown" : "FemaleLizardMouthUp", 0.5f, animLayer: 3);
        if (guideActive) Guide.DoScaledAnimationAsync(dPad ? "GuideDown" : "GuideUp");

        SoundByte.PlayOneShotGame("loveLizards/female" + UnityEngine.Random.Range(1, 6));
        SetInputs(!dPad);
        hasMissed = true;
    }
    if (PlayerInput.GetIsAction(InputAction_BasicPress) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && femaleToggle)
    {
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardHold", 0.5f, animLayer: 2);
    }
    if (PlayerInput.GetIsAction(InputAction_BasicRelease) && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && femaleToggle)
    {
        FemaleLizard.DoScaledAnimationAsync("FemaleLizardRelease", 0.5f, animLayer: 2);
    }
    float normalizedBeat = conductor.GetPositionFromBeat(cueBeat, 3);
    if (normalizedBeat >= 0)
    {
        if (normalizedBeat > 1f)
        {
            doingCue = false;
        }
        else
        {
            doingCue = true;
        }
    }
}

private void BackgroundColorUpdate()
{
    if (background1 != null) background1.color = colorEase1.GetColor();
    if (background2 != null) background2.color = colorEase2.GetColor();
    if (background3 != null) background3.color = colorEase3.GetColor();

    if (shadowMaterial != null)
    {
        shadowMaterial.SetColor("_AddColor", shadowColorEase.GetColor());
    }
}

public void BackgroundColor(double beat, float length, Color start1, Color end1, Color start2, Color end2, Color start3, Color end3, Color shadowStart, Color shadowEnd, int ease)
{
    colorEase1 = new ColorEase(beat, length, start1, end1, ease);
    colorEase2 = new ColorEase(beat, length, start2, end2, ease);
    colorEase3 = new ColorEase(beat, length, start3, end3, ease);
    shadowColorEase = new ColorEase(beat, length, shadowStart, shadowEnd, ease);
}

private void PersistColor(double beat)
{
    var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("loveLizards", new string[] { "changeBG" }).FindAll(x => x.beat < beat);
    if (allEventsBeforeBeat.Count > 0)
    {
        allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat));
        var lastEvent = allEventsBeforeBeat[^ 1];
        BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["start2"], lastEvent["end2"], lastEvent["start3"], lastEvent["end3"], lastEvent["shadowStart"], lastEvent["shadowEnd"], lastEvent["ease"]);
    }
}

public override void OnGameSwitch(double beat)
{
    DoInactiveDance(beat);
    PersistColor(beat);
}

private void DoInactiveDance(double beat)
{
    RiqEntity danceEntity = gameManager.Beatmap.Entities.FindLast(e => e.beat - 1 < beat && e.beat + e.length >= beat && e.datamodel == "loveLizards/dance");
    if (danceEntity != null)
    {
        RiqEntity e = danceEntity;
        Dance(e.beat, e["toggle"], e["react"]);
    }
}

public override void OnLateBeatPulse(double beat)
{
    if (maleBop && !MaleLizard.IsPlayingAnimationNames())
    {
        BopOnce((int)WhoBops.MaleLizard);
    }
    if (femaleBop && !FemaleLizard.IsPlayingAnimationNames())
    {
        BopOnce((int)WhoBops.FemaleLizard);
    }
    bopL = !bopL;
}

public void Bop(double beat, float length, int whoBops, int whoBopsAuto)
{
    bopL = false;

    List<BeatAction.Action> actions = new ();

    for (int i = 0; i < length; i++)
    {
        actions.Add(new (beat + i, () => { BopOnce(whoBops); }));
}

            switch (whoBopsAuto)
            {
                case ((int) WhoBops.Both):
                    maleBop = true;
                    femaleBop = true;
                    break;
                case ((int) WhoBops.MaleLizard):
                    maleBop = true;
                    femaleBop = false;
                    break;
                case ((int) WhoBops.FemaleLizard):
                    maleBop = false;
                    femaleBop = true;
                    break;
                case ((int) WhoBops.None):
                    maleBop = false;
                    femaleBop = false;
                    break;
            }

            _ = BeatAction.New(this, actions);
}

private void BopOnce(int who)
{
    if (!canBop) return;
    switch (who)
    {
        case ((int)WhoBops.Both):
            MaleLizard.DoScaledAnimationAsync(!bopL ? "MaleLizardBopL" : "MaleLizardBopR", 0.5f, animLayer: 0);
            FemaleLizard.DoScaledAnimationAsync(bopL ? "FemaleLizardBopL" : "FemaleLizardBopR", 0.5f, animLayer: 0);
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) FemaleLizard.DoScaledAnimationAsync("Idle", 0.5f, animLayer: 2);
            break;
        case ((int)WhoBops.MaleLizard):
            MaleLizard.DoScaledAnimationAsync(!bopL ? "MaleLizardBopL" : "MaleLizardBopR", 0.5f, animLayer: 0);
            break;
        case ((int)WhoBops.FemaleLizard):
            FemaleLizard.DoScaledAnimationAsync(bopL ? "FemaleLizardBopL" : "FemaleLizardBopR", 0.5f, animLayer: 0);
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch) FemaleLizard.DoScaledAnimationAsync("Idle", 0.5f, animLayer: 2);
            break;
        case ((int)WhoBops.None):
            break;
    }
}

public void Dance(double beat, bool toggle, bool react)
{
    List<BeatAction.Action> actions = new ();
    List<MultiSound.Sound> sounds = new ();

    actions.Add(new (beat - 1, () =>
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardStepR", 0.5f, animLayer: 0);
        if (femaleToggle) FemaleLizard.DoScaledAnimationAsync("FemaleLizardStepR", 0.5f, animLayer: 0);
        cueBeat = beat - 1;
        canBop = false;
    }, ignoreSwing: toggle));

    actions.Add(new (beat - (toggle ? 0.33 : 0.25), () =>
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardStepL", 0.5f, animLayer: 0);
        if (femaleToggle) FemaleLizard.DoScaledAnimationAsync("FemaleLizardStepL", 0.5f, animLayer: 0);
    }, ignoreSwing: toggle));
    actions.Add(new (beat, () =>
            {
        MaleLizard.DoScaledAnimationAsync("MaleLizardStepR", 0.5f, animLayer: 0);
        if (femaleToggle) FemaleLizard.DoScaledAnimationAsync("FemaleLizardStepR", 0.5f, animLayer: 0);
    }, ignoreSwing: toggle));
    actions.Add(new (beat + (toggle ? 0.67 : 0.5), () =>
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardStepL", 0.5f, animLayer: 0);
        if (femaleToggle)
        {
            FemaleLizard.DoScaledAnimationAsync("FemaleLizardStepL", 0.5f, animLayer: 0);
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.Autoplay) FemaleLizard.DoScaledAnimationAsync("FemaleLizardHold", 0.5f, animLayer: 2);
        }
    }, ignoreSwing: toggle));

    sounds.Add(new MultiSound.Sound("loveLizards/cowbell", beat - (toggle ? 0.33 : 0.25), ignoreSwing: toggle));
    sounds.Add(new MultiSound.Sound("loveLizards/cowbell", beat, ignoreSwing: toggle));
    sounds.Add(new MultiSound.Sound("loveLizards/cowbell", beat + (toggle ? 0.67 : 0.5), ignoreSwing: toggle));

    actions.Add(new (beat + 1, () =>
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardShakeL", 0.5f, animLayer: 0);
        FemaleLizard.DoScaledAnimationAsync("Idle", animLayer: 0);
    }, ignoreSwing: toggle));
    actions.Add(new (beat + (toggle ? 1.33 : 1.25), () => { MaleLizard.DoScaledAnimationAsync("MaleLizardShakeR", 0.5f, animLayer: 0); }, ignoreSwing: toggle));
    actions.Add(new (beat + (toggle ? 1.67 : 1.5), () => { MaleLizard.DoScaledAnimationAsync("MaleLizardShakeL", 0.5f, animLayer: 0); }, ignoreSwing: toggle));
    actions.Add(new (beat + (toggle ? 2 : 1.75), () => { MaleLizard.DoScaledAnimationAsync("MaleLizardShakeR", 0.5f, animLayer: 0); }, ignoreSwing: toggle));

    sounds.Add(new MultiSound.Sound("loveLizards/maleShake", beat + 1, ignoreSwing: toggle));
    sounds.Add(new MultiSound.Sound("loveLizards/maleShake", beat + (toggle ? 1.33 : 1.25), ignoreSwing: toggle));
    sounds.Add(new MultiSound.Sound("loveLizards/maleShake", beat + (toggle ? 1.67 : 1.5), ignoreSwing: toggle));
    sounds.Add(new MultiSound.Sound("loveLizards/maleShake", beat + (toggle ? 2 : 1.75), ignoreSwing: toggle));

    actions.Add(new (beat + 2, () => { if (!doingCue) canBop = true; }));

    if (femaleToggle)
    {
        var inputType = dPad ? InputAction_Pad : InputAction_Nrm;
        nrmEvents.Add(ScheduleInput(beat, 1, inputType, OnHit, OnMiss, null, ignoreSwing: toggle));
        nrmEvents.Add(ScheduleInput(beat, toggle ? 1.33 : 1.25, inputType, OnHit, OnMiss, null, ignoreSwing: toggle));
        nrmEvents.Add(ScheduleInput(beat, toggle ? 1.67 : 1.5, inputType, OnHit, OnMiss, null, ignoreSwing: toggle));
        nrmEvents.Add(ScheduleInput(beat, toggle ? 2 : 1.75, inputType, OnHit, OnMiss, null, ignoreSwing: toggle));

        actions.Add(new (beat + (toggle ? 3 : 2.5), () =>
        {
            if (react) MaleReact();
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.Autoplay) FemaleLizard.DoScaledAnimationAsync("FemaleLizardRelease", 0.5f, animLayer: 2);
        }));
    }

    _ = MultiSound.Play(sounds, forcePlay: true);
    _ = BeatAction.New(this, actions);
}

public void ToggleFemale(bool toggle)
{
    femaleToggle = toggle;
    FemaleLizard.gameObject.SetActive(toggle);
    guideActive = toggle && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch;
    Guide.gameObject.SetActive(guideActive);
}

private void SetInputs(bool setDPad)
{
    dPad = setDPad;
    if (nrmEvents.Count == 0) return;
    if (setDPad)
    {
        foreach (var input in nrmEvents)
        {
            input.SetInputAction(InputAction_Pad);
        }
    }
    else
    {
        foreach (var input in nrmEvents)
        {
            input.SetInputAction(InputAction_Nrm);
        }
    }
}

public void MaleReact()
{
    if (hasMissed)
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardYawn", 0.5f, animLayer: 1);
        SoundByte.PlayOneShotGame("loveLizards/maleYawn");
    }
    else
    {
        MaleLizard.DoScaledAnimationAsync("MaleLizardSmile", 0.5f, animLayer: 1);
        SoundByte.PlayOneShotGame("loveLizards/maleHeart");
    }
    hasMissed = false;
}

public void OnHit(PlayerActionEvent caller, float state)
{
    FemaleLizard.DoScaledAnimationAsync(dPad ? "FemaleLizardSlideDown" : "FemaleLizardSlideUp", 0.5f, animLayer: 1);
    FemaleLizard.DoScaledAnimationAsync(dPad ? "FemaleLizardMouthDown" : "FemaleLizardMouthUp", 0.5f, animLayer: 3);
    if (guideActive) Guide.DoScaledAnimationAsync(dPad ? "GuideDown" : "GuideUp");

    SoundByte.PlayOneShotGame("loveLizards/female" + UnityEngine.Random.Range(1, 6));
    SetInputs(!dPad);
}

public void OnMiss(PlayerActionEvent caller)
{
    hasMissed = true;
}
    }
}