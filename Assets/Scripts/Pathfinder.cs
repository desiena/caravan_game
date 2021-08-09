using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{
    // Assumes there is a room at start and goal, and that a path exists.
    public static List<Vector3> GetPath(Vector3 startPos, Vector3 goalPos, RoomGrid grid)
    {
        (int, int) start = grid.PosToIndex(startPos - grid.transform.position);
        (int, int) goal = grid.PosToIndex(goalPos - grid.transform.position);
        return GetPath(start, goal, grid);
    }
    
    // Assumes there is a room at start and goal, and that a path exists.
    public static List<Vector3> GetPath((int, int) start, (int, int) goal, RoomGrid grid)
    {
        List<(int, int)> openSet = new List<(int, int)>();
        openSet.Add(start);

        Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();

        Dictionary<(int, int), int> gScore = new Dictionary<(int, int), int>();
        gScore.Add(start, 0);

        Dictionary<(int, int), int> fScore = new Dictionary<(int, int), int>();
        fScore.Add(start, PathHeuristic(start, goal));

        (int, int) current;
        while (openSet.Count > 0)
        {
            openSet.OrderBy(nameof => fScore[nameof]);
            current = openSet[0];
            openSet.Remove(current);
            // Debug.Log($"{current.Item1}, {current.Item2}");

            if (current.Item1 == goal.Item1 && current.Item2 == goal.Item2)
                return ReconstructPath(cameFrom, current, grid);

            foreach ((int, int) neighbor in GetNeighbors(current))
            {
                if (!grid.HasRoom(neighbor))
                    continue;
                int tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || gScore[neighbor] > tentativeGScore)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + PathHeuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return new List<Vector3>();
    }

    static int PathHeuristic((int, int) start, (int, int) goal)
    {
        return Mathf.Abs(start.Item1 - goal.Item1) + Mathf.Abs(start.Item2 - goal.Item2);
    }

    static List<Vector3> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current, RoomGrid grid)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(grid.IndexToPosition(current.Item1, current.Item2));
        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(grid.IndexToPosition(current.Item1, current.Item2));
        }
        return path;
    }

    static (int, int)[] GetNeighbors((int, int) coord)
    {
        int x = coord.Item1;
        int y = coord.Item2;
        return new (int, int)[]
        {
            (x + 1, y),
            (x - 1, y),
            (x, y + 1),
            (x, y - 1)
        };

    }
}
