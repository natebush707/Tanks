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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnFire(InputValue other)
    {
        Quaternion rotation = new Quaternion(firingPosition.rotation.x,firingPosition.rotation.y,firingPosition.rotation.z,firingPosition.rotation.w);
        Rigidbody bullet = Instantiate(bulletPrefab, firingPosition.position,firingPosition.rotation) as Rigidbody;
        bullet.velocity = launchForce * (firingPosition.forward);
        sfxAudio.Play();
    }
}
