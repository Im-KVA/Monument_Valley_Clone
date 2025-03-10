using UnityEngine;

namespace KVA.SoundManager
{
    public class PlaySoundExit : StateMachineBehaviour
    {
        [SerializeField] private SoundType _sound;
        [SerializeField, Range(0, 1)] private float _volume = 1;
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SoundManager.PlaySound(_sound, null, _volume);
        }
    }
}
