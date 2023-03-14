using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class tankshootie : MonoBehaviour
{
    public float maxCharge = 25f;
    public float minCharge = 5f;
    public float chargeSpeed = 5f;
    public Rigidbody bulletPrefab;
    public Transform firingPosition;
    public AudioSource sfxAudio;
    [SerializeField]
    private bool mouseDown = false;
    private bool fired = false;

    [SerializeField]
    private float chargeShot = 0;

    private void Start()
    {
        chargeShot = minCharge;
    }

    private void Update()
    {
        if (mouseDown && !fired)
        {
            chargeShot += chargeSpeed * Time.deltaTime;
            chargeShot = Mathf.Min(chargeShot, maxCharge);
            if (chargeShot >= maxCharge)
            {
                fire();
            }
        }
    }

    void OnFire(InputValue other)
    {
        mouseDown = other.isPressed;

        if(!mouseDown && !fired){
            fire();
        }
        fired = false;
    }
    void fire()
    {
        if (GameObject.FindGameObjectsWithTag("shell").Length < 3)
        {
            Rigidbody bullet = Instantiate(bulletPrefab, firingPosition.position, firingPosition.rotation) as Rigidbody;
            bullet.velocity = chargeShot * (firingPosition.forward);
            bullet.tag = "shell";
            sfxAudio.Play();
            chargeShot = minCharge;
            fired = true;
        }
    }
}
