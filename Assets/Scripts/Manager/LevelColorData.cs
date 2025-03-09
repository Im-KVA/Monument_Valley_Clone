using UnityEngine;

[CreateAssetMenu(fileName = "LevelColorData", menuName = "Game/Level Color Data")]
public class LevelColorData : ScriptableObject
{
    [System.Serializable]
    public struct MaterialColor
    {
        public Material material;
        public Color color;
    }

    public MaterialColor[] materialColors;
}
