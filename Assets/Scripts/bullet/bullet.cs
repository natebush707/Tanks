using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called before the first frame update\
    public AudioSource shellExplosion;
    public ParticleSystem explosionParticle;
    public float maxLife = 2f;

    private ParticleSystem particleEffect;

    void Start()
    {
        Destroy(gameObject, maxLife);
    }

    private void OnTriggerEnter(Collider other)
    {
        // add damage to tank in here
        shellExplosion.transform.parent = null;
        explosionParticle.Play();
        shellExplosion.Play();
        Destroy(explosionParticle.gameObject, explosionParticle.main.duration);
        Destroy(gameObject);
    }
}
