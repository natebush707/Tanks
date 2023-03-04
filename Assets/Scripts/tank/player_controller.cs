using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class player_controller : MonoBehaviour
{
    public AudioSource tankDriving;
    public AudioClip drivingSfx;
    public AudioClip engineIdle;
    public GameObject turret;
    [Range(1, 5)]
    public float movementSpeed;
    [Range(20, 100)]
    public float rotationSpeed;
    public GameObject bulletRef;
    public float pitchRange = 0.2f;

    private Vector2 tankMovement;
    private Vector2 mouseMovement;
    private Transform tankTransform;
    private float offset;
    // Start is called before the first frame update
    void Start()
    {
        tankTransform = gameObject.GetComponent<Transform>();
        offset = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //moves the turret to mouse position
        if (Mathf.Abs(tankMovement.x) < 0.1f && Mathf.Abs(tankMovement.y) < 0.1f)
        {
            if (tankDriving.clip == drivingSfx)
            {
                tankDriving.clip = engineIdle;
                tankDriving.pitch = Random.Range(1 - pitchRange, 1 + pitchRange);
                tankDriving.Play();
            }
        }
        else
        {
            if (tankDriving.clip == engineIdle)
            {
                tankDriving.clip = drivingSfx;
                tankDriving.pitch = Random.Range(1 - pitchRange, 1 + pitchRange);
                tankDriving.Play();
            }
        }

    }
    void OnMove(InputValue other)
    {
        tankMovement = other.Get<Vector2>();
        // Debug.Log(tankMovement);
    }

    void OnLook(InputValue other)
    {
        mouseMovement = other.Get<Vector2>();
    }
    void FixedUpdate()
    {
        // moves the tank around
        float Speed = tankMovement.y * movementSpeed * Time.deltaTime;
        float angle = tankMovement.x * rotationSpeed * Time.deltaTime;
        float cameraDistance = Vector3.Magnitude(Camera.main.transform.position - this.transform.position);

        offset -= tankMovement.x * rotationSpeed * Time.deltaTime;

        tankTransform.Rotate(Vector3.forward, angle);
        tankTransform.Translate(Vector3.down * Speed);
        Vector3 direction = Camera.main.ScreenToWorldPoint(new Vector3(mouseMovement.x, mouseMovement.y, cameraDistance)) - turret.transform.position;
        float rotationAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        // turret.transform.localRotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
        turret.transform.localRotation = Quaternion.AngleAxis(rotationAngle + offset, Vector3.forward);
        // Debug.Log(mouseMovement);
    }
}
