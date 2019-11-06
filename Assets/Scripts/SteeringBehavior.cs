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
    public float neighbourDistance = 3f; // min distance other neighbors can be close to flocking agent 
    public float decayCoefficient = 2f; // holds the constant coefficient of decay for the inverse square law force 
    Vector3 originalPos;
    Vector3 originalVel;
    public int counter;

    protected void Start() {
        agent = GetComponent<NPCController>();
        if(!isLeadSteering) wanderOrientation = agent.orientation;
        originalPos = agent.position;
        originalVel = agent.velocity;
        counter = 0;
    }
    /* function to set the list of flocking agents for current flocking ageint */
    public void SetFlock(List<GameObject> flk) {
        flock = flk;
    }


    public Vector3 Seek() {
        return new Vector3(0f, 0f, 0f);
    }

    public Vector3 Flee()
    {
        return new Vector3(0f, 0f, 0f);
    }

    // calculate the target to puruse 
    public Vector3 Pursue() {
        // work out the distance to target  
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;
        // work out our current speed
        float speed = agent.velocity.magnitude;
        // for our end prediction
        float prediction;
        // check if speed is too small to give a reasonable prediction time 
        if (speed <= distance / maxPrediction) {
            prediction = maxPrediction;
        } else {
            prediction = distance / speed;
        }
        // visual 
        agent.DrawCircle(target.position + target.velocity * prediction, 0.3f);
        // Create the structure to hold our output
        Vector3 steering = (target.position + target.velocity * prediction) - agent.position;
        // Give full acceleration along this direction
        steering.Normalize();
        steering *= maxAcceleration;
        //output the steering
        return steering;


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
    // TODO make this work with list of flockmates
    public Vector3 CollisionPrediction() {

        Vector3 distance = target.position - agent.position;
        Vector3 veloDiff = target.velocity - agent.velocity;
        float t_closest = -Vector3.Dot(distance, veloDiff) / Mathf.Pow(veloDiff.magnitude, 2f);
        if (t_closest < 0) {
            return Vector3.zero;
        }
        Vector3 agentFuture = agent.position + agent.velocity * t_closest;
        Vector3 targetFuture = target.position + target.velocity * t_closest;
        if ((agentFuture - targetFuture).magnitude < 1f) {
            Vector3 evasion = -agent.velocity;
            agent.DrawCircle(evasion.normalized * maxAcceleration, 0.4f);
            return evasion.normalized * maxAcceleration;
        } else {
            return Vector3.zero;
        }
    }
    public Vector3 avoid_collisions() {
        // create structure for our output 
        Vector3 steering;
        // store the first collision time 
        float shortestTime = Mathf.Infinity;
        // store the target that collides then, and other data that we will need and can avoid recalculating 
        NPCController firstTarget = null;
        float firstMinSeparation = 0;
        float firstDistance = 0;
        float distance = 0;
        Vector3 firstRelativePos = Vector3.zero;
        Vector3 firstRelativeVel = Vector3.zero;
        Vector3 relativePos = Vector3.zero;
        Vector3 relativeVel = Vector3.zero;
        // loop through each boid
        foreach (GameObject boid in flock) {
            // only want to avoid collisions with other flockmates 
            if(boid != this.gameObject) {
                // calculate the time to collision
                relativePos = boid.GetComponent<NPCController>().position - agent.position;
                relativeVel = boid.GetComponent<NPCController>().velocity - agent.velocity;
                float relativeSpeed = relativeVel.magnitude;
                float timeToCollision = (Vector3.Dot(relativePos, relativeVel)) / (relativeSpeed * relativeSpeed);
                // check if it is going to be a collision at all
                distance = relativePos.magnitude;
                float minSeparation = distance - relativeSpeed * shortestTime;
               // if(minSeparation > 2 * 0.5) {
                 //   continue;
               // }
                // check if it is the shortest 
                if(timeToCollision > 0 && timeToCollision < shortestTime) {
                    // store the time, flocker, and other data 
                    shortestTime = timeToCollision;
                    firstTarget = boid.GetComponent<NPCController>();
                    firstMinSeparation = minSeparation;
                    firstDistance = distance;
                    firstRelativePos = relativePos;
                    firstRelativeVel = relativeVel;
                }
            }
            
        }
  
        // if we have no target, then exit 
        if(firstTarget == null) {
            return Vector3.zero;
        }
        // if we're going to hit exactly, or if we're already colliding, then do the steering based on current position
        if(firstMinSeparation <= 0 || distance < 2 * 0.5) {
            relativePos = firstTarget.position - agent.position;
        } else { // otherwise, calculate the future relative position
            relativePos = firstRelativePos + firstRelativeVel * shortestTime;
        }
        // avoid the target 
        relativePos.Normalize();
        steering = relativePos * maxAcceleration;
        return steering;


    }
    

    public Vector3 Flock() {
        float separationWeight = 1f;
        float coherenceWeight = 1f;
        float veloMatchWeight = 1f;
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
    
    /* function to return a new velocity that steers the agent away from other boids too close */
    public Vector3 computeSeparation() {
        
        // structure to hold our output 
        Vector3 steering = Vector3.zero;
        // loop through each target (other flockers)
        foreach (GameObject go in flock) {
            if(go != this.gameObject) {
                float strength = 0;
                // check if the target is close 
                Vector3 direction = go.GetComponent<NPCController>().position - agent.position;
                float distance = direction.magnitude;
                if (distance < neighbourDistance) {
                    // calculate the strength of repulsion 
                    // strength = maxAcceleration * (neighbourDistance - distance) / neighbourDistance;
                    strength = Mathf.Min(1 / (distance * distance), maxAcceleration);
                }
                // add the acceleration
                direction.Normalize();
                steering += strength * direction;
            }
     
        }
        // we've gone through the targets, now return the result 
        return steering;
    }
    /* function to steer the agent towards the average heading of local flockmates */
    public Vector3 computeAlign() {

        Vector3 steering = Vector3.zero;
        // loop through each target (other flockers)
        foreach (GameObject go in flock) {
            if(go != this.gameObject) {
                // acceleration tries to get to the target(s) velocity 
                steering += go.GetComponent<NPCController>().velocity - agent.velocity;
                
            }
        
        }
        steering /= timeToTarget;
        // check if the acceleration is too fast 
        if (steering.magnitude > maxAcceleration) {
            steering.Normalize();
            steering *= maxAcceleration;
        }
        // output the steering 
        return steering;

    }/* function to steer the agent toward the average position of local flockmates */
    public Vector3 computeCohesion() {

        
        // Create the structure to hold our output
        Vector3 steering = Vector3.zero;
        int count = 0;
        foreach (GameObject go in flock) {

            if(go != this.gameObject) {
                float distance = (go.GetComponent<NPCController>().position - agent.position).magnitude;
                if(distance > 0) {
                    steering += go.GetComponent<NPCController>().velocity;
                    count++;
                }
                
            }

        }
        steering /= count;

        return steering;
        
        /*
        Vector3 sumPositions = Vector3.zero;
        foreach (GameObject go in flock) {
            sumPositions += go.GetComponent<NPCController>().position;
        }
        Vector3 average = sumPositions / flock.Count;
        Vector3 direction = average - agent.position;
        Vector3 des = direction.normalized * maxSpeed;
        Vector3 steering = des - agent.velocity;
        // Check if the acceleration is too fast
        if (steering.magnitude > maxAcceleration) {
            steering.Normalize();
            steering *= maxAcceleration;
        }
        return steering;
        */
    }
    public Vector3 applyRules() {
        // structure to hold our output 
        Vector3 steering = Vector3.zero;
        // get the three behaviors 
        Vector3 separation = computeSeparation();
        //Vector3 alignment = computeAlign();
        Vector3 cohesion = computeCohesion();
        // assign a weight to each 
        float separationWeight = 1f;
        //float alighWeight = 0.3f;
        float cohesionWeight = 1f;
        steering += separation * separationWeight;
        //steering += alignment * alighWeight;
        steering += cohesion * cohesionWeight;
        
        if(steering.magnitude > maxAcceleration) {
            steering.Normalize();
            steering *= maxAcceleration;
        }
        return steering;

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

 

    public Vector3 followPath() {
        Vector3 pathTarget = Path[current].transform.position;
        while (Vector3.Distance(agent.position, pathTarget) < 1f) {
            current++;
            if (current > Path.Length) {
                return Vector3.zero;
            }
            pathTarget = Path[current].transform.position;
        }
        // Create the structure to hold our output
        Vector3 steering;

        // get the direction to the target 
        Vector3 direction = pathTarget - agent.position;
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

        return steering;
    }

    public Vector3 stop() {
        Vector3 direction = -agent.velocity;
        return direction.normalized * maxAcceleration;
    }




}
