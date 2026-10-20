using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

using Jukebox;
using HeavenStudio.Util;
using System.Linq;
using BurstLinq;

namespace HeavenStudio.Editor.Track
{
    public class TimelineBlockManager : MonoBehaviour
    {
        public static TimelineBlockManager Instance { get; private set; }

        [SerializeField] private TimelineEventObj EntityTemplate;
        [SerializeField] private TimelineEventObj GameSwitchTemplate;
        [SerializeField] private BlockDeleteFX BlockDeleteFXTemplate;
        public Dictionary<Guid, TimelineEventObj> EntityMarkers = new();
        public ObjectPool<TimelineEventObj> EventPool { get; private set; }
        public ObjectPool<TimelineGameSwitch> LongEventsPool { get; private set; }

        private Timeline timeline;

        private RiqEntity entityToSet;

        public bool InteractingWithEvents { get; private set; } = false;
        public bool MovingAnyEvents { get; private set; } = false;

        public void SetEntityToSet(RiqEntity entity)
        {
            entityToSet = entity;
        }

        private void Awake()
        {
            Instance = this;

            timeline = GetComponent<Timeline>();

            EventPool = new ObjectPool<TimelineEventObj>(CreateMarker, OnTakeMarkerFromPool, OnReturnMarkerToPool, OnDestroyMarker, true, 125, 1500);
            LongEventsPool = new ObjectPool<TimelineGameSwitch>(CreateLongMarker, OnTakeLongMarkerFromPool, OnReturnMarkerToPool, OnDestroyMarker, true, 10, 150);
        }

        public void Load()
        {
            var timeLeft = timeline.leftSide;
            var timeRight = timeline.rightSide;

            foreach (var marker in EntityMarkers)
            {
                Destroy(marker.Value.gameObject);
            }

            EntityMarkers.Clear();
            EventPool.Clear();
            LongEventsPool.Clear();

            foreach (var entity in GameManager.instance.Beatmap.Entities)
            {
                var vLeft = entity.beat + entity.length >= timeLeft;
                var vRight = entity.beat < timeRight;
                var active = vLeft && vRight;

                if (!active) continue;

                entityToSet = entity;
                string[] split = entity.datamodel.Split('/');
                if (split[0] == "gameManager" && (split[1] == "switchGame" || split[1] == "end" || split[1] == "startSong"))
                {
                    LongEventsPool.Get();
                }
                else
                {
                    EventPool.Get();
                }

                Debug.Log(entity.datamodel);
            }
        }

        public TimelineEventObj CreateEntity(RiqEntity entity, bool longMarker = false)
        {
            entityToSet = entity;
            var marker = longMarker ? LongEventsPool.Get() : EventPool.Get();
            marker.UpdateMarker();

            return marker;
        }

        public void OnZoom()
        {
            foreach (var marker in EntityMarkers.Values)
            {
                marker.SetWidthHeight();
            }
        }

        public void UpdateMarkerColors()
        {
            foreach (var marker in EntityMarkers.Values)
            {
                marker.SetColor();
            }
        }

