using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float speed = 5f;
    public float waypointThreshold = 0.01f;
    public enum State
    {
        Creeping,
        Escaping,
        Pathfinding,
        PermanentChase,
        Kill
    }
    public enum TargetType
    {
        None,
        Hallway,
        Nook,
        Escape,
        Drone,
        Player
    }
    public enum DirProbabilities
    {
        Up,
        Left,
        Right,
        Down
    }
    [SerializeField]
    float
        upProb = 0.25f,
        downProb = 0.25f,
        leftProb = 0.25f,
        rightProb = 0.25f;
    public float ReturnProb(DirProbabilities prob) //Will be able to return the probability of a certain direction, based on the Direction from the Enum
    {
        switch (prob)
        {
            case DirProbabilities.Up: return upProb;
            case DirProbabilities.Down: return downProb;
            case DirProbabilities.Left: return leftProb;
            case DirProbabilities.Right: return rightProb;
            default: return upProb;
        }
    }

    public Vector3 playerLOS = new Vector3();

    public void UpdatePlayerLOS(Vector3 LOS) => playerLOS = LOS; //The Game Manager will update the player's line of sight (LookingDir global position - Player position)
    public DirProbabilities FindClosestDir() // Using the playerLOS, this will find the closest direction so we can update the probabilities
    {
        Vector2 xz = new Vector2(playerLOS.x, playerLOS.z);
        float minAngle = Vector2.Angle(xz, new Vector2(0, 1));
        DirProbabilities result = DirProbabilities.Up;
        if (Vector2.Angle(xz, new Vector2(0, -1)) < minAngle)
        {
            minAngle = Vector2.Angle(xz, new Vector2(0, -1));
            result = DirProbabilities.Down;
        }
        if (Vector2.Angle(xz, new Vector2(-1, 0)) < minAngle)
        {
            minAngle = Vector2.Angle(xz, new Vector2(-1, 0));
            result = DirProbabilities.Left;
        }
        if (Vector2.Angle(xz, new Vector2(1, 0)) < minAngle){
            minAngle = Vector2.Angle(xz, new Vector2(1, 0));
            result = DirProbabilities.Right;
        }
        return result;
    }
    public void UpdateDir(DirProbabilities dir) // Adds the weight of the player looking in one direction, then normalizes the probabilities (Should about add to 1)
    {
        switch (dir)
        {
            default:
            case DirProbabilities.Up: upProb += dirMulti; break;
            case DirProbabilities.Down: downProb += dirMulti; break;
            case DirProbabilities.Left: leftProb += dirMulti; break;
            case DirProbabilities.Right: rightProb += dirMulti; break;
        }
        float sumProb = upProb + downProb + leftProb + rightProb;
        upProb /= sumProb;
        downProb /= sumProb;
        leftProb /= sumProb;
        rightProb /= sumProb;
    }

    float dirMulti = 0.001f;

    public State state;
    public TargetType targetType;
    private List<Vector3> worldPath;
    private int currentIndex = 0;
    private bool isMoving = false;


    // Start is called before the first frame update
    void Start()
    {
        //SetNewTargetToHallway()
    }

    void Update()
    {
        switch (state)
        {
            default:
            case State.Pathfinding:
                if (MoveUpdate())
                {
                    switch(targetType)
                    {
                        default:
                        case TargetType.Hallway:
                            targetType = TargetType.Nook;
                            //FindNook()
                            //SetNewTargetToNook()
                            break;
                        case TargetType.Nook:
                            state = State.Creeping;
                            break;
                        case TargetType.Escape:
                        case TargetType.Drone:
                            //SetNewTargetToHallway()
                            break;
                        case TargetType.Player:
                            state = State.PermanentChase;
                            break;
                    }
                }
                break;
            case State.Creeping:
                if (StopCreeping()) //make function work later - Markov
                {
                    state = State.Pathfinding;
                    //FindNook()
                    //SetNewTargetToNook()
                }
                break;
            case State.Escaping:
                if (true) //<-- Replace with a function that checks if player LOS is lost
                    state = State.Pathfinding;
                break;
            case State.PermanentChase:
                // If Player LOS, slow down, if not speed up
                //SetNewTargetToPlayer()
                if (MoveUpdate())
                    state = State.Kill;
                break;
            case State.Kill:
                //Wont have to change states from here.
                break;
        }
        if (true) // --> function that checks if player LOS - This will only update the direction probabilties if the monster can see the player
        {
            UpdateDir(FindClosestDir());
        }
    }


    public void SetPath(List<MazeCell> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("AStar: Empty path!");
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

    bool MoveUpdate()
    {
        if (!isMoving || worldPath == null || worldPath.Count == 0)
            return true;

        Vector3 target = worldPath[currentIndex];

        // Move toward the current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
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
                return true;
            }
        }
        return false;
    }
    bool StopCreeping()
    {
        return true;
    }
}
