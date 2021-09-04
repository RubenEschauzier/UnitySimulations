using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    // Evolveable genes / parameters
    public int movespeedLand;
    public int movespeedWater;
    public float smellRadius;
    public float sightAngle;
    public int sightRange;
    public int size;

    // These don't evolve
    public LayerMask foodLayer;
    public LayerMask entityLayer;
    public LayerMask waterLayer;
    public LayerMask seeThroughMask;
    public float alpha;
    public float beta;
    public float theta;
    public int gender; // 0 = Male, 1 = Female
    public bool herbivore;
    OneGrid grid;

    float timeDirection;
    Rigidbody2D rb;
    Vector2 direction;
    float thirst;
    float hunger;
    GameObject currentTarget;
    float prevScore;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        grid = GameObject.FindWithTag("grid").GetComponent<OneGrid>();

        hunger = 0;
        thirst = 0;
        direction = new Vector2(Random.value, Random.value).normalized;
        currentTarget = null;
        prevScore = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        hunger += Time.deltaTime * size;
        thirst += Time.deltaTime * size;

        Vector2 entityPosition = gameObject.transform.position;
        Vector2 foodPosition = SmellCheck(entityPosition, herbivore);
        GameObject target = VisionCheck(entityPosition);
        if(target != null)
        {
            Vector2 targetPosition = target.transform.position;
            float distanceTarget = Vector2.Distance(targetPosition, entityPosition);
            if (distanceTarget < .5)
            {
                InteractWithEnvironment(target);
            }
            direction = (targetPosition - entityPosition).normalized;
            currentTarget = target;
        }

        if (foodPosition != new Vector2(0, 0))
        {
            //Debug.Log(foodPosition);

        }
        if (Random.value < 0.01 && target == null )
        {
            direction.x = direction.x + Random.Range(-1.0f, 1.0f);
            direction.y = direction.y + Random.Range(-1.0f, 1.0f);
        }
  

        direction = direction.normalized;
        gameObject.transform.up = direction;
        
        rb.MovePosition(rb.position + direction * movespeedLand * Time.deltaTime);

    }
    Vector2 SmellCheck(Vector2 position, bool herbivore)
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        if (herbivore)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, smellRadius, foodLayer);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;

            if (collider != null)
            {
                return collider.transform.position;
            }
            else
            {
                return new Vector2(0, 0);
            }
        }
        else
        {
            Collider2D collider = Physics2D.OverlapCircle(position, smellRadius, entityLayer);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            if (collider != null)
            {
                return collider.transform.position;
            }
            else
            {
                return new Vector2(0, 0);
            }
        }
    }

    GameObject VisionCheck(Vector2 position)
    {
        // Fix bug with walkability when collider on square :/
        // Still need program land / water
        // First check predator danger, if too high just quit the function and run! (DONE)
        // Then check each detected entity (excluding predators) (DONE except for mates)
        // Calculate for each entity the predator danger and score (DONE "")
        // Pick with highest score (DONE)
        // Return direction to move (DONE)
        // If nothing is returned do behavior of wall checks (check for first 'open' direction in vision, open defined as no wall within 1 distance) if no open direction turn right / left

        Collider2D[] disabledObjects = getOverlappingGameObjects(position);
        GameObject bestObject = null;
        List<GameObject> predators = DetectPredators(position);
        Vector2 locationClosestPredator = new Vector2(0,0);
        float predatorDanger = 0;
        //Replace this with function defined below;
        if (predators.Count > 0) {
            predatorDanger = GetPredatorDanger(position, predators, out locationClosestPredator);
            if (predatorDanger > 0.5)
            {
                Vector2 newDirection = (locationClosestPredator - position);
                direction = -1*newDirection;
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                enableDisabledObjects(disabledObjects);
                return bestObject;
            }

        }
        // If detects wall and nothing else, try to move around it, else change direction slightly and randomly with small chance
        bool detectWall;

        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, sightRange);

        Vector2 positionDetected = new Vector2(0,0);
        Vector2 bestPosition = new Vector2(0, 0);

        float bestScore = 5f;
        foreach (Collider2D hit in hits)
        {
            positionDetected = hit.transform.position;
            float angle = Mathf.Acos((Vector2.Dot((positionDetected - position).normalized, gameObject.transform.up.normalized)));
            if (angle < (Mathf.Deg2Rad * sightAngle) / 2)
            {
                RaycastHit2D collision = Physics2D.Raycast(position, positionDetected - position);

                if (collision != null)
                {
                    int layerObject = collision.collider.gameObject.layer;
                    float distanceObject = Vector2.Distance(positionDetected, position);
                    float score = 0;
                    float danger = 0;
                    // Calculate scores for each object detected based on thirst/hunger, distance to object and the proximity of other predators to the object
                    switch (layerObject)
                    {
                        case 4:     
                            if (predators.Count > 0)
                            {
                                danger = GetPredatorDanger(positionDetected, predators, out locationClosestPredator);
                            }
                            score = alpha * (thirst - 10 ) - beta * distanceObject - theta * danger;
                            break;
                        case 6:
                            detectWall = true;
                            score = -1;
                            break;
                        case 7:
                            if (herbivore) {
                                if (predators.Count > 0)
                                {
                                    danger = GetPredatorDanger(positionDetected, predators, out locationClosestPredator);
                                }
                                score = alpha * (hunger + 1) - beta * distanceObject - theta * danger;
                            }
                            break;
                        case 8:
                            if(collision.collider.gameObject.GetComponent<EntityController>().herbivore == herbivore)
                            {
                                if (predators.Count > 0)
                                {
                                    danger = GetPredatorDanger(positionDetected, predators, out locationClosestPredator);
                                }
                                score = alpha * 40 - beta * distanceObject - theta * danger;
                            }
                            else if(collision.collider.gameObject.GetComponent<EntityController>().herbivore == true && herbivore == false)
                            {
                                score = alpha * hunger - beta * distanceObject;
                            }
                            else
                            {
                                score = -1;
                            }
                            break;

                    }
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPosition = positionDetected;
                        bestObject = collision.collider.gameObject;
                    }
                }


            }
            // Still implement logic for finding mate
        }
        enableDisabledObjects(disabledObjects);
        if (bestScore > 0.1 + prevScore)
        {
            prevScore = bestScore;
            Debug.DrawRay(position, (bestPosition - position));
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            return bestObject;
        }
        else
        {
            return currentTarget;
        }

    }

    Vector2 WallCheck(Vector2 position)
    {
        return new Vector2(0, 0);
    }

    List<GameObject> DetectPredators(Vector2 positionEntity)
    {
        Vector2 positionDetected = new Vector2(0, 0);
        List<GameObject> predators = new List<GameObject>();
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Collider2D[] hits_entity = Physics2D.OverlapCircleAll(positionEntity, sightRange, entityLayer);
        if (hits_entity.Length > 0)
        {
        foreach (Collider2D hit in hits_entity)
            {
                positionDetected = hit.transform.position;
                float angle = Mathf.Acos((Vector2.Dot((positionDetected - positionEntity).normalized, gameObject.transform.up.normalized)));
                if (angle < (Mathf.Deg2Rad * sightAngle) / 2)
                {
                    RaycastHit2D collision = Physics2D.Raycast(positionEntity, positionDetected - positionEntity, sightRange, ~seeThroughMask);
                    if (collision != null)
                    {
                        Debug.Log(collision);
                        if (collision.collider.gameObject.GetComponent<EntityController>().herbivore == false)
                        {
                            predators.Add(collision.collider.gameObject);
                        }

                    }
                }
            }
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        return predators;
    }
    float GetPredatorDanger(Vector2 position, List<GameObject> predators, out Vector2 locationClosestPredator)
    {
        float predatorDanger = 0;
        locationClosestPredator = new Vector2(0, 0);
        foreach (GameObject predator in predators)
        {
            float currentDanger = 0;
            if (predator.GetComponent<EntityController>().size >= size)
            {
                currentDanger = 1 - (Vector2.Distance(predator.transform.position, position) / sightRange);
            }

            if (currentDanger > predatorDanger)
            {
                predatorDanger = currentDanger;
                locationClosestPredator = predator.transform.position;
            }
        }
        return predatorDanger;

    }

    // Turn off any collider that is on the position our entity is on. This allows our entity to see through a certain collider. The radius can't be too big or else the entity will not interact with anything.
    Collider2D[] getOverlappingGameObjects(Vector2 currentPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, .1f, ~entityLayer);
        if (colliders.Length > 0)
        { 
            foreach (var collider in colliders)
            {
                var go = collider.gameObject;
                go.GetComponent<Collider2D>().enabled = false;

            }
        }
        return colliders;

    }

    void enableDisabledObjects(Collider2D[] disabledObjects)
    {
        foreach (Collider2D disabledCollider in disabledObjects){
            disabledCollider.gameObject.GetComponent<Collider2D>().enabled = true;
        }

    }

    void InteractWithEnvironment(GameObject targetObject)
    {
        if (herbivore)
        {
            if (targetObject.layer == 7 && hunger > 5)
            {
                EatFood(targetObject, 25);
            }
            if (targetObject.layer == 4 && thirst > 5)
            {
                thirst += -25;
            }
            if (targetObject.layer == 8)
            {
                // implement mating
            }

        }
        if (!herbivore)
        {
            if (targetObject.layer == 8)
            {
                if (targetObject.GetComponent<EntityController>().herbivore == true && hunger > 5)
                {
                    EatFood(targetObject, 50);
                }
                else
                {
                    // implement mating
                }
            }
            if (targetObject.layer == 4 && thirst > 5)
            {
                thirst += -25;
            }
        }
    }

    void EatFood(GameObject food, int decrease)
    {
        GameObject.Destroy(food);
        // Still implement that entity is able to acces the grid
        grid.SetValue(food.transform.position, 0);
        hunger += -decrease;
    }
}
