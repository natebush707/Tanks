using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called before the first frame update\
    public AudioSource shellExplosion;
    public ParticleSystem explosionParticle;
    public ParticleSystem smokeTrail;
    public float maxLife = 2f;


    void Start()
    {
        Destroy(gameObject, maxLife);
    }

    private void OnTriggerEnter(Collider other)
    {
        // add damage to tank in here
        shellExplosion.transform.parent = null;
        smokeTrail.transform.position = this.transform.position;
        smokeTrail.transform.parent = null;
        smokeTrail.Stop();
        explosionParticle.Play();
        shellExplosion.Play();
        Destroy(smokeTrail.gameObject,smokeTrail.main.duration);
        Destroy(explosionParticle.gameObject, explosionParticle.main.duration);
        Destroy(gameObject);
    }
}
