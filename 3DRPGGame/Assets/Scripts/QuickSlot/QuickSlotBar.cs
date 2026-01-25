using UnityEngine;

public class QuickSlotBar : MonoBehaviour
{
    [SerializeField] private QuickSlot[] slots = new QuickSlot[4];

    private void Update()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (slots[i] != null) slots[i].UseSlot();
            }
        }
    }

    private void UseIndex(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length) return;


        if (slots[index] != null) slots[index].UseSlot();
    }
}