using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// management class for all Nodes
public class Graph : MonoBehaviour
{
    // all of the Nodes in the current level/maze
    private List<Node> _allNodes = new();

    // end of level
    [SerializeField] private Node _goalNode;
    public Node GoalNode => _goalNode;

    private void Awake()
    {
        _allNodes = FindObjectsOfType<Node>().ToList();
        InitNodes();
    }

    private void Start()
    {
        InitNeighbors();
    }

    // locate the specific Node at target position within rounding error
    public Node FindNodeAt(Vector3 pos)
    {
        foreach (Node n in _allNodes)
        {
            Vector3 diff = n.transform.position - pos;

            if (diff.sqrMagnitude < 0.01f)
            {
                return n;
            }
        }
        return null;
    }

    // locate the closest Node in screen space, given an array of Nodes
    public Node FindClosestNode(Node[] nodes, Vector3 pos)
    {
        Node closestNode = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Node n in nodes)
        {
            Vector3 diff = n.transform.position - pos;

            Vector3 nodeScreenPosition = Camera.main.WorldToScreenPoint(n.transform.position);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(pos);
            diff = nodeScreenPosition - screenPosition;

            if (diff.sqrMagnitude < closestDistanceSqr)
            {
                closestNode = n;
                closestDistanceSqr = diff.sqrMagnitude;
            }
        }
        return closestNode;
    }

    // find the closest Node in the entire Graph
    public Node FindClosestNode(Vector3 pos)
    {
        return FindClosestNode(_allNodes.ToArray(), pos);
    }

    // clear breadcrumb trail
    public void ResetNodes()
    {
        foreach (Node node in _allNodes)
        {
            node.PreviousNode = null;
        }
    }

    // set the Graph for each Node
    private void InitNodes()
    {
        foreach (Node n in _allNodes)
        {
            if (n != null)
            {
                n.InitGraph(this);
            }
        }
    }

    // set neighbors for each Node; must run AFTER all Nodes are initialized
    private void InitNeighbors()
    {
        foreach (Node n in _allNodes)
        {
            if (n != null)
            {
                n.FindNeighbors();
            }
        }
    }
}

