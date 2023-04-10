using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Utils;

public class ram_chase : MonoBehaviour
{

    /* 
        find what node is closest to player tank.
        find path to that node. 
        traverse path to that node, until direct line of sight created to tank

        upon direct line of sight, follow player tank directly, until again out of site- in this case, note last location, beeline for it, then follow exact steps enemy tank took until it is found, then beeline effect activated.
    
    */
    //made up types
    public enum ai_enum { chaser, rammer };

    //tank stuffs
    private GameObject player_tank;
    private GameObject ai_turret;
    private GameObject shell;
    private GameObject pew_location;
    private AudioSource ekusupurojon_sound;

    //Audio stuffs
    private AudioSource tank_audio_source;
    private AudioClip driving_sfx;
    private AudioClip engine_idle;

    //Settings
    [Range(1, 5)]
    public int difficulty;
    public ai_enum ai_type;
    public Material override_color;
    public ParticleSystem death_ekusupurojon;

    private float stop_distance = 1.0f;

    //ai specific stuff
    private Action previous_state;                          //holds previous policy for directing this tank
    private Action movement_policy;                         //only handles driving, holds method to run depending on game state
    private Action updateFSM;                               //holds FSM change sequence
    private Vector3 future_location;                        //stores next location AI is moving towards - mvp value of this enemy tank
    private bool player_sighted;                            //i mean...
    private GameObject ghost_mf;                            //invisible thingie that is actually followed
    private float look_ahead = 2.0f;                        //how far ghost mf can be ahead of tank
    private float next_shot_time;                           //when the next shot will be made by tank
    private float gps_refresh_rate = 0.20f;                 //how often gps should recalculate path
    private float next_gps_refresh;                         //next time refresh should happen.
    private List<top_level_eye> eyes_of_ra;                 //holds each eye and all eyes visible from it
    private bool ghost_mode = false;                        //follow the ghost? or a direct loc? meant to resolve bad paths on corners
    private Vector3 mokuteki;                               //actual loc that will always be chased
    private bool is_shooter;                                //does the tank shoot?
    private List<int> waypoints;                            //path for tank to follow as list of ints
    private int wp_index;                                   //which waypoint within waypoints list tank currently going to
    private float stay_away_distance;                       //how far from the player tank will the enemy tank try to be

    //difficulty stuff
    private float speed = 0.5f;
    private float rot_speed = 10.0f;
    private float shot_range = 5.0f;
    private float shot_force = 2000.0f;
    private float fire_rate = 0.25f;

    //TESTING TESTING TESTING


    private Vector3 testing_location;

    // Start is called before the first frame update
    void Start()
    {
        //TESTING TESTING TESTING


        //Load some stuff from within

        //Load Gameobjects
        this.player_tank = GameObject.FindGameObjectWithTag("Player");
        this.ai_turret = this.transform.Find("TankRenderers/TankTurret").gameObject;
        GameObject [] temp_eyes_of_ra = GameObject.FindGameObjectsWithTag("Eyes of Ra");

        this.shell = Resources.Load<GameObject>("Prefabs/Shell");
        this.pew_location = this.transform.Find("TankRenderers/TankTurret/gun").gameObject;

        //Load Materials
        Material red = Resources.Load<Material>("Materials/Red");
        Material orange = Resources.Load<Material>("Materials/Orange");
        Material blue = Resources.Load<Material>("Materials/Blue");

        //Load Audio
        this.tank_audio_source = this.AddComponent<AudioSource>();
        this.engine_idle = Resources.Load<AudioClip>("Sound/EngineIdle");
        this.driving_sfx = Resources.Load<AudioClip>("Sound/EngineDriving");
        this.ekusupurojon_sound = this.AddComponent<AudioSource>();
        this.ekusupurojon_sound.clip = Resources.Load<AudioClip>("Sound/EnemyExplosion");


        //set some initial stuff
        this.tank_audio_source.clip = this.driving_sfx;
        this.set_difficulty_settings();
        this.difficulty = 1;
        this.next_shot_time = 0;
        this.ghost_mf_setup();
        this.eyes_of_ra = new List<top_level_eye>();
        this.enscribe_eyes(ref temp_eyes_of_ra);
        this.waypoints = new List<int>();

        //check what type of tank this is - to allow future tank types to exist... maybe... please dont just leave the project as is...
        switch (ai_type)
        {
            case ai_enum.rammer:
                this.set_tankie(this.updateFSM_rammer, this.ai_mode_gps, red, false, "rammer", 0); break;
            case ai_enum.chaser:
                this.set_tankie(this.updateFSM_rammer, this.ai_mode_gps, blue, true, "chaser", 7); break;
        }

        //TESTING TESTING TESTING TESTING

    }

