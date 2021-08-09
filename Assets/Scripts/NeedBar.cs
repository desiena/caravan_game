using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeedBar : MonoBehaviour
{
    public Person person;
    public Person.NeedType needType;
    Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        if (person != null)
        {
            slider.value = person.needs[needType].currentValue;
        }
    }
}
