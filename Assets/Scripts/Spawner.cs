using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject obstacle;
    public Transform agentSpawnPoint;
    public Transform road1Start;
    public Transform road2Start;
    public float obstacleSpeed = 5f;

    public float spawnCooldown = 2f;
    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnCooldown)
        {
            SpawnObstacle();
            spawnTimer = 0f;
        }
    }

    void SpawnObstacle()
    {
        GameObject newObstacle = Instantiate(obstacle);

        // Determine random start point
        Transform startPoint = Random.Range(0, 2) == 0 ? road1Start : road2Start;
        Vector3 startPosition = startPoint.position;
        startPosition.y = 0.5f; // Adjust y-coordinate to lift obstacle
        newObstacle.transform.position = startPosition;

        // Calculate direction towards the agent
        Vector3 directionToAgent = (agentSpawnPoint.position - startPoint.position).normalized;

        // Set obstacle velocity
        Rigidbody obstacleRigidbody = newObstacle.GetComponent<Rigidbody>();
        obstacleRigidbody.velocity = directionToAgent * obstacleSpeed;

        // Destroy obstacle after reaching agent
        Destroy(newObstacle, Vector3.Distance(startPoint.position, agentSpawnPoint.position) / obstacleSpeed);
    }
}
