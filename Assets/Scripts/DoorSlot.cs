using System.Collections;
using UnityEngine;

public class DoorSlot : MonoBehaviour
{
    public GameObject door;

    public void CreateDoor(GameObject doorPrefab)
    {
        door = Instantiate(doorPrefab, transform);
        door.transform.localPosition = Vector3.zero;
    }

    public void DestroyDoor()
    {
        if (door != null)
        {
            Destroy(door);
            door = null;
        }
    }
}
