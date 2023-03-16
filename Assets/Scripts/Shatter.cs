using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatter : MonoBehaviour
{
    public GameObject BrokenWall;
    void OnTriggerEnter(Collider other)
    {
        // if (other.tag == "shell")
        // {
        //     Instantiate(BrokenWall, transform.position, transform.rotation);
        //     Destroy(gameObject);
        // }
        Instantiate(BrokenWall, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
