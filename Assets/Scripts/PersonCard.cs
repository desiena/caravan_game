using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PersonCard : MonoBehaviour
{
    public NeedBar[] needBars;
    public TextMeshProUGUI nameTag;

    Person focusPerson;

    public void SetPerson(Person person)
    {
        if (focusPerson != null)
            focusPerson.focusOfCard = false;
        focusPerson = person;
        if (person != null)
        {
            nameTag.text = person.personName;
            person.focusOfCard = true;
        }
        foreach (NeedBar needBar in needBars)
        {
            needBar.person = person;
            needBar.gameObject.SetActive(person != null);
        }
    }
}
