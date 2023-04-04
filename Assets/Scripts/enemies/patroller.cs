using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class patroller : MonoBehaviour
{
    public GameObject[] waypoints;
    int currentWP = 0;

    private Transform player;
    private float playerDistance, nextShot;
    private bool isDead = false;

    public GameObject shell;
    public Transform shellSpawner;
    public AudioSource explosionSound;
    public ParticleSystem deathExplosion;

    public float speed = 3.0f;
    public float rotSpeed = 3.0f;
    public float lookAhead = 2.0f;
    public float shotRange = 10.0f;
    public float shotForce = 100.0f;
    public float fireRate = 0.5f;

    GameObject tracker;

    void Start()
    {
        // get player's position
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // set up ghost tracker game object
        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;
    }

    void GhostTracker()
    {
        // stop and wait if player is too far behind
        if (Vector3.Distance(tracker.transform.position, this.transform.position) > lookAhead)
            return;

        // waypoint reached, set next waypoint
        if (Vector3.Distance(tracker.transform.position, waypoints[currentWP].transform.position) < 1)
            currentWP++;

        // waypoints exhausted, start over
        if (currentWP >= waypoints.Length)
            currentWP = 0;

        // snap to next waypoint and move directly to it
        tracker.transform.LookAt(waypoints[currentWP].transform);
        tracker.transform.Translate(0, 0, (speed + 2) * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        // do nothing if patroller or player is dead
        if (!player || isDead) return;
        
        GhostTracker();

        playerDistance = Vector3.Distance(player.position, this.transform.position);

        if (playerDistance <= shotRange)
        {
            // player is in range, stop moving and rotate toward player
            Quaternion lookAtPlayer = Quaternion.LookRotation(player.position - this.transform.position);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookAtPlayer, rotSpeed * Time.deltaTime);

            // shoot at player
            if (Time.time >= nextShot)
            {
                nextShot = Time.time + 1f / fireRate;
                patrollerShootie();
            }
        }
        else
        {
            // player not in range, rotate and move toward ghost tracker
            Quaternion lookAtTracker = Quaternion.LookRotation(tracker.transform.position - this.transform.position);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookAtTracker, rotSpeed * Time.deltaTime);
            this.transform.Translate(0, 0, speed * Time.deltaTime);
        }
    }

    private void patrollerShootie()
    {
        // spawn and tag shell
        GameObject shellClone = Instantiate(shell, shellSpawner.position, shellSpawner.rotation);
        shellClone.GetComponent<bullet>().maxLife = 5f;
        shellClone.tag = "enemyShell";

        // add forward force to shell
        shellClone.GetComponent<Rigidbody>().AddForce(shellSpawner.forward * shotForce);
    }

    private void OnTriggerEnter(Collider other)
    {
        // play explosion particle system and audio clip on received damage from player
        if (other.tag == "shell" && !isDead)
        {
            isDead = true;
            deathExplosion.Play();
            explosionSound.Play();
        }
    }
}
