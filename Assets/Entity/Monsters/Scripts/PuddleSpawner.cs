using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.VisualScripting;

public class PuddleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject puddlePrefab;
    public BoxCollider2D spawnArea;
    public float spawnInterval = 15f;
    public int maxPuddles = 5;
    
    private float spawnTimer;
    private int currentPuddleCount = 0;
    private bool monsterSpawned = false;
    private List<PuddleController> activePuddles = new List<PuddleController>();
    
    void Update()
    {
        if (currentPuddleCount < maxPuddles)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnPuddle();
                spawnTimer = spawnInterval;
            }
        }
    }
    
    void SpawnPuddle()
    {
        if (puddlePrefab == null || spawnArea == null) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        GameObject puddle = Instantiate(puddlePrefab, spawnPosition, Quaternion.identity);
        PuddleController puddleController = puddle.GetComponent<PuddleController>();
        puddleController.Initialize(this);
        
        activePuddles.Add(puddleController);
        currentPuddleCount++;
    }
    
    public bool CanSpawnMonster()
    {
        Debug.Log($"Монстр заспавнен: {monsterSpawned}");
        return !monsterSpawned;
    }
    
    public void OnMonsterSpawned()
    {
        monsterSpawned = true;
    }
    
    public void OnMonsterReturned()
    {
        monsterSpawned = false;
    }
    
    public void OnPuddleDestroyed(PuddleController puddle)
    {
        activePuddles.Remove(puddle);
        currentPuddleCount = Mathf.Max(0, currentPuddleCount - 1);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        Vector3 randomPoint = new Vector3(randomX, randomY, 0);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return randomPoint;
    }
    
    public void DeleteAll()
    {
        foreach(PuddleController pc in activePuddles)
        {
            Destroy(pc.gameObject);
            currentPuddleCount = 0;
            maxPuddles = 0;
        }
    }
}