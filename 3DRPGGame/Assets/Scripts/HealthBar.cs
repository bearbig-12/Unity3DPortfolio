using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider _slider;
    
    private float _targetValue;


    private void Start()
    {
        _targetValue = _slider.value;
    }

    void Update()
    {
        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * 10f);
    }


    public void SetMaxHealth(int health)
    {
        _slider.maxValue = health;
        _slider.value = health;
        _targetValue = health;
    }

    public void SetHealth(int health)
    {
        _targetValue = health;
    }

}
