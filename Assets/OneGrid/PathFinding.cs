using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathFinding : MonoBehaviour
{
    public Transform seeker, target;
    OneGrid grid;

    void Awake()
    {
        grid = GetComponent<OneGrid>();
    }

    public void FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        Node2 startNode = grid.GetNode(startPosition);
        Node2 targetNode = grid.GetNode(targetPosition);

        List<Node2> openNodes = new List<Node2>();
        HashSet<Node2> closedNodes = new HashSet<Node2>();
        openNodes.Add(startNode);

        while (openNodes.Count > 0)
        {
            Node2 currentNode = openNodes[0];
            foreach(Node2 n in openNodes)
            {
                if (n.GetFCost() < currentNode.GetFCost())
                {
                    currentNode = n;
                }
                if (n.GetFCost() == currentNode.GetFCost())
                {
                    if (n.hCost < currentNode.hCost)
                    {
                        currentNode = n;
                    }
                }
            }
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                List<Node2> shortestPath = new List<Node2>();
                Node2 currentRetraceNode = targetNode;
                while(currentRetraceNode != startNode)
                {
                    shortestPath.Add(currentRetraceNode);
                    currentRetraceNode = currentRetraceNode.parent;
                  
                }
                shortestPath.Reverse();
                grid.path = shortestPath;
                return;
                //return shortestPath.Reverse();
            }
            foreach (Node2 neighbour in grid.GetAllNeighbours(currentNode))
            {
                if(neighbour.obstructed || closedNodes.Contains(neighbour))
                {
                    continue;
                }
                int newCostNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newCostNeighbour < neighbour.gCost || !openNodes.Contains(neighbour))
                {
                    neighbour.gCost = newCostNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openNodes.Contains(neighbour))
                    {
                        openNodes.Add(neighbour);
                    }

                }
            }


        }
    }

    public int GetDistance(Node2 node1, Node2 node2)
    {
        int distanceX = (int) Math.Abs(node1.position.x - node2.position.x);
        int distanceY = (int) Math.Abs(node1.position.y - node2.position.y);

        if (distanceX > distanceY)
        {
            return (14 * distanceY + 10 * (distanceX - distanceY));
        }
        return (14 * distanceX + 10 * (distanceY - distanceX));
        
    }
    // Update is called once per frame
    void Update()
    {
        FindPath(seeker.position, target.position);
    }
}
