using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    public Slider _slider;
    private float _targetValue;

    private void Start()
    {
        _targetValue = _slider.value;
    }

    private void Update()
    {
        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * 10f);
    }

    public void SetMaxExp(int exp)
    {
        _slider.maxValue = exp;
    
    }

    public void SetExp(int exp)
    {
        _targetValue = exp;
    }
}
