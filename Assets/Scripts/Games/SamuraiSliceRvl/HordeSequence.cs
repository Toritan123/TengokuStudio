using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_SamuraiSliceRvl
{
    public class HordeSequence : MonoBehaviour
    {
        public bool IsComboMissed { get; set; } = false;
        public PlayerActionEvent EndComboInput { get; set; }

        private List<HordeDemon> activeDemons = new List<HordeDemon>();
        private SamuraiSliceRvl game;
        private int currentSpawnIndex = 0;

        public void Init(SamuraiSliceRvl game)
        {
            this.game = game;
        }
        void Update()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused || GameManager.instance.currentGame != "samuraiSliceRvl")
            {
                Destroy(gameObject);
                return;
            }
        }
        public void SpawnBatch(int count)
        {
            if (game.hordeDemonPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos;

                if (game.hordeSpawnPositions != null && currentSpawnIndex < game.hordeSpawnPositions.Length)
                {
                    spawnPos = game.hordeSpawnPositions[currentSpawnIndex];
                    currentSpawnIndex++;
                }
                else
                {
                    // Fallback random position
                    spawnPos = new Vector3(UnityEngine.Random.Range(6f, 9f), UnityEngine.Random.Range(-1f, 1f), 0);
                }
                
                HordeDemon demon = Instantiate(game.hordeDemonPrefab, game.transform);
                demon.transform.localPosition = spawnPos;
                
                int sortOrder = 1;
                Vector3 targetPos = new Vector3(-2f, spawnPos.y, 0f);
                
                demon.Init(game, this, sortOrder, currentSpawnIndex == 5, targetPos);

                activeDemons.Add(demon);
            }
        }

        public void ScheduleRushes(double baseBeat)
        {
            for (int i = 0; i < activeDemons.Count; i++)
            {
                var demon = activeDemons[i];
                double rushTime = baseBeat + ((double)i / 7.0);
                
                BeatAction.New(game, new List<BeatAction.Action>() {
                    new BeatAction.Action(rushTime, () => {
                        if (demon != null) demon.Rush();
                    })
                });
            }
        }

        public void KillNext(bool isFinal)
        {
            if ((!game.IsReady && PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch) || 
                (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch && !game.IsHolding)) return;

            if (activeDemons.Count > 0)
            {
                HordeDemon demon = activeDemons[0];
                
                DemonSliced prefab = game.hordeSlicedPrefab;
                if (prefab == null && game.slicedDemonPrefabs != null && game.slicedDemonPrefabs.Length > 0)
                {
                    prefab = game.slicedDemonPrefabs[0];
                }

                if (demon != null) demon.Kill(prefab, isFinal);
                
                activeDemons.RemoveAt(0);
            }
        }

        public void FailRest()
        {
            if (activeDemons.Count > 0)
            {
                HordeDemon lastDemon = activeDemons[activeDemons.Count - 1];
                
                for (int i = 0; i < activeDemons.Count - 1; i++)
                {
                    DemonSliced prefab = game.hordeSlicedPrefab;
                    if (prefab == null && game.slicedDemonPrefabs != null && game.slicedDemonPrefabs.Length > 0)
                    {
                        prefab = game.slicedDemonPrefabs[0];
                    }

                    if (activeDemons[i] != null) 
                    {
                        activeDemons[i].Kill(prefab, false);
                    }
                }

                if (lastDemon != null)
                {
                    lastDemon.FallFail();
                }
                activeDemons.Clear();
            }
        }
    }
}
