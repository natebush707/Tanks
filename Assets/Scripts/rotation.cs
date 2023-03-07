using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotation : MonoBehaviour
{
    void Update()
    {
        transform.rotation = transform.parent.localRotation;
    }
}
