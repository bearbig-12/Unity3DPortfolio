using UnityEngine;

[CreateAssetMenu(fileName = "LoadingTips", menuName = "Game/Loading Tips")]
public class LoadingTips : ScriptableObject
{
    [TextArea(2, 4)]
    public string[] tips = new string[]
    {
        "Tip: Press Space to roll and dodge enemy attacks.",
        "Tip: Hold Shift while moving to sprint, but watch your stamina!",
        "Tip: Visit the shop to buy potions and upgrade your equipment.",
        "Tip: Bosses have multiple attack patterns. Learn them to survive!",
        "Tip: Use skills wisely - they have cooldowns.",
        "Tip: Explore the map to find hidden treasures.",
        "Tip: Press Q to lock on to nearby enemies."
    };

    public string GetRandomTip()
    {
        if (tips == null || tips.Length == 0)
            return "";

        return tips[Random.Range(0, tips.Length)];
    }
}
