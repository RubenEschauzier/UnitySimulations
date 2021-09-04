using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System;


public class OneGrid : MonoBehaviour
{
    public GameObject border;
    public GameObject water;
    public GameObject bush;
    public GameObject food;

    public LayerMask obstructedMask;
    public int height;
    public int width;
    public float cellSize;
    public Vector2 originPosition;

    public Node2[,] grid;

    public int value = 1;
    public List<Node2> path;


    // Start is called before the first frame update
    void Start()
    {
        originPosition = new Vector2(-1 * Mathf.FloorToInt(width / 2), -1 * Mathf.FloorToInt(height / 2));
        CreateGrid();

        for (int i = 0; i < height; i++)
        {
            Vector3 edgePosition1 = GetPositionWorld(0, i);
            edgePosition1 = GetMiddleCube(edgePosition1);
            Vector3 edgePosition2 = GetPositionWorld(width - 1, i);
            edgePosition2 = GetMiddleCube(edgePosition2);

            SetValue(edgePosition1, value);
            SetValue(edgePosition2, value);
            SetObstructed(edgePosition1, true);
            SetObstructed(edgePosition2, true);

            GameObject newBorder = Instantiate(border, edgePosition1, Quaternion.identity, this.transform);
            GameObject newBorder2 = Instantiate(border, edgePosition2, Quaternion.identity, this.transform);
        }

        for (int j = 0; j < width; j++)
        {
            Vector3 edgePosition1 = GetPositionWorld(j, 0);
            edgePosition1 = GetMiddleCube(edgePosition1);
            Vector3 edgePosition2 = GetPositionWorld(j, height - 1);
            edgePosition2 = GetMiddleCube(edgePosition2);

            SetValue(edgePosition1, value);
            SetValue(edgePosition2, value);
            SetObstructed(edgePosition1, true);
            SetObstructed(edgePosition2, true);

            GameObject newBorder = Instantiate(border, edgePosition1, Quaternion.identity, this.transform);
            GameObject newBorder2 = Instantiate(border, edgePosition2, Quaternion.identity, this.transform);
        }

    }


