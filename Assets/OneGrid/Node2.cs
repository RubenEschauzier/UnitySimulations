using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node2
{
    public bool obstructed;
    public Vector3 position;
    public int value;

    public int multiplier;
    public int hCost;
    public int gCost;

    public Node2 parent;

    public Node2(Vector3 _position, bool _obstructed, int _value, int _multiplier)
    {
        position = _position;
        value = _value;
        obstructed = _obstructed;
        multiplier = _multiplier;
    }

    public void SetValueNode(int newVal)
    {
        this.value = newVal;
    }

    public int GetValueNode()
    {
        return this.value;
    }

    public void SetObstructedNode(bool obstructed)
    {
        this.obstructed = obstructed;
    }

    public bool GetObstructedNode()
    {
        return this.obstructed;
    }
    public int GetFCost()
    {
        return gCost + hCost;
    }

}
