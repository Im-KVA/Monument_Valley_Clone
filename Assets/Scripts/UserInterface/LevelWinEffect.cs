using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using KVA.SoundManager;

public class LevelWinEffect : MonoBehaviour
{
    [SerializeField] private List<GameObject> _childObjectsToAnim;
    [SerializeField] private List<GameObject> _childObjectsToKill;
    [SerializeField] private float _minShakeStrength = 0.1f;
    [SerializeField] private float _maxShakeStrength = 0.3f;
    [SerializeField] private float _minShakeDuration = 0.2f;
    [SerializeField] private float _maxShakeDuration = 0.5f;
    [SerializeField] private float _minFallDelay = 0.2f;
    [SerializeField] private float _maxFallDelay = 1f;
    [SerializeField] private float _fallDistance = 1f;
    [SerializeField] private float _fallDuration = 0.5f;
    private float _lastSoundTime = 0f;
    private float _soundCooldown = 0.1f;

    private Dictionary<GameObject, Sequence> _activeSequences = new();

    private void OnDisable()
    {
        foreach (var seq in _activeSequences.Values)
        {
            seq.Kill();
        }
        _activeSequences.Clear();
    }

    public void PlayEffect()
    {
        foreach (GameObject child in _childObjectsToKill)
        {
            child.SetActive(false);
        }
        PlayAnim();
    }

    public void PlayAnim()
    {
        foreach (var child in _childObjectsToAnim)
        {
            if (child == null) continue;

            if (_activeSequences.TryGetValue(child, out Sequence existingSeq) && existingSeq.IsActive())
            {
                existingSeq.Kill();
            }

            child.SetActive(true);
            Vector3 originalPos = child.transform.position;
            Vector3 fallPos = originalPos + Vector3.down * _fallDistance;

            float shakeStrength = Random.Range(_minShakeStrength, _maxShakeStrength);
            float shakeTime = Random.Range(_minShakeDuration, _maxShakeDuration);
            float fallDelay = Random.Range(_minFallDelay, _maxFallDelay);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(child.transform.DOShakePosition(shakeTime, shakeStrength, 10, 90, false, true))
                    .AppendInterval(fallDelay)
                    .AppendCallback(() => {
                        if (Random.value < 0.5f)
                        {
                            if (Time.time - _lastSoundTime > _soundCooldown)
                            {
                                SoundManager.PlaySound(SoundType.BLOCKFALL);
                                _lastSoundTime = Time.time;
                            }
                        }
                    })
                    .Append(child.transform.DOMove(fallPos, _fallDuration).SetEase(Ease.InQuad))
                    .OnKill(() => child.SetActive(false))
                    .SetAutoKill(true);

            _activeSequences[child] = sequence;
            sequence.Play();
        }
    }

}
