using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
using KVA.SoundManager;

// monitors win condition and stops/pauses gameplay as needed
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // reference to player's controller component
    [SerializeField] private PlayerController _playerController;

    // have we completed the win condition (i.e. reached the goal)?
    private bool _isGameOver;
    public bool IsGameOver => _isGameOver;

    // delay before restarting, etc.
    public float delayTime = 2f;

    // invoked on awake
    public UnityEvent awakeEvent;

    // invoked when starting the level
    public UnityEvent initEvent;

    // invoked before ending the level
    public UnityEvent endLevelEvent;


    private void Awake()
    {
        awakeEvent.Invoke();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        DOTween.SetTweensCapacity(500, 250);
    }

    // check for win condition every frame
    private void Update()
    {
        if (_playerController != null && _playerController.HasReachedGoal())
        {
            Win();
        }
    }

    public void SetPlayer(PlayerController pc)
    {
        _playerController = pc;
    }

    // win and end the level
    private void Win()
    {
        // flag to ensure Win only triggers once
        if (_isGameOver || _playerController == null)
        {
            return;
        }
        _isGameOver = true;

        SoundManager.PlaySound(SoundType.WIN);

        // disable player controls
        _playerController.EnableControls(false);

        // play win animation
        StartCoroutine(WinRoutine());
        LevelManager.Instance.OnWinLoadLevelWinEffect();
    }

    // invoke end level event and wait
    private IEnumerator WinRoutine()
    {
        endLevelEvent?.Invoke();

        // yield Animation time
        yield return new WaitForSeconds(delayTime);
    }

    // restart the scene
    public void Restart(float delay)
    {
        SoundManager.PlaySound(SoundType.CLICKBUTTON);
        StartCoroutine(RestartRoutine(delay));
    }

    public void NextLevel(float delay)
    {
        SoundManager.PlaySound(SoundType.CLICKBUTTON);
        StartCoroutine(LoadNextLevelRoutine(delay));
    }

    private IEnumerator LoadNextLevelRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        LevelManager.Instance.LoadNextLevel();
        _isGameOver = false;
    }

    // wait for a delay and restart the scene
    private IEnumerator RestartRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);

        yield return new WaitForSeconds(delay);

        int level = LevelManager.Instance.CurrentLevel;
        LevelManager.Instance.LoadLevel(level);
        _isGameOver = false;
    }
}

