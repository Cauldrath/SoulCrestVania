using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cameraTransform;
    public bool flippable;

    void LateUpdate()
    {

        if (flippable && Vector3.Dot(transform.right, this.cameraTransform.right) < 0)
        {
            transform.rotation = this.cameraTransform.rotation;
            transform.right = this.cameraTransform.right * -1;
        }
        else
        {
            transform.rotation = this.cameraTransform.rotation;
        }
    }
}
