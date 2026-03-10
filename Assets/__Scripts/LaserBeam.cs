using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    public float lifeTime = 10f;

    private float deathTime;

    void Start()
    {
        deathTime = Time.time + lifeTime;
    }

    void Update()
    {
        if (Time.time >= deathTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        KillEnemy(other);
    }

    private void OnTriggerStay(Collider other)
    {
        KillEnemy(other);
    }

    private void KillEnemy(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        Destroy(enemy.gameObject);
    }
}