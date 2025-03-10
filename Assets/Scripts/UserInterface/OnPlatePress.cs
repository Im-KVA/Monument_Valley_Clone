using KVA.SoundManager;
using UnityEngine;

public class OnPlatePress : MonoBehaviour
{
    public Node nodeA;
    public Node nodeB;
    private bool _isPlayingSound = false;

    public void EnableLink(bool state)
    {
        if (nodeA == null || nodeB == null)
            return;

        nodeA.EnableEdge(nodeB, state);
        nodeB.EnableEdge(nodeA, state);

        if (_isPlayingSound) return;
        SoundManager.PlaySound(SoundType.PRESSPLATE);
        _isPlayingSound = true;
    }

}