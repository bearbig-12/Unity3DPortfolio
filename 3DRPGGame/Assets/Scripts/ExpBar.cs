using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    public Slider _slider;

    private float _targetValue;
    private bool _isAnimating;

    private void Start()
    {
        _targetValue = _slider.value;
    }

    private void Update()
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

    public void SetMaxExp(int exp)
    {
        _slider.maxValue = exp;
    
    }

    public void SetExp(int exp)
    {
        _targetValue = exp;
        _isAnimating = true;
    }
}
