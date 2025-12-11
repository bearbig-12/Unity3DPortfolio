using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider _slider;

    public void SetMaxStamina(int stamina)
    {
        _slider.maxValue = stamina;
        _slider.value = stamina;
    }

    public void SetStamina(int stamina)
    {
        _slider.value = stamina;
    }
}
