using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// MapStateManager is the place to keep a succession of events or "states" when building 
/// a multi-step AI demo. Note that this is a way to manage 
/// 
/// State changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>

public class FieldMapManager : MonoBehaviour {
    // Set prefabs
    public GameObject PlayerPrefab;     // You, the player
    public GameObject HunterPrefab;     // Agent doing chasing
    public GameObject WolfPrefab;       // Agent getting chased
    public GameObject RedPrefab;        // Red Riding Hood, or just "team red"
    public GameObject BluePrefab;       // "team blue"
    public GameObject TreePrefab;       // New for Assignment #2

    public NPCController house;         // for future use

    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;

    public int TreeCount;
 
    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.
    private List<GameObject> trees;

    private int currentPhase = 0;           // This stores where in the "phases" the game is.
    private int previousPhase = 0;          // The "phases" we were just in

    //public int Phase => currentPhase;

    LineRenderer line;                 
    public GameObject[] Path;
    public Text narrator;
    [Header("our variables")]
    public static int fromForestNum;
    public int numBoids;
    public bool CamToPlayer;
    public bool firstClick;
    public GameObject[] pathOne; // first zigzag path 
    public GameObject[] pathTwo; // second zigzag paths
    LineRenderer line_2;
    public GameObject Line2;
    public GameObject SpawnerP1;
    public GameObject SpawnerP2;
    public int numPushed;
    public bool inPhase;

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    void Start() {
        // narrator.text = "This is the place to mention major things going on during the demo, the \"narration.\"";
        narrator.text = "Welcome to our demonstration of complex steering behaviors\n" +
                        "1 - Part 1\n" +
                        "2 - Part 2\n" +
                        "3 - Part 3\n" + "S: Start or Restart Simulation";
        trees = new List<GameObject>();
        SpawnTrees(TreeCount);

        spawnedNPCs = new List<GameObject>();
        Line2 = GameObject.Find("Line2");

        //spawnedNPCs.Add(SpawnItem(spawner1, HunterPrefab, null, SpawnText1, 4));
       // Invoke("SpawnWolf", 12);
       // Invoke("Meeting1", 30);
    }

