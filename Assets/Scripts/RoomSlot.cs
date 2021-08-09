using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSlot : MonoBehaviour
{
    public Room room;
    
    public bool AttachRoom(Room room)
    {
        if (this.room != null) return false;
        this.room = room;
        return true;
    }

    public void ReleaseRoom()
    {
        room = null;
    }

    public bool HasRoom()
    {
        return room != null;
    }
}
