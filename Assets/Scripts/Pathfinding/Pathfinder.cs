using System.Collections.Generic;
using UnityEngine;

// generates a path through a Graph
[RequireComponent(typeof(Graph))]
public class Pathfinder : MonoBehaviour
{

    // path start Node (usually current Node of the Player)
    [SerializeField] private Node _startNode;

    // path end Node
    [SerializeField] private Node _destinationNode;
    [SerializeField] private bool _searchOnStart;

    // next Nodes to explore
    private List<Node> _frontierNodes;

    // Nodes already explored
    private List<Node> _exploredNodes;

    // Nodes that form a path to the goal Node (for Gizmo drawing)
    private List<Node> _pathNodes;

    // is the search complete?
    private bool _isSearchComplete;

    // has the destination been found?
    private bool _isPathComplete;

    // structure containing all Nodes
    private Graph _graph;

    // properties
    public Node StartNode { get { return _startNode; } set { _startNode = value; } }
    public Node DestinationNode { get { return _destinationNode; } set { _destinationNode = value; } }
    public List<Node> PathNodes => _pathNodes;
    public bool IsPathComplete => _isPathComplete;
    public bool SearchOnStart => _searchOnStart;

    private void Awake()
    {
        _graph = GetComponent<Graph>();
    }

    private void Start()
    {
        if (_searchOnStart)
        {
            _pathNodes = FindPath();
        }
    }

    // initialize all Nodes/lists
    private void InitGraph()
    {
        // validate required components
        if (_graph == null || _startNode == null || _destinationNode == null)
        {
            return;
        }

        _frontierNodes = new List<Node>();
        _exploredNodes = new List<Node>();
        _pathNodes = new List<Node>();

        _isSearchComplete = false;
        _isPathComplete = false;

        // remove results of previous searches
        _graph.ResetNodes();

        // first Node
        _frontierNodes.Add(_startNode);
    }

    // use a simple Breadth-first Search to explore one iteration
    private void ExpandFrontier(Node node)
    {
        // validate Node
        if (node == null)
        {
            return;
        }

        // loop through all Edges
        for (int i = 0; i < node.Edges.Count; i++)
        {
            // skip Edge if neighbor already explored or invalid
            if (node.Edges[i] == null ||
                node.Edges.Count == 0 ||
                _exploredNodes.Contains(node.Edges[i].neighbor) ||
                _frontierNodes.Contains(node.Edges[i].neighbor))
            {
                continue;
            }

            // create PreviousNode breadcrumb trail if Edge is active
            if (node.Edges[i].isActive && node.Edges[i].neighbor != null)
            {
                node.Edges[i].neighbor.PreviousNode = node;

                // add neighbor Nodes to frontier Nodes
                _frontierNodes.Add(node.Edges[i].neighbor);
            }
        }
    }

    // set the PathNodes from the startNode to destinationNode
    public List<Node> FindPath()
    {
        List<Node> newPath = new List<Node>();

        if (_startNode == null || _destinationNode == null || _startNode == _destinationNode)
        {
            return newPath;
        }

        // prevents infinite loop
        const int MAXITERATIONS = 100;
        int iterations = 0;

        // initialize all Nodes
        InitGraph();

        // search the graph until goal is found or all nodes explored (or exceeding some limit)
        while (!_isSearchComplete && _frontierNodes != null && iterations < MAXITERATIONS)
        {
            iterations++;

            // if we still have frontier Nodes to check
            if (_frontierNodes.Count > 0)
            {
                // remove the first Node
                Node currentNode = _frontierNodes[0];
                _frontierNodes.RemoveAt(0);

                // and add to the exploredNodes
                if (!_exploredNodes.Contains(currentNode))
                {
                    _exploredNodes.Add(currentNode);
                }

                // add unexplored neighboring Nodes to frontier
                ExpandFrontier(currentNode);

                // if we have found the destination Node
                if (_frontierNodes.Contains(_destinationNode))
                {
                    // generate the Path to the goal
                    newPath = GetPathNodes();
                    _isSearchComplete = true;
                    _isPathComplete = true;
                }
            }
            // if whole graph explored but no path found
            else
            {
                _isSearchComplete = true;
                _isPathComplete = false;
            }
        }
        return newPath;
    }

    public List<Node> FindPath(Node start, Node destination)
    {
        this._destinationNode = destination;
        this._startNode = start;
        return FindPath();
    }

    // find the best path given a bunch of possible Node destinations
    public List<Node> FindBestPath(Node start, Node[] possibleDestinations)
    {
        List<Node> bestPath = new List<Node>();
        foreach (Node n in possibleDestinations)
        {
            List<Node> possiblePath = FindPath(start, n);

            if (!_isPathComplete && _isSearchComplete)
            {
                continue;
            }

            if (bestPath.Count == 0 && possiblePath.Count > 0)
            {
                bestPath = possiblePath;
            }

            if (bestPath.Count > 0 && possiblePath.Count < bestPath.Count)
            {
                bestPath = possiblePath;
            }
        }

        if (bestPath.Count <= 1)
        {
            ClearPath();
            return new List<Node>();
        }

        _destinationNode = bestPath[bestPath.Count - 1];
        _pathNodes = bestPath;
        return bestPath;
    }

    public void ClearPath()
    {
        _startNode = null;
        _destinationNode = null;
        _pathNodes = new List<Node>();
    }

    // given a goal node, follow PreviousNode breadcrumbs to create a path
    public List<Node> GetPathNodes()
    {
        // create a new list of Nodes
        List<Node> path = new List<Node>();

        // start with the goal Node
        if (_destinationNode == null)
        {
            return path;
        }
        path.Add(_destinationNode);

        // follow the breadcrumb trail, creating a path until it ends
        Node currentNode = _destinationNode.PreviousNode;

        while (currentNode != null)
        {
            path.Insert(0, currentNode);
            currentNode = currentNode.PreviousNode;
        }
        return path;
    }

    private void OnDrawGizmos()
    {
        if (_isSearchComplete)
        {
            foreach (Node node in _pathNodes)
            {

                if (node == _startNode)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
                }
                else if (node == _destinationNode)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(node.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(node.transform.position, 0.15f);
                }

                Gizmos.color = Color.yellow;
                if (node.PreviousNode != null)
                {
                    Gizmos.DrawLine(node.transform.position, node.PreviousNode.transform.position);
                }
            }
        }
    }

    public void SetStartNode(Vector3 position)
    {
        StartNode = _graph.FindClosestNode(position);
    }
}
