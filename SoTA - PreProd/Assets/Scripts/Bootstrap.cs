using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public GameObject[] persistentPrefabs; 
    public string firstSceneName = "MainMenu";

    void Awake()
    {
        foreach (var prefab in persistentPrefabs)
        {
            GameObject obj = Instantiate(prefab);
            DontDestroyOnLoad(obj);
        }

        SceneManager.LoadScene(firstSceneName);
    }
}
