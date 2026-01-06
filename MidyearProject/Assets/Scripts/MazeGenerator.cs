using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject EndCube;
    public GameObject Player;
    public int width;
    public int height;
    public float cellLength;
    public float wallSize;
    public bool[,] visitedCells;
    public List<GameObject> walls = new List<GameObject>();
    public Vector2Int startPos;
    public Vector2Int endPos;
    public MazeCell[,] cells;
    public MazeGameManager gameManager;

    //Bottom Left Corner (Lowest X and Lowest Y)
    int roomX;
    int roomY;

    // Start is called before the first frame update
    void Start()
    {
        roomX = width / 2 - 1;
        roomY = height / 2 - 1;
        visitedCells = new bool[width, height];
        cells = new MazeCell[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i, j] = new MazeCell(i, j, new Vector3(i * cellLength, 0, j * cellLength));
            }
        }
        GenerateMaze();
        gameManager.Func();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //ResetEverything();
            //endPos = new Vector2Int(Random.Range(0, width - 1), Random.Range(0, height - 1));
            //gameManager.UpdatePathfinding(endPos);
        }
    }

    public void ResetEverything()
    {
        gameManager.Reset();
        ClearMaze();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i, j].ResetDirections();
            }
        }
        GenerateMaze();
        gameManager.Func();
    }
    void GenerateMaze()
    {
        CreateGrid();
        visitedCells[0, 0] = true;
        Generate(0f, 0f);
        CarveExit();
        CarveOutPlayerRoom();
        CarveOutPaths();
        startPos = RandomizePlayer();
        Debug.Log("Generated Maze");
    }

    void CreateGrid()
    {
        Vector3 pos = Vector3.zero;
        pos.y = wallSize / 2;
        for (int i = -1; i < width; i++)
        {
            for (int j = -1; j < width; j++)
            {
                pos.x = i * cellLength;
                pos.z = j * cellLength;
                if (j != -1)
                {
                    CreateWall(1f, cellLength, pos + new Vector3(cellLength / 2, 0, 0));
                }
                if (i != -1)
                {
                    CreateWall(cellLength, 1f, pos + new Vector3(0, 0, cellLength / 2));
                }
            }
        }
    }

    void CreateWall(float w, float l, Vector3 pos)
    {
        GameObject wall = GameObject.Instantiate(wallPrefab);
        wall.transform.position = pos;
        Transform trans = wall.transform;
        trans.localScale = new Vector3(trans.localScale.x * w, trans.localScale.y * wallSize, trans.localScale.z * l);
        wall.transform.localScale = trans.localScale;
        walls.Add(wall);
    }
    void Generate(float fx, float fy)
    {
        int x = (int)fx;
        int y = (int)fy;
        while (UncheckedNeighbors(x, y).Count > 0)
        {
            List<Vector2Int> neighbors = UncheckedNeighbors(x, y);
            int index = UnityEngine.Random.Range(0, neighbors.Count);
            Vector2Int n = neighbors[index];
            RemoveWall(x * cellLength, y * cellLength, n.x * cellLength, n.y * cellLength);
            visitedCells[n.x, n.y] = true;
            cells[x, y].AlterDirection(n - new Vector2Int(x, y), true);
            cells[n.x, n.y].AlterDirection(-(n - new Vector2Int(x, y)), true);
            Generate(n.x, n.y);
        }
        return;
    }

    List<Vector2Int> UncheckedNeighbors(int x, int y)
    {
        List<Vector2Int> l = new List<Vector2Int>();
        if (x < width - 1)
            if (!visitedCells[x + 1, y])
                l.Add(new Vector2Int(x + 1, y));
        if (x > 0)
            if (!visitedCells[x - 1, y])
                l.Add(new Vector2Int(x - 1, y));
        if (y < height - 1)
            if (!visitedCells[x, y + 1])
                l.Add(new Vector2Int(x, y + 1));
        if (y > 0)
            if (!visitedCells[x, y - 1])
                l.Add(new Vector2Int(x, y - 1));
        return l;
    }
    void RemoveWall(float x1, float y1, float x2, float y2)
    {
        Vector3 midpoint = new Vector3((x1 + x2) / 2, wallSize / 2, (y1 + y2) / 2);
        GameObject choppedWall = walls[0];
        float minDistance = float.MaxValue;
        for (int i = walls.Count - 1; i >= 0; i--)
        {
            GameObject wall = walls[i];
            if ((wall.transform.position - midpoint).magnitude < minDistance)
            {
                choppedWall = wall;
                minDistance = (wall.transform.position - midpoint).magnitude;
            }
        }
        walls.Remove(choppedWall);
        Destroy(choppedWall);
    }

    void ClearMaze()
    {
        for (int i = walls.Count - 1; i >= 0; i--)
        {
            GameObject wall = walls[i];
            walls.Remove(wall);
            Destroy(wall);
        }
        visitedCells = new bool[width, height];
    }
    Vector2Int RandomizePlayer()
    {
        int RandX = UnityEngine.Random.Range(0, width);
        int RandZ = UnityEngine.Random.Range(0, height);
        Player.transform.position = new Vector3(RandX * cellLength, wallSize / 2, RandZ * cellLength);
        return new Vector2Int(RandX, RandZ);
    }

    void CarveExtras()
    {
        int h = Mathf.FloorToInt(3 * height / 4);
        for (int i = Mathf.FloorToInt(width / 3); i <= Mathf.FloorToInt(2 * width / 3); i++)
        {
            if (!cells[i, h].CheckDirection(new Vector2Int(1, 0)))
            {
                cells[i, h].AlterDirection(new Vector2Int(1, 0), true);
                cells[i + 1, h].AlterDirection(new Vector2Int(-1, 0), true);
                RemoveWall(i * cellLength, h * cellLength, (i + 1) * cellLength, h * cellLength);
            }
        }

    }
    void CarveOutPlayerRoom()
    {

        for (int j = 0; j <= 2; j++)
        {
            for (int i = 0; i <= 2; i++)
            {
                cells[roomX + i, roomY + j].playerRoom = true;
                //Clear out Center
                if (i != 2) {
                    if (!cells[roomX + i, roomY + j].CheckDirection(new Vector2Int(1, 0)))
                    {
                        RemoveWall((roomX + i) * cellLength, (roomY + j) * cellLength, (roomX + i + 1) * cellLength, (roomY + j) * cellLength);
                        cells[roomX + i, roomY + j].AlterDirection(new Vector2Int(1, 0), true);
                        cells[roomX + i + 1, roomY + j].AlterDirection(new Vector2Int(-1, 0), true);
                    }
                }
                if (j != 2) {
                    if (!cells[roomX + i, roomY + j].CheckDirection(new Vector2Int(0, 1)))
                    {
                        RemoveWall((roomX + i) * cellLength, (roomY + j) * cellLength, (roomX + i) * cellLength, (roomY + j + 1) * cellLength);
                        cells[roomX + i, roomY + j].AlterDirection(new Vector2Int(0, 1), true);
                        cells[roomX + i, roomY + j + 1].AlterDirection(new Vector2Int(0, -1), true);
                    }
                }
            }
        }
        //Build surrounding walls
        Vector3 pos = new Vector3(0, wallSize / 2, 0);
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cells[roomX - 1, roomY + i].CheckDirection(new Vector2Int(1, 0)))
                {
                    CreateWall(1f, cellLength, pos + new Vector3(roomX * cellLength - cellLength / 2, 0, (roomY + i) * cellLength));
                    cells[roomX - 1, roomY + i].AlterDirection(new Vector2Int(1, 0), false);
                    cells[roomX, roomY + i].AlterDirection(new Vector2Int(-1, 0), false);
                }
                if (cells[roomX + 2, roomY + i].CheckDirection(new Vector2Int(1, 0)))
                {
                    CreateWall(1f, cellLength, pos + new Vector3((roomX + 3) * cellLength - cellLength / 2, 0, (roomY + i) * cellLength));
                    cells[roomX + 2, roomY + i].AlterDirection(new Vector2Int(1, 0), false);
                    cells[roomX + 3, roomY + i].AlterDirection(new Vector2Int(-1, 0), false);
                }
                if (cells[roomX + i, roomY - 1].CheckDirection(new Vector2Int(0, 1)))
                {
                    CreateWall(cellLength, 1f, pos + new Vector3((roomX + i) * cellLength, 0, roomY * cellLength - cellLength / 2));
                    cells[roomX + i, roomY - 1].AlterDirection(new Vector2Int(0, 1), false);
                    cells[roomX + i, roomY].AlterDirection(new Vector2Int(0, -1), false);
                }
                if (cells[roomX + i, roomY + 2].CheckDirection(new Vector2Int(0, 1)))
                {
                    CreateWall(cellLength, 1f, pos + new Vector3((roomX + i) * cellLength, 0, (roomY + 3) * cellLength - cellLength / 2));
                    cells[roomX + i, roomY + 2].AlterDirection(new Vector2Int(0, 1), false);
                    cells[roomX + i, roomY + 3].AlterDirection(new Vector2Int(0, -1), false);
                }
            }
        }

    }
    void CarveOutPaths()
    {
        Vector2Int center = new Vector2Int(roomX + 1, roomY + 1);
        for (int i = 1; i < Mathf.Floor(2 * width / 5); i++)
        {
            if (!cells[center.x + i, center.y].CheckDirection(new Vector2Int(1, 0)))
            {
                cells[center.x + i, center.y].AlterDirection(new Vector2Int(1, 0), true);
                cells[center.x + i + 1, center.y].AlterDirection(new Vector2Int(-1, 0), true);
                RemoveWall((center.x + i) * cellLength, cellLength * center.y, (center.x + i + 1) * cellLength, cellLength * center.y);
            }
            if (!cells[center.x - i, center.y].CheckDirection(new Vector2Int(-1, 0)))
            {
                cells[center.x - i, center.y].AlterDirection(new Vector2Int(-1, 0), true);
                cells[center.x - i - 1, center.y].AlterDirection(new Vector2Int(1, 0), true);
                RemoveWall((center.x - i) * cellLength, cellLength * center.y, (center.x - i - 1) * cellLength, cellLength * center.y);
            }
        }
        for (int i = 1; i < Mathf.Floor(2 * height / 5); i++)
        {
            if (!cells[center.x, center.y - i + 1].CheckDirection(new Vector2Int(0, -1)))
            {
                cells[center.x, center.y - i + 1].AlterDirection(new Vector2Int(0, -1), true);
                cells[center.x, center.y - i].AlterDirection(new Vector2Int(0, 1), true);
                RemoveWall(cellLength * center.x, cellLength * (center.y - i + 1), cellLength * center.x, cellLength * (center.y - i));
            }

        }
    }

    Vector2Int CarveExit()
    {
        Vector2Int exitPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        int randEdge = ((int)Mathf.Floor(Random.Range(0, 4)));
        switch (randEdge)
        {
            default:
            case 0:
                //UP
                exitPos.y = height - 1;
                RemoveWall(exitPos.x * cellLength, exitPos.y * cellLength, exitPos.x * cellLength, (exitPos.y + 1) * cellLength);
                break;
            case 1:
                exitPos.y = 0;
                while (exitPos.x <= width / 2 + 2 && exitPos.x >= width / 2)
                    exitPos.x = Random.Range(0, width);
                RemoveWall(exitPos.x * cellLength, exitPos.y * cellLength, exitPos.x * cellLength, (exitPos.y - 1) * cellLength);
                //DOWN
                break;
            case 2:
                exitPos.x = width - 1;
                while (exitPos.y < height / 2 + 2 && exitPos.y > height / 2)
                    exitPos.y = Random.Range(0, height);
                RemoveWall(exitPos.x * cellLength, exitPos.y * cellLength, (exitPos.x + 1) * cellLength, exitPos.y * cellLength);
                //RIGHT
                break;
            case 3:
                exitPos.x = 0;
                while (exitPos.y < height / 2 + 2 && exitPos.y > height / 2)
                    exitPos.y = Random.Range(0, height);
                RemoveWall(exitPos.x * cellLength, exitPos.y * cellLength, (exitPos.x - 1) * cellLength, exitPos.y * cellLength);
                //LEFT
                break;
        }
        return endPos;
    }
    void PutBallHere(int x, int y)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x * cellLength, wallSize / 2, y * cellLength);
    }

    public List<MazeCell> GetMainNodes()
    {
        List<MazeCell> result = new List<MazeCell>();
        result.Add(cells[roomX + 1, roomY + 1]);
        result.Add(cells[roomX + 1, Mathf.FloorToInt(height / 5)]);
        result.Add(cells[Mathf.FloorToInt(width / 5), roomY + 1]);
        result.Add(cells[Mathf.FloorToInt(4 * width / 5), roomY + 1]);
        return result;
    }
}
