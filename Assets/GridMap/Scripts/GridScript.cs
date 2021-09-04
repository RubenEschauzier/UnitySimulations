using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] gridArray;
    private Vector3 originPosition;
    private TextMesh[,] textArray;
    private bool debugRun; 

    public Grid(int height, int width, float cellSize, Vector3 originPosition, bool debugRun)
    {
        this.height = height;
        this.width = width;
        this.cellSize = cellSize;
        this.debugRun = debugRun;

        gridArray = new int[height, width];
        textArray = new TextMesh[height, width];

        this.originPosition = new Vector3(-1 * Mathf.FloorToInt(height/2), -1 * Mathf.FloorToInt(width/2), 0);

        GameObject MeshVisual = GameObject.FindGameObjectWithTag("MeshVisual");

        if (this.debugRun)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    textArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y].ToString(), MeshVisual.transform, GetPositionWorld(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetPositionWorld(x, y), GetPositionWorld(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetPositionWorld(x, y), GetPositionWorld(x + 1, y), Color.white, 100f);

                }
            }
            Debug.DrawLine(GetPositionWorld(0, width), GetPositionWorld(height, width), Color.white, 100f);
            Debug.DrawLine(GetPositionWorld(height, 0), GetPositionWorld(height, width), Color.white, 100f);

        }

    }
    public Vector3 GetPositionWorld(int x, int y)
    {
        return new Vector3(x, y) * cellSize + this.originPosition;

    }
    
    public void getXYValue(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition-this.originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition-this.originPosition).y / cellSize);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x <= height && y <= height)
        {
            gridArray[x, y] = value;
            if (this.debugRun)
            {
                textArray[x, y].text = gridArray[x, y].ToString();
            }
        }
    }
    public void SetValue(Vector3 worldPosition, int value)
    {
        int x;
        int y;
        getXYValue(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= height && y <= height)
        {
            return gridArray[x, y];
        }
        else
        {
            return -1;
        }

    }

    public int GetValue(Vector3 worldPosition)
    {
        int x;
        int y;
        getXYValue(worldPosition, out x, out y);
        return GetValue(x, y);

    }

    public int[,] GetGrid()
    {
        return gridArray;
    }

}
