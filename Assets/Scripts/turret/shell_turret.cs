using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shell_turret : MonoBehaviour
{
    private Transform player;
    private float playerDistance, nextShot;
    private bool isDead = false;

    public GameObject turretGun, turretShell;
    public Transform firingPosition;
    public AudioSource explosionSound;
    public ParticleSystem deathExplosion;
    public float turretRange = 10f;
    public float turretForce = 100f;
    public float fireRate = 0.5f;

    private void Start()
    {
        // get player's transform on scene start
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // fetch Audio and Particle components
        explosionSound = GetComponent<AudioSource>();
        deathExplosion = GetComponent<ParticleSystem>();
    }
    private void Update()
    {
        // calculate current distance to player
        playerDistance = Vector3.Distance(player.position, transform.position);
        
        // if player is in range, aim and shoot
        if (playerDistance <= turretRange && !isDead)
        {
            // rotate turret and firing position toward player
            turretGun.transform.LookAt(player);
            turretGun.transform.Rotate(0, -90, 0);
            firingPosition.LookAt(player);

            // check set interval for fire rate before shooting
            if(Time.time >= nextShot)
            {
                nextShot = Time.time + 1f / fireRate;
                turretShootie();
            }
        }
    }

    private void turretShootie()
    {
        // spawn shell and make it last longer
        GameObject shellClone = Instantiate(turretShell, firingPosition.position, firingPosition.rotation);
        shellClone.GetComponent<bullet>().maxLife = 5f;
   
        // add forward force
        shellClone.GetComponent<Rigidbody>().AddForce(firingPosition.forward * turretForce);
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
