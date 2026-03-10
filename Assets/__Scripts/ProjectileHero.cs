using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]
    private eWeaponType _type;

    [Header("Phaser Settings")]
    public float phaserWaveFrequency = 12f;
    public float phaserWaveMagnitude = 0.5f;

    [Header("Missile Settings")]
    public float missileTurnRate = 240f;

    [Header("Impact Effects")]
    public GameObject missileImpactPrefab;
    public float impactEffectLifetime = 2f;

    private float birthTime;
    private float x0;
    private bool hasImpacted = false;

    // This public property masks the private field _type
    public eWeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        birthTime = Time.time;
        x0 = transform.position.x;
    }

    void Update()
    {
        switch (type)
        {
            case eWeaponType.phaser:
                UpdatePhaser();
                break;

            case eWeaponType.missile:
                UpdateMissile();
                break;
        }

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offLeft) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offRight))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to match the 
    /// WeaponDefinition.
    /// </summary>
    public void SetType(eWeaponType eType)
    {
        _type = eType;

        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        if (def != null && rend != null)
        {
            rend.material.color = def.projectileColor;
        }
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.linearVelocity; }
        set { rigid.linearVelocity = value; }
    }

    private void UpdatePhaser()
    {
        Vector3 pos = transform.position;
        pos.x = x0 + Mathf.Sin((Time.time - birthTime) * phaserWaveFrequency) * phaserWaveMagnitude;
        transform.position = pos;
    }

    private void UpdateMissile()
    {
        GameObject target = FindClosestEnemy();
        if (target == null) return;

        Vector3 dir = (target.transform.position - transform.position).normalized;

        Vector3 currentVel = vel;
        float speed = currentVel.magnitude;

        if (speed <= 0.01f)
        {
            WeaponDefinition def = Main.GET_WEAPON_DEFINITION(type);
            if (def != null) speed = def.velocity;
            else speed = 20f;
        }

        Vector3 desiredVel = dir * speed;

        vel = Vector3.RotateTowards(
            currentVel,
            desiredVel,
            missileTurnRate * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        if (vel != Vector3.zero)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private GameObject FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        if (enemies == null || enemies.Length == 0) return null;

        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.gameObject;
            }
        }

        return closest;
    }

    private void SpawnImpactEffect()
    {
        if (type != eWeaponType.missile) return;
        if (missileImpactPrefab == null) return;

        GameObject fx = Instantiate(missileImpactPrefab, transform.position, Quaternion.identity);
        Destroy(fx, impactEffectLifetime);
    }

    private void HandleMissileImpact(Collider other)
    {
        if (hasImpacted) return;
        if (type != eWeaponType.missile) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        hasImpacted = true;
        Debug.Log("Missile impacted enemy: " + enemy.name);
        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void HandleMissileImpact(Collision collision)
    {
        if (hasImpacted) return;
        if (type != eWeaponType.missile) return;

        Enemy enemy = collision.collider.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        hasImpacted = true;
        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleMissileImpact(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleMissileImpact(collision);
    }
}