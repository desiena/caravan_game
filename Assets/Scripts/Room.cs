using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    eatery,
    gym,
    conservatory,
    bunks,
    lounge
}

public class Room : MonoBehaviour
{
    public (int, int) slotIndex;
    public NeedModifier[] needModifiers;
    public RoomType roomType;
    public Seat[] seats;

    bool hover;
    bool selected;
    public bool placed;
    Vector3 previousPosition;
    Vector3 selectionOffset;
    Camera main;
    RoomGrid grid;
    int seatIndex;

    void Start()
    {
        main = Camera.main;
        grid = GameObject.Find("Room Grid").GetComponent<RoomGrid>();
    }

    void OnMouseEnter()
    {
        hover = true;
    }

    void OnMouseExit()
    {
        hover = false;
    }

    void OnMouseDown()
    {
        previousPosition = transform.position;
        Vector3 rawSelectionOffset = main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        selectionOffset = new Vector3(rawSelectionOffset.x, rawSelectionOffset.y, rawSelectionOffset.z);
        selected = true;
        if (placed)
            grid.RemoveRoom(previousPosition);

        grid.SetSelectedPerson(null);
    }

    void OnMouseUp()
    {
        if (grid.AddRoom(this))
        {
            slotIndex = grid.PosToIndex(transform.position - grid.transform.position);
        }
        else
        {
            transform.position = previousPosition;
            grid.AddRoom(this);
        }
        placed = true;
        selected = false;
    }

    void Update()
    {
        float scale = !hover ? 6.5f : 7f;
        transform.localScale = new Vector3(scale, scale, 1f);
        if (selected)
            transform.position = grid.SnapPosition(main.ScreenToWorldPoint(Input.mousePosition) - selectionOffset);
    }

    public void Load(SerializableRoom room)
    {
        slotIndex = (room.slotIndex1, room.slotIndex2);
    }

    public NeedModifier[] GetNeedModifiers()
    {
        return needModifiers;
    }

    public bool DoesRefill(Person.NeedType needType)
    {
        foreach (NeedModifier needModifier in needModifiers)
        {
            if (needModifier.needType == needType)
                return true;
        }
        return false;
    }

    public bool SettleIn(Person person)
    {
        foreach (Seat seat in seats)
        {
            if (seat.HasOccupant())
                continue;
            seat.SetOccupant(person);
            person.SetSeat(seat);
            MoveToSeat(seat.transform, person);
            return true;
        }
        return false;
    }
    
    void MoveToSeat(Transform seat, Person person)
    {
        Vector3 place = new Vector3(
            seat.position.x + UnityEngine.Random.Range(-.4f, .4f),
            seat.position.y + UnityEngine.Random.Range(-.4f, .4f),
            0f
        );
        person.path = new List<Vector3> { place };
    }
}

[Serializable]
public class SerializableRoom
{
    public int slotIndex1;
    public int slotIndex2;
    public RoomType roomType;

    public SerializableRoom(Room room)
    {
        slotIndex1 = room.slotIndex.Item1;
        slotIndex2 = room.slotIndex.Item2;
        roomType = room.roomType;
    }
}

[Serializable]
public class NeedModifier
{
    public Person.NeedType needType;
    public float baseRate;
}
