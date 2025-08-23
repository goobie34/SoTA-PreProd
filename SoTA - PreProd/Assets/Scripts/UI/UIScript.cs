using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Author: Sixten
/// 
/// Modified by: Gabbriel
/// 
/// </summary>

public class UIScript : MonoBehaviour
{
    // TWEAKABLE VARIABLES -- Unity editor
    [Header("Start Menu Items")]
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject mainMenuStartObject;
    [SerializeField] private GameObject mainMenuList;

    [Header("End Menu Items")]
    [SerializeField] private GameObject endMenuObject;
    [SerializeField] private GameObject endMenuStartObject;
    [SerializeField] private GameObject endMenuList;

    [Header("Pause Menu Items")]
    [SerializeField] private GameObject pauseMenuObject;
    [SerializeField] private GameObject pauseMenuStartObject;
    [SerializeField] private GameObject menuList;

    [Header("Other Menus Items")]
    [SerializeField] private GameObject HUD;

    [Header("First Selected Items")]
    [SerializeField] private GameObject mainMenuSelection;
    [SerializeField] private GameObject endMenuSelection;
    [SerializeField] private GameObject pauseMenuSelection;


    // STORING/VALUE VARIABLES
    private bool isPaused;
    private GameObject playerObject;

    private bool inStartScene = false;
    private bool inEndScene = false;
    private static bool isUsingController = false;
    public static bool IsUsingController { get { return isUsingController; } }

    // ENGINE METHODS ====================================== // 
    public static UIScript Instance { get; private set; }

    private void Awake() // we only want 1 UI object in the scene
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Destroying duplicate UIScript");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() // Get the player object @ start
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update() // Inefficient but works. 
    {
        if (playerObject == null) // Try and find the player object again
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            var playerInput = playerObject.GetComponent<PlayerInput>(); // player input component needs to be stored in it's own variable for some reason

            if (playerInput != null && playerInput.isActiveAndEnabled) // fix null reference exception
            {
                if (isUsingController && playerInput.currentActionMap.name != "PlayerControlController")
                {
                    playerInput.SwitchCurrentActionMap("PlayerControlController");
                }
                else if (!isUsingController && playerInput.currentActionMap.name != "New action map")
                {
                    playerInput.SwitchCurrentActionMap("New action map");
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            inStartScene = true;
            inEndScene = false;

            mainMenuObject.SetActive(true);

            HUD.SetActive(false);
            endMenuObject.SetActive(false);
            pauseMenuObject.SetActive(false);

            if (EventSystem.current.firstSelectedGameObject != mainMenuSelection)
            {
                Focus(mainMenuSelection);
            }

        }
        else if (SceneManager.GetActiveScene().name == "EndScene")
        {
            inStartScene = false;
            inEndScene = true;

            endMenuObject.SetActive(true);

            HUD.SetActive(false);
            mainMenuObject.SetActive(false);
            pauseMenuObject.SetActive(false);

            if (EventSystem.current.firstSelectedGameObject != endMenuSelection)
            {
                Focus(endMenuSelection);
            }

        }
        else // In game/levels
        {
            inStartScene = false;
            inEndScene = false;

            HUD.SetActive(true);

            mainMenuObject.SetActive(false);
            endMenuObject.SetActive(false);

            if (EventSystem.current.firstSelectedGameObject != pauseMenuSelection)
            {
                Focus(pauseMenuSelection);
            }
        }
    }

    private void OnPauseGame(InputValue value)
    {
        if ((!inStartScene && !inEndScene))
        {
            if (DialogueManager.InADialogue)
            {
                DialogueManager.QuitTalking = true;
                UnPauseGame();
            }
            else
            {
                if (isPaused) UnPauseGame();
                else PauseGame();
            }
        }
    }

    // METHODS ====================================== //
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void UnPauseGame() // This method is from a tutorial workshop but with modifications to work with our game
    {
        isPaused = false;
        Time.timeScale = 1;
        playerObject.GetComponent<PlayerInput>().enabled = true;
        pauseMenuObject.SetActive(false);
        ResetPauseUI();
    }

    public void LoadLevel(string levelName) // This method is from a tutorial workshop but with modifications to work with our game
    {
        if (isPaused)
            UnPauseGame();
        SceneManager.LoadScene(levelName);
    }

    private void PauseGame() // This method is from a tutorial workshop but with modifications to work with our game
    {
        isPaused = true;
        Time.timeScale = 0;
        playerObject.GetComponent<PlayerInput>().enabled = false;
        pauseMenuObject.SetActive(true);
    }

    private void ResetPauseUI() // This method is from a tutorial workshop but with modifications to work with our game
    {
        for (int i = 0; i < menuList.transform.childCount; i++)
        {
            menuList.transform.GetChild(i).gameObject.SetActive(false);
        }
        pauseMenuStartObject.SetActive(true);
    }

    // Disabled/not used due to it being buggy...
    public void Focus(GameObject objectToFocus) // Pass in button element to focus on
    {
        if (objectToFocus == null)
        {
            Debug.LogWarning("UISelector: Object to focus is null!");
            return;
        }

        StartCoroutine(FocusNextFrame(objectToFocus));
    }

    private IEnumerator FocusNextFrame(GameObject objectToFocus) 
    {
        yield return null; 

        while (objectToFocus == null || !objectToFocus.activeInHierarchy)
            yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objectToFocus); // Now we finally focus on the object in the menu
    }

    public void IsOnController()
    {
        isUsingController = !isUsingController;
    }

}
