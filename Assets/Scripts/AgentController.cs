using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class AgentController : Agent
{
    public Transform target;
    public float speedMultiplier = 0.5f;
    public float jumpHeight = 5.0f;
    public float jumpTime = 0.1f;

    private Rigidbody rigidBody;
    private float timer;
    private bool hasLanded;
    private bool hasCollided = false;

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            transform.SetLocalPositionAndRotation(new Vector3(-12.5f, 0.6f, 0f), Quaternion.identity);
        }

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }

        hasCollided = false;
        hasLanded = true;

        target.localPosition = new Vector3(Random.Range(-25f, 0f), 0.6f, Random.Range(-12f, 12f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        transform.Translate(controlSignal * speedMultiplier);
        
        rigidBody.AddForce(controlSignal * speedMultiplier, ForceMode.VelocityChange);
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up };
        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 12f))
            {
                if (hit.collider.CompareTag("Obstacle"))
                {
                    if (Mathf.Abs(transform.localPosition.y - 0.2f) < 0.1f && hasLanded && timer <= 0f)
                    {
                        AddReward(1.0f);
                        print("Agent has jumped.");
                        rigidBody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                        hasLanded = false;
                        timer = jumpTime;
                    }
                }
            }
        }


        if (!hasLanded)
        {
            timer -= Time.deltaTime;
        }

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        if (distanceToTarget < 1.42f)
        {
            AddReward(3.0f);
            print(GetCumulativeReward());
            target.localPosition = new Vector3(Random.Range(-25f, 0f),
                                         0.6f, Random.Range(-12f, 12f));
        }


        if (transform.localPosition.y < 0 || hasCollided)
        {
            AddReward(-1.0f);
            print(GetCumulativeReward());
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Intersection"))
        {
            timer = 0f;
            hasLanded = true;
        }
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Road"))
        {
            hasCollided = true;
        }
    }

}