    /// <summary>
    /// This is where you put the code that places the level in a particular phase.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    private void Update()
    {
        // if we transitioned from forest scene to here, the phase number has been assigned 
        if (fromForestNum == 1 || fromForestNum == 2) {
            if(fromForestNum != currentPhase) {
                previousPhase = currentPhase;
                currentPhase = fromForestNum;
                fromForestNum = 0;
            }
        }
        int num;
        

        string inputstring = Input.inputString;
        if (inputstring.Length > 0)
        {
            Debug.Log(inputstring);

            if (inputstring[0] == 'R')
            {
                DestroyTrees();
                SpawnTrees(TreeCount);
            }
            // check if the S button has been pressed to either start or restart selected presentation
            if(inputstring[0] == 'S') {
                if (currentPhase == 1) {
                    Restart();
                    EnterMapStateOne();
                }
                if(currentPhase == 2) {
                    Restart();
                    EnterMapStateTwo();
                }
                if(currentPhase == 3) {
                    Restart();
                    EnterMapStateThree();
                }
                
            }
            if(inputstring[0] == 'C' && currentPhase == 2) {
               // Text coneText = new Text;
                if (spawnedNPCs.Count != 0) {
                    for(int i = 0; i < spawnedNPCs.Count; i++) {
                        // spawnedNPCs[i].GetComponent<NPCController>().label.text;
                        spawnedNPCs[i].GetComponent<NPCController>().isCollisionPrediction = false;
                        spawnedNPCs[i].GetComponent<NPCController>().isConeCheck = true;
                        
                    }
                }
                narrator.text = "In Part 2, we will demonstrate two groups of agents following predefined paths.\n" +
                                    "Press S to start or restart\n" + "Press C for cone check\n" + "Press P for collision prediction\n" +
                                    "\n cone check active\n";
            }
            if (inputstring[0] == 'P' && currentPhase == 2) {
                // Text coneText = new Text;
                if (spawnedNPCs.Count != 0) {
                    for (int i = 0; i < spawnedNPCs.Count; i++) {
                        // spawnedNPCs[i].GetComponent<NPCController>().label.text;
                        spawnedNPCs[i].GetComponent<NPCController>().isConeCheck = false;
                        spawnedNPCs[i].GetComponent<NPCController>().isCollisionPrediction = true;

                    }
                }
                narrator.text = "In Part 2, we will demonstrate two groups of agents following predefined paths.\n" +
                                    "Press S to start or restart\n" + "Press C for cone check\n" + "Press P for collision prediction\n" +
                                    "\n collision prediction active\n";
            }
            if (inputstring[0] == 'J' && currentPhase == 2) {
                // Text coneText = new Text;
                if (spawnedNPCs.Count != 0) {
                    for (int i = 0; i < spawnedNPCs.Count; i++) {
                        // spawnedNPCs[i].GetComponent<NPCController>().label.text;
                        spawnedNPCs[i].GetComponent<NPCController>().isConeCheck = true;
                        spawnedNPCs[i].GetComponent<NPCController>().isCollisionPrediction = true;
                    }
                }
                narrator.text = "In Part 2, we will demonstrate two groups of agents following predefined paths.\n" +
                                    "Press S to start or restart\n" + "Press C for cone check\n" + "Press P for collision prediction\n" +
                                    "\n cone check/collision prediction active\n";
            }

            // Look for a number key click
            if (inputstring.Length > 0)
            {
                if (Int32.TryParse(inputstring, out num))
                {
                    if (num != currentPhase)
                    {
                        inPhase = false;
                        Restart();
                        previousPhase = currentPhase;
                        currentPhase = num;

                    }
                    
                }
            }
        } else {
            previousPhase = currentPhase;
        }
        // Check if a game event had caused a change of phase.
        if (currentPhase == previousPhase)
            return;


        /************* FRAMEWORK VERSION
       // If we get here, we've been given a new phase, from either source
       switch (currentPhase) {
           case 0:
               EnterMapStateZero();
               break;

           case 1:
               EnterMapStateOne();
               break;

           case 2:
               EnterMapStateTwo();
               break;

           case 3:
               break;
       }
       **************/

        switch (currentPhase)
            {
                case 0:
                    narrator.text = "Please enter a valid input:\n" +
                                    "1 - Part 1\n" + "2 - Part 2\n" +
                                    "3 - Part 3\n" + "S: Start or Restart Simulation";
                    break;
                case 1:
                    if(!inPhase) narrator.text = "Press S to start Part 1: Flocking of our simulation.";    
                    break;
                case 2:
                    narrator.text = "In Part 2, we will demonstrate two groups of agents following predefined paths.\n" +
                                    "Press S to start or restart\n" + "Press C for cone check\n" + "Press P for collision prediction\n" + 
                                    "Press J for Cone check and Collision Prediction";
                narrator.resizeTextForBestFit = true;
                narrator.alignment = TextAnchor.UpperLeft;
                //narrator.text = "Press S to start Part 2: Cone Check and Collision Prediction for Obstacle Avoidance";
                break;
                case 3:
                    Restart();
                    EnterMapStateThree();
               // narrator.text = "Press S to start Part 3: Raycasting for Obstacle Avoidance";
                    break;
            }
    }
    /* function that restarts the current presentation */
    private void Restart() {
        foreach(GameObject npc in spawnedNPCs) {
            Destroy(npc);
        }
        spawnedNPCs.Clear();
        ClearPaths();
       // line.positionCount = 0;
        //line_2.positionCount = 0;
    }
    private void EnterMapStateOne() {
        inPhase = true;
        narrator.text = "In Part 1, we will demonstrate the flocking behavior with a group of 20 agents"
                       + " following a lead boid";
        PlayerPrefab.SetActive(true);
        numBoids = 20;
        // delcare list of new flocking agents 
        List<GameObject> theFlock = new List<GameObject>();
        // make the lead boid the player 
        PlayerPrefab.GetComponent<NPCController>().isLeadBoid = true;
        PlayerPrefab.GetComponent<SteeringBehavior>().isLeadSteering = true;
        // add to list of spawnedNPCs
        //spawnedNPCs.Add(tempPlayer);
        // add each boid to the list of spawnedNPCs and to the list of flocking agents 
        for (int i = 0; i < numBoids; i++) {
            GameObject temp = SpawnItem(spawner1, WolfPrefab, PlayerPrefab.GetComponent<NPCController>(), SpawnText1, 1);
            spawnedNPCs.Add(temp);
            theFlock.Add(temp);
        }
       // theFlock.Add(PlayerPrefab); NO 
        // set the list for each agent 
        for (int i = 0; i < spawnedNPCs.Count; i++) {
            spawnedNPCs[i].GetComponent<SteeringBehavior>().neighbourDistance = 3f;
            spawnedNPCs[i].GetComponent<SteeringBehavior>().SetFlock(theFlock);
        }
        
    }

