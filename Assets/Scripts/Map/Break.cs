using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : MonoBehaviour
{
    public GameObject objectShatter;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "shell")
        {
            GameObject test = Instantiate(objectShatter, transform.position, transform.rotation);
            Destroy(gameObject);
            Destroy(test, 1.75f);
        }
    }


}
