using System.Collections.Generic;
using UnityEngine;

// Coordinates the whole mini-game:
// 1. Generates maze
// 2. Runs A*
// 3. Spawns NPC and Goal
// 4. Gives path to NPC
// 5. Visualizes the path

public class MazeGameManager : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public AStarPathfinder pathfinder;

    public GameObject npcPrefab;
    public GameObject goalPrefab;
    public GameObject pathTilePrefab;

    private GameObject npcInstance;
    private GameObject goalInstance;
    private List<GameObject> pathList = new List<GameObject>();

    public void Func()
    {
        // 1. Generate maze
        //Should happen
        // 2. Run A* to get path
        List<MazeCell> path = pathfinder.FindPath();

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("No path found, cannot start game.");
            return;
        }

        // 3. Spawn NPC at start
        MazeCell startCell = path[0];
        npcInstance = Instantiate(npcPrefab, startCell.worldPosition, Quaternion.identity);

        // 4. Spawn Goal at end
        MazeCell goalCell = path[path.Count - 1];
        goalInstance = mazeGenerator.EndCube;

        // 5. Give path to NPC movement script
        npcInstance.GetComponent<NPCMovement>().SetPath(path);

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
        for (int i = 0; i < pathList.Count; i++)
        {
            GameObject f = pathList[i];
            pathList.Remove(f);
            Destroy(f);
        }
    }
}
