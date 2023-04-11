using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatter : MonoBehaviour
{
    // public ParticleSystem wallExplosion;
    [SerializeField] ParticleSystem WallExplosion;
    public GameObject BrokenWall;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "shell")
        {
            WallExplosion.Play();
            // Instantiate(BrokenWall, transform.position, transform.rotation);
            Destroy(gameObject, WallExplosion.main.duration);
        }
    }
}
