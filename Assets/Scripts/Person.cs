using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Person : MonoBehaviour
{
    public enum NeedType
    {
        Hunger,
        Social,
        Inspiration,
        Exercise,
        Sleep
    }

    public int arrivals;
    public int departures;
    public float navStopThreshold;
    public float walkSpeed;
    public (int, int) slotIndex;
    public Dictionary<NeedType, Need> needs;
    public Need[] needArray => needs.Values.ToArray();
    public NeedModifier[] needModifiers;
    public string personName;
    public List<Vector3> path;
    public bool focusOfCard;
    public SpriteRenderer outline;
    public Seat seat;

    bool hover;
    bool selected;
    public  bool settled;
    Vector3 selectionOffset;
    Vector3 previousPosition;
    Camera mainCam;
    RoomGrid grid;
    float needVariance = 0.6f;

    public Person()
    {
        needs = new Dictionary<NeedType, Need>();
        foreach (NeedType needType in Enum.GetValues(typeof(NeedType)))
        {
            Need need = new Need(needType);
            needs.Add(needType, new Need(needType));
        }
    }

    void Start()
    {
        // If this person is newly created, as opposed to being loaded from a save.
        if (personName == "")
        {
            personName = NVJOBNameGen.GiveAName(7);
            foreach (Need need in needs.Values)
            {
                need.decay *= UnityEngine.Random.Range(1f - needVariance, 1f + needVariance);
                need.refill *= UnityEngine.Random.Range(1f - needVariance, 1f + needVariance);                
                need.importance *= UnityEngine.Random.Range(1f - needVariance, 1f + needVariance);                
            }
        }
        mainCam = Camera.main;
        grid = GameObject.Find("Room Grid").GetComponent<RoomGrid>();
        path = new List<Vector3>();
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
        Vector3 rawSelectionOffset = mainCam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        selectionOffset = new Vector3(rawSelectionOffset.x, rawSelectionOffset.y, rawSelectionOffset.z);
        selected = true;
        grid.SetSelectedPerson(this);
    }

    void OnMouseUp()
    {
        List<Vector3> pathCandidate = Pathfinder.GetPath(previousPosition, transform.position, grid);
        if (pathCandidate.Count > 0)
        {
            SetPath(pathCandidate);
            if(grid.GetRoom(slotIndex) != null)
            {
                LeaveSeat();
                departures++;
            }
            // Take on the slot index of the target room.
            slotIndex = grid.PosToIndex(transform.position - grid.transform.position);
            settled = false;
            needModifiers = null;
        }
        transform.position = previousPosition;
        selected = false;
    }

    void SetPath(List<Vector3> newPath)
    {
        path = newPath;
        for (int i = 0; i < path.Count; i++)
        {
            path[i] = new Vector3(path[i].x, path[i].y, 0f);
        }
    }

    bool SetPath((int, int) destination)
    {
        List<Vector3> pathCandidate = Pathfinder.GetPath(slotIndex, destination, grid);
        if (pathCandidate.Count == 0)
            return false;
        SetPath(pathCandidate);
        slotIndex = destination;
        settled = false;
        needModifiers = null;
        return true;
    }

    void Update()
    {
        float scale = !hover ? 3f : 3.2f;
        transform.localScale = new Vector3(scale, scale, 1f);
        outline.enabled = focusOfCard;
        if (selected)
        {
            transform.position = mainCam.ScreenToWorldPoint(Input.mousePosition) - selectionOffset;
        }
        else if (path.Count > 0)
        {
            if ((transform.position - path[path.Count - 1]).sqrMagnitude < navStopThreshold)
            {
                path.RemoveAt(path.Count - 1);
                // Check at each step that the next room exists. This accomodates changes to the room layout during navigation.
                if (path.Count > 0 && grid.GetRoom(grid.PosToIndex(path[path.Count - 1] - grid.transform.position)) == null)
                {
                    path.Clear();
                    slotIndex = grid.PosToIndex(transform.position - grid.transform.position);
                }
            }
            else
            {
                Vector3 move = (path[path.Count - 1] - transform.position).normalized * walkSpeed * Time.deltaTime;
                transform.position += move;
            }
        }
        else if (!settled)
        {
            Room currentRoom = grid.GetRoom(slotIndex);
            if (currentRoom != null && currentRoom.SettleIn(this))
            {
                // The room exists and there's room for this person.
                // Time to reap the rewards!
                needModifiers = currentRoom.GetNeedModifiers();
                settled = true;
                arrivals++;
            }
            else
            {
                MoveOn();
            }
        }
        else if (HadEnough())
        {
            Room currentRoom = grid.GetRoom(slotIndex);
            if (MoveOn())
            {
                LeaveSeat();
                departures++;
            }
        }

        // All needs are worsened by a small amount.
        foreach (Need need in needs.Values)
        {
            need.Decay();
        }

        if (needModifiers != null)
        {
            foreach (NeedModifier needMod in needModifiers)
            {
                needs[needMod.needType].Refill(needMod.baseRate);
            }
        }
        // Todo: prevent z wandering.
        transform.position.Set(transform.position.x, transform.position.y, 0f);
    }

    void LeaveSeat()
    {
        if (seat != null)
        {
            seat.ClearOccupant();
            seat = null;
        }
    }

    public void SetSeat(Seat seat)
    {
        this.seat = seat;
    }

    // Probabalistically select the next room based on current needs
    // and the distance to the closest room to address them.
    // Returns true when the person has successfully been directed to a room,
    // otherwise returns false.
    bool MoveOn()
    {
        ProspectiveNeed[] topNeeds = GetTopNeeds();
        float totalDistance = 0f;
        foreach (ProspectiveNeed pNeed in topNeeds)
        {
            pNeed.roomIndex = grid.GetClosest(slotIndex, pNeed.needType);
            pNeed.distance = grid.GetDistance(slotIndex, pNeed.roomIndex);
            totalDistance += pNeed.distance;
        }
        float totalWeight = 0f;
        foreach (ProspectiveNeed pNeed in topNeeds)
        {
            pNeed.weight = Mathf.Clamp01(pNeed.urgency - pNeed.distance / totalDistance);
            totalWeight += pNeed.weight;
        }
        float selectionWeight = 0f;
        float selection = UnityEngine.Random.Range(0f, totalWeight);
        foreach (ProspectiveNeed pNeed in topNeeds)
        {
            selectionWeight += pNeed.weight;
            if (selectionWeight > selection)
            {
                if (pNeed.roomIndex == (-1, -1))
                    return false;
                return SetPath(pNeed.roomIndex);
            }
        }
        return false;
    }

    public void Load(SerializablePerson person)
    {
        personName = person.personName;
        slotIndex = (person.slotIndex1, person.slotIndex2);
        foreach (SerializableNeed sNeed in person.needs)
        {
            Need need = new Need(sNeed);
            needs[need.type] = need;
        }
    }

    // Checks if all needs currently being modified by the room are satisfied.
    // Note: this will need to be changed should a room have a negative affect on any need.
    bool HadEnough()
    {
        foreach (NeedModifier needModifier in needModifiers)
        {
            if (needs[needModifier.needType].currentValue < 80)
                return false;
        }
        return true;
    }

    // Returns a list of the top 3 most urgent needs.
    // Urgency is between 0 and 1.
    ProspectiveNeed[] GetTopNeeds()
    {
        List<ProspectiveNeed> pNeeds = new List<ProspectiveNeed>();
        foreach (Need need in needs.Values)
        {
            ProspectiveNeed pNeed = new ProspectiveNeed();
            pNeed.needType = need.type;
            pNeed.urgency = (100f - need.currentValue) / 100f;
            pNeeds.Add(pNeed);
        }
        pNeeds = pNeeds.OrderBy(n => n.urgency).ToList();
        return new ProspectiveNeed[]
        {
            pNeeds[pNeeds.Count - 1],
            pNeeds[pNeeds.Count - 2],
            pNeeds[pNeeds.Count - 3],
        };
    }

    class ProspectiveNeed
    {
        public NeedType needType;
        public float urgency;
        public float distance;
        public float weight;
        public (int, int) roomIndex;
    }
}

