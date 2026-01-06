using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    float
        upProb = 0.25f,
        downProb = 0.25f,
        leftProb = 0.25f,
        rightProb = 0.25f;
    public MazeGameManager manager;
    public MazeGenerator generator;
    public float speed = 5f;
    public float waypointThreshold = 0.01f;
    public MazeCell playerRoom;
    public MazeCell downHallway;
    public MazeCell rightHallway;
    public MazeCell leftHallway;
    public DirProbabilities currHallway;
    public float time = 0f;
    public DirProbabilities[] Hallways = new DirProbabilities[3];
    public Transform playerLocation;
    Vector3 directionToPlayer = Vector3.zero;
    float playerLOSResetTime = 0f;
    public DroneMovement drone;

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

    public void UpdatePlayerLOS(Vector3 LOS) => playerLOS = -LOS; //The Game Manager will update the player's line of sight (LookingDir global position - Player position)
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
        if (Vector2.Angle(xz, new Vector2(1, 0)) < minAngle)
        {
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
    public List<MazeCell> mazeCellPath;
    public int currentIndex = 0;
    private bool isMoving = false;


    // Start is called before the first frame update
    void Start()
    {
        SetNewTargetToHallway();
    }

    void Update()
    {
        directionToPlayer = (playerLocation.position - transform.position).normalized;
        switch (state)
        {
            default:
            case State.Pathfinding:
                if (MoveUpdate())
                {
                    switch (targetType)
                    {
                        default:
                        case TargetType.Hallway:
                            targetType = TargetType.Nook;
                            SetNewTargetToNook(FindNook());
                            break;
                        case TargetType.Nook:
                            state = State.Creeping;
                            break;
                        case TargetType.Escape:
                        case TargetType.Drone:
                            SetNewTargetToHallway();
                            break;
                        case TargetType.Player:
                            state = State.PermanentChase;
                            break;
                    }
                }
                if (playerIsLooking())
                {
                    state = State.Escaping;
                    speed = 15f;
                    targetType = TargetType.Escape;
                    SetNewTargetToRandom();
                }
                break;
            case State.Creeping:
                if (StopCreeping())
                {
                    state = State.Pathfinding;
                    SetNewTargetToNook(FindNook());
                }
                break;
            case State.Escaping:
                MoveUpdate();
                if (!playerIsLooking())
                    playerLOSResetTime += Time.deltaTime;
                if(playerLOSResetTime > 2f)
                {
                    state = State.Pathfinding;
                    speed = 5f;
                }
                if (playerIsLooking())
                    playerLOSResetTime = 0f;
                break;
            case State.PermanentChase:
                if (playerIsLooking())
                    speed = 2f;
                else
                    speed = 7f;
                SetNewTargetToPlayer();
                if (MoveUpdate())
                    state = State.Kill;
                break;
            case State.Kill:
                //Wont have to change states from here.
                break;
        }
        if (isPlayerLOS()) // --> function that checks if player LOS - This will only update the direction probabilties if the monster can see the player
        {
            UpdateDir(FindClosestDir());
        }
        if (DroneCheck()) // -> Checks to see if the drone has put out a signalss
            SetNewTargetToDrone();
    }


    bool isPlayerLOS()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, directionToPlayer);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f) && hit.transform == playerLocation)
            return true;
        return false;
    }

    bool playerIsLooking()
    {
        return Vector3.Angle(-directionToPlayer, playerLOS) < 30 && isPlayerLOS();
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
        mazeCellPath = path;
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
        time += Time.deltaTime;
        if (time > 3)
        {
            time = 0f;
            return ReturnProb(currHallway) < Random.value;
        }
        return false;
    }

    public void SetMainNodes(MazeCell room, MazeCell down, MazeCell left, MazeCell right)
    {
        playerRoom = room;
        room.playerRoom = true;
        downHallway = down;
        leftHallway = left;
        rightHallway = right;
        Hallways[0] = DirProbabilities.Down;
        Hallways[1] = DirProbabilities.Left;
        Hallways[2] = DirProbabilities.Right;
    }

    public MazeCell GetNode(DirProbabilities dir)
    {
        switch (dir)
        {
            default:
            case DirProbabilities.Down: return downHallway;
            case DirProbabilities.Left: return leftHallway;
            case DirProbabilities.Right: return rightHallway;
        }
    }
    void SetNewTargetToHallway()
    {
        targetType = TargetType.Hallway;
        float minProb = 1f;
        DirProbabilities hallway = DirProbabilities.Up;
        foreach (DirProbabilities dir in Hallways)
        {
            if (ReturnProb(dir) < minProb)
            {
                minProb = ReturnProb(dir);
                hallway = dir;
            }
        }
        currHallway = hallway;
        MazeCell h = GetNode(currHallway);
        manager.UpdatePathfinding(new Vector2Int(h.x, h.y));
    }

    MazeCell FindNook()
    {
        targetType = TargetType.Nook;
        Vector2Int currPos = new Vector2Int(playerRoom.x, mazeCellPath[currentIndex - 1].y);
        Vector2Int mainDir = Vector2Int.up;
        bool checkingUp = false;
        switch (currHallway)
        {
            case DirProbabilities.Down:
            default:
                break;
            case DirProbabilities.Left:
                mainDir = Vector2Int.right;
                currPos = new Vector2Int(mazeCellPath[currentIndex - 1].x, playerRoom.y);
                checkingUp = true;
                break;
            case DirProbabilities.Right:
                mainDir = Vector2Int.left;
                currPos = new Vector2Int(mazeCellPath[currentIndex - 1].x, playerRoom.y);
                checkingUp = true;
                break;
        }
        bool nookFound = false;
        currPos += mainDir;
        MazeCell curr = generator.cells[currPos.x, currPos.y];
        Debug.Log("Going direction (" + mainDir.x + ", " + mainDir.y + "), and checking up is " + checkingUp);
        Debug.Log("Current is: (" + curr.x + ", " + curr.y + ")");
        Debug.Log("Player Room is: (" + playerRoom.x + ", " + playerRoom.y + ")");
        while (!nookFound && (Mathf.Abs(curr.y - playerRoom.y) > 2 || Mathf.Abs(curr.x - playerRoom.x) > 2))
        {
            Debug.Log("Went into loop");
            if (checkingUp)
            {
                Debug.Log("Checking Up and down at (" + curr.x + ", " + curr.y + ")");
                if(curr.CheckDirection(new Vector2Int(0, 1))){
                    nookFound = true;
                    curr = generator.cells[currPos.x, currPos.y + 1];
                }
                else if (curr.CheckDirection(new Vector2Int(0, -1)))
                {
                    nookFound = true;
                    curr = generator.cells[currPos.x, currPos.y - 1];
                }
            }
            else
            {
                Debug.Log("Checking Right and Left at (" + curr.x + ", " + curr.y + ")");
                if (curr.CheckDirection(new Vector2Int(1, 0)))
                {
                    nookFound = true;
                    curr = generator.cells[currPos.x + 1, currPos.y];
                }
                else if(curr.CheckDirection(new Vector2Int(-1, 0)))
                {
                    nookFound = true;
                    curr = generator.cells[currPos.x -1, currPos.y];
                }
            }
            if (!nookFound)
            {
                currPos += mainDir;
                curr = generator.cells[currPos.x, currPos.y];
            }
        }
        if (!nookFound)
        {
            curr = playerRoom;
            targetType = TargetType.Player;
        }
        return curr;
        
    }

    void SetNewTargetToNook(MazeCell nook)
    {
        manager.UpdatePathfinding(new Vector2Int(nook.x, nook.y));
    }

    void SetNewTargetToRandom()
    {
        int w = Random.Range(0, Mathf.FloorToInt(generator.width / 3));
        int h = Random.Range(0, Mathf.FloorToInt(generator.height / 3));
        w = Random.Range(0, 1) * (generator.width - 1 - 2 * w) + w;
        manager.UpdatePathfinding(new Vector2Int(w, h));
    }

    void SetNewTargetToPlayer()
    {
        float edge = (float)generator.cellLength / 2f;
        int x = 1;
        int y = 1;
        while(edge < playerLocation.position.x || edge < playerLocation.position.z)
        {
            edge += generator.cellLength;
            if (edge < playerLocation.position.x)
                x++;
            if (edge < playerLocation.position.z)
                y++;
        }
        generator.cells[x, y].playerRoom = true;
        Debug.Log("Player is at (" + x + ", " + y + ")");
        manager.UpdatePathfinding(new Vector2Int(x, y));
    }


    bool DroneCheck()
    {
        if(drone.signal && state != State.PermanentChase && !isPlayerLOS())
        {
            drone.signal = false;
            return true;
        }
        return false;
    }
    void SetNewTargetToDrone()
    {
        state = State.Pathfinding;
        targetType = TargetType.Drone;
        float edge = (float)generator.cellLength / 2f;
        int x = 1;
        int y = 1;
        while(edge < drone.posX || edge < drone.posY)
        {
            edge += generator.cellLength;
            if(edge < drone.posX)
                x++;
            if(edge < drone.posY)
                y++;
        }
        Debug.Log("Going to drone at (" + x + ", " + y + ")");
        manager.UpdatePathfinding(new Vector2Int(x, y));
    }
}
