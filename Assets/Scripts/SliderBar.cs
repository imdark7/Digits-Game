using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    private Slider slider;
    public int Value => (int) slider.value;

    public SliderBar Init(Vector3 pos, Color color, int maxValue)
    {
        enabled = false;
        slider = GetComponent<Slider>();
        transform.position = pos;
        transform.Find("Fill Area").GetComponentInChildren<Image>().color = color;
        slider.maxValue = maxValue;
        enabled = true;
        return this;
    }

    public IEnumerator SetValue(int value)
    {
        if (value > slider.maxValue || value < slider.minValue)
        {
            yield break;
        }

        yield return SlowSetValue((int) slider.value, value);
    }

    private IEnumerator SlowSetValue(int from, int to)
    {
        while (from != to)
        {
            if (from > to)
            {
                from--;
            }
            else
            {
                from++;
            }

            slider.value = from;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void Increase()
    {
        slider.value++;
    }

    public void SetMaxValue(int value)
    {
        slider.maxValue = value;
    }
}