[System.Serializable]
public class SerializablePerson
{
    public string personName;
    public int slotIndex1;
    public int slotIndex2;
    public SerializableNeed[] needs;

    public SerializablePerson(Person person)
    {
        personName = person.personName;
        slotIndex1 = person.slotIndex.Item1;
        slotIndex2 = person.slotIndex.Item2;
        needs = new SerializableNeed[person.needs.Count];
        int i = 0;
        foreach (Need need in person.needs.Values)
        {
            needs[i++] = new SerializableNeed(need);
        }
    }
}

public class Need
{
    public Person.NeedType type;
    [Range(0, 100)]
    public float currentValue;
    [Range(0, 1)]
    public float importance;
    [Range(0, 1)]
    public float decay;
    [Range(0, 1)]
    public float refill;

    public Need(Person.NeedType type)
    {
        this.type = type;
        currentValue = 50;
        importance = 0.5f;
        decay = 0.5f;
        refill = 0.5f;
    }

    public Need(SerializableNeed need)
    {
        this.type = (Person.NeedType) Enum.Parse(typeof(Person.NeedType), need.type);
        currentValue = need.currentValue;
        importance = need.importance;
        decay = need.decay;
        refill = need.refill;
    }

    // Used for decision making.
    // Product of current value and importance.
    // Could be modified by animCurve later in dev.
    public float GetUrgency()
    {
        return currentValue / importance;
    }

    public void Decay()
    {
        currentValue -= decay * Time.deltaTime;
        currentValue = Mathf.Clamp(currentValue, 0f, 100f);
    }

    public void Refill(float baseRate)
    {
        currentValue += baseRate * refill * Time.deltaTime;
        currentValue = Mathf.Clamp(currentValue, 0f, 100f);
    }
}

[Serializable]
public class SerializableNeed
{
    public string type;
    public float currentValue;
    public float importance;
    public float decay;
    public float refill;

    public SerializableNeed(Need need)
    {
        type = need.type.ToString();
        currentValue = need.currentValue;
        importance = need.importance;
        decay = need.decay;
        refill = need.refill;
    }
}
