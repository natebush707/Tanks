using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public LayerMask players;
    public LayerMask enemies;
    public float radius = 2f;
    // Start is called before the first frame update\
    public AudioSource shellExplosion;
    public ParticleSystem explosionParticle;
    public float maxLife = 2f;
    public float damage = 25f;
    private bool hittingShit;
    private ParticleSystem particleEffect;

    void Start()
    {
        Destroy(gameObject, maxLife);
    }

    private void OnTriggerExit(Collider other)
    {
        if(hittingShit) return;
        hittingShit=true;

        
        Collider[] enemyColiders = Physics.OverlapSphere(transform.position, radius, enemies);
        Collider[] playerColiders = Physics.OverlapSphere(transform.position, radius, players);

        for (int i = 0; i < enemyColiders.Length; i++)
        {
            var enemyScript = enemyColiders[i].gameObject.GetComponent<shell_turret>();
            if (enemyScript != null && this.gameObject.tag == "shell")
            {
                enemyScript.setDead();
            }
        }

        if (playerColiders.Length > 0)
        {
            var playerScript = playerColiders[0]?.gameObject.GetComponentInParent<tankHealth>();
            if (playerScript && this.gameObject.tag == "enemyShell")
            {
                Debug.Log(playerScript.name);
                playerScript.takeDamage(damage);
            }
        }

        // add damage to tank in here
        shellExplosion.transform.parent = null;
        explosionParticle.Play();
        shellExplosion.Play();
        Destroy(explosionParticle.gameObject, explosionParticle.main.duration);
        Destroy(gameObject);
    }

}
