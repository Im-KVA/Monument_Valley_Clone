using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [SerializeField] private float _duration = 0.2f;
    [SerializeField] private float _magnitude = 0.1f;
    private Vector3 _originalPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _originalPosition = transform.localPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(_duration, _magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float xOffset = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * magnitude;
            float yOffset = (Mathf.PerlinNoise(0f, Time.time * 10f) - 0.5f) * magnitude;

            transform.localPosition = _originalPosition + new Vector3(xOffset, yOffset, 0f);
            yield return null;
        }

        transform.localPosition = _originalPosition;
    }
}
