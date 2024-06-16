using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class Arena : MonoBehaviour
    {
        [Serializable]
        public struct WaveSpawns
        {
            public int bouffonCount;
            public int collectorCount;
        }
        public WaveSpawns[] spawnCount;
        public Transform[] spawnPositions;
        
        public Transform[] collectorSpawnPositions;

        public float timeBetweenSpawns;
        public float timeBetweenWaves;
        public float playerProximityTolerance = 5;

        public ChargerBehavior bouffonPrefab;
        public CollectorBehavior collectorPrefab;

        public List<Enemy> currentEnemies = new List<Enemy>();

        public GameObject[] gates;

        private bool finishedSpawning;

        public bool lastBattle;
        [ShowIf("lastBattle")] public Collider tornadoColl;
        [ShowIf("lastBattle")] public GameObject gateToDisable;

        private void Awake()
        {
            foreach (var go in gates)
            {
                go.SetActive(false);
            }
        }

        public void TriggerArenaEvent()
        {
            foreach (var go in gates)
            {
                go.SetActive(true);
            }

            StartCoroutine(ArenaEvent());
            AudioManager.instance.PlayUISound(5, 2, 0f);
        }

        List<Transform> tempList = new List<Transform>();

        IEnumerator ArenaEvent()
        {
            foreach (var spawns in spawnCount)
            {
                //Collector Spawn
                tempList = FillValidList(spawnPositions);
                for (int i = 0; i < spawns.bouffonCount; i++)
                {
                    int rand = 0;
                    Vector3 posToSpawn;
                    if (tempList.Count == 0)
                        tempList = FillValidList(spawnPositions);
                    
                    rand = Random.Range(0, tempList.Count);
                    posToSpawn = tempList[rand].position + Vector3.down * 5;

                    var temp = Instantiate(bouffonPrefab, posToSpawn, Quaternion.identity);
                    temp.arenaSpawn = true;
                    temp.spawnPositions = spawnPositions;
                    temp.respawnOnDeath = false;
                    temp.arena = this;
                    currentEnemies.Add(temp);
                    tempList.Remove(tempList[rand]);
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }
                
                
                
                //Collector Spawn
                tempList.Clear();
                tempList = FillValidList(collectorSpawnPositions);
                for (int i = 0; i < spawns.collectorCount; i++)
                {
                    int rand = 0;
                    Vector3 posToSpawn;
                    if (tempList.Count == 0)
                        tempList = FillValidList(collectorSpawnPositions);
                    
                    rand = Random.Range(0, tempList.Count);
                    posToSpawn = tempList[rand].position;

                    var temp = Instantiate(collectorPrefab, posToSpawn, Quaternion.identity);
                    temp.arenaSpawn = true;
                    temp.respawnOnDeath = false;
                    temp.arena = this;
                    temp.spawnEnemyPos = spawnPositions;
                    currentEnemies.Add(temp);
                    tempList.Remove(tempList[rand]);
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }

                yield return new WaitForSeconds(timeBetweenWaves);
                while (currentEnemies.Count > 0)
                {
                    yield return null;
                }
            }

            finishedSpawning = true;
        }

        private void Update()
        {
            if (!finishedSpawning) return;
            if (currentEnemies.Count != 0) return;
            if (destroying) return;
            destroying = true;
            DestroyMethod();
        }

        void DestroyMethod()
        {
            if (!lastBattle)
            {
                foreach (var go in gates)
                {
                    go.SetActive(false);
                }

                Destroy(gameObject, 10);
                return;
            }

            //trucs à setup à la fin du combat de l'arène
            gateToDisable.SetActive(false);
            GameManager.instance.canStartEndingCinematic = true;
            tornadoColl.enabled = true;
        }

        private bool destroying;

       


        List<Transform> FillValidList( Transform[] array)
        {
            List<Transform> temp = new List<Transform>();
            foreach (var pos in array)
            {
                if (Vector3.Distance(pos.position, PlayerController.instance.transform.position)
                    > playerProximityTolerance)
                {
                    temp.Add(pos);
                }

            }

            if (temp.Count == 0)
            {
                Debug.LogError("Aucun spawn valide trouvé, le joueur est trop près de tous les spawns possibles");
                return spawnPositions.ToList();
            }

            return temp;
        }
    }
}