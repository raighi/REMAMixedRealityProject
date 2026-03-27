using UnityEngine;

[CreateAssetMenu(fileName = "NewTextSequence", menuName = "Text/Sequence")]
public class TextSequence : ScriptableObject
{
    [TextArea(4, 15)]
    public string[] messages;
}