        public void UpdateMarkers()
        {
            var timeLeft = timeline.leftSide;
            var timeRight = timeline.rightSide;

            for (var i = 0; i < GameManager.instance.Beatmap.Entities.Count; i++)
            {
                var entity = GameManager.instance.Beatmap.Entities[i];

                var vLeft = entity.beat + entity.length >= timeLeft;
                var vRight = entity.beat < timeRight;

                var active = vLeft && vRight;

                var containsMarker = EntityMarkers.ContainsKey(entity.guid);

                if (containsMarker)
                {
                    var marker = EntityMarkers[entity.guid];
                    if (marker.selected || marker.moving) active = true;
                }

                string[] split = entity.datamodel.Split('/');
                if (active)
                {
                    if (!containsMarker)
                    {
                        entityToSet = entity;
                        if (split[0] == "gameManager" && (split[1] == "switchGame" || split[1] == "end" || split[1] == "startSong"))
                        {
                            LongEventsPool.Get();
                        }
                        else
                        {
                            EventPool.Get();
                        }
                    }
                    EntityMarkers[entity.guid].UpdateMarker();
                }
                else
                {
                    if (containsMarker)
                    {
                        if (split[0] == "gameManager" && (split[1] == "switchGame" || split[1] == "end" || split[1] == "startSong"))
                        {
                            LongEventsPool.Release(EntityMarkers[entity.guid] as TimelineGameSwitch);
                        }
                        else
                        {
                            EventPool.Release(EntityMarkers[entity.guid]);
                        }
                    }
                }
            }

            InteractingWithEvents = false;
            MovingAnyEvents = false;
            foreach (var marker in EntityMarkers.Values)
            {
                if (marker.moving || marker.resizing || marker.mouseHovering || marker.mouseOver)
                    InteractingWithEvents = true;
                if (marker.moving)
                    MovingAnyEvents = true;
            }
        }

        public void SortMarkers()
        {
            // Debug.Log("Sorted timeline blocks.");

            var sortedBlocks = EntityMarkers.Values.OrderByDescending(c => c.entity.length).ToList();

            var i = 0;
            foreach (var block in EntityMarkers.Values)
            {
                var index = sortedBlocks.FindIndex(c => c.entity.guid == block.entity.guid);
                block.transform.SetSiblingIndex(index + 1);

                i++;
            }
        }

        private TimelineEventObj CreateMarker()
        {
            TimelineEventObj marker = Instantiate(EntityTemplate.gameObject, Timeline.instance.TimelineEventsHolder).GetComponent<TimelineEventObj>();
            return marker;
        }

        private TimelineGameSwitch CreateLongMarker()
        {
            TimelineGameSwitch marker = Instantiate(GameSwitchTemplate.gameObject, Timeline.instance.TimelineEventsHolder).GetComponent<TimelineGameSwitch>();
            return marker;
        }

        private void OnTakeMarkerFromPool(TimelineEventObj marker)
        {
            marker.SetEntity(entityToSet);
            marker.SetMarkerInfo();

            SortMarkers();

            EntityMarkers.Add(entityToSet.guid, marker);
        }

        private void OnTakeLongMarkerFromPool(TimelineGameSwitch marker)
        {
            marker.SetEntity(entityToSet);
            marker.SetMarkerInfo();

            SortMarkers();

            marker.gameObject.SetActive(true);
            EntityMarkers.Add(entityToSet.guid, marker);
        }

        private void OnReturnMarkerToPool(TimelineEventObj marker)
        {
            EntityMarkers.Remove(marker.entity.guid);
            marker.gameObject.SetActive(false);
        }

        private void OnDestroyMarker(TimelineEventObj marker)
        {
            Destroy(marker.gameObject);
        }

        // NOTE (PELLY): This should probably be pooled in the future.
        public void CreateDestroyFX(RiqEntity entity)
        {
            var deleteFX = Instantiate(BlockDeleteFXTemplate, BlockDeleteFXTemplate.transform.parent);
            deleteFX.gameObject.SetActive(true);

            bool selected = false;
            if (EntityMarkers.ContainsKey(entity.guid))
            {
                if (Selections.instance.eventsSelected.Contains(EntityMarkers[entity.guid]))
                {
                    selected = true;
                }
            }

            string[] split = entity.datamodel.Split('/');
            if (split[0] == "gameManager")
            {
                if (split[1] == "switchGame")
                {
                    deleteFX.Create(entity.beat, StringUtils.Hex2RGB(EventCaller.instance.GetMinigame(split[2]).color));
                }
                else if (split[1] == "end" || split[1] == "startSong")
                {
                    deleteFX.Create(entity.beat, Color.white);
                }
                else
                {
                    deleteFX.Create(entity.beat, entity.length, (int)entity["track"], selected);
                }
            }
            else
            {
                deleteFX.Create(entity.beat, entity.length, (int)entity["track"], selected);
            }
        }
    }
}