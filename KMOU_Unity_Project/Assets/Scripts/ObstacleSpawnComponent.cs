using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawnComponent : MonoBehaviour
{
    public GameObject prefabObj;
    public float spawnRate = 0;
    public float minRate = 3;
    public float maxRate = 6;

    public float spawnYPos = 0;
    public float spawnMinXPos = 0;
    public float spawnMaxXPos = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnCoroutune());
    }

    void Spawn()
    {
        float x = Random.Range(spawnMinXPos, spawnMaxXPos);
        Instantiate(prefabObj, new Vector3(x, spawnYPos, 0), Quaternion.identity);
    }

    IEnumerator SpawnCoroutune()
    {
        while(true)
        {
            if (GameManager.i.IsGameOver) yield break;

            spawnRate = Random.Range(minRate, maxRate);
            yield return new WaitForSeconds(spawnRate);
            Spawn();
        }
    }

}
