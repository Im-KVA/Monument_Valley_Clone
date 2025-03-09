using UnityEngine;

[RequireComponent(typeof(Animator))]
// marker identifying mouse clicks 
public class Cursor : MonoBehaviour
{
    // extra distance offset toward camera
    [SerializeField] private float _offsetDistance = 1f;

    private Camera _cam;

    // cursor AnimationController
    private Animator _animController;

    private void Awake()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
        }
        _animController = GetComponent<Animator>();
    }

    // always look at camera
    void LateUpdate()
    {
        if (_cam != null)
        {
            Vector3 cameraForward = _cam.transform.rotation * Vector3.forward;
            Vector3 cameraUp = _cam.transform.rotation * Vector3.up;

            transform.LookAt(transform.position + cameraForward, cameraUp);
        }
    }

    // show cursor at a position with an optional offset toward camera
    public void ShowCursor(Vector3 position)
    {
        if (_cam != null && _animController != null)
        {
            Vector3 cameraForwardOffset = _cam.transform.rotation * new Vector3(0f, 0f, _offsetDistance);
            transform.position = position - cameraForwardOffset;

            _animController.SetTrigger("ClickTrigger");
        }
    }
}
