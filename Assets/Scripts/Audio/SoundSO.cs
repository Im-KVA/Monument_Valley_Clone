using UnityEngine;

namespace KVA.SoundManager
{
    [CreateAssetMenu(menuName = "SoundManager/SoundsSO", fileName = "SoundsSO")]
    public class SoundsSO : ScriptableObject
    {
        public SoundList[] sounds;
    }
}
