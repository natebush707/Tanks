using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class chaser_tank_ai : MonoBehaviour
{

    /* 
        find what node is closest to player tank.
        find path to that node. 
        traverse path to that node, until direct line of sight created to tank

        upon direct line of sight, follow player tank directly, until again out of site- in this case, note last location, beeline for it, then follow exact steps enemy tank took until it is found, then beeline effect activated.
    
    */
    //tank stuffs
    private GameObject player_tank;
    private GameObject ai_turret;

    //Audio stuffs
    private AudioSource tank_audio_source;
    private AudioClip drivingSfx;
    private AudioClip engineIdle;

    //Settings
    public int ai_difficulty;
    public String ai_type;

    //ai specific stuff
    private Action previous_state;                          //holds previous policy for directing this tank
    private Action movement_policy;                         //only handles driving, holds method to run depending on game state
    private Action updateFSM;                               //holds FSM change sequence
    private GameObject[] eyes_of_ra;                        //has locations of all eyes of ra, which are spots from which player tank is detected
    private Vector2 future_location;                        //stores next location AI is moving towards - mvp value of this enemy tank
    public GameObject[] patrolling_checkpoints;             //allows for more than one patroller to exist on any given map - instead of using tag method which would be kind of tedious for more than one patroller


    private Vector2 testing_location;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("inside start beignning");
        this.player_tank = GameObject.FindGameObjectWithTag("Player");
        this.ai_turret = this.transform.Find("tank_player/Turret.018").gameObject;
        Debug.Log("turret loaded");
        this.eyes_of_ra = GameObject.FindGameObjectsWithTag("Eyes of Ra");
        Debug.Log("afer ra");
        this.tank_audio_source = this.AddComponent<AudioSource>();
        this.engineIdle = Resources.Load<AudioClip>("EngineIdle.aiff");
        this.drivingSfx = Resources.Load<AudioClip>("EngineDriving.aiff");
        Debug.Log("yooo");
        Debug.Log(this.drivingSfx);
        this.tank_audio_source.clip = this.engineIdle;

        //check what type of tank this is - to allow future tank types to exist... maybe... please dont just leave the project as is...
        switch(this.ai_type.ToLower())
        {
            case "rammer":
                this.updateFSM = updateFSM_rammer;
                this.movement_policy = this.ai_mode_gps;
                break;
            case "chaser":
                this.updateFSM = updateFSM_chaser;
                this.movement_policy = this.ai_mode_gps;
                break;
            case "patroller":
                this.updateFSM = updateFSM_patroller;
                this.movement_policy = this.ai_mode_patrol;
                break;
            default:
                //bro forgor :skull:
                throw new Exception("bruh moment type: didnt specify what kind of enemy");
        }

        //TESTING TESTING TESTING TESTING
        testing_location = new Vector2(30, 20);
        this.ai_difficulty = 45;


    }

    // Update is called once per frame
    private void Update()
    {
        this.handle_audio();
    }

    private void FixedUpdate()
    {
        if (player_tank == null)
        {
            this.tank_audio_source.clip = engineIdle;
            return;
        }

        this.drive();
        return;
        this.updateFSM();
        this.movement_policy();
        this.drive();
        this.operate_turret();
    }

    void handle_audio()
    {
        this.tank_audio_source.Play();
        Debug.Log("audio played...?", this.tank_audio_source.clip);
    }

    //ALL FSM DEFINITIONS USING METHODS BELOW THESE FSM DEF'S INSTRUCTING HOW EACH TYPE OF TANK WILL MOVE.
    //did this way to make it easier to make new FSM definitions, chosen over putting into functions directly
    private void updateFSM_rammer()
    {
        bool i_seent_him = this.player_spotted();
        if (i_seent_him && previous_state == null)
            this.movement_policy = this.ai_mode_follow;
        else if (i_seent_him)
            this.movement_policy = this.ai_mode_follow;
        else
            this.movement_policy = this.ai_mode_mirror;
    }

    //make your own function here steve, that moves between the states as you want, after making your own method down below that sets future_location depending on how you want the tank to move
    private void updateFSM_chaser() { }

    private void updateFSM_patroller() { }

    //MAKES MOVE BASED ON NEXT LOCATION AI_THINK HAS DECIDED IT SHOULD BE AT.
    private void drive()
    {
        //drive towards next location as dictated by future_location member
        //point the tank at the location
        //start driving when within some safe degrees
        float angle = this.ai_difficulty * Time.deltaTime;
        this.transform.Rotate(Vector3.up, angle);
        Debug.Log("angle:");
        Debug.Log(angle);
    }

    private void operate_turret()
    {

    }
    
    //returns bool depending on if line of sight has been created with player tank.
    private bool player_spotted()
    {
        return true;
    }

    //SOME METHODS HOLDING DIFFERENT MODES AI USES TO FIND THE NEXT LOCATION IT SHOULD AIM TO BE AT - also holds FSM state changing
    private void ai_mode_follow()
    {
        //chases in a straight line, the player tank
    }
    private void ai_mode_gps()
    {
        //uses position of player tank which is not in direct line of sight to find a path with eyes of ra
    }
    private void ai_mode_mirror()
    {
        //uses moves last made by player tank to guide itself until in line of sight again
    }

    private void ai_mode_patrol()
    {
        //uses input game objects to patrol a location
    }

}