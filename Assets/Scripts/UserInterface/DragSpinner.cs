using KVA.SoundManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// allows a target Transform to be rotated based on mouse click and drag
[RequireComponent(typeof(Collider))]
public class DragSpinner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SpinAxis
    {
        X,
        Y,
        Z
    }

    // transform to spin
    [SerializeField] private Transform _targetToSpin;

    // axis of rotation
    [SerializeField] private SpinAxis _spinAxis = SpinAxis.X;

    // used to calculate angle to mouse pointer
    [SerializeField] private Transform _pivot;

    // minimum distance in pixels before activating mouse drag
    [SerializeField] private int _minDragDist = 10;

    //[SerializeField] private Linker linker;

    // vector from pivot to mouse pointer
    private Vector2 _directionToMouse;

    // are we currently spinning?
    private bool _isSpinning;

    private bool _isActive;

    // angle (degrees) from clicked screen position 
    private float _angleToMouse;

    // angle to mouse on previous frame
    private float _previousAngleToMouse;

    // Vector representing axis of rotation
    private Vector3 _axisDirection;

    public UnityEvent snapEvent;
    public UnityEvent onSpinnerDragged;

    void Start()
    {
        switch (_spinAxis)
        {
            case SpinAxis.X:
                _axisDirection = Vector3.right;
                break;
            case SpinAxis.Y:
                _axisDirection = Vector3.up;
                break;
            case SpinAxis.Z:
                _axisDirection = Vector3.forward;
                break;
        }
        EnableSpinner(true);
    }

    // begin spin drag
    public void OnBeginDrag(PointerEventData data)
    {
        if (!_isActive || GameManager.Instance.IsGameOver)
        {
            return;
        }

        SoundManager.PlaySound(SoundType.BEGINDRAG);

        onSpinnerDragged?.Invoke();
        _isSpinning = true;

        // get the angle to the mouse position on down frame
        Vector3 inputPosition = new Vector3(data.position.x, data.position.y, 0f);
        _directionToMouse = inputPosition - Camera.main.WorldToScreenPoint(_pivot.position);

        // store the angle to mouse pointer on down frame
        _previousAngleToMouse = Mathf.Atan2(_directionToMouse.y, _directionToMouse.x) * Mathf.Rad2Deg;

    }

    // end spin on mouse release; then round to right angle
    public void OnEndDrag(PointerEventData data)
    {
        if (GameManager.Instance.IsGameOver) return;

        onSpinnerDragged?.Invoke();
        if (_isActive)
        {
            SoundManager.PlaySound(SoundType.ENDDRAG);
            SnapSpinner();
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (GameManager.Instance.IsGameOver) return;

        if (_isSpinning && Camera.main != null && _pivot != null && _isActive)
        {
            // get the angle to the current mouse position
            Vector3 inputPosition = new Vector3(data.position.x, data.position.y, 0f);
            _directionToMouse = inputPosition - Camera.main.WorldToScreenPoint(_pivot.position);
            _angleToMouse = Mathf.Atan2(_directionToMouse.y, _directionToMouse.x) * Mathf.Rad2Deg;

            // if we have dragged a minimum threshold, rotate the target to follow the mouse movements around the pivot
            // (left-handed coordinate system; positive rotations are clockwise)
            if (_directionToMouse.magnitude > _minDragDist)
            {
                Vector3 newRotationVector = (_previousAngleToMouse - _angleToMouse) * _axisDirection;
                _targetToSpin.Rotate(newRotationVector);
                _previousAngleToMouse = _angleToMouse;
            }
        }
    }

    // release and snap to 90-degrees interval
    private void SnapSpinner()
    {
        _isSpinning = false;

        // snap to nearest 90-degree interval
        RoundToRightAngles(_targetToSpin);

        // invoke event (e.g. to update the SpinnerControl)
        if (snapEvent != null)
        {
            snapEvent.Invoke();
        }
    }

    // round to nearest 90 degrees
    private void RoundToRightAngles(Transform xform)
    {
        float roundedXAngle = Mathf.Round(xform.eulerAngles.x / 90f) * 90f;
        float roundedYAngle = Mathf.Round(xform.eulerAngles.y / 90f) * 90f;
        float roundedZAngle = Mathf.Round(xform.eulerAngles.z / 90f) * 90f;

        xform.eulerAngles = new Vector3(roundedXAngle, roundedYAngle, roundedZAngle);
    }

    //enable/disable
    public void EnableSpinner(bool state)
    {
        _isActive = state;

        // force snap the spinner on disable
        if (!_isActive)
        {
            SnapSpinner();
        }
    }
}
