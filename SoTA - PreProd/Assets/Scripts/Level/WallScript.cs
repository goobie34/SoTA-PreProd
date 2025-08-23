using TMPro;
using UnityEngine;
/// <summary>
/// Author:Gabbriel, Karin
/// 
/// Modified by:
/// 
/// </summary>
public class WallScript : MonoBehaviour, IActivatable
{
    Vector3 defaultPosition;    //position when NOT activated
    Vector3 activatedPosition;  //position activated (is fetched from a child object)

    public float DistanceToTravel { get; private set; } //set in start
    
    bool isActive = false;

    float moveDuration = 1.0f;

    void Start()
    {
        defaultPosition = transform.position;

        Transform activatedTransform = transform.Find("ActivatedPosition");

        if (transform.childCount <= 0)
        {
            Debug.Log("Error. Wall needs child to indicate its activated position!");
        } else
        {
            activatedPosition = activatedTransform.position;
        }

        DistanceToTravel = Vector3.Distance(defaultPosition, activatedPosition);
    }

    public void Activate()
    {
        if (!isActive)
        {
            StopAllCoroutines();
            StartCoroutine(MoveToPosition(activatedPosition));
            isActive = true;
        }
    }

    public void Deactivate()
    {
        if(isActive)
        {
            StopAllCoroutines();
            StartCoroutine(MoveToPosition(defaultPosition));
            isActive = false;
        }
    }

    System.Collections.IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}
