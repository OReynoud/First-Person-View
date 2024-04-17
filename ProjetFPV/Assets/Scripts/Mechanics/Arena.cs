using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class Arena : MonoBehaviour
    {
        public int[] spawnCount;
        public Transform[] spawnPositions;

        public float timeBetweenSpawns;
        public float timeBetweenWaves;

        public ChargerBehavior bouffonPrefab;

        public List<Enemy> currentEnemies = new List<Enemy>();

        public GameObject[] gates;

        private bool finishedSpawning;

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
        }

        IEnumerator ArenaEvent()
        {
            foreach (var spawns in spawnCount)
            {
                List<Transform> tempList = spawnPositions.ToList();
                for (int i = 0; i < spawns; i++)
                {
                    int rand = 0;
                    Vector3 posToSpawn;
                    if (tempList.Count == 0)
                    {
                        
                        rand = Random.Range(0, spawnPositions.Length);
                        posToSpawn = spawnPositions[rand].position + Vector3.down * 5;
                    }
                    else
                    {
                        rand = Random.Range(0, tempList.Count);
                        posToSpawn = tempList[rand].position + Vector3.down * 5;
                    }
                    var temp = Instantiate(bouffonPrefab, posToSpawn, Quaternion.identity);
                    temp.arenaSpawn = true;
                    temp.locationToSpawn = posToSpawn;
                    temp.spawnPositions = spawnPositions;
                    temp.respawnOnDeath = false;
                    temp.arena = this;
                    currentEnemies.Add(temp);
                    tempList.Remove(tempList[rand]);
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }

                yield return new WaitForSeconds(timeBetweenWaves);
            }

            finishedSpawning = true;
        }

        private void Update()
        {
            if (!finishedSpawning) return;
            if (currentEnemies.Count != 0) return;
            if (destroying)return;
            destroying = true;
            DestroyMethod();
        }

        void DestroyMethod()
        {
            foreach (var go in gates)
            {
                go.SetActive(false);
            }
            Destroy(gameObject,10);
        }

        private bool destroying;
    }
}
