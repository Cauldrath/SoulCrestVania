using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToCamera : AIBehavior
{
    public float tolerance = 0.5f;

    public override void AIUpdate(GameObject ControlledObject, AIResult result, GameObject Target, Camera ViewCamera, float deltaTime)
    {
        Vector3 cameraForward = ViewCamera.transform.forward;
        float targetDistance = Vector3.Dot(Target.transform.position - ViewCamera.transform.position, cameraForward);
        float destDistance = Vector3.Dot(result.destination - ViewCamera.transform.position, cameraForward);
        if (targetDistance - tolerance > destDistance)
        {
            targetDistance -= tolerance;
        } else if (targetDistance + tolerance < destDistance)
        {
            targetDistance += tolerance;
        }
        result.destination += cameraForward * (targetDistance - destDistance);
    }
}