    // Update is called once per frame
    private void Update()
    {
        if (player_tank == null)
        {
            this.tank_audio_source.clip = engine_idle;
            return;
        }
        this.mokuteki = this.get_goal();
        this.player_sighted = this.player_spotted();
        this.updateFSM();
        this.drive();
        this.operate_turret();
        this.let_that_thang_rang();
    }

    private void FixedUpdate()
    {
        if (player_tank == null)
            return;
        this.movement_policy();
        //this.tank_audio_source.Play();
    }

    //ALL FSM DEFINITIONS USING METHODS BELOW THESE FSM DEF'S INSTRUCTING HOW EACH TYPE OF TANK WILL MOVE.
    //did this way to make it easier to make new FSM definitions, chosen over putting into methods directly
    private void updateFSM_rammer()
    {
        if (this.player_sighted)
            this.movement_policy = this.ai_mode_follow;
        else
            this.movement_policy = this.ai_mode_gps;
    }

    //MAKES MOVE TO GOAL BASED ON NEXT LOCATION MOVEMENT POLICY HAS DECIDED IT SHOULD BE AT.
    private void drive()
    {
        //drive towards next location as dictated by mokuteki
        if ((this.mokuteki - this.transform.position).magnitude < this.stop_distance)
            return;

        Vector4 rotation = (this.mokuteki - this.transform.position);
        Quaternion look_at_goal = Quaternion.LookRotation(rotation, Vector3.up);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, look_at_goal, this.rot_speed * Time.deltaTime);
        if (Vector3.Angle(this.transform.forward, rotation) < 35.0f)    //meant to not drive forward when facing the opposite direction goal is at, and only turn instead
            this.transform.Translate(0, 0, this.speed * Time.deltaTime);
    }

    private void operate_turret()
    {
        //shout out to nathan for writing this beast (i am a pirate)
        Vector3 flat_location = player_tank.transform.position - this.ai_turret.transform.position;
        flat_location.y = 0;
        Quaternion look_at_player = Quaternion.LookRotation(flat_location , Vector3.up);
        this.ai_turret.transform.rotation = Quaternion.Slerp(this.ai_turret.transform.rotation, look_at_player, this.rot_speed * Time.deltaTime);
    }

    private void let_that_thang_rang()
    {
        if(this.is_shooter && this.player_sighted && Time.time >= this.next_shot_time) //beef iss on sight we dont play here
        {
            // spawn and tag shell
            GameObject shellClone = Instantiate(shell, this.pew_location.transform.position, this.pew_location.transform.rotation);
            shellClone.GetComponent<bullet>().maxLife = 5f;
            shellClone.tag = "enemyShell";

            // add forward force to shell
            shellClone.GetComponent<Rigidbody>().AddForce(this.pew_location.transform.forward * this.shot_force);

            //next time to shoot
            this.next_shot_time = Time.time + 1f / this.fire_rate;
        }
    }

    //goes towards future location as located by algorithm currently in use
    void ghost_tracker()
    {
        // stop and wait if player is too far behind
        if (Vector3.Distance(ghost_mf.transform.position, this.transform.position) > this.look_ahead)
            return;

        // snap to next waypoint and move directly to it
        ghost_mf.transform.LookAt(this.future_location);
        ghost_mf.transform.Translate(0, 0, this.speed * Time.deltaTime);
    }

    //is there a direct line of sight with the player?
    private bool player_spotted()
    {
        Physics.Linecast(this.pew_location.transform.position, this.player_tank.GetComponent<Renderer>().bounds.ClosestPoint(this.player_tank.transform.position), out RaycastHit hit);
        return (hit.point - this.player_tank.transform.position).magnitude < this.stop_distance;
    }

    //conditional allows for creating extra waypoints when geometry necessitates it, follow ghost or go straight to the goal (smooth or no smoothing)
    private Vector3 get_goal()
    {
        Vector3 mokuteki_temp;
        if (this.ghost_mode)
            mokuteki_temp = this.ghost_mf.transform.position;
        else
            mokuteki_temp = this.future_location;

        return mokuteki_temp;
    }

    //SOME METHODS HOLDING DIFFERENT MODES AI USES TO FIND THE NEXT LOCATION IT SHOULD AIM TO BE AT

    //nighttime terrorizer
    private void ai_mode_follow()
    {
        //Debug.Log("ai mode follow");
        if ((this.transform.position - this.player_tank.transform.position).magnitude < this.stay_away_distance)
            return;
        this.future_location = this.player_tank.transform.position;
    }

    //uses position of player tank which is not in direct line of sight to find a path with eyes of ra
    private void ai_mode_gps()
    {


        if (this.waypoints.Count > wp_index + 1)
        {
            float tank_waypoint_distance = (this.transform.position - this.eyes_of_ra[this.waypoints[wp_index]].eye.transform.position).magnitude;
            if(tank_waypoint_distance < this.stop_distance)
                wp_index++;

            this.future_location = this.eyes_of_ra[this.waypoints[wp_index]].eye.transform.position;
        }
        if (Time.time < this.next_gps_refresh)
            return;

        //uses position of player tank which is not in direct line of sight to find a path with eyes of ra
        float player_champion;
        float player_challenger;
        int player_champ_id;
        float ai_champion;
        float ai_challenger;
        int ai_champ_id;
        player_champ_id = 0;
        ai_champ_id = 0;
        player_champion = (this.eyes_of_ra[0].eye.transform.position - this.player_tank.transform.position).magnitude;
        ai_champion = (this.eyes_of_ra[0].eye.transform.position - this.transform.position).magnitude;
        for (int index = 1; index < this.eyes_of_ra.Count; index++)
        {
            player_challenger = (this.eyes_of_ra[index].eye.transform.position - this.player_tank.transform.position).magnitude;
            if (player_challenger < player_champion)
            {
                player_champion = player_challenger;
                player_champ_id = index;
            }

            ai_challenger = (this.eyes_of_ra[index].eye.transform.position - this.transform.position).magnitude;
            if (ai_challenger < ai_champion)
            {
                ai_champion = ai_challenger;
                ai_champ_id = index;
            }
        }
        //ai_champ_id holds index of eye closest to enemy, and player_champ_id of eye closest to player at this point
        //pathfind to node closest to that mf we got beef wit
        this.waypoints = this.astar(ai_champ_id, player_champ_id);
        this.wp_index = 0;

        //set where tank should travel to
        this.future_location = this.eyes_of_ra[this.waypoints[wp_index]].eye.transform.position;
        this.next_gps_refresh = Time.time + 1f / this.gps_refresh_rate;

    }
    private void ai_mode_resolve_bad_path()
    {
        //sets mid way point between goal and obstacles to successfully clear it.
    }

    private void ai_mode_patrol()
    {
        //uses input game objects to patrol a location
    }

    //SETTING SOME RESOURCES

    private void ghost_mf_setup()
    {
        this.ghost_mf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        DestroyImmediate(ghost_mf.GetComponent<Collider>());
        this.ghost_mf.GetComponent<MeshRenderer>().enabled = false;
        this.ghost_mf.transform.position = this.transform.position;
        this.ghost_mf.transform.rotation = this.transform.rotation;
    }

    //make the whole tank the color a material is, didnt seem like you could make colors so i just used materials. easily refactorable tho if necessary
    private void set_tank_color(Material mat)
    {
        Transform my_son =  this.transform.Find("TankRenderers");
        for (int index = 0; index < my_son.childCount; index++)
            my_son.GetChild(index).GetComponent<Renderer>().material.color = mat.color;
    }

    private void set_tankie(Action updateFSM_type, Action ai_mode, Material mat, bool is_shooter, string tag_name, float stay_away_distance)
    {
        this.stay_away_distance = stay_away_distance;
        this.gameObject.tag = tag_name;
        this.updateFSM = updateFSM_type;
        this.movement_policy = ai_mode;
        this.is_shooter = is_shooter;
        if (this.override_color == null)
            this.set_tank_color(mat);
        else
            this.set_tank_color(this.override_color);
    }

    private void set_difficulty_settings()
    {
        this.speed *= this.difficulty;
        this.rot_speed *= this.difficulty;
        this.shot_range *= this.difficulty;
        this.fire_rate *= this.difficulty;
    }

    //HELPER METHODS
    private void enscribe_eyes(ref GameObject[] temp_eyes)
    {
        for(int index = 0; index < temp_eyes.Length; index++)
        {
            this.eyes_of_ra.Add(new top_level_eye(ref temp_eyes[index]));
        }
    }

    //return a list of visible neighbours that are visible
    private List<int> visible_nodes(int index)
    {
        List<int> temp_vis = new List<int>();
        for (int closed_eye = 0; closed_eye < this.eyes_of_ra.Count; closed_eye++)
        {
            if (!Physics.Linecast(this.eyes_of_ra[index].eye.transform.position, this.eyes_of_ra[closed_eye].eye.transform.position))
            {
                temp_vis.Add(closed_eye);
            }
        }

        return temp_vis;
    }

    private List<int> astar(int start, int end)
    {
        int eye_num = -1;
        float minecraft_distance;
        bool goal_found = false;
        float total_cost;
        List<int> visited = new List<int>();
        List<int> children;
        for (int index = 0; index < this.eyes_of_ra.Count; index++)
        {
            this.eyes_of_ra[index].distance_so_far = 0;
            this.eyes_of_ra[index].parent_index = -1;
            this.eyes_of_ra[index].nodes_so_far = 0;
        }

        PriorityQueue<int, float> queue = new PriorityQueue<int, float>();
        queue.Enqueue(start, 0);
        visited.Add(start);

        while(queue.Count > 0 && !goal_found)
        {
            eye_num = queue.Dequeue();
            if (eye_num == end)
                break;

            children = this.visible_nodes(eye_num);
            for (int index = 0; index < children.Count; index++)
            {
                minecraft_distance = this.manhattan_distance(this.eyes_of_ra[children[index]].eye.transform.position, this.player_tank.transform.position);
                this.eyes_of_ra[children[index]].distance_so_far += this.eyes_of_ra[eye_num].distance_so_far + (this.eyes_of_ra[eye_num].eye.transform.position - this.eyes_of_ra[index].eye.transform.position).magnitude;
                this.eyes_of_ra[children[index]].nodes_so_far = this.eyes_of_ra[eye_num].nodes_so_far + 1;
                total_cost = minecraft_distance + this.eyes_of_ra[children[index]].distance_so_far + this.eyes_of_ra[children[index]].nodes_so_far;
                if (!visited.Contains(children[index]) || total_cost < this.eyes_of_ra[children[index]].cost_so_far)
                {
                    this.eyes_of_ra[children[index]].cost_so_far = total_cost;
                    visited.Add(children[index]);
                    this.eyes_of_ra[children[index]].parent_index = eye_num;
                    queue.Enqueue(children[index], this.eyes_of_ra[children[index]].cost_so_far);
                }
            }
        }

        return this.backtrack_path(start, eye_num);
    }


    private float manhattan_distance(Vector3 start, Vector3 end)
    {
        Vector3 distance = end - start;
        return distance.x + distance.y;
    }

    private List<int> backtrack_path(int start, int tail)
    {
        List<int> path = new List<int>();
        while (start != tail)
        {
            path.Add(tail);
            tail = this.eyes_of_ra[tail].parent_index;
        }
        path.Add(tail);
        path.Reverse();

        return path;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "shell")
        {
            death_ekusupurojon.Play();
            ekusupurojon_sound.Play();
            Destroy(this);
        }
    }



    // STEVE THIS SECTION FOR YOU MY KING IF YOU WANT TO MAKE THE CHASERS THAT KEEPS DISTANCE
    //ACTUALLY NVM THIS MF BROKEN AS HELL JUST LEAVE IT BE

    //FIRST CREATE YOUR OWN STATE CHANGING DECIDING FUNCTION IN HERE:
    private void updateFSM_chaser() 
    {
        //you need to set the "function pointer" this.movement policy to something, depending on the world state and stuff
        //an example is held in this function so you can ctrl click to it:
        this.updateFSM_rammer();
        //you have this.player_sighted bool to know if the player has been seen, and ai_mode_gps to travel to the player:
        this.movement_policy = this.ai_mode_gps;
    }



}

class top_level_eye
{
    public GameObject eye;
    public List<GameObject> visible_eyes;
    public int parent_index;
    public float distance_so_far;
    public int nodes_so_far;
    public float cost_so_far;
    public top_level_eye(ref GameObject eye)
    {
        this.eye = eye;
    }
}