    // Update is called once per frame
    void Update()
        {
            if (Input.GetMouseButton(0))
            {
                bool ClickUI = false;
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    if (EventSystem.current.currentSelectedGameObject.name == "Button")
                    {
                        ClickUI = true;
                    }
                }

                if (!ClickUI)
                {
                    Vector3 mouseCoords = Input.mousePosition;
                    Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mouseCoords);

                    if (GetValue(worldPosition) == 2 || GetValue(worldPosition) == 0)
                    {
                        if (value == 1)
                        {
                            SetValue(worldPosition, value);
                            SetObstructed(worldPosition, true);
                            Vector3 objectPosition = GetMiddleCube(worldPosition);
                            GameObject newBorder = Instantiate(border, objectPosition, Quaternion.identity, this.transform);
                            

                        }

                        if (value == 3)
                        {
                            SetValue(worldPosition, value);
                            Vector3 objectPosition = GetMiddleCube(worldPosition);
                            GameObject newBorder = Instantiate(bush, objectPosition, Quaternion.identity, this.transform);

                        }
                    }
                    if (GetValue(worldPosition) == 0 || GetValue(worldPosition) == 1 || GetValue(worldPosition) == 3 && GetValue(worldPosition) != -1)
                    {
                        //grid.SetValue(worldPosition, this.value);

                        if (value == 2 && GetValue(worldPosition) == 0)
                        {
                            SetValue(worldPosition, value);
                            Vector3 objectPosition = GetMiddleCube(worldPosition);
                            GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);
                        }
                        if (value == 2 && GetValue(worldPosition) == 1)
                        {
                            SetValue(worldPosition, 5);
                            Vector3 objectPosition = GetMiddleCube(worldPosition);
                            GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);

                        }
                        if (value == 2 && GetValue(worldPosition) == 3)
                        {
                            SetValue(worldPosition, 6);
                            Vector3 objectPosition = GetMiddleCube(worldPosition);
                            GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);

                        }



                    }
                    if (GetValue(worldPosition) == 0 && GetValue(worldPosition) != -1 && this.value == 4)
                    {
                        SetValue(worldPosition, 4);
                        Vector3 objectPosition = GetMiddleCube(worldPosition);
                        GameObject newBorder = Instantiate(food, objectPosition, Quaternion.identity, this.transform);

                    }
                    if (value == 10)
                    {
                        Vector3 objectPosition = GetMiddleCube(worldPosition);
                        RaycastHit2D hit = Physics2D.Raycast(new Vector2(objectPosition.x, objectPosition.y), new Vector2(0, 0), 100f);
                        if (hit.collider != null)
                        {
                            Debug.Log("Hit!");

                            if (hit.transform.gameObject.layer != 3)
                            {
                                GameObject.Destroy(hit.transform.gameObject);
                                SetValue(worldPosition, 0);
                                SetObstructed(worldPosition, false);

                            }


                        }

                    }


                }
            }
        }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 1));
        if (grid != null)
        {
            foreach (Node2 n in grid)
            {
                Gizmos.color = (n.obstructed) ?  new Color(1, 0, 0, 0.5f) : new Color(0, 0, 1, 0.5f);
                if(path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = new Color(1, 1, 1, 0.5f);
                        Vector2 PositionBelow = new Vector2(n.position.x, n.position.y - .25f);
                        Vector2 PositionAbove = new Vector2(n.position.x, n.position.y + .25f);
                        Vector2 PositionF = new Vector2(n.position.x, n.position.y + .5f);

                        //Handles.Label(PositionAbove, (n.hCost).ToString());
                        //Handles.Label(PositionBelow, (n.gCost).ToString());
                        //Handles.Label(PositionF, (n.hCost + n.gCost).ToString());

                    }
                }
                Gizmos.DrawCube(n.position, Vector3.one * (cellSize - 0.1f));

            }
        }

    }


    void CreateGrid()
    {
        grid = new Node2[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 middlePosition = GetMiddleCube(x, y);
                bool obstructed = (Physics2D.OverlapBox(middlePosition, new Vector2(cellSize - 0.1f, cellSize - 0.1f), 0f, obstructedMask));

                //bool obstructed = (Physics.CheckSphere(worldPoint, cellSize/2, obstructedMask));
                grid[x, y] = new Node2(middlePosition, obstructed, 0, 0);
            }
        }
    }

    public List<Node2> GetAllNeighbours(Node2 n)
    {
        int posX, posY;
        ConvertPosToXY(n.position, out posX, out posY);
        Vector2 center = n.position;
        List <Node2> neighbours = new List<Node2>();
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (posX + offsetX >= 0 && posY + offsetY >= 0 && posX + offsetX < width && posY + offsetY < height)
                {
                    if (offsetX != 0 || offsetY != 0)
                    {
                        if (offsetX != 0 && offsetY != 0)
                        {
                            if (grid[posX, posY + offsetY].obstructed == true && grid[posX + offsetX, posY].obstructed == true)
                            {
                                continue;
                            }

                        }

                        neighbours.Add(grid[posX + offsetX, posY + offsetY]);
                    }
                }

            }
        }
        return neighbours;
    }

    public void ConvertPosToXY(Vector2 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public Vector3 GetPositionWorld(int x, int y)
    {
        return new Vector2(x, y) * cellSize + originPosition;

    }


    public Vector2 GetMiddleCube(int x, int y)
    {
        return (new Vector2(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2) + originPosition); 
    }

    public Vector2 GetMiddleCube(Vector2 worldPosition)
    {
        int x, y;
        ConvertPosToXY(worldPosition, out x, out y);
        return (GetMiddleCube(x, y));
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            grid[x, y].SetValueNode(value);
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        ConvertPosToXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public void SetObstructed(int x, int y, bool obstructed)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            grid[x, y].SetObstructedNode(obstructed);
        }
    }


    public void SetObstructed(Vector3 worldPosition, bool obstructed)
    {
        int x, y;
        ConvertPosToXY(worldPosition, out x, out y);
        SetObstructed(x, y, obstructed);

    }

    public int GetValue(Vector3 worldPosition)
    {
        int x;
        int y;
        ConvertPosToXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return grid[x, y].GetValueNode();
        }
        else
        {
            return -1;
        }

    }

    public Node2 GetNode(Vector2 worldPosition)
    {
        // return new Node2(new Vector3(0, 0, 0), false, 1, 1); 
        int x, y;
        ConvertPosToXY(worldPosition, out x, out y);
        return grid[x, y];
    }

    public void UpdateOnClickValue(int newValue)
    {
        this.value = newValue;
    }


}

