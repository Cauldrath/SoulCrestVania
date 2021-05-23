﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DamageTypes
{
    Collision = 1,
    Fire = 2,
    LightMelee = 4,
    Melee = 8,
    Projectile = 16,
    ProjectileDestroyer = 32,
    Explosive = 64,
    Ice = 128,
    Electric = 256
}

public enum Factions
{
    Player = 1,
    Enemy = 2,
    Hazard = 4
}

public class Damageable : MonoBehaviour
{
    [System.Serializable]
    public class DamageEvent : UnityEvent<Damager, Damageable>
    { }

    public int MaxHealth = 1;
    // Negative Health means set to max on spawn
    public int Health = -1;
    [EnumFlag]
    public DamageTypes Vulnerabilities = (DamageTypes)~0;
    public float invincibilityTime = 1.0f;
    [EnumFlag]
    public DamageTypes InvincibleVulnerabilities = 0;
    [EnumFlag]
    public DamageTypes InvincibilityTriggers = (DamageTypes)~0;
    [EnumFlag]
    public Factions DamagedByFaction = Factions.Enemy | Factions.Hazard;
    public Collider vulnerableCollider;
    public DamageEvent OnTakeDamage;

    private float invincibilityLeft = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (Health < 1)
        {
            Health = MaxHealth;
        }
        if (vulnerableCollider == null)
        {
            vulnerableCollider = GetComponent<Collider>();
        }
    }

    void FixedUpdate()
    {
        if (invincibilityLeft > 0)
        {
            invincibilityLeft -= Time.deltaTime;
        }
    }

    public bool IsInvincible()
    {
        return invincibilityLeft > 0;
    }

    public void Hit(Damager damager)
    {
        DamageTypes currentVulnerabilities = Vulnerabilities;
        if (invincibilityLeft > 0)
        {
            currentVulnerabilities = InvincibleVulnerabilities;
        }
        if ((currentVulnerabilities & damager.DamageType) != 0)
        {
            Health -= damager.damage;
            if ((InvincibilityTriggers & damager.DamageType) != 0)
            {
                invincibilityLeft = invincibilityTime;
            }
            OnTakeDamage.Invoke(damager, this);
        }
    }
}
