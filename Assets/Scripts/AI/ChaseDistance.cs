using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseDistance : AIBehavior
{
    public Vector2 goalDistance = Vector2.zero;
    public bool chaseX = true;
    public bool chaseY = true;

    public override void AIUpdate(GameObject ControlledObject, AIResult result, GameObject Target, Camera ViewCamera, float deltaTime)
    {
        if (chaseX)
        {
            Vector3 horizontalVector = result.destination - Target.transform.position;
            horizontalVector.y = 0;
            Vector3 horizontalDest = Target.transform.position + horizontalVector.normalized * goalDistance.x;
            result.destination.x = horizontalDest.x;
            result.destination.z = horizontalDest.z;
        }
        if (chaseY)
        {
            Vector3 verticalVector = result.destination - Target.transform.position;
            verticalVector.x = 0;
            verticalVector.z = 0;
            result.destination.y = (Target.transform.position + verticalVector.normalized * goalDistance.y).y;
        }
    }
}
