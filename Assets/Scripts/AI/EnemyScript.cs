using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public GameObject Target;
    public Camera ViewCamera;
    public List<AIBehavior> behaviors = new List<AIBehavior>();

    protected AIResult result = new AIResult();

    public void Update()
    {
        result.destination = transform.position;
        foreach (AIBehavior behavior in behaviors)
        {
            behavior.AIUpdate(gameObject, result, Target, ViewCamera, Time.deltaTime);
        }
    }
}
