using UnityEngine;

public class OnPlatePress : MonoBehaviour
{
    public Node nodeA;
    public Node nodeB;

    public void EnableLink(bool state)
    {
        if (nodeA == null || nodeB == null)
            return;

        nodeA.EnableEdge(nodeB, state);
        nodeB.EnableEdge(nodeA, state);
    }

}