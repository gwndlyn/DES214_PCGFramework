using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PCG : MonoBehaviour
{
    public float GridSize = 5.0f; //Size of floor and wall tiles in units
    public Vector2Int MaxMapSize = new Vector2Int(50, 50); //Maximum width and height of tile map

    public int RoomsToSpawn = 13;

    private Dictionary<string, GameObject> Prefabs; //Dictionary of all PCG prefabs
    private GameObject[,] TileMap; //Tilemap array to make sure we don't put walls over floors
    private System.Random RNG;

    private List<RoomInfo> RoomInfoList = new List<RoomInfo>();
    private List<CorridorInfo> CorridorInfoList = new List<CorridorInfo>();

    public enum PHASE
    {
        SETUP,
        HOOK,
        DEVELOPMENT,
        TURN,
        RESOLUTION
    };
    private PHASE Phase = PHASE.SETUP;

    public class RoomInfo
    {
        public Vector2Int RoomSize;
        public Vector2Int OriginPos;
        public PHASE RoomPhase;

        public RoomInfo(Vector2Int roomSize, Vector2Int ogPos)
        {
            RoomSize = roomSize;
            OriginPos = ogPos;
        }
    };

    public class CorridorInfo
    {
        public List<Vector2Int> VertexPoints = new List<Vector2Int>();
    };

    public enum DIRECTION
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    // Start is called before the first frame update
    void Start()
    {
        //Load all the prefabs we need for map generation (note that these must be in a "Resources" folder)
        Prefabs = new Dictionary<string, GameObject>();
        Prefabs.Add("floor", Resources.Load<GameObject>("Prefabs/Floor"));
        Prefabs["floor"].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the floor properly
        Prefabs.Add("special", Resources.Load<GameObject>("Prefabs/FloorSpecial"));
        Prefabs["special"].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the floor properly
        Prefabs.Add("wall", Resources.Load<GameObject>("Prefabs/Wall"));
        Prefabs["wall"].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the wall properly
        Prefabs.Add("portal", Resources.Load<GameObject>("Prefabs/Portal"));
        Prefabs.Add("enemy", Resources.Load<GameObject>("Prefabs/BaseEnemy"));
        Prefabs.Add("fast", Resources.Load<GameObject>("Prefabs/FastEnemy"));
        Prefabs.Add("spread", Resources.Load<GameObject>("Prefabs/SpreadEnemy"));
        Prefabs.Add("tank", Resources.Load<GameObject>("Prefabs/TankEnemy"));
        Prefabs.Add("ultra", Resources.Load<GameObject>("Prefabs/UltraEnemy"));
        Prefabs.Add("boss", Resources.Load<GameObject>("Prefabs/BossEnemy"));
        Prefabs.Add("heart", Resources.Load<GameObject>("Prefabs/HeartPickup"));
        Prefabs.Add("healthboost", Resources.Load<GameObject>("Prefabs/HealthBoost"));
        Prefabs.Add("shotboost", Resources.Load<GameObject>("Prefabs/ShotBoost"));
        Prefabs.Add("speedboost", Resources.Load<GameObject>("Prefabs/SpeedBoost"));
        Prefabs.Add("silverkey", Resources.Load<GameObject>("Prefabs/SilverKey"));
        Prefabs.Add("goldkey", Resources.Load<GameObject>("Prefabs/GoldKey"));
        Prefabs.Add("silverdoor", Resources.Load<GameObject>("Prefabs/SilverDoor"));
        Prefabs["silverdoor"].transform.localScale = new Vector3(GridSize / 2.0f, 1.0f, 1.0f); //Scale the door properly
        Prefabs.Add("golddoor", Resources.Load<GameObject>("Prefabs/GoldDoor"));
        Prefabs["golddoor"].transform.localScale = new Vector3(GridSize / 2.0f, 1.0f, 1.0f); //Scale the door properly

        //Delete everything visible except the hero when reloading       
        var objsToDelete = FindObjectsOfType<SpriteRenderer>();
        int totalObjs = objsToDelete.Length;
        for (int i = 0; i < totalObjs; i++)
        {
            if (objsToDelete[i].gameObject.ToString().StartsWith("Hero") == false)
                UnityEngine.Object.DestroyImmediate(objsToDelete[i].gameObject);
        }

        //Create the tile map
        TileMap = new GameObject[MaxMapSize.x, MaxMapSize.y];
        RNG = new System.Random();

        SpawnEdgeWalls();

        RoomInfo currRoom = new RoomInfo(new Vector2Int(3, 3), new Vector2Int(2, 2));
        int minRoomPerPhase = RoomsToSpawn / 5;
        int minRoomPerPhaseRemainder = RoomsToSpawn % 5;
        int remainderCheck = minRoomPerPhaseRemainder % 2;
        int setupPhaseRooms = minRoomPerPhase;
        int devPhaseRooms = minRoomPerPhase;

        if (minRoomPerPhaseRemainder == 1)
            devPhaseRooms += 1;
        else if (remainderCheck % 2 == 1)
        {
            devPhaseRooms += minRoomPerPhaseRemainder / 2 + 1;
            setupPhaseRooms += minRoomPerPhaseRemainder / 2;
        }
        else
        {
            devPhaseRooms += minRoomPerPhaseRemainder / 2;
            setupPhaseRooms += minRoomPerPhaseRemainder / 2;
        }

        int currRoomInPhase = 0;
        for (int i = 0; i < RoomsToSpawn; ++i)
        {
            //create the room first
            if (i < RoomsToSpawn - 1)
                SpawnRoom(currRoom, true);
            else
                SpawnRoom(currRoom, false);

            currRoom.RoomPhase = Phase;
            RoomInfoList.Add(currRoom);

            switch (Phase)
            {
                case PHASE.SETUP:
                    {
                        if (i > 0)
                            EnemySpawner("enemy", currRoom, 2);

                        if (currRoomInPhase == setupPhaseRooms)
                        {
                            currRoomInPhase = 0;
                            Phase = PHASE.HOOK;
                        }
                        break;
                    }
                case PHASE.HOOK:
                    {
                        EnemySpawner("enemy", currRoom, 1);
                        EnemySpawner("fast", currRoom, 2);

                        if (currRoomInPhase == minRoomPerPhase)
                        {
                            currRoomInPhase = 0;
                            Phase = PHASE.DEVELOPMENT;
                        }
                        break;
                    }
                case PHASE.DEVELOPMENT:
                    {
                        EnemySpawner("enemy", currRoom, 1);
                        EnemySpawner("fast", currRoom, 2);
                        EnemySpawner("tank", currRoom, 1);

                        if (currRoomInPhase == devPhaseRooms)
                        {
                            currRoomInPhase = 0;
                            Phase = PHASE.TURN;
                        }
                        break;
                    }
                case PHASE.TURN:
                    {
                        EnemySpawner("fast", currRoom, 2);
                        EnemySpawner("tank", currRoom, 1);
                        EnemySpawner("boss", currRoom, 1);

                        if (currRoomInPhase == minRoomPerPhase)
                        {
                            currRoomInPhase = 0;
                            Phase = PHASE.RESOLUTION;
                        }
                        break;
                    }
                case PHASE.RESOLUTION:
                    {
                        EnemySpawner("enemy", currRoom, 2);

                        break;
                    }
                default:
                    break;
            }

            ++currRoomInPhase;

            currRoom = DecideNextRoomPos();

        }

        SpawnCorridors();
        FillInWalls();

    }

    // PCG Room Helper Functions ------------------------------------------------------------------------------

    void SpawnRoom(RoomInfo roomInfo, bool hasCorridor)
    {
        //push starting to lower left
        Vector2Int lowerLeftPos = roomInfo.OriginPos;
        lowerLeftPos.x -= (lowerLeftPos.x - 1) / 2;
        lowerLeftPos.y -= (lowerLeftPos.y - 1) / 2;

        //create floor tile
        for (int x = 0; x < roomInfo.RoomSize.x; ++x)
            for (int y = 0; y < roomInfo.RoomSize.y; ++y)
                SpawnFloorTile(new Vector2Int(lowerLeftPos.x + x, lowerLeftPos.y + y));

        //for last tile
        if (hasCorridor)
            Spawn("portal", roomInfo.OriginPos);
    }

    RoomInfo DecideNextRoomPos()
    {
        bool mapBoundsIsSafe = false;

        Vector2Int roomSize = new Vector2Int();
        Vector2Int nextOriginPos = new Vector2Int();

        while (!mapBoundsIsSafe)
        {
            //decide  room size
            roomSize.x = RNG.Next(1, 4) * 2 + 1;
            roomSize.y = RNG.Next(1, 4) * 2 + 1;

            Vector2Int halfRoomSize = new Vector2Int((roomSize.x - 1) / 2, (roomSize.y - 1) / 2);

            //decide room pos
            nextOriginPos.x = RNG.Next(0, MaxMapSize.x);
            nextOriginPos.y = RNG.Next(0, MaxMapSize.y);

            //bounds checking
            if ((nextOriginPos.x - halfRoomSize.x <= 0) 
                || (nextOriginPos.y - halfRoomSize.y <= 0)
                || (nextOriginPos.x + halfRoomSize.x >= MaxMapSize.x) 
                || (nextOriginPos.y + halfRoomSize.y >= MaxMapSize.y))
            {
                //need to check if there are already rooms here
                foreach (var room in RoomInfoList)
                {
                    Vector2Int halfRoomTemp = new Vector2Int((room.RoomSize.x - 1) / 2, (room.RoomSize.y - 1) / 2);
                    if (((room.OriginPos.x - halfRoomTemp.x <= nextOriginPos.x - halfRoomSize.x)
                        && (room.OriginPos.y - halfRoomTemp.y <= nextOriginPos.y - halfRoomSize.x))
                        || ((room.OriginPos.x + halfRoomTemp.x >= nextOriginPos.x + halfRoomSize.x)
                        && ((room.OriginPos.y + halfRoomTemp.y >= nextOriginPos.y + halfRoomSize.y))))
                        continue;
                }

                break;
            }
        }

        RoomInfo retInfo = new RoomInfo(roomSize, nextOriginPos);
        return retInfo;
    }

    void SpawnCorridors()
    {

    }

    void SpawnEdgeWalls()
    {
        for (int x = 0; x < MaxMapSize.x; ++x)
        {
            for (int y = 0; y < MaxMapSize.y; ++y)
            {
                if (x == 0 || y == 0 || x == MaxMapSize.x - 1 || y == MaxMapSize.y - 1)
                    Spawn("wall", new Vector2Int(x, y));
            }
        }
    }

    void FillInWalls()
    {
        for (int x = 0; x < MaxMapSize.x; ++x)
        {
            for (int y = 0; y < MaxMapSize.y; ++y)
            {
                Vector2Int size = new Vector2Int(x, y);
                GameObject obj = GetTile(size);

                if (obj == null)
                    Spawn("wall", size);
            }
        }
    }

    // General Spawn Helper Functions ------------------------------------------------------------------------------

    void EnemySpawner(string type, RoomInfo roominfo, int count)
    {
        Vector2Int[] spawnPosArr = new Vector2Int[count];
        int x = roominfo.RoomSize.x / count;
        int y = roominfo.RoomSize.y / count;

        if (count > 1)
        {
            //for (int i = 0; i < count; ++i)
            //    spawnPosArr[i] = roominfo.LowerLeftPos + i * new Vector2Int(x, y);
        }
        else if (count == 1)
        {
            //spawnPosArr[0] = new Vector2Int(roominfo.LowerLeftPos.x + roominfo.RoomSize.x / 2, roominfo.LowerLeftPos.y + roominfo.RoomSize.y / 2);
        }

        for (int i = 0; i < count; ++i)
            Spawn(type, spawnPosArr[i]);
    }


    // General Spawn Helper Functions ------------------------------------------------------------------------------

    //Get a tile object (only walls and floors, currently)
    GameObject GetTile(Vector2Int pos)
    {
        if (Math.Abs(pos.x) > MaxMapSize.x || Math.Abs(pos.y) > MaxMapSize.y)
            return Prefabs["wall"];
        return TileMap[pos.x, pos.y];
    }

    //Spawn a tile object if one isn't already there
    void SpawnFloorTile(Vector2Int pos)
    {
        if (GetTile(pos) != null)
            return;
        TileMap[pos.x, pos.y] = Spawn("floor", pos);
    }

    //Spawn any object
    GameObject Spawn(string obj, Vector2Int pos)
    {
        return Instantiate(Prefabs[obj], new Vector3(pos.x * GridSize, pos.y * GridSize, 0.0f), Quaternion.identity);
    }

    //Spawn any object rotated 90 degrees left
    GameObject SpawnRotateLeft(string obj, float x, float y)
    {
        return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.AngleAxis(-90, Vector3.forward));
    }

    //Spawn any object rotated 90 degrees right
    GameObject SpawnRotateRight(string obj, float x, float y)
    {
        return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.AngleAxis(90, Vector3.forward));
    }
}
