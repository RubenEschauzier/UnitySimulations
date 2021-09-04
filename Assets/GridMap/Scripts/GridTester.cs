using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class GridTester : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject border;
    public GameObject water;
    public GameObject bush;
    public GameObject food;
    public PathGrid pathFinder;


    public int height;
    public int width;
    public float cellSize;
    public Vector3 origin;
    public bool debugRun;

    private bool ClickUI;
    private int value;
    private Grid grid;
    private float offSet;
    private int[,] gridArray;
    private Vector3 previousMiddle;

    private void Start()
    {
        offSet = Camera.main.GetComponent<followCamera>().GetZ();
        // Camera.main.transform.position = new Vector3(Mathf.FloorToInt(width), Mathf.FloorToInt(height), offSet);
        // GameObject.FindWithTag("Player").transform.position = origin + new Vector3(Mathf.FloorToInt(width/2), Mathf.FloorToInt(height/2), 0);

        grid = new Grid(width, height, cellSize, origin, debugRun);
        gridArray = grid.GetGrid();
        value = 1;


        for (int i = 0; i < gridArray.GetLength(1); i++)
        {
            Vector3 edgePosition1 = grid.GetPositionWorld(0, i);
            edgePosition1 = getMiddleGrid(edgePosition1);
            Vector3 edgePosition2 = grid.GetPositionWorld(gridArray.GetLength(0)-1, i);
            edgePosition2 = getMiddleGrid(edgePosition2);

            grid.SetValue(edgePosition1, this.value);
            grid.SetValue(edgePosition2, this.value);
            GameObject newBorder = Instantiate(border, edgePosition1, Quaternion.identity, this.transform);
            GameObject newBorder2 = Instantiate(border, edgePosition2, Quaternion.identity, this.transform);
        }

        for (int j = 0; j < gridArray.GetLength(0); j++)
        {
            Vector3 edgePosition1 = grid.GetPositionWorld(j, 0);
            edgePosition1 = getMiddleGrid(edgePosition1);
            Vector3 edgePosition2 = grid.GetPositionWorld(j, gridArray.GetLength(1) - 1);
            edgePosition2 = getMiddleGrid(edgePosition2);

            grid.SetValue(edgePosition1, this.value);
            grid.SetValue(edgePosition2, this.value);
            GameObject newBorder = Instantiate(border, edgePosition1, Quaternion.identity, this.transform);
            GameObject newBorder2 = Instantiate(border, edgePosition2, Quaternion.identity, this.transform);
        }




    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ClickUI = false;
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
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseCoords);

                if (grid.GetValue(worldPosition) != 1 && grid.GetValue(worldPosition) != 3 && grid.GetValue(worldPosition) != -1 && grid.GetValue(worldPosition) != 4)
                {

                    if (this.value == 1)
                    {
                        grid.SetValue(worldPosition, this.value);
                        Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                        GameObject newBorder = Instantiate(border, objectPosition, Quaternion.identity, this.transform);
                        pathFinder.UpdateGrid(objectPosition);

                    }
                    if (this.value == 3)
                    {
                        grid.SetValue(worldPosition, this.value);
                        Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                        GameObject newBorder = Instantiate(bush, objectPosition, Quaternion.identity, this.transform);

                    }
                }
                if (grid.GetValue(worldPosition) == 0 || grid.GetValue(worldPosition) == 1 || grid.GetValue(worldPosition) == 3 && grid.GetValue(worldPosition) != -1 && this.value == 2)
                {
                    //grid.SetValue(worldPosition, this.value);

                    if (this.value == 2 && grid.GetValue(worldPosition) == 0)
                    {
                        grid.SetValue(worldPosition, this.value);
                        Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                        GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);
                    }
                    if (this.value == 2 && grid.GetValue(worldPosition) == 1)
                    {
                        grid.SetValue(worldPosition, 5);
                        Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                        GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);

                    }
                    if (this.value == 2 && grid.GetValue(worldPosition) == 3)
                    {
                        grid.SetValue(worldPosition, 6);
                        Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                        GameObject newBorder = Instantiate(water, objectPosition, Quaternion.identity, this.transform);

                    }



                }
                if (grid.GetValue(worldPosition) == 0 && grid.GetValue(worldPosition) != -1 && this.value == 4)
                {
                    grid.SetValue(worldPosition, 4);
                    Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                    GameObject newBorder = Instantiate(food, objectPosition, Quaternion.identity, this.transform);

                }
                if (this.value == 10)
                {
                    Vector3 objectPosition = this.getMiddleGrid(worldPosition);
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(objectPosition.x, objectPosition.y), new Vector2(0, 0), 100f);
                    if (hit.collider != null)
                    {
                        Debug.Log("Hit!");

                        if (hit.transform.gameObject.layer != 3)
                        {
                            GameObject.Destroy(hit.transform.gameObject);
                            grid.SetValue(worldPosition, 0);

                        }


                    }

                }
               
                Vector3 middlePosition = this.getMiddleGrid(worldPosition);
                pathFinder.UpdateGrid(previousMiddle);
                previousMiddle = middlePosition;


            }

        }

    }

    public void UpdateOnClickValue(int newValue)
    {
        this.value = newValue;
    }

    private Vector3 getMiddleGrid(Vector3 worldPosition)
    {
        int x;
        int y;
        grid.getXYValue(worldPosition, out x, out y);
        Vector3 temp = grid.GetPositionWorld(x, y);
        temp.x = temp.x + this.cellSize / 2;
        temp.y = temp.y + this.cellSize / 2;

        return temp;
    }
}
