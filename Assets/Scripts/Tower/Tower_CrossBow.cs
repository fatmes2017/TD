using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_CrossBow : Tower
{

    private Crossbow_Visuals visuals;

    [Header("Crossbow details")]
    [SerializeField] private int damage;
    [SerializeField] private Transform gunPoint;

    protected override void Awake()
    {
        base.Awake();

        visuals = GetComponent<Crossbow_Visuals>();
    }

    protected override void Attack()
    {
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity))
        {
            towerHead.forward = directionToEnemy;

            Enemy enemyTarget = null;

            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(damage);
                enemyTarget = currentEnemy;
            }


            visuals.PlayAttackVFX(gunPoint.position, hitInfo.point,enemyTarget);
            visuals.PlayReloaxVFX(attackCooldown);
        }
    }
}
