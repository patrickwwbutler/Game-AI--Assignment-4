using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour {

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    public List<GameObject> flock;

    [Header("Our variables")]
    public bool isLeadSteering;
    float neighbourDistance = 2.0f;


    protected void Start() {
        agent = GetComponent<NPCController>();
        if(!isLeadSteering) wanderOrientation = agent.orientation;
    }

    public Vector3 Seek() {
        return new Vector3(0f, 0f, 0f);
    }

    public Vector3 Flee()
    {
        return new Vector3(0f, 0f, 0f);
    }


    // Calculate the target to pursue
    public Vector3 Pursue() {
        return new Vector3(0f, 0f, 0f);
    }

    public float Face()
    {
        return 0f;
    }
  
    public Vector3 Arrive() {

        // Create the structure to hold our output
        Vector3 steering;

        // get the direction to the target 
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;
        float targetSpeed;
        // Check if we are there, return no steering

        //  If we are outside the slowRadius, then go max speed
        if (distance < targetRadiusL) {
            //return Vector3.zero;
            targetSpeed = 0;
        }
        else if (distance > slowRadiusL) {
            targetSpeed = maxSpeed;
        } // Otherwise calculate a scaled speed
        else {
            targetSpeed = (maxSpeed * distance) / slowRadiusL;
        }

        // The target velocity combines speed and direction
        Vector3 targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        // Acceleration tries to get to the target velocity
        steering = targetVelocity - agent.velocity;
        steering = steering / timeToTarget;

        // Check if the acceleration is too fast
        if (steering.magnitude > maxAcceleration) {
            steering.Normalize();
            steering *= maxAcceleration;
        }

        // output the steering 
        return steering;
    }

    public Vector3 Flock() {
        float separationWeight = 0.2f;
        float coherenceWeight = 0.5f;
        float veloMatchWeight = 0.3f;
        List<GameObject> nearby = new List<GameObject>();
        foreach(GameObject boid in flock) {
            if( boid != this.gameObject) {
                if ((agent.position - boid.transform.position).magnitude < 3f) {
                    nearby.Add(boid);
                }
            }
            
        }
        Vector3 steering = Vector3.zero;
        Vector3 separation = Vector3.zero;
        foreach(GameObject boid in nearby) {
            if(boid != this.gameObject) {
                float distance = (agent.position - boid.transform.position).magnitude;
                Vector3 direction = agent.position - boid.transform.position;
                separation += direction / (distance * distance);
            }

        }
        steering += separation.normalized * separationWeight;
        Vector3 center = Vector3.zero;
        foreach(GameObject boid in nearby) {
            center += boid.transform.position;
        }
        center /= nearby.Count;
        Vector3 coherenceDirection = center - agent.position;
        steering += coherenceDirection.normalized * coherenceWeight;
        Vector3 avgVelo = Vector3.zero;
        foreach(GameObject boid in nearby) {
            if(boid != this.gameObject) {
                NPCController boidCon = boid.GetComponent<NPCController>();
                avgVelo += boidCon.velocity;
            }
           
        }
        avgVelo /= nearby.Count;
        Vector3 veloDelta = agent.velocity - avgVelo;
        steering += veloMatchWeight * veloDelta.normalized;
        return steering.normalized * maxAcceleration;
    }
    /*
    public void ApplyRules() {

        Vector3 centre = Vector3.zero;
        Vector3 avoid = Vector3.zero;
        float flockingSpeed = 0.1f;
        float dist;
        int flockSize;
        foreach (GameObject go in flock) {

            if(go != this.gameObject) {
                dist = Vector3.Distance(go.transform.position, this.transform.position);
                if(dist <= neighbourDistance) {
                    avoid = avoid + (this.transform.position - go.transform.position);
                }
                flockingSpeed = flockingSpeed + go.GetComponent<NPCController>().velocity;
            }
        }




    }
    */

    public void SetFlock(List<GameObject> flk) {
        flock = flk;
    }




}
