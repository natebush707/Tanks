using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class tankshootie : MonoBehaviour
{
    public Rigidbody bulletPrefab;
    public Transform firingPosition;
    public AudioSource sfxAudio;
    public float launchForce;

    void OnFire(InputValue other)
    {
        if (GameObject.FindGameObjectsWithTag("shell").Length < 3)
        {
            Rigidbody bullet = Instantiate(bulletPrefab, firingPosition.position, firingPosition.rotation) as Rigidbody;
            bullet.velocity = launchForce * (firingPosition.forward);
            bullet.tag = "shell";
            sfxAudio.Play();
        }
    }
}
