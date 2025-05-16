using System.Runtime.CompilerServices;
using UnityEngine;

public class ObjectDialougeScript : MonoBehaviour, IInteractable
{
    [field: Header("Default convo (assumes player using keyboard)")]
    [SerializeField] SO_Dialogue convo;

    [field: Header("Controller specific convo")]
    [SerializeField] bool dependsOnInputMethod;
    [SerializeField] SO_Dialogue convoController; //this convo is only used if dependsOnInputMethod == true

    public void Interact()
    {
        //if convo has no controller specific content
        if (!dependsOnInputMethod)
        {
            DialogueManager.Instance.Queue(convo);
            return;
        }

        //if we get here, convo has controller specific content
        if (UIScript.IsUsingController)
        {
            //we are using controller
            DialogueManager.Instance.Queue(convoController);
        }
        else
        {
            //we are using keyboard
            DialogueManager.Instance.Queue(convo);
        }
    }
}
