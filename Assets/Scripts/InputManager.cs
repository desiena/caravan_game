using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public GameObject personPrefab;
    public Transform spawnLocation;
    public float zoomScale;
    public float panScale;
    public float maxCamSize;
    public float minCamSize;
    public float maxCamX;
    public float minCamX;
    public float maxCamY;
    public float minCamY;

    Camera main;
    RoomGrid grid;

    // Start is called before the first frame update
    void Start()
    {
        main = Camera.main;
        grid = GameObject.Find("Room Grid").GetComponent<RoomGrid>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Instantiate(roomPrefabs[0], spawnLocation.position, Quaternion.identity);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Instantiate(roomPrefabs[1], spawnLocation.position, Quaternion.identity);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            Instantiate(roomPrefabs[2], spawnLocation.position, Quaternion.identity);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            Instantiate(roomPrefabs[3], spawnLocation.position, Quaternion.identity);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            Instantiate(roomPrefabs[4], spawnLocation.position, Quaternion.identity);
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            Person person = Instantiate(personPrefab, spawnLocation.position, Quaternion.identity).GetComponent<Person>();
            person.slotIndex = (12, 0);
            grid.AddPerson(person);
        }
        float deltaX = Input.GetAxis("Horizontal") * main.orthographicSize * panScale / maxCamSize;
        float deltaY = Input.GetAxis("Vertical") * main.orthographicSize * panScale / maxCamSize;
        float newX = Mathf.Clamp(main.transform.position.x + deltaX, minCamX, maxCamX);
        float newY = Mathf.Clamp(main.transform.position.y + deltaY, minCamY, maxCamY);
        main.transform.position = new Vector3(newX, newY, main.transform.position.z);
    }

    void OnGUI()
    {
        float size = main.orthographicSize + -Input.mouseScrollDelta.y * zoomScale;
        main.orthographicSize = Mathf.Clamp(size, minCamSize, maxCamSize);
    }
}
