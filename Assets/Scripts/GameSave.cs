using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSave
{
    public SerializablePerson[] people;
    public SerializableRoom[] rooms;

    public GameSave(List<Room> rooms, List<Person> people)
    {
        this.people = new SerializablePerson[people.Count];
        this.rooms = new SerializableRoom[rooms.Count];
        for (int i = 0; i < people.Count; i++)
        {
            this.people[i] = new SerializablePerson(people[i]);
        }
        for (int i = 0; i < rooms.Count; i++)
        {
            this.rooms[i] = new SerializableRoom(rooms[i]);
        }
    }
}
