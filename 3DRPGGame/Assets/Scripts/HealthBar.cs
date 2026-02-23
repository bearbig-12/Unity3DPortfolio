using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider _slider;

    private float _targetValue;
    private bool _isAnimating;

    private void Start()
    {
        _targetValue = _slider.value;
    }

    void Update()
    {
        if (!_isAnimating) return;

        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * 10f);

        // 목표값에 거의 도달하면 애니메이션 중지
        if (Mathf.Abs(_slider.value - _targetValue) < 0.01f)
        {
            _slider.value = _targetValue;
            _isAnimating = false;
        }
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
        _isAnimating = true;
    }

}
