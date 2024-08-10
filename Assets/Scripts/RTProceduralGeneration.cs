using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RTProceduralGeneration : MonoBehaviour
{
    [Header("Terrain Gen")]
    [SerializeField] int width; // width of our map
    [SerializeField] int height; // height of our map
    [SerializeField] float smoothness; // a floating number that smoothes perlin noise
    int WaterLevel; // water level at surface
    int[] perlinHeightArray; // an array to hold floating surface ground height with the length of width

    [Header("Mineral Gen")]
    float randomStoneFillPercent; // percentage of stone we want our map to have - mostly useless for now

    [Header("Cave Gen")]
    [Range(0,1)]
    [SerializeField] float modifier; // a modifier number used in perlin noise
    [Range(0,100)]
    [SerializeField] int randomFillPercent; // percentage of ground tile compare to cave
    [SerializeField] int smoothAmount; //number of times we soften pesudo random generated ground and caves

    //[Header("Water Gen")]
    LinkedListStack<Coordinates> mapList = new LinkedListStack<Coordinates>(); // a linkedlist-stack that we use for traversing caves and helps water generation in caves
    private int minWater, maxWater, randHeight; // used for cave water generation

    //[Header("Surface environment Gen")]

    [Header("Tile")]
    [SerializeField] TileBase groundTile;
    [SerializeField] TileBase caveTile;
    [SerializeField] TileBase waterTile;
    [SerializeField] TileBase stoneTile;
    [SerializeField] TileBase topWaterTile;
    [SerializeField] TileBase TreeTile;
    [SerializeField] Tilemap groundTileMap;
    [SerializeField] Tilemap caveTileMap;
    [SerializeField] Tilemap waterTileMap;
    [SerializeField] Tilemap environmentTileMap;

    [Header("Tile")]
    [SerializeField] float seed; // seed of our generated maps
    [SerializeField] float scale; // used for scaling the Perlin noise
    [SerializeField] int octaves; // used for number of octaves of Perlin noise
    [SerializeField] float persistance; // used for persistance of the Perlin noise
    [SerializeField] float lacunarity; // used for lacunarity of the Perlin noise
    float maxNoiseHeight, minNoiseHeight; // used to determine the minimum and maximum height of perlin noise values so later we can later lock them between zero and one

    protected int[,] map; // a two dimensional array that holds values and numbers of each element on the map
    private int tempGroundCheck; // a temporary number that checks if number of grounds around a block is more than stones and is used in smoothing function

    private void Start() {
        perlinHeightArray = new int[width];
        Generation();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Generation();
        }
    }
    private void Generation() {
        clearMap();
        map = GenerateArray(width, height, true); // declration of map array with zero or air
        map = TerrainGeneration(map); // generating ground and cave
        SmoothMap(smoothAmount); // smoothing caves
        map = stoneGeneration(map); // generating stone layers and chunks
        map = caveAnalysis(map); // analysing caves and generating water inside them
        map = surfaceWaterLevel(map); // generating surface water
        map = treeGeneration(map); // generating trees
        RenderMap(map, groundTileMap, caveTileMap,waterTileMap, environmentTileMap, groundTile, caveTile, waterTile, stoneTile, TreeTile); // rendering all the tiles and tile maps
    }

    public int[,] GenerateArray(int width, int height, bool empty) {
        // *** function description : ***
        // this function generates a two dimensional array with zeros only.
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                map[x, y] = (empty) ? 0 : 1;
            }
        }return map;
    }
    public int[,] TerrainGeneration(int[,] map) {
        // *** function description : ***
        // this function uses pesudo random to generate ground and cave based on value of randomFillPercent.
        System.Random pesudoRandom = new System.Random(seed.GetHashCode());
        //System.Random pesudoStoneRandom = new System.Random((int) DateTime.Now.Ticks);
        int perlinHeight;
        //int maxHeight = 0;
        //int minHeight = height;
        for (int x = 0; x < width; x++) {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height/2);
            perlinHeight += height / 2;
            perlinHeightArray[x] = perlinHeight;
            for (int y = 0; y < perlinHeight; y++) {
                //map[x, y] = 1; we use this line for ground generation only
                //int caveValue = Mathf.RoundToInt(Mathf.PerlinNoise((x * modifier) + seed, (y * modifier) + seed)); we use these two lines for using perlin noise to generate caves.
                //map[x, y] = (caveValue == 1)? 2 : 1;
                //float stoneValue = Mathf.PerlinNoise((pesudoStoneRandom.Next(1, 100) * modifier) + seed, (y * modifier) + seed);
                map[x, y] = (pesudoRandom.Next(1, 100) < randomFillPercent) ? 1 : 2;
                /*if (map[x,y] == 1) {
                    map[x, y] = ((stoneValue * 100) < randomStoneFillPercent) ? 6 : 1;
                }*/ //activate this part to return back to default stone production.
                //assist.GetSurroundingGround(x, y, minHeight, maxHeight);
            }
        }
        return map;
    }
    void SmoothMap( int smoothAmount) {
        // *** function description : ***
        // this function works based on surroundings of a block and decided whether there should be cave, ground or air.
        for (int i = 0; i < smoothAmount; i++) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < perlinHeightArray[x]; y++) {
                    if (y == 0) {
                        map[x, y] = 6;
                    }
                    else if (x == 0 || y == 0 || x == width - 1 || y == perlinHeightArray[x] - 1) {
                        map[x, y] = 1;
                    }
                    else {
                        int surroundingGroundCount = GetSurroundingGroundCount(x, y);
                        if (surroundingGroundCount > 4) {
                            map[x, y] = (tempGroundCheck == 1) ? 1 : 6;
                            tempGroundCheck = 0;
                        }
                        else if (surroundingGroundCount < 4) {
                            map[x, y] = 2;
                            tempGroundCheck = 0;
                        }
                    }
                }
            }
        }
    }
    int GetSurroundingGroundCount(int gridx, int gridy) {
        // *** function description : ***
        // this function goes through surrounding blocks of a specific coordinate and collects information about air, ground and cave blocks.
        int groundCount = 0;
        int Ground = 0, Stone = 0;
        for (int nebx = gridx-1; nebx <= gridx+1; nebx++) {
            for (int neby = gridy-1; neby <= gridy+1; neby++) {
                if (nebx >= 0 && nebx < width && neby >= 0 && neby < height) { //shouldn't be out of neightbourhood zone
                    if (nebx != gridx || neby != gridy) { //shouldn't be the middle tile
                        if (map[nebx,neby] == 1) {
                            groundCount++;
                            Ground++;
                        }else if (map[nebx, neby] == 6) {
                            groundCount++;
                            Stone++;
                        }
                    }
                }
            }
        }
        tempGroundCheck = (Ground >= Stone) ? 1 : 6;
        return groundCount;
    }
    private int[,] stoneGeneration(int[,] map) {
        // *** function description : ***
        // this function uses the latest update of perline noise generation and generates three layers of stone horizontally and makes sure that we will always jave stone at the most button one and coming up, moderate to low amount stone have.
        float[,] noiseMap = new float[width, height];
        if (scale <= 0) scale = 0.0001f;
        maxNoiseHeight = float.MinValue;
        minNoiseHeight = float.MaxValue;
        float fillPercent = 60;
        int modifiedHeight, modifiedHeightStartPoint;
        float Scale;
        for (int j = 0; j < 3; j++) {
            for (int x = 0; x < width; x++) {
                if (j == 0) {
                    modifiedHeightStartPoint = 0;
                    modifiedHeight = perlinHeightArray[x] / 3;
                    Scale = 20;
                }
                else if (j == 1) {
                    modifiedHeightStartPoint = perlinHeightArray[x] / 3;
                    modifiedHeight = perlinHeightArray[x] * 2/3;
                    Scale = 15;
                }
                else if (j == 2) {
                    modifiedHeightStartPoint = perlinHeightArray[x] * 2/3;
                    modifiedHeight = perlinHeightArray[x];
                    Scale = 10;
                }
                else {
                    return null;
                }
                for (int y = modifiedHeightStartPoint; y < modifiedHeight; y++) {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++) {
                        float sampleX = x / Scale * frequency;
                        float sampleY = y / Scale * frequency;
                        float perlinValue = Mathf.PerlinNoise(sampleX + seed, sampleY + seed) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                    noiseMap[x, y] = noiseHeight;
                }
            }
        }
        for (int j = 0; j < 3; j++) {
            if (j == 0) fillPercent = 0;
            else if (j == 1) fillPercent = 0.4f;
            else if (j == 2) fillPercent = 0.8f;
            for (int x = 0; x < width; x++) {
            if (j == 0) {
                modifiedHeightStartPoint = 0;
                modifiedHeight = perlinHeightArray[x] / 3;
            }
            else if (j == 1) {
                modifiedHeightStartPoint = perlinHeightArray[x] / 3;
                modifiedHeight = (perlinHeightArray[x] / 3) * 2;
            }
            else if (j == 2) {
                modifiedHeightStartPoint = (perlinHeightArray[x] / 3) * 2;
                modifiedHeight = perlinHeightArray[x];
            }
            else {
                    return null;
            }
            for (int y = modifiedHeightStartPoint; y < modifiedHeight; y++) {
                    noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                    if (noiseMap[x, y] > fillPercent && map[x, y] == 1) {
                        map[x, y] = 6;
                    }
                }
            }
        }
        return map;
    }
    int[,] caveAnalysis(int[,] map) {
        // *** function description : ***
        // this function traverses through map and finds caves and provide necessary variables for cave analysis assist function.
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (map[x, y] == 2) {
                    minWater = height - 1;
                    maxWater = 0;
                    caveAnalysisAssistant(map, x, y);
                    randHeight = 0;
                }
            }
        }
        return map;
    }
    void caveAnalysisAssistant(int[,] map, int x, int y) {
        // *** function description : ***
        // this function uses flood fill algorithm to travers each cave and mark it as a traversed cave and then goes through the same cave and based on a random hight, fill the cave with water.
        if (y < minWater) minWater = y;
        if (y > maxWater) maxWater = y;
        if (map[x, y] == 2) {
            map[x, y] = 3;
            mapList.Push(new Coordinates(x, y));
            Coordinates coord = mapList.Peek();
            caveAnalysisAssistant(map, coord.X, coord.Y);
        }else if (y > 0 && map[x,(y -1)] == 2) {
            mapList.Push(new Coordinates(x,y -1));
            map[x, y - 1] = 3;
            Coordinates coord = mapList.Peek();
            caveAnalysisAssistant(map, coord.X, coord.Y);
        }else if (y < map.GetLength(1) - 1 && map[x, (y + 1)] == 2) {
            mapList.Push(new Coordinates(x, y + 1));
            map[x, y + 1] = 3;
            Coordinates coord = mapList.Peek();
            caveAnalysisAssistant(map, coord.X, coord.Y);
        }else if (x > 0 && map[(x -1), y] == 2) {
            mapList.Push(new Coordinates((x -1), y));
            map[(x -1), y] = 3;
            Coordinates coord = mapList.Peek();
            caveAnalysisAssistant(map, coord.X, coord.Y);
        }else if (x < map.GetLength(0) - 1 && map[(x +1), y] == 2) {
            mapList.Push(new Coordinates((x +1), y));
            map[(x +1), y] = 3;
            Coordinates coord = mapList.Peek();
            caveAnalysisAssistant(map, coord.X, coord.Y);
        } else {
            if (mapList.Count() > 0) {
                mapList.Pop();
                if (mapList.Count() > 0) {
                    Coordinates coord = mapList.Peek();
                    caveAnalysisAssistant(map, coord.X, coord.Y);
                }
            }
        }
        if(randHeight > 0) {
            if (map[x, y] == 3) {
                map[x, y] = (y <= randHeight) ?  4 : 5;
                mapList.Push(new Coordinates(x, y));
                Coordinates coord = mapList.Peek();
                caveAnalysisAssistant(map, coord.X, coord.Y);
            }
            else if (y > 0 && map[x, (y - 1)] == 3) {
                map[x, y -1] = (y -1 <= randHeight) ? 4 : 5;
                mapList.Push(new Coordinates(x, y - 1));
                Coordinates coord = mapList.Peek();
                caveAnalysisAssistant(map, coord.X, coord.Y);
            }
            else if (y < map.GetLength(1) - 1 && map[x, (y + 1)] == 3) {
                map[x, y +1] = (y +1 <= randHeight) ? 4 : 5;
                mapList.Push(new Coordinates(x, y + 1));
                Coordinates coord = mapList.Peek();
                caveAnalysisAssistant(map, coord.X, coord.Y);
            }
            else if (x > 0 && map[(x - 1), y] == 3) {
                map[x -1, y] = (y <= randHeight) ? 4 : 5;
                mapList.Push(new Coordinates((x - 1), y));
                Coordinates coord = mapList.Peek();
                caveAnalysisAssistant(map, coord.X, coord.Y);
            }
            else if (x < map.GetLength(0) - 1 && map[(x + 1), y] == 3) {
                map[x +1, y] = (y <= randHeight) ? 4 : 5;
                mapList.Push(new Coordinates((x + 1), y));
                Coordinates coord = mapList.Peek();
                caveAnalysisAssistant(map, coord.X, coord.Y);
            }
            else {
                if (mapList.Count() > 0) {
                    mapList.Pop();
                    if (mapList.Count() > 0) {
                        Coordinates coord = mapList.Peek();
                        caveAnalysisAssistant(map, coord.X, coord.Y);
                    }
                }
            }
        }else if (randHeight == 0) {
            randHeight = RandomWaterHeight(minWater, maxWater);
            mapList.Push(new Coordinates(x, y));
            Coordinates coordi = mapList.Peek();
            caveAnalysisAssistant(map, coordi.X, coordi.Y);
        }
    }
    int[,] surfaceWaterLevel(int[,] map) {
        // *** function description : ***
        // this function based on a previously declared surface water level addes water on the surface of map.
        int waterLevel = WaterLevelDecleration(perlinHeightArray);
        for (int x = 0; x < width; x++) {
            for (int y = perlinHeightArray[x]; y <= waterLevel; y++) {
                map[x, y] = 4;
            }
        }
        return map;
    }
    private int[,] treeGeneration(int[,] map) {
        // *** function description : ***
        // this function checks surface of ground and if there were no water on surface or stone instead ground and also if there were no tree in radious of 2 blocks around the targeted block, will generate tress in that location.
        bool collision = false;
        for (int x = 0; x < width; x++) {
            int y = perlinHeightArray[x];
            System.Random pesudoTreeRandom = new System.Random((int)DateTime.Now.Ticks);
            if (map[x,y] == 0 && map[x,y-1] == 1) {
                Debug.Log("working");
                for (int xmas = x - 5; xmas <= x + 5; xmas++) {
                    if (xmas >= 0 && xmas < width && y + 2 < height) { //shouldn't be out of neightbourhood zone
                        if (map[xmas, y + 2] != 0) collision = true;
                    }
                }
                if (collision == false) {
                    if (pesudoTreeRandom.Next(0, 101) < 30) map[x, y + 2] = 7;
                }
                collision = false;
            }
        }
        return map;
    }
    public void RenderMap(int[,] map, Tilemap groundTileMap, Tilemap caveTileMap,Tilemap waterTileMap, Tilemap environmentTileMap, TileBase groundTileBase, TileBase caveTileBase, TileBase waterTileBase, TileBase stoneTileBase, TileBase treeTileBase) {
        // *** function description : ***
        // this function applies map array into tile maps and translate them to tiles
        for (int x = 0; x< width; x++) {
            for (int y = 0; y < height; y++) {
                if (map[x, y] == 1) {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileBase);
                }else if (map[x, y] == 2 || map[x, y] == 5) {
                    caveTileMap.SetTile(new Vector3Int(x, y, 0), caveTileBase);
                }else if (map[x, y] == 4) {
                    waterTileMap.SetTile(new Vector3Int(x, y, 0), waterTileBase);
                }else if (map[x, y] == 6) {
                    groundTileMap.SetTile(new Vector3Int(x, y, 0), stoneTileBase);
                }else if (map[x,y] == 7) {
                    environmentTileMap.SetTile(new Vector3Int(x, y, 0), treeTileBase);
                }
            }
        }
    }
    private void clearMap() {
        // *** function description : ***
        // this function clears all tile maps and prepares screen for new generations.
        groundTileMap.ClearAllTiles();
        caveTileMap.ClearAllTiles();
        waterTileMap.ClearAllTiles();
        environmentTileMap.ClearAllTiles();
    }
    int RandomWaterHeight(int min, int max) {
        // *** function description : ***
        // this function calculates random water level in caves.
        System.Random pesudoRandom = new System.Random((int)DateTime.Now.Ticks);
        int randomHeight = pesudoRandom.Next(min - 1, max);
        return randomHeight;
    }
    private int WaterLevelDecleration(int[] perlinHeight) {
        // *** function description : ***
        // this function calculates surface water level based on understanding the amount minimum and maximum points of surface ground and reducing it to one third of it. 
        int perlinHeightMax = 0;
        int perlinHeightMin = height;
        for (int x = 0; x < width; x++) {
            perlinHeightMax = (perlinHeight[x] >= perlinHeightMax) ? perlinHeight[x] : perlinHeightMax;
            perlinHeightMin = (perlinHeight[x] <= perlinHeightMin) ? perlinHeight[x] : perlinHeightMin;
        }
        WaterLevel = perlinHeightMin +( (perlinHeightMax - perlinHeightMin) / 3);
        //System.Random pesudoRandom = new System.Random((int)DateTime.Now.Ticks);
        //WaterLevel = pesudoRandom.Next(perlinHeightMin , perlinHeightMax);
        return WaterLevel;
    }
}

struct Coordinates {
    // *** struct description : ***
    // this struct helps to keep the coordinates of a location on map.
    public int X, Y;
    public Coordinates(int x, int y) {
        X = x; Y = y;
    }
}

