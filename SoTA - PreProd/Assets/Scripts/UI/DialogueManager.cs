using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Author: Sixten
/// 
/// Modified by:
/// 
/// </summary>

public class DialogueManager : MonoBehaviour
{
    //IIRC this whole source file is from the tutorial but with minor (if any) changes
    // Written by myself though

    // EXPOSED VARIABLES
    public static DialogueManager Instance { get; private set; }
    public static bool InADialogue { 
        get{
            return inADialogue;
        }
    }

    public static bool QuitTalking;

    // STORING/VALUE VARIABLES
    private bool typingLetters;
    private static bool inADialogue;
    private string completedDialogue;
    private Queue<SO_Dialogue.Info> dialogueQueue;
    [SerializeField] private PlayerHealth playerHealth;

    // TWEAKABLE VARIABLES
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float textDelay = 0.1f;

    // ENGINE METHODS ====================================== // 
    void Awake() // Singleton moment
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        dialogueQueue = new Queue<SO_Dialogue.Info>();

        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();

    }

    private void Update()
    {

        if (dialogueText == null)
        {
            dialogueText = FinderHelper.FindInactiveByTag("DialogueText")?.GetComponent<TMP_Text>();
        }

        if (dialogueBox == null)
        {
            dialogueBox = FinderHelper.FindInactiveByTag("DialogueBox");
        }

        if (QuitTalking)
        {
            EndDialogue();
            QuitTalking = false;
        }
    }

    void OnInteract(InputValue value)
    {
        if (inADialogue)
        {
            DeQueue();
        }
    }

    // METHODS ====================================== //
    public IEnumerator TypeText(SO_Dialogue.Info info)
    {
        completedDialogue = info.dialouge;

        typingLetters = true;
        
        for (int i = 0; i < info.dialouge.ToCharArray().Length; i++)
        {
            yield return new WaitForSeconds(textDelay);
            dialogueText.text += info.dialouge.ToCharArray()[i];
        }
        
        typingLetters = false;
    }

    public void EndDialogue()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = true;
        dialogueBox.SetActive(false);
        inADialogue = false;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LoreTileCloseSFX, Vector3.zero);
        //To stop player dying while reading lore tile
        playerHealth.ResumeHealthDrain();
    }

    public void CompleteText()
    {
        dialogueText.text = completedDialogue;
    }

    public void Queue(SO_Dialogue dialogue)
    {
        if (inADialogue)
        {
            return;
        }

        //To stop player dying while reading lore tile
        playerHealth.PauseHealthDrain();

        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = false;
        dialogueBox.SetActive(true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LoreTileOpenSFX, Vector3.zero);
        inADialogue = true;
        dialogueQueue.Clear();
        
        foreach (SO_Dialogue.Info line in dialogue.dialogueInfo)
        {
            dialogueQueue.Enqueue(line);
        }
        
        DeQueue();
    }

    void DeQueue()
    {
        if (typingLetters)
        {
            CompleteText();
            StopAllCoroutines();
            typingLetters = false;
            return;
        }
        
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        SO_Dialogue.Info info = dialogueQueue.Dequeue();
        completedDialogue = info.dialouge;
        dialogueText.text = "";
        StartCoroutine(TypeText(info));
    }


}