    private void EnterMapStateTwo()
    {
        PlayerPrefab.gameObject.SetActive(false);
        CreatePath();
        numBoids = 12;

        // add all flockers to list of spawned npcs 
        for (int i = 0; i < numBoids; i++) {
            GameObject temp;
            if(i > 5) { // for the second list of flockers, spawn in different place (Spawner 2)
                temp = SpawnItem(SpawnerP2, WolfPrefab, null, SpawnText2, 0);
            } else {
                temp = SpawnItem(SpawnerP1, WolfPrefab, null, SpawnText2, 0); // spawn in (Spawner 1)
            }
            spawnedNPCs.Add(temp); // add to list 
        }
        // initialize two lists of flockers 
        List<GameObject> flock1 = new List<GameObject>();
        List<GameObject> flock2 = new List<GameObject>();
        // make the first group follow the path of the first lead boid and path one 
        for (int i = 0; i < 6; i++) {
            spawnedNPCs[i].GetComponent<SteeringBehavior>().follower = spawnedNPCs[0].GetComponent<SteeringBehavior>().agent;
            spawnedNPCs[i].GetComponent<SteeringBehavior>().setPath(pathOne);
            spawnedNPCs[i].GetComponent<NPCController>().phase = 2;
            flock1.Add(spawnedNPCs[i]);
        }
        // make the second group follow the path of the second lead boid and path two 
        for(int i = 6; i < numBoids; i++) {
            spawnedNPCs[i].GetComponent<SteeringBehavior>().follower = spawnedNPCs[6].GetComponent<SteeringBehavior>().agent;
            spawnedNPCs[i].GetComponent<SteeringBehavior>().setPath(pathTwo);
            spawnedNPCs[i].GetComponent<NPCController>().phase = 2;
            spawnedNPCs[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
            flock2.Add(spawnedNPCs[i]);
        }
        // set flocks to avoid for both 
        for(int i = 0; i < flock1.Count; i++) {
            // first flock avoids second flock 
            flock1[i].GetComponent<SteeringBehavior>().SetFlock(flock1);
            flock1[i].GetComponent<SteeringBehavior>().SetOtherFlock(flock2);
            // second flock avoids first flock 
            flock2[i].GetComponent<SteeringBehavior>().SetFlock(flock2);
            flock2[i].GetComponent<SteeringBehavior>().SetOtherFlock(flock1);
        }


    }

    private void EnterMapStateThree()
    {
        ForestMapManager.fromFieldNum = 3;
        SceneManager.LoadScene("forest", 0);
        // narrator.text = "Entering Phase Three";

        // currentPhase = 2; // or whatever. Won't necessarily advance the phase every time

        //spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }


    // ... Etc. Etc.

    /// <summary>
    /// SpawnItem placess an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="phase"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText, int phase)
    {

        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2), 0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity);
        if (target)
        {
            temp.GetComponent<SteeringBehavior>().target = target;
        }
        temp.GetComponent<NPCController>().label = spawnText;
        temp.GetComponent<NPCController>().phase = phase;
        //Camera.main.GetComponent<CameraController>().player = temp;
        return temp;
    }

