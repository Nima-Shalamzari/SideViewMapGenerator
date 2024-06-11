using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralGeneration : MonoBehaviour
{
    //[SerializeField] public GameObject dirtTile, grassTile, stoneTile;
    [SerializeField] public int width,height, stoneMin, stoneMax;
    [SerializeField] public Tilemap dirtTileMap, grassTileMap, StoneTileMap;
    [SerializeField] public TileBase dirt, grass, stone;

    [Range(0, 100)]
    [SerializeField] float heightValue, smoothness;
    [SerializeField] float seed;
    public void Start() {
        seed = Random.Range(-100000, 100000);
        Generation();
    }


    private void Generation() {
        for (int x = 0; x < width; x++) {

            //int minHeight = height - 1;
            //int maxHeight = height + 2;
            //height = Random.Range(minHeight, maxHeight);

            int height = Mathf.RoundToInt(heightValue * Mathf.PerlinNoise(x/ smoothness , seed));

            int minStoneSpawnDistance = height - stoneMin;
            int maxStoneSpawnDistance = height - stoneMax;
            int totalStoneSpawnDistance = Random.Range(minStoneSpawnDistance, maxStoneSpawnDistance);

            for (int y = 0; y < height; y++) {
                if (y < totalStoneSpawnDistance) {
                    //spawnObject(stoneTile, x, y);
                    StoneTileMap.SetTile(new Vector3Int(x, y, 0), stone);
                } else {
                    //spawnObject(dirtTile, x, y);
                    dirtTileMap.SetTile(new Vector3Int(x, y, 0), dirt);
                }

            }
            if (totalStoneSpawnDistance == height) {
                //spawnObject(stoneTile, x, height);
                StoneTileMap.SetTile(new Vector3Int(x, height, 0), stone);
            }
            else {
                //spawnObject(grassTile, x, height);
                grassTileMap.SetTile(new Vector3Int(x, height, 0), grass);
            }
        }
        
    }

    //private void spawnObject (GameObject obj, int width, int height) {
    //    obj = Instantiate(obj, new Vector2(width, height), Quaternion.identity);
    //    obj.transform.parent = this.transform;
    //}
}
