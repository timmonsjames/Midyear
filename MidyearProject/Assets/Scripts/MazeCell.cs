using System;
using UnityEngine;

// Represents one cell in the maze grid AND stores A* data.
public class MazeCell
{
    public int x;
    public int y;
    public bool playerRoom;
    public bool path;
    public bool visited = false;// We'll treat some cells as walls if needed
    public Vector3 worldPosition;

    // A* fields:
    public int gCost;      // distance from start
    public int hCost;      // heuristic to goal
    public int fCost;      // g + h
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public MazeCell parent; // previous cell on the best path

    public MazeCell(int x, int y, Vector3 worldPos)
    {
        this.x = x;
        this.y = y;
        this.worldPosition = worldPos;
        up = false;
        down = false;
        left = false;
        right = false;
    }
    public void AlterDirection(Vector2Int dir, bool yn)
    {
        if(dir.x == 0)
        {
            if (dir.y == 1)
                up = yn;
            else
                down = yn;
        }
        else
        {
            if (dir.x == 1)
                right = yn;
            else
                left = yn;
        }
    }
    public bool CheckDirection(Vector2Int dir)
    {
        bool result = true;
        if (dir.x == 0)
        {
            if (dir.y == 1)
                result = up;
            else
                result = down;
        }
        else
        {
            if (dir.x == 1)
                result = right;
            else
                result = left;
        }
        return result;
    }

    public void ResetDirections()
    {
        up = false; down = false; left = false; right = false;
    }
}
