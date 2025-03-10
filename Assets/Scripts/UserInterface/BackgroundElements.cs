using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using KVA.SoundManager;

public class BackgroundElements : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private float _moveDistance = 5f;
    [SerializeField] private float _moveDuration = 3f;
    [SerializeField] private float _teleportDelay = 1f;
    private Tweener _rotationTween;
    private Sequence _movementSequence;
    private Vector3 _startPosition;

    private void OnEnable()
    {
        _startPosition = transform.position;
        StartRotationAndMoving();
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private void OnDestroy()
    {
        StopMovement();
    }

    private void StartRotationAndMoving()
    {
        if (_rotationTween != null && _rotationTween.IsActive()) return;
        if (_movementSequence != null && _movementSequence.IsActive()) return;

        _rotationTween = transform.DORotate(_rotationAxis * 360f, _rotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);


        _movementSequence = DOTween.Sequence();
        _movementSequence.Append(transform.DOMoveZ(_startPosition.z + _moveDistance, _moveDuration).SetEase(Ease.InOutSine));
        _movementSequence.AppendInterval(_teleportDelay);
        _movementSequence.AppendCallback(() => {
            transform.position = _startPosition;
        });

        _movementSequence.SetLoops(-1, LoopType.Restart);
    }

    private void StopMovement()
    {
        if (_rotationTween != null)
        {
            _rotationTween.Kill();
            _rotationTween = null;
        }

        if (_movementSequence != null)
        {
            _movementSequence.Kill();
            _movementSequence = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DoPunch();
    }

    private void DoPunch()
    {
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 0.5f);
        SoundManager.PlaySound(SoundType.PLATECHANGEBLOCK);
    }
}
