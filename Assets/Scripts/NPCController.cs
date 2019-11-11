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
    public bool isLeadBoid;
    public PlayerController redLead;
    public GameObject fieldManager;
    int count = 0;
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

                linear =  0.8f * ai.Pursue() + 1.5f * ai.computeSeparation() + 1f * ai.computeCohesion() + 0.9f * ai.computeAlign();
                //linear = 0.5f * ai.Pursue() + ai.avoid_collisions();
                if(count < 5) {
                  //  Debug.Log("here is the lin" + linear);
                }
                count++;

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 2: // PATH FOLLOWING WITH CONE CHECK/ COLLISION PREDICTION FOR PART 2
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\n"; // not title for following flockers 
                }

                linear = 0.8f * ai.followPath();
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 3: 
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\n";
                }

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 4: // LEAD BOID (PLAYER)
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nLead Boid: You";
                }
                isLeadBoid = true;
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 5: // PATH FOLLOWING WITH OBSTACLE AVOIDANCE FOR PART 2: LEAD BOID
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nLead boid";
                }

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
        }
        
        update(linear, angular, Time.deltaTime);
        
        
        if (label) {
          //  label.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    private void update(Vector3 steeringlin, float steeringang, float time) {

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
