using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGrid : MonoBehaviour
{
    public LayerMask obstructedMask;
    public int height;
    public int width;
    public float cellSize;
    public Vector3 originPosition;
    public Node[,] grid;

    // Start is called before the first frame update
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 1));
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.obstructed)?new Color(0,0,1,0.5f):new Color(1,0,0,0.5f);
                Gizmos.DrawCube(n.position, Vector3.one * (cellSize-0.1f));

            }
        }

    }

    // Update is called once per frame
    void Start()
    {
        originPosition = new Vector3(-1 * Mathf.FloorToInt(width / 2), -1 * Mathf.FloorToInt(height / 2), 0);
        CreateGrid();
 
    }

    void CreateGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3 worldPoint = GetPositionWorld(x, y);
                Vector2 middlePosition = GetMiddleGrid(worldPoint);
                bool unobstructed = !(Physics2D.OverlapBox(middlePosition, new Vector2(cellSize-0.1f, cellSize-0.1f), obstructedMask));

                //bool obstructed = (Physics.CheckSphere(worldPoint, cellSize/2, obstructedMask));
                grid[x, y] = new Node(unobstructed, middlePosition, 0);
            }
        }
    }

    public Vector3 GetPositionWorld(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void getXYValue(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void UpdateGrid(int x, int y)
    {
        Vector2 worldPoint = GetPositionWorld(x, y);
        Vector2 middlePosition = GetMiddleGrid(worldPoint);
        bool unobstructed = !(Physics2D.OverlapBox(middlePosition, new Vector2(cellSize, cellSize), obstructedMask));
        grid[x, y] = new Node(unobstructed, worldPoint, 0);

    }


    public void UpdateGrid(Vector2 worldPoint)
    {
        int x;
        int y;
        getXYValue(worldPoint, out x, out y);
        Vector2 middlePosition = GetMiddleGrid(worldPoint);
        bool unobstructed = !(Physics2D.OverlapBox(middlePosition, new Vector2(cellSize, cellSize), obstructedMask));
        grid[x, y] = new Node(unobstructed, worldPoint, 0);
    }

    private Vector3 GetMiddleGrid(Vector3 worldPosition)
    {
        int x;
        int y;
        getXYValue(worldPosition, out x, out y);
        Vector3 temp = GetPositionWorld(x, y);
        temp.x = temp.x + this.cellSize / 2;
        temp.y = temp.y + this.cellSize / 2;

        return temp;
    }


}
