using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject personPrefab;
    public RoomType[] roomTypes;
    public GameObject[] roomTypePrefabs;

    Dictionary<RoomType, GameObject> roomTypeMap;
    RoomGrid grid;
    int saveSlot;
    bool built;

    void Awake()
    {     
        grid = GameObject.Find("Room Grid").GetComponent<RoomGrid>();
        roomTypeMap = new Dictionary<RoomType, GameObject>();
        for (int i = 0; i < roomTypes.Length; i++)
        {
            roomTypeMap.Add(roomTypes[i], roomTypePrefabs[i]);
        }
        // Load();
    }

    void Update()
    {
        if(!built)
        {
            Load();
            built = true;
        }
    }

    public void Save()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + $"/gameSave{saveSlot}.bin";
        FileStream stream = new FileStream(path, FileMode.Create);

        GameSave save = new GameSave(grid.GetAllRooms(), grid.GetAllPeople());

        formatter.Serialize(stream, save);
        stream.Close();
    }

    public void Load()
    {
        string path = Application.persistentDataPath + $"/gameSave{saveSlot}.bin";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameSave save = formatter.Deserialize(stream) as GameSave;

            stream.Close();

            BuildGame(save);
        }
        else
        {
            Debug.LogError("Missing Save data.");
        }
    }

    void BuildGame(GameSave save)
    {
        GameObject newThing;
        Person person;
        Room room;
        foreach (SerializableRoom sRoom in save.rooms)
        {
            newThing = Instantiate(roomTypeMap[sRoom.roomType], Vector3.zero, Quaternion.identity);
            room = newThing.GetComponent<Room>();
            room.Load(sRoom);
            grid.AddRoom(room, room.slotIndex);
            grid.SetPosition(room);
            room.placed = true;
        }
        foreach (SerializablePerson sPerson in save.people)
        {
            newThing = Instantiate(personPrefab, Vector3.zero, Quaternion.identity);
            person = newThing.GetComponent<Person>();
            person.Load(sPerson);
            grid.AddPerson(person);
        }
    }
}
