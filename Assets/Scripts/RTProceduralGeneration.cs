using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RTProceduralGeneration : MonoBehaviour
{
    [Header("Terrain Gen")]
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float smoothness;

    [Header("Cave Gen")]
    [Range(0,1)]
    [SerializeField] float modifier;

    [Header("Tile")]
    [SerializeField] TileBase groundTile;
    [SerializeField] TileBase caveTile;
    [SerializeField] Tilemap groundTileMap;
    [SerializeField] Tilemap caveTileMap;

    [SerializeField] float seed;

    int[,] map;

    private void Start() {
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, groundTileMap, caveTileMap, groundTile, caveTile);
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Generation();
        }
    }

    private void Generation() {
        seed = Time.time;
        clearMap();
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, groundTileMap, caveTileMap, groundTile, caveTile);
    }

    public int[,] GenerateArray(int width, int height, bool empty) {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                map[x, y] = (empty) ? 0 : 1;
            }
        }
        return map;
    }

    public int[,] TerrainGeneration(int[,] map) {
        int perlinHeight;
        for (int x = 0; x < width; x++) {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height/2);
            perlinHeight += height / 2;
            for (int y = 0; y < perlinHeight; y++) {
                //map[x, y] = 1;
                int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x * modifier) + seed, (y * modifier) + seed));
                map[x, y] = (caveValue == 1)? 2 : 1;
            }
        }
        return map;
    }

    public void RenderMap(int[,] map, Tilemap groundTileMap, Tilemap caveTileMap, TileBase groundTilebase, TileBase caveTile) {
        for (int x = 0; x< width; x++) {
            for (int y = 0; y < height; y++) {
                if (map[x,y] == 1) {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTilebase);
                } else if (map[x,y] ==2) {
                    caveTileMap.SetTile(new Vector3Int(x, y, 0), caveTile);
                }
            }
        }
    }

    private void clearMap() {
        groundTileMap.ClearAllTiles();
        caveTileMap.ClearAllTiles();
    }
}
