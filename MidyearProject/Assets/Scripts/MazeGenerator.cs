using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
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
            for(int j = 0; j < height; j++)
            {
                cells[i, j] = new MazeCell(i, j, new Vector3(i * cellLength,0, j * cellLength));
            }
        }
        GenerateMaze();
        gameManager.Func();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || (Player.transform.position - EndCube.transform.position).magnitude < 2)
        {
            gameManager.Reset();
            ClearMaze();
            for(int i = 0; i < width; i++)
            {
                for (int j = 0;j < height; j++)
                {
                    cells[i,j].ResetDirections();
                }
            }
            GenerateMaze();
            gameManager.Func();
        }
    }

    void GenerateMaze()
    {
        CreateGrid();
        visitedCells[0, 0] = true;
        StartCoroutine(Generate(0f, 0f));
        endPos = CarveExit();
        CarveOutPlayerRoom();
        startPos = RandomizePlayer();
        Debug.Log("Generated Maze");
    }

    void CreateGrid()
    {
        Vector3 pos = Vector3.zero;
        pos.y = wallSize / 2;
        for(int i = -1; i < width; i++)
        {
            for(int j = -1; j < width; j++)
            {
                pos.x = i * cellLength;
                pos.z = j * cellLength;
                if(j != -1)
                {
                    CreateWall(1f, cellLength, pos + new Vector3(cellLength/2, 0, 0));
                }
                if(i != -1)
                {
                    CreateWall(cellLength, 1f, pos + new Vector3(0, 0, cellLength/2));
                }
            }
        }
        CreateWall(cellLength * width, cellLength * height, new Vector3((width - 1) * cellLength / 2, -wallSize / 2, (height - 1) * cellLength / 2));
    }

    void CreateWall(float w, float l, Vector3 pos)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = pos;
        Transform trans = wall.transform;
        trans.localScale = new Vector3(trans.localScale.x * w, trans.localScale.y, trans.localScale.z * l);
        wall.transform.localScale = trans.localScale;
        walls.Add(wall);
    }
    IEnumerator Generate(float fx, float fy)
    {
        int x = (int) fx;
        int y = (int)fy;
        while (UncheckedNeighbors(x, y).Count > 0)
        {
            List<Vector2Int> neighbors = UncheckedNeighbors(x, y);
            int index = UnityEngine.Random.Range(0, neighbors.Count);
            Vector2Int n = neighbors[index];
            RemoveWall(x * cellLength, y * cellLength, n.x * cellLength, n.y * cellLength);
            visitedCells[n.x, n.y] = true;
            cells[x , y].AlterDirection(n - new Vector2Int(x, y), true);
            cells[n.x, n.y].AlterDirection(-(n - new Vector2Int(x, y)), true);
            StartCoroutine(Generate(n.x, n.y));
        }
        yield return new WaitForSeconds(1f);
    }

    List<Vector2Int> UncheckedNeighbors(int x, int y)
    {
        List<Vector2Int> l = new List<Vector2Int>();
        if (x < width - 1)
            if (!visitedCells[x+1, y])
                l.Add(new Vector2Int(x + 1, y));
        if (x > 0)
            if (!visitedCells[x-1, y])
                l.Add(new Vector2Int(x - 1, y));
        if(y < height - 1)
            if (!visitedCells[x, y+1])
                l.Add(new Vector2Int(x, y+1));
        if (y > 0)
            if (!visitedCells[x, y-1])
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
            if((wall.transform.position - midpoint).magnitude < minDistance)
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
        for(int i = walls.Count - 1; i>=0; i--)
        {
            GameObject wall = walls[i];
            walls.Remove(wall);
            Destroy(wall);
        }
        visitedCells = new bool[width, height];
    }

    Vector2Int CreateEndCube()
    {
        int RandX = UnityEngine.Random.Range(0, width);
        int RandZ = UnityEngine.Random.Range(0, height);
        int RandEdge = UnityEngine.Random.Range(1, 5);
        Vector3 endPos = new Vector3(RandX * cellLength, wallSize / 2, RandZ * cellLength);
        Vector2Int result = new Vector2Int();
        switch (RandEdge)
        {
            case 1:
                endPos.x = 0;
                result = new Vector2Int(0, RandZ);
                break;
            case 2:
                endPos.x = (width - 1) * cellLength;
                result = new Vector2Int((width - 1), RandZ);
                break;
            case 3:
                endPos.z = 0;
                result = new Vector2Int(RandX, 0);
                break;
            case 4:
                endPos.z = (height - 1) * cellLength;
                result = new Vector2Int(RandX, (height - 1));
                break;
        }
        EndCube.transform.position = endPos;
        return result;
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
        
    }
    void CarveOutPlayerRoom()
    {
        for(int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 2; i++)
            {
                //Clear out Center
                cells[roomX + i, roomY + j].AlterDirection(new Vector2Int(1, 0), true);
                cells[roomX + i + 1, roomY + j].AlterDirection(new Vector2Int(-1, 0), true);
                RemoveWall((roomX + i) * cellLength, (roomY + j) * cellLength, (roomX + i + 1) * cellLength, (roomY + j) * cellLength);
                cells[roomX + j, roomY + i].AlterDirection(new Vector2Int(0, 1), true);
                cells[roomX + j, roomY + i + 1].AlterDirection(new Vector2Int(0, -1), true);
                RemoveWall((roomX + j) * cellLength, (roomY + i) * cellLength, (roomX + j) * cellLength, (roomY + i + 1) * cellLength);
            }
            //Build surrounding walls

        }

    }
    void CarveOutPaths()
    {
        
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
}
