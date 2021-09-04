using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationControl : MonoBehaviour
{

    // Links for shadows :
    // https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
    // https://www.redblobgames.com/articles/visibility/
    // https://ncase.me/sight-and-light/
    // https://www.youtube.com/watch?v=fc3nnG2CG8U

    public GameObject border;
    public GameObject water;
    public GameObject bush;
    public GameObject food;
    public GameObject entity;

    public LayerMask foodLayer;

    // Define the grid
    OneGrid grid;
    // Define paramters from the grid
    int width_terrain;
    int height_terrain;
    float cellSize;
    // Offset from middle of grid, don't really know why this is needed, but hey it works
    Vector3 offSet;

    // Define array / bitmap of food
    int[,] foodLocation;
    int[,] waterLocation;
    GameObject[,] entityLocation;
    int[,] borderLocation;
    int[,] maskLayer;

    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindWithTag("grid").GetComponent<OneGrid>();

        width_terrain = grid.width;
        height_terrain = grid.height;
        cellSize = grid.cellSize;
        offSet = new Vector3(cellSize / 2, cellSize / 2, cellSize / 2);

        foodLocation = new int[width_terrain, height_terrain];
        entityLocation = new GameObject[width_terrain, height_terrain];
        borderLocation = new int[width_terrain, height_terrain];

        generateTerrain(.3f, .3f, .95f, .95f);
        generateInitialEntities();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnDrawGizmos()
    {
        for (int i = 0; i < width_terrain; i++)
        {
            for (int j = 0; j < height_terrain; j++)
            {
                if (borderLocation[i,j] == 1)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(grid.GetPositionWorld(i, j) + offSet, .75f);
                }
            }
        }
    }

    public List<Vector2> CheckLineOfSight(int x0, int y0, int x1, int y1)
    {
        // Returns true if object is within line of sight using Bresenham line algorithm
        List<Vector2> arrayLine;
        if (Mathf.Abs(y1-y0) < Mathf.Abs(x1 - x0))
        {
            if (x0 > x1)
            {
                arrayLine = CreateLineLow(x1, y1, x0, y0);
            }
            else
            {
                arrayLine = CreateLineLow(x0, y0, x1, y1);
            }
        }
        else
        {
            if (y0 > y1)
            {
                arrayLine = CreateLineHigh(x1, y1, x0, y0);
            }
            else
            {
                arrayLine = CreateLineHigh(x0, y0, x1, y1);
            }
        }
        return arrayLine;
    }

    public List<Vector2> CreateLineHigh(int x0, int y0, int x1, int y1)
    {
        List<Vector2> result = new List<Vector2>();
        int dx = x1 - x0;
        int dy = y1 - y0;
        int xi = 1;

        if(dx < 0) {
            xi = -1;
            dx = -dx;
        }
        int D = (2 * dx) - dy;
        int x = x0;
        for(int i = y0; i < y1; i++)
        {
            result.Add(new Vector2(x, i));
            if (D > 0)
            {
                x = x + xi;
                D = D + (2 * (dx - dy));
            }
            else
            {
                D = D + 2 * dx;
            }
        }
        return result;
    }
    public List<Vector2> CreateLineLow(int x0, int y0, int x1, int y1)
    {
        List<Vector2> result = new List<Vector2>();
        int dx = x1 - x0;
        int dy = y1 - y0;
        int yi = 1;

        if (dy < 0)
        {
            yi = -1;
            dy = -dy;
        }
        int D = (2 * dy) - dx;
        int y = y0;
        for (int i = x0; i < x1; i++)
        {
            result.Add(new Vector2(i, y));
            if (D > 0)
            {
                y = y + yi;
                D = D + (2 * (dy - dx));
            }
            else
            {
                D = D + 2 * dy;
            }
        }
        return result;
    }



    void FixedUpdate()
    {
        // Fix wall behavior
        // Resources: https://www.reddit.com/r/gamedev/comments/3q0yn8/here_is_some_free_unity_movement_ai_ive_made/
        // http://wiki.unity3d.com/index.php/Scripts/Controllers
        // Change this loop for performance, instead use list with coordinates for entities
        for (int i = 0; i < width_terrain; i++)
        {
            for (int j = 0; j < height_terrain; j++)
            {
                if (entityLocation[i, j] != null)
                {
                    // Pre-allocate hunger and thirst since we will always use this and sometimes more than once
                    float entityHunger = entityLocation[i, j].GetComponent<SimpleEntity>().hunger;
                    float entityThirst = entityLocation[i, j].GetComponent<SimpleEntity>().thirst;
                    int entitySpeed = entityLocation[i, j].GetComponent<SimpleEntity>().speed;

                    if (entityHunger > 100)
                    {
                        entityLocation[i, j].GetComponent<SimpleEntity>().KillEntity();
                        entityLocation[i, j] = null;
                    }
                    else
                    {
                        // Initialise variables needed to find best food
                        float bestFoodDistance = float.PositiveInfinity;
                        int bestFoodX = 0; int bestFoodY = 0;
                        Vector2 bestFoodLocation = new Vector2(-1, -1);

                        // Pre-allocate some variables for less computation
                        int entityVisionRange = entityLocation[i, j].GetComponent<SimpleEntity>().visionRange;
                        Vector2 entityLocationCurrent = entityLocation[i, j].transform.position;

                        // Make sure we only search within things that are possibly within range of our entity to save computing power
                        int lowX = Mathf.Clamp(i - entityVisionRange, 0, width_terrain); int highX = Mathf.Clamp(i + entityVisionRange + 1, 0, width_terrain);
                        int lowY = Mathf.Clamp(j - entityVisionRange, 0, height_terrain); int highY = Mathf.Clamp(j + entityVisionRange + 1, 0, height_terrain);

                        // Some debugging tools
                        Debug.DrawLine(grid.GetPositionWorld(lowX, lowY), grid.GetPositionWorld(lowX, highY));
                        Debug.DrawLine(grid.GetPositionWorld(lowX, highY), grid.GetPositionWorld(highX, highY));
                        Debug.DrawLine(grid.GetPositionWorld(highX, highY), grid.GetPositionWorld(highX, lowY));
                        Debug.DrawLine(grid.GetPositionWorld(lowX, lowY), grid.GetPositionWorld(highX, lowY));

                        List<Vector2> borderRepulsion = new List<Vector2>();
                        // Search the space that can contain food that the entity can see
                        if (entityHunger > 10)
                        {
                            for (int x = lowX; x < highX; x++)
                            {
                                for (int y = lowY; y < highY; y++)
                                {

                                    if (foodLocation[x, y] == 1)
                                    {
                                        // Check line of sight using the Bresenham's line algorithm to draw a line from food to our entity, if in that line it passes over a border it is obstructed and the food is invisible to the entity
                                        List<Vector2> pathCoords = CheckLineOfSight(x, y, i, j);
                                        bool obstructed = false;
                                        foreach (Vector2 coord in pathCoords)
                                        {
                                            if (borderLocation[(int)coord.x, (int)coord.y] == 1)
                                            {
                                                obstructed = true;
                                            }
                                        }
                                        if (!obstructed)
                                        {
                                            // If it contains food we calculate both the distance and angle to find if the entity would see the food.
                                            // The distance is because a square can be longer on the outer edges that our intended vision range

                                            Vector2 foodLocationCurrent = grid.GetPositionWorld(x, y) + offSet;
                                            float angle = Mathf.Acos((Vector2.Dot((new Vector2(x, y) - new Vector2(i, j)).normalized, entityLocation[i, j].transform.up.normalized)));
                                            float distance = Vector2.Distance(foodLocationCurrent, entityLocationCurrent);
                                            if (angle < (Mathf.Deg2Rad * 120) / 2 && distance < entityVisionRange + 1)
                                            {
                                                // Here we find the best food
                                                if (bestFoodDistance > distance)
                                                {
                                                    bestFoodDistance = distance;
                                                    bestFoodLocation = foodLocationCurrent;
                                                    bestFoodX = x; bestFoodY = y;
                                                }
                                            }

                                        }
                                    }
                                    //if (borderLocation[x,y] == 1)
                                    //{
                                    //    // THIS DOESN'T WORK
                                    //    float distanceToBorder = Vector2.Distance(grid.GetPositionWorld(x, y) + offSet, grid.GetPositionWorld(i, j) + offSet);
                                    //    float angle = Mathf.Acos((Vector2.Dot((grid.GetPositionWorld(x,y) - grid.GetPositionWorld(i,j)).normalized, entityLocation[i, j].transform.up.normalized)));

                                    //    if (distanceToBorder < 1.5f)
                                    //    {
                                    //        // Get force drawing the unit away from the wall
                                    //        // Calculated by taking the opposite of the normalized vector to the wall and scaling it with 1/distance to wall to ensure that close borders have stronger opposing force
                                    //        Debug.DrawLine(grid.GetPositionWorld(x, y) + offSet, entityLocation[i, j].transform.position);
                                    //        Vector2 opposingForce = - (((8f - distanceToBorder*4) / 4)+1)* (grid.GetPositionWorld(x, y) + offSet - grid.GetPositionWorld(i, j) + offSet).normalized;
                                    //        borderRepulsion.Add(opposingForce);
                                    //    }
                                    //    // Behavior for walls here
                                    //}
                                }
                            }

                        }

                        // Check if we found any food at all
                        if (bestFoodLocation != new Vector2(-1, -1))
                        {
                            Debug.DrawLine(bestFoodLocation, entityLocationCurrent);

                            // The entity that we will make move
                            GameObject entityChanged = entityLocation[i, j];

                            // If we are hungry and close to food we eat it!
                            // Removes the object using entity function and increases hunger by 25, we also set the foodLocation array to 0
                            if (bestFoodDistance < 1.25 && entityHunger > 15)
                            {
                                Collider2D foodCollider = Physics2D.OverlapCircle(bestFoodLocation, .5f, foodLayer);
                                entityChanged.GetComponent<SimpleEntity>().eatFood(foodCollider.gameObject);
                                foodLocation[bestFoodX, bestFoodY] = 0;
                            }

                            // Get direction of movement and pass that, location of food to entity and make entity face food
                            Vector2 direction = (bestFoodLocation - entityLocationCurrent).normalized;
                            entityChanged.GetComponent<SimpleEntity>().direction = direction;
                            entityChanged.GetComponent<SimpleEntity>().targetPosition = bestFoodLocation;
                            entityChanged.transform.up = direction;

                            // Convert to Vector2 to Vector3 to use it in making character move 
                            Vector3 direction3 = direction;

                            // Move the entity, remeber this is hardcoded for now ADD SPEED PARAMETER
                            entityChanged.GetComponent<Rigidbody2D>().MovePosition(entityChanged.transform.position + direction3 * entitySpeed * Time.fixedDeltaTime);

                            // Update the location of the entity within our array
                            int newI; int newJ;
                            grid.ConvertPosToXY(entityChanged.transform.position, out newI, out newJ);
                            if (newI != i || newJ != j)
                            {
                                entityLocation[i, j] = null;
                                entityLocation[newI, newJ] = entityChanged;
                            }
                        }
                        else

                        {
                            Vector3 direction = new Vector3(0, 0, 0);
                            GameObject entityChanged = entityLocation[i, j];
                            //if (borderRepulsion.Count > 1)
                            //{
                            //    // THIS DOESN'T WORK CHANGE THIS
                            //    foreach (Vector2 repulsion in borderRepulsion)
                            //    {
                            //        //Debug.DrawRay(entityChanged.transform.position, repulsion);
                            //        direction += (Vector3)repulsion;
                            //        Debug.Log(repulsion);
                            //        //direction += entityChanged.transform.up;
                            //    }
                            //    if (entityChanged.GetComponent<SimpleEntity>().currentPosition == entityChanged.GetComponent<SimpleEntity>().previousPosition)
                            //    {
                            //        direction = -direction;
                            //    }
                            //}
                            //else
                            //{
                            //
                            //}
                            direction = entityChanged.GetComponent<SimpleEntity>().direction;
                            if (Random.value < 0.01)
                            {
                                direction.x = direction.x + Random.Range(-1.0f, 1.0f);
                                direction.y = direction.y + Random.Range(-1.0f, 1.0f);
                            }
                            

                            direction = direction.normalized;
                            Debug.DrawRay(entityChanged.transform.position, direction * 2);
                            entityChanged.transform.up = direction;
                            entityChanged.GetComponent<SimpleEntity>().direction = direction;
                            entityChanged.GetComponent<SimpleEntity>().previousPosition = entityChanged.transform.position;
                            entityChanged.GetComponent<Rigidbody2D>().MovePosition(entityChanged.transform.position + direction * entitySpeed * Time.fixedDeltaTime);
                            entityChanged.GetComponent<SimpleEntity>().currentPosition = entityChanged.transform.position;

                            int newI; int newJ;
                            grid.ConvertPosToXY(entityChanged.transform.position, out newI, out newJ);
                            if (newI != i || newJ != j)
                            {
                                entityLocation[i, j] = null;
                                entityLocation[newI, newJ] = entityChanged;
                            }

                        }
                    }
                }
            }
        }
    }

    void generateTerrain(float borderUpperBound, float waterLowerBound, float bushLowerBound, float foodLowerBound)
    {
        List<GameObject> simulationObjects = new List<GameObject>();

        int width_terrain = grid.width;
        int height_terrain = grid.height;
        float cellSize = grid.cellSize;

        Vector3 offSet = new Vector3(cellSize / 2, cellSize / 2, cellSize / 2);

        // Generate walls using Perlin Noise
        for (int i = 0; i < width_terrain; i++)
        {
            for (int j = 0; j < height_terrain; j++)
            {
                float rnd = Mathf.PerlinNoise(((float)i / width_terrain - .5f) * 10, ((float)j / height_terrain - .5f) * 10);
                if (rnd < borderUpperBound)
                {
                    Vector2 worldPosition = grid.GetPositionWorld(i, j) + offSet;
                    grid.SetValue(worldPosition, 1);
                    // grid.SetValue(worldPosition, 1);
                    grid.SetObstructed(worldPosition, true);
                    GameObject newBorder = Instantiate(border, worldPosition, Quaternion.identity, this.transform);
                    simulationObjects.Add(newBorder);
                    borderLocation[i, j] = 1;
                }
                if (i == 0 || i == width_terrain - 1 || j == 0 || j == height_terrain - 1)
                {
                    borderLocation[i, j] = 1;
                }
            }
        }
        // Generate bushes using random noise
        for (int x = 1; x < width_terrain - 2; x++)
        {
            for (int y = 1; y < height_terrain - 2; y++)
            {
                float rndBush = Random.Range(0f, 1f);
                if (rndBush > bushLowerBound && grid.GetValue(x, y) != 1)
                {
                    Vector2 worldPosition = grid.GetPositionWorld(x, y) + offSet;
                    grid.SetValue(worldPosition, 3);
                    GameObject newBorder = Instantiate(bush, worldPosition, Quaternion.identity, this.transform);
                    simulationObjects.Add(newBorder);

                }
            }
        }
        int offSetWater = Random.Range(0, 1000);
        for (int k = 1; k < width_terrain - 1; k++)
        {
            for (int z = 1; z < height_terrain - 1; z++)
            {
                float rndWater = Mathf.PerlinNoise(((float)(k + offSetWater) / width_terrain - .5f) * 10, ((float)(z + offSetWater) / height_terrain - .5f) * 10);
                if (rndWater < waterLowerBound && (k != width_terrain || z != height_terrain))
                {
                    Vector2 worldPosition = grid.GetPositionWorld(k, z) + offSet;
                    grid.SetValue(worldPosition, 2);
                    GameObject newBorder = Instantiate(water, worldPosition, Quaternion.identity, this.transform);
                    simulationObjects.Add(newBorder);

                }
            }
        }
        for (int x = 1; x < width_terrain - 1; x++)
        {
            for (int y = 1; y < height_terrain - 1; y++)
            {
                float rndFood = Random.Range(0f, 1f);
                if (rndFood > foodLowerBound && grid.GetValue(x, y) == 0 && (x != width_terrain || y != height_terrain))
                {
                    Vector2 worldPosition = grid.GetPositionWorld(x, y) + offSet;
                    grid.SetValue(worldPosition, 4);
                    foodLocation[x, y] = 1;

                    GameObject newBorder = Instantiate(food, worldPosition, Quaternion.identity, this.transform);
                    simulationObjects.Add(newBorder);

                }
            }
        }
    }

    void generateInitialEntities()
    {
        List<GameObject> simulationEntities = new List<GameObject>();

        for (int i = 1; i < width_terrain - 1; i++)
        {
            for (int j = 1; j < height_terrain - 1; j++)
            {
                float rndEntity = Random.Range(0f, 1f);

                //if (grid.GetValue(i,j) == 0 && rndEntity > .995)
                if (i == 10 && j == 10)
                {
                    int visionRange = Random.Range(0,6);
                    int moveSpeed = Random.Range(1,3);

                    Vector2 worldPosition = grid.GetPositionWorld(i, j) + offSet;
                    GameObject entityCreated = Instantiate(entity, worldPosition, Quaternion.identity, this.transform);

                    entityCreated.GetComponent<SimpleEntity>().speed = moveSpeed;
                    entityCreated.GetComponent<SimpleEntity>().visionRange = visionRange;

                    entityLocation[i, j] = entityCreated;

                }
            }
        }
    }
}
