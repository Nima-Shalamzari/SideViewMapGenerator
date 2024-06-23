using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTPGAssist
{
    int[,] map;
    int width, height;

    public RTPGAssist(int[,] map, int width, int height) {
        this.map = map;
        this.width = width;
        this.height = height;
    }

    public void GetSurroundingGround(int gridx, int gridy, int min, int max) {
        //int tempMin = Min(gridy, min);
        //int tempMax = Max(gridy, max);
        for (int neby = gridy - 1; neby <= gridy + 1; neby++) {
            if (neby >= 0 && neby < height) {
                if (neby == gridy - 1) {
                    for (int nebx = gridx - 1; nebx <= gridx + 1; nebx++) {
                        if (nebx >= 0 && nebx < width) {
                            if (map[nebx, neby] == 2) {
                                //tempMin = Min(neby, min);
                                GetSurroundingGround(nebx, neby, Min(min, neby), max);
                            }
                        }
                    }
                }
                else if (neby == gridy) {
                    int nebx = gridx + 1;
                    if (nebx < width) {
                        if (map[nebx, neby] == 2) {
                            GetSurroundingGround(nebx, neby, min, max);
                        }
                    }
                }
                else if (neby == gridy + 1) {
                    for (int nebx = gridx + 1; nebx >= gridx - 1; nebx--) {
                        if (nebx >= 0 && nebx < width) {
                            if (map[nebx, neby] == 2) {
                                //tempMax = Max(neby, max);
                                GetSurroundingGround(nebx, neby, min, Max(max, neby));
                            }
                        }
                    }
                    neby = gridy;
                    int nex = gridx - 1;
                    if (nex >= 0 && nex < width) {
                        if (map[nex, neby] == 2) {
                            //tempMax = Max(neby, max);
                            GetSurroundingGround(nex, neby, min, Max(max, neby));
                        }
                    }
                }
            }
        }
    }

    private int Max(int Maxy, int max) {
        Maxy = (max > Maxy) ? max : Maxy;
        return Maxy;
    }
    private int Min(int Miny, int min) {
        Miny = (min < Miny) ? min : Miny;
        return Miny;
    }
}
