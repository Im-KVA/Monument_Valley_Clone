using UnityEngine;

namespace KVA.SoundManager
{
    public class PlaySoundEnter : StateMachineBehaviour
    {
        [SerializeField] private SoundType _sound;
        [SerializeField, Range(0, 1)] private float _volume = 1;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SoundManager.PlaySound(_sound, null, _volume);
        }
    }
}
