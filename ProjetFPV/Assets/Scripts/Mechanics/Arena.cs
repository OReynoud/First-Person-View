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
        public float playerProximityTolerance = 5;

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
            AudioManager.instance.PlayUISound(5, 2, 0f);
        }

        List<Transform> tempList = new List<Transform>();

        IEnumerator ArenaEvent()
        {
            foreach (var spawns in spawnCount)
            {
                tempList = FillValidList();
                for (int i = 0; i < spawns; i++)
                {
                    int rand = 0;
                    Vector3 posToSpawn;
                    if (tempList.Count == 0)
                        tempList = FillValidList();
                    
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
            foreach (var go in gates)
            {
                go.SetActive(false);
            }

            Destroy(gameObject, 10);
        }

        private bool destroying;


        List<Transform> FillValidList()
        {
            List<Transform> temp = new List<Transform>();
            foreach (var pos in spawnPositions)
            {
                if (Vector3.Distance(pos.position, PlayerController.instance.transform.position)
                    < playerProximityTolerance)
                    continue;

                temp.Add(pos);
            }

            if (tempList.Count == 0)
            {
                Debug.LogError("Aucun spawn valide trouvé, le joueur est trop près de tous les spawns possibles");
                return spawnPositions.ToList();
            }

            return temp;
        }
    }
}