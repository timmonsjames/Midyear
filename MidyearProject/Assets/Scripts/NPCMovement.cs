using System.Collections.Generic;
using UnityEngine;

// This script DOES NOT run A*.
// It simply follows the path that AStarPathfinder computed.

public class NPCMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float waypointThreshold = 0.05f;

    private List<Vector3> worldPath;
    private int currentIndex = 0;
    private bool isMoving = false;

    // Called after A* is done
    public void SetPath(List<MazeCell> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("NPCMovement: Empty path!");
            return;
        }

        worldPath = new List<Vector3>();

        // Convert MazeCell -> world positions
        foreach (MazeCell cell in path)
        {
            worldPath.Add(cell.worldPosition);
        }

        currentIndex = 0;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving || worldPath == null || worldPath.Count == 0)
            return;

        Vector3 target = worldPath[currentIndex];

        // Move toward the current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // Rotate to face direction
        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }

        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, target) < waypointThreshold)
        {
            currentIndex++;
            if (currentIndex >= worldPath.Count)
            {
                isMoving = false;
                Debug.Log("NPC reached the goal!");
            }
        }
    }
}
