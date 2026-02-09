using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/NPCDialogueData")]
public class NPCDialogueData : ScriptableObject
{
    public string npcName = "NPC";
    [TextArea(2, 5)]
    public string[] greetingLines;
    [TextArea(2, 5)]
    public string[] farewellLines;
}
