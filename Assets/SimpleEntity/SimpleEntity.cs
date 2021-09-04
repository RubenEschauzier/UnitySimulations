using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEntity : MonoBehaviour
{
    public float hunger;
    public float thirst;
    public int speed;
    public int visionRange;
    public bool predator;

    public Rigidbody2D rb;

    public Vector2 currentPosition;
    public Vector2 previousPosition;
    public Vector2 targetPosition;
    public Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        hunger = 25;
        thirst = 25;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void eatFood(GameObject food)
    {
        Destroy(food);
        hunger = Mathf.Clamp(hunger - 25, 0, 100);
    }
    void FixedUpdate()
    {
        hunger += Time.fixedDeltaTime*speed;
        thirst += Time.fixedDeltaTime*speed;
    }


    public void KillEntity()
    {
        Destroy(transform.gameObject);
    } 
}
