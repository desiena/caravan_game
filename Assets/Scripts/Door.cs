using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject upperDoor;
    public GameObject lowerDoor;
    public int peopleInRange;

    Vector3 upperOpen;
    Vector3 upperClosed;
    Vector3 lowerOpen;
    Vector3 lowerClosed;

    Renderer m_Renderer;

    void Awake()
    {
        m_Renderer = GetComponent<SpriteRenderer>();
        m_Renderer.enabled = false;
        upperOpen = new Vector3(0f, 0.08f, 0f);
        upperClosed = new Vector3(0f, 0.0333f, 0f);
        lowerOpen = new Vector3(0f, -0.08f, 0f);
        lowerClosed = new Vector3(0f, -0.0333f, 0f);
    }

    public void Connect()
    {
        m_Renderer.enabled = true;
    }

    public void Disconnect()
    {
        m_Renderer.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        peopleInRange++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        peopleInRange--;
    }

    void Update()
    {
        if (peopleInRange > 0)
        {
            upperDoor.transform.localPosition = upperOpen;
            lowerDoor.transform.localPosition = lowerOpen;
        }
        else
        {
            upperDoor.transform.localPosition = upperClosed;
            lowerDoor.transform.localPosition = lowerClosed;
        }
    }
}
