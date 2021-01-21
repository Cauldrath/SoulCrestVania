using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform player;
    public Quaternion rotation = new Quaternion();
    public float max_rotation_speed = 360.0f; // Measured in degrees per second
    public float min_distance = 6f;
    public float max_distance = 7f;
    public float max_horizontal = 2.5f;
    public float max_vertical = 2.5f;

    private Camera attached_camera;
    private CameraFocus[] focusables;

    void Start()
    {
        attached_camera = GetComponent<Camera>();
        UpdateFocusables();
    }

    public void UpdateFocusables()
    {
        focusables = GameObject.FindObjectsOfType<CameraFocus>();
    }

    void LateUpdate()
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(attached_camera);
        Vector3 cameraDirection = attached_camera.transform.rotation * Vector3.forward;
        bool otherFound = false;
        int otherPriority = 0;
        Vector3 otherPosition = Vector3.zero;
        foreach (CameraFocus focus in focusables)
        {
            //if (focus != null && GeometryUtility.TestPlanesAABB(cameraPlanes, focus.attached_renderer.bounds) && (!otherFound || focus.priority > otherPriority))
            if (focus != null && (!otherFound || focus.priority > otherPriority)) {
                Vector3 closestPoint = focus.attached_collider.ClosestPoint(player.transform.position);
                if ((closestPoint - player.transform.position).magnitude < max_horizontal * 2)
                {
                    otherFound = true;
                    otherPriority = focus.priority;
                    otherPosition = closestPoint;
                }
            }
        }
        Quaternion goalRotation = rotation;
        if (otherFound)
        {
            Vector3 focusVector = Vector3.Cross(Vector3.up, (otherPosition - player.transform.position).normalized);
            focusVector.Normalize();
            if (focusVector == Vector3.zero)
            {
                goalRotation = attached_camera.transform.rotation;
            } else {
                if (Vector3.Dot(cameraDirection, focusVector) < 0)
                {
                    focusVector = focusVector * -1;
                }
                goalRotation = Quaternion.LookRotation(focusVector, Vector3.up);
            }
        }
        Vector3 goalAngles = goalRotation.eulerAngles;
        Vector3 currentAngles = transform.rotation.eulerAngles;
        float angleDiff = goalAngles.y - currentAngles.y;
        if (angleDiff > 180) angleDiff -= 360;
        if (angleDiff < -180) angleDiff += 360;
        float maxAngle = Time.deltaTime * max_rotation_speed;
        if (angleDiff > maxAngle)
        {
            goalAngles.y = currentAngles.y + maxAngle;
        }
        else if (angleDiff < -maxAngle)
        {
            goalAngles.y = currentAngles.y - maxAngle;
        }
        Quaternion finalRotation = Quaternion.Euler(goalAngles);
        transform.rotation = finalRotation;
        Vector3 lookVector = transform.rotation * Vector3.forward;
        Vector3 rightVector = transform.rotation * Vector3.right;
        Vector3 upVector = transform.rotation * Vector3.up;
        float lookDistance = Vector3.Dot(lookVector, player.transform.position - transform.position);
        float rightDistance = Vector3.Dot(rightVector, player.transform.position - transform.position);
        float upDistance = Vector3.Dot(upVector, player.transform.position - transform.position);
        if (lookDistance < min_distance)
        {
            transform.position += lookVector * (lookDistance - min_distance);
        }
        if (lookDistance > max_distance)
        {
            transform.position += lookVector * (lookDistance - max_distance);
        }
        if (rightDistance > max_horizontal)
        {
            transform.position += rightVector * (rightDistance - max_horizontal);
        }
        if (rightDistance < -max_horizontal)
        {
            transform.position += rightVector * (rightDistance + max_horizontal);
        }
        if (upDistance > max_vertical)
        {
            transform.position += upVector * (upDistance - max_vertical);
        }
        if (upDistance < -max_vertical)
        {
            transform.position += upVector * (upDistance + max_vertical);
        }
    }
}
