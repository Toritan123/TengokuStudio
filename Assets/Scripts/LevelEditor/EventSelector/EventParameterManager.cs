using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Jukebox;
using System.Linq;
using System;
using static HeavenStudio.EntityTypes;
using HeavenStudio.Common;
using HeavenStudio.Games;
using HeavenStudio.Editor.Track;

namespace HeavenStudio.Editor
{
    public class EventParameterManager : MonoBehaviour
    {
        [Header("General References")]
        [SerializeField] private GameObject eventSelector;
        [SerializeField] private GridGameSelector gridGameSelector;

        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject NoteP;
        [SerializeField] private GameObject ButtonP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject AssetP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;
        private static Dictionary<Type, GameObject> PropertyPrefabs;


        public RiqEntity entity;
        public List<RiqEntity> entities = new();
        private List<TimelineEventObj> markers = new();

        public bool active;

        private int childCountAtStart;
        string setFont;

        public Dictionary<string, EventPropertyPrefab> currentProperties = new();

        public bool canDisable = true;

        public static EventParameterManager instance { get; set; }

        private void Awake()
        {
            instance = this;

            if (PropertyPrefabs == null)
            {
                PropertyPrefabs = new() {
                    { typeof(Integer), IntegerP },
                    { typeof(Float), FloatP },
                    { typeof(Note), NoteP },
                    { typeof(Dropdown), DropdownP },
                    { typeof(NoteSampleDropdown), DropdownP },
                    { typeof(Asset), AssetP },
                    { typeof(Button), ButtonP },
                    { typeof(Color), ColorP },
                    { typeof(bool), BooleanP },
                    { typeof(string), StringP },
                };
            }

            Editor.instance?.SetGameEventTitle("");
        }

        private void Start()
        {
            childCountAtStart = transform.childCount;
        }

        private void Update()
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                if (canDisable && active)
                {
                    Disable();
                }
            }
            canDisable = true;
        }

        public void Disable()
        {
            
            active = false;
            eventSelector.SetActive(true);

            DestroyParams();

            SetFont(GridGameSelector.instance.SelectedMinigame);
            
            Editor.instance?.SetGameEventTitle($"<font={setFont}>{gridGameSelector.SelectedMinigame.displayName.Replace("\n", "")}</font>");
        }

        public void StartParams(TimelineEventObj marker)
        {
            active = true;
            AddParams(marker);
        }

        private void AddParams(TimelineEventObj marker)
        {
            var entity = marker.entity;

            string[] split = entity.datamodel.Split('/');
            var minigame = EventCaller.instance.GetMinigame(split[0]);
            Minigames.GameAction action = minigame.actions.Find(c => c.actionName == split[1]);

            if (action.parameters != null)
            {
                DestroyParams();

                eventSelector.SetActive(false);
                this.entity = entity;
                
                this.markers.Clear();
                this.entities.Clear();

                if (Selections.instance.eventsSelected.Count > 1 && Selections.instance.eventsSelected.Any(x => x.entity == entity))
                {
                    foreach (var m in Selections.instance.eventsSelected)
                    {
                        if (m.entity.datamodel == entity.datamodel)
                        {
                            this.markers.Add(m);
                            this.entities.Add(m.entity);

                            m.editingParams = true;
                        }
                    }
                }
                else
                {
                    this.markers.Add(marker);
                    this.entities.Add(marker.entity);
                    marker.editingParams = true;
                }

                string col = ColorUtility.ToHtmlStringRGB(EditorTheme.theme.LayerGradientIndex((int)entity["track"]));

                string gameName = action.displayName;
                const int cutOff = 20;
                if (gameName.Length > (cutOff + 3)) {
                    gameName = gameName[..cutOff] + "...";
                }
                if (this.markers.Count > 1) {
                    gameName += $" ({this.markers.Count})";
                }
                
                SetFont(minigame);
                Editor.instance.SetGameEventTitle(Localization.F("<color=#{0}>{1}</color> at ♪ {2:F2} on <color=#{0}>Track {3}</color>", col, $"<font={setFont}>{gameName}</font>", entity.beat, (int)entity["track"] + 1));

                for (int i = 0; i < action.parameters.Count; i++)
                {
                    var p = action.parameters[i];
                    string caption = p.caption;
                    if (p.advanced) caption = "<color=#ff8>" + p.caption;

                    if (p.advanced && !PersistentDataManager.gameSettings.advancedFeatures)
                        currentProperties.Add(p.propertyName, null);
                    else
                        currentProperties.Add(p.propertyName, AddParam(p.propertyName, p.parameter, caption, p.tooltip));
                }

                foreach (var p in action.parameters)
                {
                    if (p.collapseParams == null || p.collapseParams.Count == 0) continue;
                    EventPropertyPrefab input = currentProperties[p.propertyName];
                    foreach (var c in p.collapseParams)
                    {
                        List<EventPropertyPrefab> collapseables = c.collapseables.Select(x => currentProperties[x]).ToList();
                        input.propertyCollapses.Add(new EventPropertyPrefab.PropertyCollapse(collapseables, c.CollapseOn, entity));
                    }
                    input.SetCollapses(p.parameter);
                }

                foreach (var p in action.parameters)
                {
                    EventPropertyPrefab prop = currentProperties[p.propertyName];

                    if (p.advanced && !PersistentDataManager.gameSettings.advancedFeatures) continue;

                    prop.PostLoadProperties(p.parameter);
                }

                active = true;
            }
            else
            {
                active = false;
            }
        }

        private EventPropertyPrefab AddParam(string propertyName, object type, string caption, string tooltip = "")
        {
            Type typeType = type.GetType();
            GameObject propertyPrefab = DropdownP; // enum check is hardcoded because enums are awesome (lying)
            if (!typeType.IsEnum && !PropertyPrefabs.TryGetValue(typeType, out propertyPrefab))
            {
                Debug.LogError("Can't make property interface of type: " + typeType);
                return null;
            }

            GameObject input = Instantiate(propertyPrefab, transform);
            input.SetActive(true);
            input.transform.localScale = Vector3.one;

            if (tooltip != string.Empty)
            {
                Tooltip.AddTooltip(input, tooltip, null, PersistentDataManager.gameSettings.showParamTooltips);

            }

            EventPropertyPrefab property = input.GetComponent<EventPropertyPrefab>();
            property.SetProperties(propertyName, type, caption);

            return property;
        }

        private void DestroyParams()
        {
            Editor.instance.editingInputField = false;
            active = false;
            for (int i = childCountAtStart; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            foreach (var m in markers)
            {
                if (m != null) m.editingParams = false;
            }

            this.markers.Clear();
            this.entities.Clear();

            currentProperties.Clear();
        }

        private void SetFont(Minigames.Minigame mg)
        {
            if (PersistentDataManager.gameSettings.fontType == 1)
            {
                Editor.instance.currentFont = "Rodin DB";
            }
            else
            {
                Editor.instance.currentFont = mg.font;
            }
            setFont = Editor.instance.currentFont;
        }
    }
}