    /// <summary>
    /// SpawnTrees will randomly place tree prefabs all over the map. The diameters
    /// of the trees are also varied randomly.
    /// 
    /// Note that it isn't particularly smart about this (yet): notably, it doesn't
    /// check first to see if there is something already there. This should get fixed.
    /// </summary>
    /// <param name="numTrees">desired number of trees</param>
    private void SpawnTrees(int numTrees)
    {
        float MAX_X = 25;  // Size of the map; ideally, these shouldn't be hard coded
        float MAX_Z = 20;
        float less_X = MAX_X - 1;
        float less_Z = MAX_Z - 1;

        float diameter;

        for (int i = 0; i < numTrees; i++)
        {
            //Vector3 size = spawner.transform.localScale;
            Vector3 position = new Vector3(UnityEngine.Random.Range(-less_X, less_X), 0, UnityEngine.Random.Range(-less_Z, less_Z));
            GameObject temp = Instantiate(TreePrefab, position, Quaternion.identity);

            // diameter will be somewhere between .2 and .7 for both X and Z:
            diameter = UnityEngine.Random.Range(0.2F, 0.7F);
            temp.transform.localScale = new Vector3(diameter, 1.0F, diameter);

            trees.Add(temp);
          
        }
    }

    private void DestroyTrees()
    {
        GameObject temp;
        for (int i = 0; i < trees.Count; i++)
        {
            temp = trees[i];
            Destroy(temp);
        }
        // Following this, write whatever methods you need that you can bolt together to 
        // create more complex movement behaviors.
    }
    private void SpawnWolf()
    {
        narrator.text = "The Wolf appears. Most wolves are ferocious, but this one is docile.";
        spawnedNPCs.Add(SpawnItem(spawner2, WolfPrefab, null, SpawnText2, 4));
    }

    private void Meeting1() {
        if (currentPhase == 0) {
            spawnedNPCs[0].GetComponent<SteeringBehavior>().target = spawnedNPCs[1].GetComponent<NPCController>();
            spawnedNPCs[1].GetComponent<SteeringBehavior>().target = spawnedNPCs[0].GetComponent<NPCController>();
            SetArrive(spawnedNPCs[0]);
            SetArrive(spawnedNPCs[1]);
        }
    }

    private void SpawnWolf2() {
        narrator.text = "The Wolf looks for shelter, and spots little Red.";
        spawnedNPCs.Add(SpawnItem(spawner3, WolfPrefab, spawnedNPCs[2].GetComponent<NPCController>(), SpawnText1, 1));
        spawnedNPCs[3].GetComponent<NPCController>().label.enabled = true;
    }

    private void Meeting2() {
        narrator.text = "The two converse, and little Red directs the Wolf to her house.";
        spawnedNPCs[2].GetComponent<NPCController>().DestroyPoints();
        spawnedNPCs[2].GetComponent<NPCController>().phase = 5;
        spawnedNPCs[3].GetComponent<SteeringBehavior>().target = house;
        spawnedNPCs[3].GetComponent<NPCController>().phase = 1; ;
        Invoke("SpawnHunter", 10);
    }

    private void SpawnHunter() {
        narrator.text = "The Hunter arrives, determined to catch the killer. He spots a house and moves accordingly.";
        spawnedNPCs.Add(SpawnItem(spawner3, HunterPrefab, house, SpawnText2, 1));
        spawnedNPCs[4].GetComponent<NPCController>().label.enabled = true;
    }

    private void End() {
        narrator.text = "Days later, reports come in. The killer is still at large, but police have found one clue on its identity. "
            +"A little red hood. END";
        currentPhase++;
    }

    private void SetArrive(GameObject character) {

        character.GetComponent<NPCController>().phase = 3;
        character.GetComponent<NPCController>().DrawConcentricCircle(character.GetComponent<SteeringBehavior>().slowRadiusL);
    }

    private void CreatePath()
    {
        Color red =  Color.red;
        Color blue = Color.blue;
        // create the first path 
        line = GetComponent<LineRenderer>();
        // set its color to red 
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = red;
        line.endColor = red;
        line.positionCount = pathOne.Length;
        for (int i = 0; i < pathOne.Length; i++)
        {
            line.SetPosition(i, pathOne[i].transform.position);
        }
        // create the second path 
        line_2 = Line2.GetComponent<LineRenderer>(); 
        // set its color to blue 
        line_2.material = new Material(Shader.Find("Sprites/Default"));
        line_2.startColor = blue;
        line_2.endColor = blue;
        line_2.positionCount = pathTwo.Length;
        for(int i = 0; i < pathTwo.Length; i++) {
            line_2.SetPosition(i, pathTwo[i].transform.position);
        }
    }
    private void ClearPaths() {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line_2 = Line2.GetComponent<LineRenderer>();
        line_2.positionCount = 0;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
