using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> _objectsToMove;
    [SerializeField] private List<GameObject> _objectsToShow;
    [SerializeField] private List<Vector3> _targetPositions;
    [SerializeField] private List<Vector3> _targetRotations;
    [SerializeField] private float _punchScaleAmount = 0.2f;
    [SerializeField] private float _punchDuration = 0.3f;
    [SerializeField] private float _punchDelayBetweenObjects = 0.1f;
    [SerializeField] private float _moveDuration = 1.5f;
    [SerializeField] private float _moveDelay = 0.3f;
    [SerializeField] private float _snapEffectStrength = 0.1f;

    private Dictionary<GameObject, Vector3> _originalScales = new();
    private int _completedAnimations = 0;
    private bool _isStartMoving = false;

    private void Start()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            if (_objectsToMove[i] != null)
            {
                _originalScales[_objectsToMove[i]] = _objectsToMove[i].transform.localScale;
            }
        }
    }

    public void StartMoving()
    {
        if (_isStartMoving) return;

        CameraShaker.Instance?.Shake();

        StartCoroutine(PlayPunchEffects());
        _isStartMoving = true;
    }

    private IEnumerator PlayPunchEffects()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            GameObject obj = _objectsToMove[i];

            if (obj == null) continue;

            obj.transform.DOPunchScale(Vector3.one * _punchScaleAmount, _punchDuration, 8, 0.5f);

            yield return new WaitForSeconds(_punchDelayBetweenObjects);
        }

        yield return new WaitForSeconds(_punchDuration + _moveDelay);

        MoveObjectsToTarget();
    }

    private void MoveObjectsToTarget()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            GameObject obj = _objectsToMove[i];
            if (obj == null) continue;

            Vector3 targetPos = _targetPositions[i];
            Quaternion targetRot = Quaternion.Euler(_targetRotations[i]);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(obj.transform.DOMove(targetPos, _moveDuration).SetEase(Ease.InOutQuad))
                    .Join(obj.transform.DORotateQuaternion(targetRot, _moveDuration).SetEase(Ease.InOutQuad))
                    .Append(obj.transform.DOShakeScale(0.3f, _snapEffectStrength, 10, 90, false))
                    .OnComplete(() => CheckDestroy())
                    .Play();
        }
    }

    private void CheckDestroy()
    {
        _completedAnimations++;
        if (_completedAnimations >= _objectsToMove.Count)
        {
            foreach (GameObject objects in _objectsToShow)
            {
                if (_objectsToShow.Count == 0) break;
                objects.SetActive(true);
            }
            this.enabled = false;
        }
    }
}
