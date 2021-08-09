using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGrid : MonoBehaviour
{
    public int width;
    public int height;
    public float spacing = 30f;
    public GameObject roomSlotPrefab;
    public GameObject doorSlotPrefab;
    public GameObject doorPrefab;
    public RoomSlot[] roomSlots;
    public DoorSlot[] horizontalDoorSlots;
    public DoorSlot[] verticalDoorSlots;
    public NeedBar[] needBars;
    public PersonCard personCard;

    Camera main;
    List<Person> people;
    float personZ = 0;

    void Start()
    {
        main = Camera.main;
        SetupDoors();
        people = new List<Person>();
    }

    void SetupDoors()
    {
        horizontalDoorSlots = new DoorSlot[(height - 1) * width];
        verticalDoorSlots = new DoorSlot[height * (width - 1)];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                Vector3 position = IndexToPosition(x, y) - Vector3.up * spacing / 2;
                GameObject newDoorSlot = Instantiate(doorSlotPrefab, position, Quaternion.Euler(0f, 0f, 90f));
                horizontalDoorSlots[x + y * width] = newDoorSlot.GetComponent<DoorSlot>();
            }
        }
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = IndexToPosition(x, y) + Vector3.right * spacing / 2;
                GameObject newDoorSlot = Instantiate(doorSlotPrefab, position, Quaternion.identity);
                verticalDoorSlots[x + y * (width - 1)] = newDoorSlot.GetComponent<DoorSlot>();
            }
        }
    }

    public void GenerateSlots()
    {
        if (roomSlots != null)
        {
            for (int i = 0; i < roomSlots.Length; i++)
            {
                if (roomSlots[i] != null)
                {
                    // DestroyImmediate is used here because this method is called in edit mode.
                    DestroyImmediate(roomSlots[i].gameObject);
                }
            }
        }
        roomSlots = new RoomSlot[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject slotObject = Instantiate(roomSlotPrefab, IndexToPosition(x, y) + Vector3.forward, Quaternion.identity);
                slotObject.transform.SetParent(transform);
                roomSlots[GetSlotIndex(x, y)] = slotObject.GetComponent<RoomSlot>();
            }
        }
    }

    // Returns global coords.
    public Vector3 SnapPosition(Vector3 loosePosition)
    {
        (int, int) index = PosToIndex(loosePosition - transform.position);
        index.Item1 = Mathf.Clamp(index.Item1, 0, width - 1);
        index.Item2 = Mathf.Clamp(index.Item2, 0, height - 1);
        Vector3 snapped = IndexToPosition(index.Item1, index.Item2);
        snapped.Set(snapped.x, snapped.y, loosePosition.z);
        return snapped;
    }

    public void AddPerson(Person person)
    {
        people.Add(person);
        Vector3 roomPos = IndexToPosition(person.slotIndex.Item1, person.slotIndex.Item2);
        person.transform.position = new Vector3(roomPos.x, roomPos.y, personZ);
    }

    public bool AddRoom(Room room)
    {
        (int, int) slotIndex = PosToIndex(room.transform.position - transform.position);
        return AddRoom(room, slotIndex);
    }

    public bool AddRoom(Room room, (int, int) slotIndex)
    {
        RoomSlot slot = roomSlots[GetSlotIndex(slotIndex.Item1, slotIndex.Item2)];
        if (!slot.AttachRoom(room))
            return false;
        AddDoors(slotIndex.Item1, slotIndex.Item2);
        return true;
    }

    public void RemoveRoom(Vector3 position)
    {
        (int, int) slotIndex = PosToIndex(position - transform.position);
        RoomSlot slot = roomSlots[GetSlotIndex(slotIndex.Item1, slotIndex.Item2)];
        slot.ReleaseRoom();
        RemoveDoors(slotIndex.Item1, slotIndex.Item2);
    }

    public bool HasRoom((int, int) coord)
    {
        if (coord.Item1 < 0 || coord.Item1 >= width || coord.Item2 < 0 || coord.Item2 >= height)
            return false;
        return roomSlots[GetSlotIndex(coord.Item1, coord.Item2)].HasRoom();
    }

    public (int, int) GetClosest((int, int) start, Person.NeedType needType)
    {
        (int, int) closest = (-1, -1);
        float leastDistance = float.MaxValue;
        foreach (Room room in GetAllRooms())
        {
            if (!room.DoesRefill(needType))
                continue;
            float distance = GetDistance(start, room.slotIndex);
            if (distance < leastDistance)
            {
                leastDistance = distance;
                closest = room.slotIndex;
            }
        }
        return closest;
    }

    public float GetDistance((int, int) start, (int, int) destination)
    {
        if (destination == (-1, -1))
            return 1000f;
        return Mathf.Abs(start.Item1 - destination.Item1) + Mathf.Abs(start.Item2 - destination.Item2);
    }

    public void SetSelectedPerson(Person person)
    {
        personCard.SetPerson(person);
        personCard.gameObject.SetActive(person != null);
    }

    void AddDoors(int x, int y)
    {
        // Top
        if (y > 0 && roomSlots[GetSlotIndex(x, y - 1)].HasRoom())
        {
            horizontalDoorSlots[x + width * (y - 1)].CreateDoor(doorPrefab);
        }

        // Bottom
        if (y < height - 1 && roomSlots[GetSlotIndex(x, y + 1)].HasRoom())
        {
            horizontalDoorSlots[x + width * y].CreateDoor(doorPrefab);
        }

        // Left
        if (x > 0 && roomSlots[GetSlotIndex(x - 1, y)].HasRoom())
        {
            verticalDoorSlots[x - 1 + (width - 1) * y].CreateDoor(doorPrefab);
        }

        // Right
        if (x < width - 1 && roomSlots[GetSlotIndex(x + 1, y)].HasRoom())
        {
            verticalDoorSlots[x + (width - 1) * y].CreateDoor(doorPrefab);
        }
    }
    
    void RemoveDoors(int x, int y)
    {
        foreach(DoorSlot slot in GetDoorSlotsFromRoomIndex(x, y))
        {
            slot.DestroyDoor();     
        }
    }

    List<DoorSlot> GetDoorSlotsFromRoomIndex(int x, int y)
    {
        List<DoorSlot> doorSlots = new List<DoorSlot>();
        // Top
        if (y > 0)
        {
            doorSlots.Add(horizontalDoorSlots[x + (y - 1) * width]);
        }

        // Bottom
        if (y < height - 1)
        {
            doorSlots.Add(horizontalDoorSlots[x + y * width]);
        }

        // Left
        if (x > 0)
        {
            doorSlots.Add(verticalDoorSlots[x - 1 + y * (width - 1)]);
        }

        // Right
        if (x < width - 1)
        {
            doorSlots.Add(verticalDoorSlots[x + y * (width - 1)]);
        }
        return doorSlots;
    }

    public (int, int) PosToIndex(Vector3 position)
    {
        int xIndex = Mathf.RoundToInt(position.x / spacing) + width / 2;
        int yIndex = Mathf.RoundToInt(-position.y / spacing);
        return (xIndex, yIndex);
    }

    public Vector3 IndexToPosition(int x, int y)
    {
        float xPos = (x - (width / 2)) * spacing;
        float yPos = -y * spacing;
        return new Vector3(xPos, yPos, 0f) + transform.position;
    }

    int GetSlotIndex(int x, int y)
    {
        return x + width * y;
    }

    public List<Room> GetAllRooms()
    {
        List<Room> rooms = new List<Room>();
        foreach(RoomSlot slot in roomSlots)
        {
            if (slot.HasRoom())
            {
                rooms.Add(slot.room);
            }
        }
        return rooms;
    }

    public List<Person> GetAllPeople()
    {
        return people;
    }

    public void SetPosition(Room room)
    {
        Vector3 pos = IndexToPosition(room.slotIndex.Item1, room.slotIndex.Item2);
        room.transform.position = new Vector3(pos.x, pos.y, 0.01f);
    }

    public Room GetRoom((int, int) slotIndex)
    {
        return roomSlots[GetSlotIndex(slotIndex.Item1, slotIndex.Item2)].room;
    }
}
