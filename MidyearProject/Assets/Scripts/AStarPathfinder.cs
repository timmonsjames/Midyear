using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AStarPathfinder : MonoBehaviour
{
    private static readonly Vector2Int[] Directions = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };
    public MazeGenerator mazeG;

    // Start is called before the first frame update
    public List<MazeCell> FindPath()
    {
        for (int i = 0; i < mazeG.width; i++)
        {
            for (int j = 0; j < mazeG.height; j++)
            {
                mazeG.cells[i, j].gCost = int.MaxValue;
                mazeG.cells[i, j].hCost = 0;
                mazeG.cells[i, j].fCost = 0;
                mazeG.cells[i, j].parent = null;
            }
        }
        MazeCell goal = mazeG.cells[mazeG.endPos.x, mazeG.endPos.y];
        MazeCell start = mazeG.cells[mazeG.startPos.x, mazeG.startPos.y];
        List<MazeCell> openSet = new List<MazeCell>();
        HashSet<MazeCell> closedSet = new HashSet<MazeCell>();
        start.gCost = 0;
        start.hCost = Heuristic(start, goal);
        start.fCost = start.gCost + start.hCost;
        openSet.Add(start);
        while (openSet.Count > 0)
        {
            MazeCell current = GetLowestFCost(openSet);

            if (current == goal)
            {
                return ReconstructPath(goal);
            }
            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var dir in Directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx < 0 || nx >= mazeG.cells.GetLength(0) || ny < 0 || ny >= mazeG.cells.GetLength(1))
                    continue;

                MazeCell neighbor = mazeG.cells[nx, ny];

                // Ignore walls and closed cells
                if (!current.CheckDirection(dir) || closedSet.Contains(neighbor))
                    continue;

                // Cost from start to neighbor through current (each move cost = 1)
                int tentativeG = current.gCost + 1;

                // If this path to neighbor is better OR neighbor is not in openSet
                if (tentativeG < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Heuristic(neighbor, goal);
                    neighbor.fCost = neighbor.gCost + neighbor.hCost;
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        Debug.LogWarning("A* :No Path Found!");
        return new List<MazeCell>();
    }
    private MazeCell GetLowestFCost(List<MazeCell> list)
    {
        MazeCell best = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < best.fCost)
            {
                best = list[i];
            }
        }
        return best;
    }

    // Manhattan distance heuristic (mazeG.cells, no diagonals)
    private int Heuristic(MazeCell a, MazeCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<MazeCell> ReconstructPath(MazeCell end)
    {
        List<MazeCell> path = new List<MazeCell>();
        MazeCell current = end;

        // Follow parent pointers back to the start
        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
}
