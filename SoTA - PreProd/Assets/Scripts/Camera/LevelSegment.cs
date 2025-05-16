using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSegment : MonoBehaviour
{
    [SerializeField] Vector2 segmentPosition;
    PlayerSegment playerSegment;

    [SerializeField] bool bgMusicEnabled = true; //to allow music to fade out on final segment of level

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerSegment = other.GetComponent<PlayerSegment>();
            playerSegment.AddSegment(this);

            AudioManager.Instance.SetBgMusicState(bgMusicEnabled);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerSegment = other.GetComponent<PlayerSegment>();
            playerSegment.RemoveSegment(this);
        }
    }
    public Vector3 GetSegmentPosition()
    {
        return new Vector3(segmentPosition.x, 0, segmentPosition.y)*11;
    }
}
