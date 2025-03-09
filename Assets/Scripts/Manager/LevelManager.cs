using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject _currentLevelInstance;
    [SerializeField] private int _currentLevelIndex;
    public int CurrentLevel => _currentLevelIndex;

    [SerializeField] private List<Material> _materialsToModify;
    [SerializeField] private List<GameObject> _levelPrefabs;
    [SerializeField] private List<LevelColorData> _levelColorData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _currentLevelIndex = 0;
        LoadLevel(_currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        _currentLevelIndex++;
        if (_currentLevelIndex >= _levelPrefabs.Count)
        {
            _currentLevelIndex = 0;
        }
        LoadLevel(_currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        // invoke any events at the start of gameplay
        GameManager.Instance.initEvent.Invoke();

        if (_currentLevelInstance != null)
        {
            KillLevel();
        }

        if (levelIndex < 0 || levelIndex >= _levelPrefabs.Count)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        _currentLevelInstance = Instantiate(_levelPrefabs[levelIndex]);

        if (levelIndex < _levelColorData.Count)
        {
            ApplyColorData(_levelColorData[levelIndex]);
        }

        // get a reference to the player
        PlayerController playerController = FindObjectOfType<PlayerController>();
        GameManager.Instance.SetPlayer(playerController);
    }

    public void KillLevel()
    {
        if (_currentLevelInstance != null)
        {
            Destroy(_currentLevelInstance);
            _currentLevelInstance = null;
        }
    }

    public void OnWinLoadLevelWinEffect()
    {
        if (_currentLevelInstance != null)
        {
            _currentLevelInstance.GetComponent<LevelWinEffect>().PlayEffect();
        }
    }

    private void ApplyColorData(LevelColorData colorData)
    {
        foreach (var entry in colorData.materialColors)
        {
            if (entry.material != null)
            {
                entry.material.color = entry.color;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Save Material Colors")]
    private void SaveMaterialColors()
    {
        if (_levelColorData.Count == 0 || _currentLevelIndex >= _levelColorData.Count)
        {
            Debug.LogWarning("Invalid LevelColorData assignment!");
            return;
        }

        LevelColorData currentColorData = _levelColorData[_currentLevelIndex];
        List<LevelColorData.MaterialColor> materialColors = new();

        foreach (var material in _materialsToModify)
        {
            if (material != null)
            {
                materialColors.Add(new LevelColorData.MaterialColor {
                    material = material,
                    color = material.color
                });
            }
        }

        currentColorData.materialColors = materialColors.ToArray();
        UnityEditor.EditorUtility.SetDirty(currentColorData);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log($"Material colors saved for Level {_currentLevelIndex + 1}!");
    }
#endif

}
