using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool obstructed;
    public Vector3 position;
    public int fScore;

    public Node(bool _obstructed, Vector3 _position, int _fScore)
    {
        obstructed = _obstructed;
        position = _position;
        fScore = _fScore;
    }


}
