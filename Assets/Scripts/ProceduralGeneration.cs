using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] public GameObject dirtTile, grassTile, stoneTile;
    [SerializeField] public int width,height, stoneMin, stoneMax;

    public void Start() {
        Generation();
    }


    private void Generation() {
        //int rand = Random.;
        //int tempX = 0;
        for (int x = 0; x < width; x++) {
            
            int minHeight = height - 1;
            int maxHeight = height + 2;
            height = Random.Range(minHeight, maxHeight);

            int minStoneSpawnDistance = height - stoneMin;
            int maxStoneSpawnDistance = height - stoneMax;
            int totalStoneSpawnDistance = Random.Range(minStoneSpawnDistance, maxStoneSpawnDistance);

            for (int y = 0; y < height; y++) {
                if (y < totalStoneSpawnDistance) {
                    spawnObject(stoneTile, x, y);
                } else {
                    spawnObject(dirtTile, x, y);
                }
                
            }
            if (totalStoneSpawnDistance == height) {
                spawnObject(stoneTile, x, height);
            }
            else {
                spawnObject(grassTile, x, height);
            }
        }
        
    }

    private void spawnObject (GameObject obj, int width, int height) {
        obj = Instantiate(obj, new Vector2(width, height), Quaternion.identity);
        obj.transform.parent = this.transform;
    }
}
