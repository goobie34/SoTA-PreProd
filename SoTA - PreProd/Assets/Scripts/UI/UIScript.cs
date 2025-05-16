using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{
    // TWEAKABLE VARIABLES

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

    // STORING/VALUE VARIABLES
    private bool isPaused;
    private GameObject playerObject;

    private bool inStartScene = false;
    private bool inEndScene = false;
    private static bool isUsingController = true; // Behövs endast 1 + static tar inte bort skiten lol
    public static bool IsUsingController { get { return isUsingController; } } //is used in lore tiles to check what dialogue to display

    // ENGINE METHODS ====================================== // 
    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");

        var playerInput = playerObject.GetComponent<PlayerInput>();

        if (playerInput.currentActionMap.name == "PlayerControlController")
        {
            isUsingController = true;
        }
        else
        {
            isUsingController = false;
        }
    }

    private void Update() // Inefficient but works
    {
        if(playerObject == null) // Try and find the player object again
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            var playerInput = playerObject.GetComponent<PlayerInput>();

            //In these two if statements, if we don't check playerInput.enabled then we will get null reference exceptions when trying to access currentActionMap.name, playerInput.enabled is set to false in UIScript and Lore Tile Script
            if (isUsingController && playerInput.enabled && playerInput.currentActionMap.name != "PlayerControlController")
            {
                playerInput.SwitchCurrentActionMap("PlayerControlController");
            }
            else if (!isUsingController && playerInput.enabled && playerInput.currentActionMap.name != "New action map")
            {
                playerInput.SwitchCurrentActionMap("New action map");
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
        }
        else if (SceneManager.GetActiveScene().name == "EndScene")
        {
            inStartScene = false;
            inEndScene = true;

            endMenuObject.SetActive(true);

            HUD.SetActive(false);
            mainMenuObject.SetActive(false);
            pauseMenuObject.SetActive(false);
        }
        else
        {
            inStartScene = false;
            inEndScene = false;

            HUD.SetActive(true);

            mainMenuObject.SetActive(false);
            endMenuObject.SetActive(false);
        }
    }


    private void OnPauseGame(InputValue value) 
    {
        if((!inStartScene && !inEndScene))
        {
            if (DialogueManager.InADialogue) // maybe do events here?
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

    public void UnPauseGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        playerObject.GetComponent<PlayerInput>().enabled = true;
        pauseMenuObject.SetActive(false);
        ResetPauseUI();
    }

    public void LoadLevel(string levelName)
    {
        if (isPaused)
            UnPauseGame();
        SceneManager.LoadScene(levelName);
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        playerObject.GetComponent<PlayerInput>().enabled = false;
        pauseMenuObject.SetActive(true);
    }

    private void ResetPauseUI()
    {
        for (int i = 0; i < menuList.transform.childCount; i++)
        {
            menuList.transform.GetChild(i).gameObject.SetActive(false);
        }
        pauseMenuStartObject.SetActive(true);
    }

    public void Focus(GameObject objectToFocus) // Pass in button element to focus on!
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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(objectToFocus);
    }

    public void IsOnController()
    {
        isUsingController = !isUsingController;
    }

}
