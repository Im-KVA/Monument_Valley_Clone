using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Node : MonoBehaviour
{
    // gizmo colors
    [SerializeField] private float _gizmoRadius = 0.1f;
    [SerializeField] private Color _defaultGizmoColor = Color.black;
    [SerializeField] private Color _selectedGizmoColor = Color.blue;
    [SerializeField] private Color _inactiveGizmoColor = Color.gray;

    // neighboring nodes + active state
    [SerializeField] private List<Edge> _edges = new();

    // Nodes specifically excluded from Edges
    [SerializeField] private List<Node> _excludedNodes;

    // reference to the graph
    private Graph _graph;

    // previous Node that forms a "breadcrumb" trail back to the start
    private Node _previousNode;

    // invoked when Player enters this node
    public UnityEvent gameEvent;

    // properties

    public Node PreviousNode { get { return _previousNode; } set { _previousNode = value; } }
    public List<Edge> Edges => _edges;

    // 3d compass directions to check for horizontal neighbors automatically(east/west/north/south)
    public static Vector3[] NeighborDirections =
    {
            new(1f, 0f, 0f),
            new(-1f, 0f, 0f),
            new(0f, 0f, 1f),
            new(0f, 0f, -1f),
        };

    private void Start()
    {
        // automatic connect Edges with horizontal Nodes
        if (_graph != null)
        {
            FindNeighbors();
        }
    }

    // draws a sphere gizmo
    private void OnDrawGizmos()
    {
        Gizmos.color = _defaultGizmoColor;
        Gizmos.DrawSphere(transform.position, _gizmoRadius);
    }

    // draws a sphere gizmo in a different color when selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _selectedGizmoColor;
        Gizmos.DrawSphere(transform.position, _gizmoRadius);

        // draws a line to each neighbor
        foreach (Edge e in _edges)
        {
            if (e.neighbor != null)
            {
                Gizmos.color = (e.isActive) ? _selectedGizmoColor : _inactiveGizmoColor;
                Gizmos.DrawLine(transform.position, e.neighbor.transform.position);
            }
        }
    }

    // fill out edge connections to neighboring nodes automatically
    public void FindNeighbors()
    {
        // search through possible neighbor offsets
        foreach (Vector3 direction in NeighborDirections)
        {
            Node newNode = _graph?.FindNodeAt(transform.position + direction);

            // add to edges list if not already included and not excluded specifically
            if (newNode != null && !HasNeighbor(newNode) && !_excludedNodes.Contains(newNode))
            {
                Edge newEdge = new Edge { neighbor = newNode, isActive = true };
                _edges.Add(newEdge);
            }
        }
    }

    // is a Node already in the Edges List?
    private bool HasNeighbor(Node node)
    {
        foreach (Edge e in _edges)
        {
            if (e.neighbor != null && e.neighbor.Equals(node))
            {
                return true;
            }
        }
        return false;
    }

    // given a specific neighbor, sets active state
    public void EnableEdge(Node neighborNode, bool state)
    {
        foreach (Edge e in _edges)
        {
            if (e.neighbor.Equals(neighborNode))
            {
                e.isActive = state;
            }
        }
    }

    public void InitGraph(Graph graphToInit)
    {
        this._graph = graphToInit;
    }
}
