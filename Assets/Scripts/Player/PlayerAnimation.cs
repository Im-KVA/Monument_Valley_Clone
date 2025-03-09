using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    // multiplier for walk AnimationClip
    [Range(0.5f, 3f)]
    [SerializeField] private float _walkAnimSpeed = 1f;

    // player Animator component
    [SerializeField] private Animator _animator;


    void Start()
    {
        if (_animator != null)
        {
            // set AnimationClip speed
            _animator.SetFloat("walkSpeedMultiplier", _walkAnimSpeed);
        }
    }

    //  toggle between idle and walking animation
    public void ToggleAnimation(bool state)
    {
        if (_animator != null)
        {
            _animator?.SetBool("isMoving", state);
        }

    }
}
