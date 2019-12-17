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
    public List<GameObject> otherFlock;

    [Header("Our variables")]
    public bool isLeadSteering;
    public float neighbourDistance = 5f; // min distance other neighbors can be close to flocking agent
    public float decayCoefficient = 2f; // holds the constant coefficient of decay for the inverse square law force
    Vector3 originalPos;
    Vector3 originalVel;
    public int counter; // for testing; had a counter so the calls wouldnt happen too many times and CRASH unity
    public bool part2;
    public bool startConeCheck;
    public bool startCollisionPrediction;
    public float coneThreshold = 120f;
    public NPCController follower;
    public float frontRayAngle = 45f;
    public bool part3;
    Vector3 nextNode;
    public GameObject npcCube;
    public float avoidDistance = 2.3f;

    protected void Start() {
        agent = GetComponent<NPCController>();
        if(!isLeadSteering) wanderOrientation = agent.orientation;
        originalPos = agent.position;
        originalVel = agent.velocity;
        counter = 0;
        startConeCheck = false;
        startCollisionPrediction = false;
        npcCube = transform.GetChild(0).gameObject;
        Debug.Log(npcCube.name);
       // part3 = false;
    }

    public void FixedUpdate() {
        /* Path finding for part 3 in FixedUpdate(), updates NPCController at end */
        if (part3) {
            // structure to hold our steering for path following 
            Steering steering = new Steering {
                linear = Vector3.zero,
                angular = 0f
            };
            // structure to hold our steering for raycasting with obstacle avoidance
            Steering avoidance = new Steering {
                linear = Vector3.zero,
                angular = 0f
            };
            float avoidMultiplier = 0f;
            // while we are travveling the path 
            if (current < Path.Length) {
                // find the current position on the path 
                nextNode = Path[current].transform.position;
                steering.angular = Align(nextNode);
                // delegate to seek 
                Vector3 seekVec = nextNode - agent.transform.position;
                if (seekVec.magnitude > maxAcceleration) {
                    seekVec = seekVec.normalized * maxAcceleration;
                }
                steering.linear = seekVec;
                /* ACCOUNT FOR FLOCKING */
                steering.linear += 2.6f * computeSeparation() + 0.15f * computeCohesion() + 0.2f * computeAlign().linear;
                
                /* CHECK FOR OBSTACLE AVOIDANCE */
                // rays forward, to the left angle, and to the right angle 
                RaycastHit hit;
                RaycastHit leftWhisker;
                RaycastHit rightWhisker;
                Vector3 rayStartPos = agent.transform.position;
                // check for a collision forward ... 
                Vector3 rayVec = Quaternion.Euler(0f,agent.rotation,0f) * agent.transform.forward;
                if(Physics.SphereCast(rayStartPos, 0.25f,rayVec, out hit, 5f)) {
                    // for visual 
                    Debug.DrawRay(rayStartPos, rayVec, Color.blue, Time.deltaTime);
                    // create a target 
                    Vector3 avoidTarget = hit.point + hit.normal * avoidDistance;
                    // delegate to seek 
                    Vector3 direction = avoidTarget - agent.position;
                    direction.Normalize();
                    direction *= maxAcceleration;
                    avoidance.linear += direction;
                    // if to close, need to move further away 
                    if(hit.distance < 2.4f) {
                        // only want to have a greater avoidance if its an obstacle 
                        if (hit.collider.gameObject.tag == "obstacle") avoidMultiplier += 1.5f;
                        // align to avoidance direction 
                        avoidance.angular += Align(avoidTarget);
                    }
                   
                }
                // check for a collision at a left angle ...
                Vector3 rightRayVec = Quaternion.Euler(0f, agent.rotation + 30f, 0f) * agent.transform.forward;
                if (Physics.SphereCast(rayStartPos, 0.05f, rightRayVec, out rightWhisker, 3f)) {
                    // for visual
                    Debug.DrawRay(rayStartPos, rightRayVec, Color.magenta, Time.deltaTime);
                    // create a target 
                    Vector3 avoidTarget = rightWhisker.point + hit.normal * avoidDistance;
                    // delegate to seek and add to overall avoidance 
                    Vector3 direction = avoidTarget - agent.position;
                    direction.Normalize();
                    direction *= maxAcceleration;
                    avoidance.linear += direction;
                    // if to close, need to move further away 
                    if (rightWhisker.distance < 2f) {
                        // only want to have a greater avoidance if its an obstacle 
                        if (rightWhisker.collider.gameObject.tag == "obstacle") avoidMultiplier += 0.5f;
                        // align to avoidance direction 
                        avoidance.angular += Align(avoidTarget);
                    }

                }
                // check for a collision at a right angle ...
                Vector3 leftRayVec = Quaternion.Euler(0f, agent.rotation - 30f, 0f) * agent.transform.forward;
                if (Physics.SphereCast(rayStartPos, 0.05f, leftRayVec, out leftWhisker, 3f)) {
                    // for visual
                    Debug.DrawRay(rayStartPos, leftRayVec, Color.yellow, Time.deltaTime);
                    // create a target 
                    Vector3 avoidTarget = leftWhisker.point + hit.normal * avoidDistance;
                    // delegate to seek and add to overall avoidance 
                    Vector3 direction = avoidTarget - agent.position;
                    direction.Normalize();
                    direction *= maxAcceleration;
                    avoidance.linear += direction;
                    // if too close, need to move further away 
                    if (leftWhisker.distance < 2f) {
                        // only want to have a greater avoidance if its an obstacle 
                        if (leftWhisker.collider.gameObject.tag == "obstacle") avoidMultiplier += 0.5f;
                        // align to avoidance direction 
                        avoidance.angular += Align(avoidTarget);
                    }

                }
                // if we need avoidance, add to overall pathfinding behavior 
                if (avoidance.linear != Vector3.zero) {
                    if(avoidMultiplier != 0f ) { // if we need greater avoidance 
                        steering.angular += avoidance.angular * avoidMultiplier * 0.8f;
                        steering.linear += avoidance.linear * avoidMultiplier * 0.8f;
                    } else { // else, scale and add to current steering 
                        steering.angular += avoidance.angular * 0.95f;
                        steering.linear += avoidance.linear * 0.95f;
                    }
                    
                }
                agent.DrawCircle(agent.position + agent.transform.forward * 1.7f, 0.5f);
                // update our agent movement 
                agent.GetComponent<NPCController>().update(steering.linear, steering.angular, Time.deltaTime);
                // if we are close to destination point on path 
                float distance = (agent.position - Path[current].transform.position).magnitude;
                if (distance <= 2.3f) {
                    // set next destination point 
                    current += 1;
                }


            }
        }

    }
    /* function to set the list of flocking agents for current flocking ageint */
    public void SetFlock(List<GameObject> flk) {
        flock = flk;
    }
    public void SetOtherFlock(List<GameObject> flk) {
        otherFlock = flk;
    }
    /* function to set the path for NPC to follow */
    public void setPath(GameObject[] path) {
        Path = path;
    }
    /* struct to hold the velocity and orientation (in some cases) */
    public struct Steering {

        public Vector3 linear;
        public float angular;

    }

    public Vector3 Seek() {
        return new Vector3(0f, 0f, 0f);
    }

    public Vector3 Flee()
    {
        return new Vector3(0f, 0f, 0f);
    }

    /* function that calculates the target to pursue */
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
       // agent.DrawCircle(target.position + target.velocity * prediction, 0.3f);
        // Create the structure to hold our output
        Vector3 steering = (target.position + target.velocity * prediction) - agent.position;
        // Give full acceleration along this direction
        steering.Normalize();
        steering *= maxAcceleration;
        //output the steering
        return steering;


    }
    /* function to have NPC face target, calls Align */
    public float Face()
    {
        return Align(target.position);
    }
    // function to have NPC align to input target vector
    public float Align(Vector3 targetVector) {

        // work out the direction to target
        Vector3 direction = targetVector - agent.position;
        // check for zero direction
        if(direction.magnitude == 0) {
            return 0;
        }
        // Get the naive direction to the target
        float rot = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // map the result to the (-pi,pi) interval
        while (rot > Mathf.PI) {
            rot -= 2 * Mathf.PI;
        }
        while (rot < -Mathf.PI) {
            rot += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rot);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA) {
            agent.rotation = 0;
        }
        // if we are outside the slowRadius, then use the maximum rotation
        float targetRotation;
        if (rotationSize > slowRadiusA) {
            targetRotation = maxRotation;
        } else { // Otherwise calculate a scaled rotation
            targetRotation = maxRotation * rotationSize / slowRadiusA;
        }

        // The final target rotation combines
        // speed (already in the variable) and direction
        targetRotation *= rot / rotationSize;
        // Acceleration tries to get to the target rotation
        float steering_angular = targetRotation - agent.rotation;
        steering_angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(steering_angular);
        if (angularAcceleration > maxAngularAcceleration) {
            steering_angular /= angularAcceleration;
         //   steering_angular = steering_angular / angularAcceleration;
            steering_angular *= maxAngularAcceleration;
        }
        //
        // output the steering
        return steering_angular;

    }
    /* function for arriving at target, modified slightly for part 3 path following */
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
            targetSpeed = 0f;
        }
        else if (distance > slowRadiusL) {
            targetSpeed = maxSpeed * 0.75f;
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
    /* not used
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
    */
    public Vector3 ConeCheck() {

        // loop through our targets (the other flock)
        foreach(GameObject flockTarget in otherFlock) {
            // get the direction to target
            Vector3 direction = flockTarget.GetComponent<NPCController>().position - agent.position;
            // get the agent's orientation as a vector
            Vector3 orientationAsVector = new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
            // if the target is within the half cone threshold, do the evasion
            if (Vector3.Dot(orientationAsVector, direction) > Mathf.Cos(Mathf.Deg2Rad * (coneThreshold/ 2)) && direction.magnitude < 10f) {
                float dist = direction.magnitude;
                // work out our current speed
                float speed = agent.velocity.magnitude;
                // for our end prediction
                float prediction;
                // check if speed is too small to give a reasonable prediction time
                if (speed <= dist / maxPrediction) {
                    prediction = maxPrediction;
                } else {
                    prediction = dist / speed;
                }
                Vector3 steering = agent.position - (flockTarget.GetComponent<NPCController>().position + flockTarget.GetComponent<NPCController>().velocity * prediction);
                // Give full acceleration along this direction
                steering.Normalize();
                steering *= maxAcceleration;
                //output the steering
                return steering;
            } else { // else, return no steering
                return Vector3.zero;
            }
        }
        return Vector3.zero; // return no steering if no flockmates / no cone check detection

    }

    public Vector3 CollisionPrediction() {
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
        // loop through each boid
        foreach (GameObject boid in otherFlock) {
            // only want to avoid collisions with other flockmates
            if(boid != this.gameObject) {
                // calculate the time to collision
                Vector3 relativePos = boid.GetComponent<NPCController>().position - agent.position;
                Vector3 relativeVel = boid.GetComponent<NPCController>().velocity - agent.velocity;
                float relativeSpeed = relativeVel.magnitude;
                float timeToCollision = -Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);
                // check if it is going to be a collision at all
                distance = relativePos.magnitude;
                if(distance < 3f) { // had to put this here ... for immediate avoidance
                    return relativePos.normalized * (-maxAcceleration) * 3f;
                }
                float minSeparation = distance - relativeSpeed * shortestTime;
                if(minSeparation > 2 * 2f) {
                    continue;
                }
               // Debug.Log("t to collision !!!! " + timeToCollision);
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
          //  Debug.Log("uh oh");
            return Vector3.zero;
        } else {
            Vector3 relativePos = Vector3.zero;
            // if we're going to hit exactly, or if we're already colliding, then do the steering based on current position
            if (firstMinSeparation <= 0 || firstDistance < 2 * 1.5f) {
                relativePos = firstTarget.position - agent.position;
            } else { // otherwise, calculate the future relative position
                relativePos = firstRelativePos + firstRelativeVel * shortestTime;
            }
            // avoid the target
            relativePos.Normalize();
            steering = -relativePos * maxAcceleration; // had to make negative; was attracting to other boids otherwise
            if((firstTarget.position - agent.position).magnitude > 5.3f) { // had to make sure within certain distance...
                return Vector3.zero;                                       // if not, flockers always avoiding despite distance
            } else {
                return steering;
            }

        }




    }

    /* not used
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
    */

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
                    // strength = Mathf.Min(1f / (distance * distance), maxAcceleration);
                    // add the acceleration
                    Vector3 movingTowards = agent.position - go.GetComponent<NPCController>().position;
                    direction.Normalize();
                    steering += movingTowards.normalized / movingTowards.magnitude;
                }

            }

        }
        // we've gone through the targets, now return the result
        return steering.normalized;
    }
    /* function to steer the agent towards the average heading of local flockmates */
    public Steering computeAlign() {

        Steering alignmentVec = new Steering {
            linear = new Vector3(0f, 0f, 0f),
            angular = 0
        };
        Vector3 avgVelocity = Vector3.zero;
        int count = 0;
        foreach (GameObject go in flock) {
            if(go != this.gameObject) {
                Vector3 direction = go.GetComponent<NPCController>().position - agent.position;
                float distance = direction.magnitude;
                if( distance < neighbourDistance) {
                    avgVelocity += go.GetComponent<NPCController>().velocity;
                    count += 1;
                }
            }
        }
      //  Debug.Log("count: " + count);
        avgVelocity /= count;
        //Vector3 targetVec = avgVelocity + agent.position;
      //  Debug.Log("ang ! " + alignmentVec.angular);
        Vector3 steering = (avgVelocity + agent.position) - agent.position;
        steering.Normalize();
        steering *= maxAcceleration;
      // Debug.Log("heres the avg veloc and pos " + avgVelocity + agent.position + " and heres the target pos " + target.position);
        if(count != 0) {
            alignmentVec.angular = Align(avgVelocity + agent.position);
        }
        alignmentVec.linear = steering;
        return alignmentVec;

    }
    /* function that calculates the average position of all the flockers and delegates for flocker to seek this position */
    public Vector3 computeCohesion() {

        // Create the structure to hold the average position
        Vector3 avgPosition = Vector3.zero;
        // Create the structure to hold our output
        Vector3 steering;
        int count = 0;
        // loop through the other flockers, add positions to average position
        foreach (GameObject go in flock) {
            if(go != this.gameObject) {
                avgPosition += go.GetComponent<NPCController>().position;
                count++;  // incrmeent number of flockers
            }

        }
        avgPosition /= count; // divide all positions by count to get average
        // get the direction to this target
        steering = avgPosition - agent.position;
        // the velocity is along this direction, at full speed
        steering.Normalize();
        steering *= maxAcceleration;
        // output the steering
        return steering;

    }

  
    /* function that makes agent follow waypoint path for part 2*/
    public Steering followPath() {

        Steering steering = new Steering {
            linear = new Vector3(0f, 0f, 0f),
            angular = 0
        };
        // if we wouldn't be checking an invalid index
        if (current < Path.Length) {
            // if we are close enough to the target position ...
            if (Vector3.Distance(Path[current].transform.position, follower.transform.position) < 2f) {
                current += 1; // get the next position
            }
        }
        if(current >= Path.Length) { // if we reach the end of the path, stop moving
            steering.linear = Vector3.zero;
        } else { // else, start moving towards the next position with the Arrive behavior
            // get the direction to the target
            Vector3 direction = Path[current].transform.position - agent.position;
            float distance = direction.magnitude;
            float targetSpeed;
            if(distance < targetRadiusL) { // check if we are there, return slower speed
                targetSpeed = 1f;
            }else if(distance > slowRadiusL) { //  If we are outside the slowRadius, then go max speed
                targetSpeed = maxSpeed;
            } else { // otherwise calculate a scaled speed
                targetSpeed = (maxSpeed * distance * 2f) / slowRadiusL;
            }
            // the target velocity combines speed and direction
            Vector3 targetVelocity = direction;
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;
            // acceleration tries to get to the target velocity
            steering.linear = targetVelocity - agent.velocity;
            steering.linear = steering.linear / timeToTarget;
            // check if the accleration is too fast
            if(steering.linear.magnitude > maxAcceleration) {
                steering.linear.Normalize();
                steering.linear *= maxAcceleration;
            }
            steering.angular = Align(agent.position + targetVelocity);
            agent.DrawCircle(agent.position + agent.transform.forward * 1.8f, 0.5f);

        }
        // output the steering
        
        return steering;

    }
    /* following the lead boid and raycasting to avoid obstacles for part 3 */
    public Steering followAndRaycast() {
        // structure to hold our steering for following the lead boid  
        Steering steering = new Steering {
            linear = Vector3.zero,
            angular = 0f
        };
        // structure to hold our steering for raycasting with obstacle avoidance
        Steering avoidance = new Steering {
            linear = Vector3.zero,
            angular = 0f
        };
        // have boids pursue main boid following path 
        steering.linear = Arrive() * 1.1f;
        steering.angular = Face() * 1.5f;
        steering.linear += 6.75f * computeSeparation() + 0.45f * computeCohesion() + 0.36f * computeAlign().linear;
        steering.angular += computeAlign().angular * 0.5f;

        
        // CHECK FOR OBSTACLE AVOIDANCE 
        // rays forward, to the left angle, and to the right angle 
        RaycastHit hit;
        RaycastHit leftWhisker;
        RaycastHit rightWhisker;
        Vector3 rayStartPos = agent.transform.position;
        float avoidMultiplier = 0f;
        // check for a collision forward ... 
        Vector3 rayVec = Quaternion.Euler(0f, agent.rotation, 0f) * agent.transform.forward;
        if (Physics.SphereCast(rayStartPos, 0.25f, rayVec, out hit, 5f)) {
            // for visual 
            Debug.DrawRay(rayStartPos, rayVec, Color.blue, Time.deltaTime);
            // create a target 
            Vector3 avoidTarget = hit.point + hit.normal * avoidDistance;
            // delegate to seek 
            Vector3 direction = avoidTarget - agent.position;
            direction.Normalize();
            direction *= maxAcceleration;
            avoidance.linear += direction;
            // if to close, need to move further away 
            if (hit.distance < 2f) {
                // only want to have a greater avoidance if its an obstacle 
                if (hit.collider.gameObject.tag == "obstacle") avoidMultiplier += 1.5f;
                // align to avoidance direction 
                avoidance.angular += Align(avoidTarget);
            }
             
        }
        // check for a collision at a left angle ...
        Vector3 rightRayVec = Quaternion.Euler(0f, agent.rotation + 35f, 0f) * agent.transform.forward;
        if (Physics.SphereCast(rayStartPos, 0.05f, rightRayVec, out rightWhisker, 3f)) {
            // for visual
            Debug.DrawRay(rayStartPos, rightRayVec, Color.magenta, Time.deltaTime);
            // create a target 
            Vector3 avoidTarget = rightWhisker.point + hit.normal * avoidDistance;
            // delegate to seek and add to overall avoidance 
            Vector3 direction = avoidTarget - agent.position;
            direction.Normalize();
            direction *= maxAcceleration;
            avoidance.linear += direction;
            // if to close, need to move further away 
            if (rightWhisker.distance < 2f) {
                // only want to have a greater avoidance if its an obstacle 
                if (rightWhisker.collider.gameObject.tag == "obstacle") avoidMultiplier += 1f;
                // align to avoidance direction 
                avoidance.angular += Align(avoidTarget);
            }

        }
        
        // check for a collision at a right angle ...
        Vector3 leftRayVec = Quaternion.Euler(0f, agent.rotation - 35f, 0f) * agent.transform.forward;
        if (Physics.SphereCast(rayStartPos, 0.05f, leftRayVec, out leftWhisker, 3f)) {
            // for visual
            Debug.DrawRay(rayStartPos, leftRayVec, Color.yellow, Time.deltaTime);
            // create a target 
            Vector3 avoidTarget = leftWhisker.point + hit.normal * avoidDistance;
            // delegate to seek and add to overall avoidance 
            Vector3 direction = avoidTarget - agent.position;
            direction.Normalize();
            direction *= maxAcceleration;
            avoidance.linear += direction;
            // if too close, need to move further away 
            if (leftWhisker.distance < 2f) {
                // only want to have a greater avoidance if its an obstacle 
                if (leftWhisker.collider.gameObject.tag == "obstacle") avoidMultiplier += 1f;
                // align to avoidance direction 
                avoidance.angular += Align(avoidTarget);
            }

        }
        // if we need avoidance, add to overall pathfinding behavior 
        if (avoidance.linear != Vector3.zero) {
            if (avoidMultiplier != 0f) { // if we need greater avoidance 
                steering.angular += avoidance.angular * avoidMultiplier * 0.8f;
                steering.linear += avoidance.linear * avoidMultiplier * 0.8f;
            } else { // else, scale and add to current steering 
                steering.angular += avoidance.angular * 0.7f;
                steering.linear += avoidance.linear * 0.7f;
            }

        }
        
      
        return steering;
    }
    /* not used 
    public Steering followPathPart3() {
        Steering steering = new Steering {
            linear = new Vector3(0f, 0f, 0f),
            angular = 0
        };
        Steering avoidanceSteering = new Steering {
            linear = new Vector3(0f, 0f, 0f),
            angular = 0
        };
        if (current < Path.Length) {
            LineRenderer line = GetComponent<LineRenderer>();
            line.positionCount = 2;
           // line.SetPosition(0, agent.position);
          //  line.SetPosition(1, Path[current].transform.position);
            steering.angular = Align(Path[current].transform.position);
        }
        // if we wouldn't be checking an invalid index
        if (current < Path.Length) {
            // if we are close enough to the target position ...
            if (Vector3.Distance(Path[current].transform.position, follower.position) < 2.5f) {
                current += 1; // get the next position
            }
        }
        if (current >= Path.Length) { // if we reach the end of the path, stop moving
            steering.linear = Vector3.zero;
        } else { // else, start moving towards the next position with the Arrive behavior
            // get the direction to the target
            Vector3 direction = Path[current].transform.position - agent.position;
            float distance = direction.magnitude;
            float targetSpeed;
            if (distance < targetRadiusL) { // check if we are there, return slower speed
                targetSpeed = 0.8f;
            } else if (distance > slowRadiusL) { //  If we are outside the slowRadius, then go max speed
                targetSpeed = maxSpeed;
            } else { // otherwise calculate a scaled speed
                targetSpeed = (maxSpeed * distance * 2f) / slowRadiusL;
            }
            // the target velocity combines speed and direction
            Vector3 targetVelocity = direction;
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;
            // acceleration tries to get to the target velocity
            steering.linear = targetVelocity - agent.velocity;
            steering.linear = steering.linear / timeToTarget;
            // check if the accleration is too fast
            if (steering.linear.magnitude > maxAcceleration) {
                steering.linear.Normalize();
                steering.linear *= maxAcceleration;
            }
            //steering.angular = Align(Path[current].transform.position);

        }
        
        // holds a collision detecter
        RaycastHit hit;
        // Holds the minimum distance to a wall (i.e., how far
        // to avoid collision) should be greater than the
        // radius of the character.
        float avoidDistance = 4f;
        // Holds the distance to look ahead for a collision
        // (i.e., the length of the collision ray)
        float raysLength = 1f;
        float whiskerLength = 0.5f;
        // shoot the ray from the characters current position
        Vector3 rayStartPos = agent.position;
        // calculate the collision ray vector
        Vector3 rayVector = Quaternion.Euler(0f, agent.rotation, 0f) * agent.transform.forward;
        rayVector.Normalize();
        if (Physics.Raycast(rayStartPos, rayVector, out hit, raysLength)) {
            Debug.DrawRay(rayStartPos, hit.point, Color.green);
            Vector3 avoidTarget = hit.transform.position + hit.normal * avoidDistance;
            Vector3 direction = avoidTarget - agent.position;
            avoidanceSteering.linear = direction.normalized * maxAcceleration;
            avoidanceSteering.angular = Align(agent.position + direction);
            
            // TODO more of vid 
        }
        if(avoidanceSteering.linear != Vector3.zero) {
            steering.linear = steering.linear + avoidanceSteering.linear * 0.45f;
        }
        // output the steering
        

        return steering;
    }
    */

    /* not used 
    public Steering AvoidObstacles() {
        Steering steering = new Steering {
            linear = new Vector3(0f, 0f, 0f),
            angular = 0
        };
        // holds a collision detecter
        RaycastHit hit;
        // Holds the minimum distance to a wall (i.e., how far
        // to avoid collision) should be greater than the
        // radius of the character.
        float avoidDistance = 0.1f;
        // Holds the distance to look ahead for a collision
        // (i.e., the length of the collision ray)
        float raysLength = 1f;
        float whiskerLength = 0.5f;
        // calculate the collision ray vector
        Vector3 rayVector = agent.velocity;
        Debug.Log(rayVector);
        rayVector.y = 0f;
        rayVector.Normalize();
        // shoot the ray from the character's current position
        Vector3 rayStartPos = agent.transform.position;
        if(Physics.Raycast(rayStartPos, rayVector, out hit, raysLength)) {
            Debug.DrawRay(rayStartPos, hit.point, Color.green);
            Vector3 avoidTarget = hit.transform.position + hit.normal * avoidDistance;
            Vector3 direction = agent.position - avoidTarget;
            direction.Normalize();
            direction *= maxAcceleration;
            steering.linear = direction;
            steering.angular = Align(agent.position + direction);
            return steering;
           // TODO more of vid 
        }
        
        //rayVector.x += 1f;
        if (Physics.Raycast(rayStartPos, Quaternion.AngleAxis(30f, agent.transform.up) * rayVector, out hit, whiskerLength)) {
            Debug.DrawRay(rayStartPos, hit.point, Color.blue);
            Vector3 avoidTarget = -hit.transform.position + hit.normal * avoidDistance;
            Vector3 direction = avoidTarget - agent.position;
            direction.Normalize();
            direction *= maxAcceleration;
            steering.linear = direction;
            steering.angular = Align(agent.position + direction);
            return steering;
            // TODO more of vid 
        }
        
      //  rayVector.x -= 2 * 1f;
        if (Physics.Raycast(rayStartPos, Quaternion.AngleAxis(-30f, agent.transform.up) * rayVector, out hit, whiskerLength)) {
            Debug.DrawRay(rayStartPos, hit.point, Color.red);
            Vector3 avoidTarget = -hit.transform.position + hit.normal * avoidDistance;
            Vector3 direction = avoidTarget - agent.position;
            direction.Normalize();
            direction *= maxAcceleration;
            steering.linear = direction;
            steering.angular = Align(agent.position + direction);
            return steering;
            // TODO more of vid 
        }
        

        return steering;
    }
    */

    public Vector3 stop() {
        Vector3 direction = -agent.velocity;
        return direction.normalized * maxAcceleration;
    }




}
