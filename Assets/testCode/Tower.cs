using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower1 : MonoBehaviour
{
    public EnemyCreator enemyCreator;
    private Transform enemy;

    [SerializeField] private Transform towerHead;



    [Header("Attack details")]

    [SerializeField] private float attackRange = 3;
    [SerializeField] private float attackCooldown = 2;
    private float lastTimeAttacked;



    [Header("Bullet details")]
    [SerializeField] private GameObject bulletPrefabe;
    [SerializeField] private float bulletSpeed = 3;

    //void Awake()
    //{
    //    enemyCreator = FindFirstObjectByType<EnemyCreator>();
    //    .
    //}

    // Update is called once per frame
    void Update()
    {


        if (enemy == null)
        {
            //  FindRandomEnemy();
            enemy = FindClosestEnemy();

            return;
        }
        if (Vector3.Distance(enemy.position, towerHead.position) < attackRange)
        {
            towerHead.LookAt(enemy);
            if (ReadyToAttack())
            {

                CreateBullet();
            }


        }

    }



    private void FindRandomEnemy()
    {
        if (enemyCreator.EnemyList().Count <= 0) return;
        int enemyIndex = Random.Range(0, enemyCreator.EnemyList().Count);
        enemy = enemyCreator.EnemyList()[enemyIndex];
        enemyCreator.EnemyList().RemoveAt(enemyIndex);

    }


    private Transform FindClosestEnemy()
    {

        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (Transform enm in enemyCreator.EnemyList())
        {

            float distance = Vector3.Distance(enm.position, transform.position);

            if (distance < closestDistance && distance <= attackRange)
            {
                closestDistance = distance;
                closestEnemy = enm;
            }


        }


        if (closestEnemy != null)


            enemyCreator.EnemyList().Remove(closestEnemy);




        return closestEnemy;
    }







    private bool ReadyToAttack()
    {
        if (Time.time > lastTimeAttacked + attackCooldown)
        {
            lastTimeAttacked = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CreateBullet()
    {


        GameObject newBullet = Instantiate(bulletPrefabe, towerHead.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody>().velocity = (enemy.position - towerHead.position).normalized * bulletSpeed;



    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
