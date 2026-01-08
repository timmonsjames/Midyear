using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

// Coordinates the whole mini-game:
// 1. Generates maze
// 2. Runs A*
// 3. Spawns NPC and Goal
// 4. Gives path to NPC
// 5. Visualizes the path

public class MazeGameManager : MonoBehaviour
{
    public Transform fakePlayerTransform;
    public GameObject gem;
    public MazeGenerator mazeGenerator;
    public AStarPathfinder pathfinder;
    public Monster monster;
    public Transform player;
    public Transform playerLOS;
    public DroneMovement drone;

    public GameObject npcPrefab;
    public GameObject goalPrefab;
    public GameObject pathTilePrefab;

    private GameObject npcInstance;
    private GameObject goalInstance;
    private List<GameObject> pathList = new List<GameObject>();

    public void Start()
    {
        // BEGIN NIGHTTIME AMBIENCE

    }

    public void Func()
    {
        // 1. Generate maze
        //Should happen
        // 2. Run A* to get path
        List<MazeCell> path = pathfinder.FindPath();

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("No path found, cannot start game.");
            mazeGenerator.ResetEverything();
            return;
        }

        // 3. Spawn NPC at start
        MazeCell startCell = path[0];
        npcInstance = Instantiate(npcPrefab, startCell.worldPosition, Quaternion.identity);

        // 4. Spawn Goal at end
        MazeCell goalCell = path[path.Count - 1];
        goalInstance = mazeGenerator.EndCube;

        // 5. Give path to NPC movement script
        monster = npcInstance.GetComponent<Monster>();
        monster.manager = this;
        monster.generator = mazeGenerator;
        monster.playerLocation = player;
        monster.drone = drone;
        monster.gem = gem;
        monster.fakePlayerLocation = fakePlayerTransform;
        monster.SetPath(path);
        List<MazeCell> mainNodes = mazeGenerator.GetMainNodes();
        monster.SetMainNodes(mainNodes[0], mainNodes[1], mainNodes[2], mainNodes[3]);
        // 6. Visualize the path
        VisualizePath(path);
    }

    void VisualizePath(List<MazeCell> path)
    {
        for(int i = 0; i<path.Count; i++)
        {
            MazeCell cell = path[i];
            Vector3 pos = cell.worldPosition + new Vector3(0, 0.01f, 0);
            GameObject b = Instantiate(pathTilePrefab, pos, Quaternion.identity);
            pathList.Add(b);
        }
    }
    public void Reset()
    {
        Destroy(npcInstance);
        for (int i = pathList.Count - 1; i >= 0; i--)
        {
            GameObject f = pathList[i];
            pathList.Remove(f);
            Destroy(f);
        }
    }

    void Update()
    {
        monster.UpdatePlayerLOS(player.position - playerLOS.position);
        CheckPlayerWin();
    }

    public void UpdatePathfinding(Vector2Int goalPos)
    {
        for (int i = pathList.Count - 1; i >= 0; i--)
        {
            GameObject f = pathList[i];
            pathList.Remove(f);
            Destroy(f);
        }
        if(monster.currentIndex >= monster.mazeCellPath.Count)
            monster.currentIndex = monster.mazeCellPath.Count - 1;
        mazeGenerator.startPos = new Vector2Int(monster.mazeCellPath[monster.currentIndex].x, monster.mazeCellPath[monster.currentIndex].y);
        mazeGenerator.endPos = goalPos;
        List<MazeCell> path = pathfinder.FindPath();
        monster.SetPath(path);
        VisualizePath(path);
    }
    
    void CheckPlayerWin()
    {
        float min = -mazeGenerator.cellLength / 2 - 0.01f;
        float max = mazeGenerator.cellLength * (mazeGenerator.width + 0.5f) + 0.01f;
        if (player.transform.position.x < min || player.transform.position.x > max || player.transform.position.z < min || player.transform.position.z > max)
            max = 80085;   //CALL FUNCTION FOR WIN SCENE && ADD TRUMPET TO WIN SCENE
    }
}
