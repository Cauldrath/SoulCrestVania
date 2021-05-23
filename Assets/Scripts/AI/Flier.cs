using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flier : EnemyScript
{
    public float speed = 10.0f;

    void FixedUpdate()
    {
        Vector3 direction = result.destination - transform.position;
        Vector3 goalVelocity = direction.normalized * speed;
        if (goalVelocity.magnitude * Time.deltaTime > direction.magnitude)
        {
            transform.position += direction * Time.deltaTime;
        } else
        {
            transform.position += goalVelocity * Time.deltaTime;
        }
    }
}
