using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour {
    // Store variables for objects
    private SteeringBehavior ai;    // Put all the brains for steering in its own module
    private Rigidbody rb;           // You'll need this for dynamic steering

    // For speed 
    public Vector3 position;        // local pointer to the RigidBody's Location vector
    public Vector3 velocity;        // Will be needed for dynamic steering

    // For rotation
    public float orientation;       // scalar float for agent's current orientation
    public float rotation;          // Will be needed for dynamic steering

    public float maxSpeed;          // what it says

    public int phase;               // use this to control which "phase" the demo is in

    private Vector3 linear;         // The resilts of the kinematic steering requested
    private float angular;          // The resilts of the kinematic steering requested

    public Text label;              // Used to displaying text nearby the agent as it moves around
    LineRenderer line;              // Used to draw circles and other things
    [Header("Our variables")]
    public bool isLeadBoid; // for when the player is the lead boid 
    public PlayerController redLead;
    public GameObject fieldManager;
    public bool isConeCheck;
    public bool isCollisionPrediction;
    int count = 0;
    public bool pathBoidLeader; // for path following, all other boids will follow leader 
    private void Start() {
        ai = GetComponent<SteeringBehavior>();
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        fieldManager = GameObject.FindGameObjectWithTag("gameManager");   
        position = rb.position;
        orientation = transform.eulerAngles.y;
        if(phase == 4 || phase == 0) {
            redLead = GameObject.FindGameObjectWithTag("Red").GetComponent<PlayerController>();
        }
        
       
        
 
    }

    /// <summary>
    /// Depending on the phase the demo is in, have the agent do the appropriate steering.
    /// 
    /// </summary>
    void FixedUpdate() {
        switch (phase) {
            case 0:
                // nothing, temporary for setting paths
                break;
            case 1: // FLOCKING FOR PART 1 
                if (label) {
                    // replace "First algorithm" with the name of the actual algorithm you're demoing
                    // do this for each phase
                    label.text = name.Replace("(Clone)","") + "\nAlgorithm: Flocking"; 
                   
                }
                // give each behavior (pursue, separation, cohesion, and alignment) a different weight and add up for the linear
                linear =  0.8f * ai.Pursue() + 10f * ai.computeSeparation()  + 0.2f * ai.computeCohesion() + 0.4f * ai.computeAlign().linear;               
                angular = ai.Face() + 0.7f * ai.computeAlign().angular;
                // will be facing character and direction of average velocity, so draw circle to show where facing 
                DrawCircle(this.position + this.transform.forward * 1.7f, 0.5f);

                //linear = 0.5f * ai.Pursue() + ai.avoid_collisions();
              
                //count++;

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 2: // PATH FOLLOWING WITH CONE CHECK/ COLLISION PREDICTION FOR PART 2
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\n"; // not title for following flockers 
                }
                linear = ai.followPath().linear * 0.7f + 5f * ai.computeSeparation() + 0.5f * ai.computeCohesion() + 0.25f * ai.computeAlign().linear;
                
                angular = ai.followPath().angular + ai.computeAlign().angular;
                // doing cone check only
                if (isConeCheck && !isCollisionPrediction && ai.ConeCheck() != Vector3.zero) {
                    linear = ai.ConeCheck() + 1f * ai.computeSeparation() + 0.2f * ai.computeAlign().linear + 0.1f * ai.computeCohesion();
                }
                // doing collision prediction only 
                if(isCollisionPrediction && !isConeCheck && ai.CollisionPrediction() != Vector3.zero) {
                    linear = ai.CollisionPrediction() + 0.95f * ai.followPath().linear + 5f * ai.computeSeparation() + 0.6f * ai.computeCohesion() + 0.2f * ai.computeAlign().linear;
                }
                // doing both 
                if(isConeCheck && isCollisionPrediction) {
                    if(ai.CollisionPrediction() != Vector3.zero && ai.ConeCheck() != Vector3.zero) {
                        linear = 0.7f * ai.CollisionPrediction() + ai.followPath().linear + 6f * ai.computeSeparation() + 
                                 0.5f * ai.computeCohesion() + 0.2f * ai.computeAlign().linear + ai.ConeCheck();
                    } else {
                        if (ai.CollisionPrediction() != Vector3.zero) {
                            linear = ai.CollisionPrediction() + 0.9f * ai.followPath().linear + 5f * ai.computeSeparation() + 0.5f * ai.computeCohesion() + 0.2f * ai.computeAlign().linear;
                        }
                        if (ai.ConeCheck() != Vector3.zero) {
                            linear = ai.ConeCheck() + 1f * ai.computeSeparation() + 0.2f * ai.computeAlign().linear;
                        }
                    }
                    
                }
                
                
                 
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 3: // PATH FOLLOWING WITH OBSTACLE AVOIDANCE FOR PART 3: LEAD BOID
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\n";
                }
                

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 4: // LEAD BOID FOR PART 1 (PLAYER)
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nLead Boid: You";
                }
                isLeadBoid = true;
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 5: // PATH FOLLOWING WITH OBSTACLE AVOIDANCE FOR PART 3: FOLLOWING FLOCKERS
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nLead boid";
                }
                linear = ai.followAndRaycast().linear;
                angular = ai.followAndRaycast().angular;
                DrawCircle(this.position + transform.forward * 1.7f, 0.5f);
                // will be facing character and direction of average velocity, so draw circle to show where facing 
                //DrawCircle(this.position + (ai.followAndRaycast().linear + 0.9f * ai.computeAlign().linear), 0.75f);
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
        }
        
        update(linear, angular, Time.deltaTime);
        
        
        if (label) {
            label.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    public void update(Vector3 steeringlin, float steeringang, float time) {

        if (!isLeadBoid) {
            // Update the orientation, velocity and rotation
            orientation += rotation * time;
            velocity += steeringlin * time;
            rotation += steeringang * time;

            if (velocity.magnitude > maxSpeed) {
                velocity.Normalize();
                velocity *= maxSpeed;
            }

            rb.AddForce(velocity - rb.velocity, ForceMode.VelocityChange);
            position = rb.position;
            rb.MoveRotation(Quaternion.Euler(new Vector3(0, Mathf.Rad2Deg * orientation, 0)));
        } else {
            if(redLead != null) {
                position = redLead.transform.position;
            }

            //velocity = redLead.GetComponent<PlayerController>().velocity;
        }
      
    }

    // <summary>
    // The next two methods are used to draw circles in various places as part of demoing the
    // algorithms.

    /// <summary>
    /// Draws a circle with passed-in radius around the center point of the NPC itself.
    /// </summary>
    /// <param name="radius">Desired radius of the concentric circle</param>
    public void DrawConcentricCircle(float radius) {
        line.positionCount = 51;
        line.useWorldSpace = false;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z));
            angle += (360f / 51);
        }
    }

    /// <summary>
    /// Draws a circle with passed-in radius and arbitrary position relative to center of
    /// the NPC.
    /// </summary>
    /// <param name="position">position relative to the center point of the NPC</param>
    /// <param name="radius">>Desired radius of the circle</param>
    public void DrawCircle(Vector3 position, float radius) {
        line.positionCount = 51;
        line.useWorldSpace = true;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z)+position);
            angle += (360f / 51);
        }
    }

    public void DestroyPoints() {
        if (line) {
            line.positionCount = 0;
        }
    }

}
