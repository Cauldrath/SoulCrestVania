using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public int priority = 0;
    public Collider attached_collider;

    private void Start()
    {
        attached_collider = GetComponent<Collider>();
    }
}
