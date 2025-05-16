using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractText : MonoBehaviour
{
    [SerializeField] private float hideDelay = 2f;
    [SerializeField] private GameObject interactObjectText;

    private float playerInteractionRange = 2f;
    private float timeSinceLeftRange;
    private bool isShowingText = false;

    public void Start()
    {
        playerInteractionRange = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteract>().InteractionRange;
    }

    private Collider InsideInteractRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, playerInteractionRange);

        foreach (Collider collider in hitColliders)
        {
            if(collider.CompareTag("Player"))
                return collider;
        }

        return null;
    }

    public void FixedUpdate()
    {
        Collider player = InsideInteractRange();

        if (player != null)
        {
            if (!isShowingText)
            {
                ShowInteractText();
            }
            timeSinceLeftRange = 0f;
        }
        else
        {
            if (isShowingText)
            {
                timeSinceLeftRange += Time.fixedDeltaTime;
                if (timeSinceLeftRange >= hideDelay)
                {
                    HideInteractText();
                }
            }
        }
    }

    private void ShowInteractText()
    {
        interactObjectText.gameObject.SetActive(true);
        isShowingText = true;
    }

    private void HideInteractText()
    {
        interactObjectText.gameObject.SetActive(false);
        isShowingText = false;
    }
}
