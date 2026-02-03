using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    [SerializeField] private int amountToSpawn = 4;
    [SerializeField] private GameObject enemyPrefab;


    private List<Transform> enemies = new List<Transform>();

    private void Start()
    {
        CreateNewEnemies();
    }

    private void CreateNewEnemies()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {

            float randomX = Random.Range(-4, 4);
            float randomZ = Random.Range(-4, 4);
            Vector3 newPosition = new Vector3(randomX, -.13f, randomZ);

            GameObject newEnemy = Instantiate(enemyPrefab, newPosition, Quaternion.identity);

            enemies.Add(newEnemy.transform);
        }
    }

    public List<Transform> EnemyList() => enemies;
    //     public List<Transform> EnemyList() {
    //         return enemies;
    //     }
}
