using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damager : MonoBehaviour
{
    [System.Serializable]
    public class DamageEvent : UnityEvent<Damager, Damageable>
    { }

    [EnumFlag]
    public DamageTypes DamageType = DamageTypes.Collision;
    [EnumFlag]
    public Factions FactionDamage = Factions.Player;
    public int damage = 1;
    public bool active = true;
    public Collider damageCollider;
    public DamageEvent OnDamage;
    public bool singleHit = false;

    private List<Damageable> objectsHit = new List<Damageable>();

    // Start is called before the first frame update
    void Start()
    {
        if (damageCollider == null)
        {
            damageCollider = GetComponent<Collider>();
        }
    }

    void FixedUpdate()
    {
        if (active)
        {
            Collider[] results = Physics.OverlapSphere(transform.position, damageCollider.bounds.extents.magnitude);
            int count = results.Length;
            for (int loop = 0; loop < count; ++loop)
            {
                Damageable damageable = results[loop].GetComponent<Damageable>();
                if (damageable && damageable.vulnerableCollider == results[loop])
                {
                    if (!objectsHit.Contains(damageable))
                    {
                        if ((damageable.DamagedByFaction & FactionDamage) != 0)
                        {
                            damageable.Hit(this);
                            OnDamage.Invoke(this, damageable);
                            if (singleHit)
                            {
                                objectsHit.Add(damageable);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ResetHit()
    {
        objectsHit.Clear();
    }
}
