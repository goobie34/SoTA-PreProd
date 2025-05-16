using UnityEngine;

[CreateAssetMenu(fileName = "New_dialouge", menuName = "Dialouge")]
public class SO_Dialogue : ScriptableObject
{
    [System.Serializable] public class Info
    {
        [TextArea(4, 8)] public string dialouge;
    }

    public Info[] dialogueInfo;

}

