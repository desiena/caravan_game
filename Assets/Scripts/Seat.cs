using System.Collections;
using UnityEngine;

public class Seat : MonoBehaviour
{
    public Person occupant;

    public bool HasOccupant()
    {
        return occupant != null;
    }

    public bool SetOccupant(Person person)
    {
        if (occupant != null)
        {
            return false;
        }
        occupant = person;
        return true;
    }

    public void ClearOccupant()
    {
        occupant = null;
    }
}
