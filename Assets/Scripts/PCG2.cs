using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PHASE
{
    SETUP,
    HOOK,
    DEVELOPMENT,
    TURN,
    RESOLUTION,
    ALTERNATE
};

public enum DIRECTION
{
    NORTH,
    SOUTH,
    EAST,
    WEST
};

public class PCG2 : MonoBehaviour
{
    //GO refs
    public GameObject RoomPrefab;
    public GameObject PortalPrefab;

    public GameObject EnemyNormal;
    public GameObject EnemyFast;
    public GameObject EnemyBoss;
    public GameObject EnemySniper;
    public GameObject EnemySpread;
    public GameObject EnemyTank;
    public GameObject EnemyUltra;

    public GameObject BoostHealth;
    public GameObject BoostSpeed;
    public GameObject BoostShot;

    //other vars
    public int MaxDungeonWidth = 50;
    public int MaxDungeonHeight = 50;

    //private PHASE Phase = PHASE.SETUP;
    public BaseRoomStats[,] RoomStats;
    private int[] NumberOfRoomsPerPhase;
    private int NumOfAltPaths;

    private Vector3 prevRoomPos;
    private int currTotalRoomWidth = 0;
    private int startY = 25;
    private int startYMinusOne;
    private int startYPlusOne;

    // Start is called before the first frame update
    void Start()
    {
        RoomStats = new BaseRoomStats[MaxDungeonWidth, MaxDungeonHeight];

        startYMinusOne = startY - 1;
        startYPlusOne = startY + 1;

        GenerateRoomsPerPhase();

        //create dungeon
        CreateDungeon();
        CreateAlternatePath();
        KnockDownWalls();
    }

    void GenerateRoomsPerPhase()
    {
        NumberOfRoomsPerPhase = new int[5];
        NumberOfRoomsPerPhase[0] = Random.Range(2, 4);
        NumberOfRoomsPerPhase[1] = Random.Range(1, 3);
        NumberOfRoomsPerPhase[2] = 4 + Random.Range(6, 12);
        NumberOfRoomsPerPhase[3] = Random.Range(3, 8);
        NumberOfRoomsPerPhase[4] = Random.Range(2, 4);

        NumOfAltPaths = Random.Range(4, 10);
    }

    void CreateDungeonByPhase(PHASE phase, int rangeX, int rangeY)
    {
        for (int i = currTotalRoomWidth; i < currTotalRoomWidth + NumberOfRoomsPerPhase[(int)phase]; ++i)
        {
            prevRoomPos = SpawnRoom(i, startY, phase);

            int sideRoomRoll = Random.Range(rangeX, rangeY);
            for (int j = startYMinusOne; j > startYMinusOne - sideRoomRoll; --j)
            {
                prevRoomPos = SpawnRoom(i, j, phase);
            }

            sideRoomRoll = Random.Range(rangeX, rangeY);
            for (int j = startYPlusOne; j < startYPlusOne + sideRoomRoll; ++j)
            {
                prevRoomPos = SpawnRoom(i, j, phase);
            }
        }
        currTotalRoomWidth += NumberOfRoomsPerPhase[(int)phase];
    }

    void CreateDungeon()
    {
        prevRoomPos = SpawnRoom(0, startY, PHASE.SETUP);
        ++currTotalRoomWidth;

        CreateDungeonByPhase(PHASE.SETUP, 1, 3);
        CreateDungeonByPhase(PHASE.HOOK, 1, 2);
        CreateDungeonByPhase(PHASE.DEVELOPMENT, 1, 5);
        CreateDungeonByPhase(PHASE.TURN, 1, 4);
        CreateDungeonByPhase(PHASE.RESOLUTION, 1, 3);

        prevRoomPos = SpawnRoom(currTotalRoomWidth++, startY, PHASE.RESOLUTION);
        Instantiate(PortalPrefab, new Vector3(prevRoomPos.x * 10f, prevRoomPos.y * 10f, 0.0f), Quaternion.identity);
    }

    void CreateAlternatePath()
    {
        for (int i = 0; i < NumOfAltPaths; ++i)
        {
            bool dir = Random.Range(0, 2) == 0;
            int posY = Random.Range(2, MaxDungeonHeight / 3);
            posY = dir ? startY + posY : startY - posY;
            int posXLeft = Random.Range(1, currTotalRoomWidth / 2);
            int posXRight = posXLeft + Random.Range(2, (currTotalRoomWidth - 1) / 2);

            //pillars first
            if (dir) //going upwards
            {
                for (int y = startY; y < posY; ++y)
                {
                    if (RoomStats[posXLeft, y] == null)
                        prevRoomPos = SpawnRoom(posXLeft, y, PHASE.DEVELOPMENT);
                    if (RoomStats[posXRight, y] == null)
                        prevRoomPos = SpawnRoom(posXRight, y, PHASE.DEVELOPMENT);
                }
            }
            else
            {
                for (int y = startY; y > posY; --y)
                {
                    if (RoomStats[posXLeft, y] == null)
                        prevRoomPos = SpawnRoom(posXLeft, y, PHASE.DEVELOPMENT);
                    if (RoomStats[posXRight, y] == null)
                        prevRoomPos = SpawnRoom(posXRight, y, PHASE.DEVELOPMENT);
                }
            }

            //connect the rop
            for (int x = posXLeft; x < posXRight + 1; ++x)
            {
                if (RoomStats[x, posY] == null)
                    prevRoomPos = SpawnRoom(x, posY, PHASE.DEVELOPMENT);
            }
        }
    }

    Vector3 SpawnRoom(int x, int y, PHASE phase)
    {
        GameObject room = Instantiate(RoomPrefab, new Vector3(x * 10.0f, y * 10.0f, 0.0f), Quaternion.identity);

        BaseRoomStats roomStats = room.GetComponent<BaseRoomStats>();

        RoomStats[x, y] = roomStats;
        roomStats.Phase = phase;

        return new Vector3((float)x, (float)y, 0.0f);

    }

    void KnockDownWalls()
    {
        for (int x = 0; x < MaxDungeonWidth; ++x)
        {
            for (int y = 0; y < MaxDungeonHeight; ++y)
            {
                if (RoomStats[x, y] != null)
                {
                    if (x < MaxDungeonWidth - 1 && RoomStats[x + 1, y] != null)
                        RoomStats[x, y].DestroyWallRequest(DIRECTION.EAST);

                    if (x > 0 && RoomStats[x - 1, y] != null)
                        RoomStats[x, y].DestroyWallRequest(DIRECTION.WEST);

                    if (y < MaxDungeonHeight - 1 && RoomStats[x, y + 1] != null)
                        RoomStats[x, y].DestroyWallRequest(DIRECTION.NORTH);

                    if (y > 0 && RoomStats[x, y - 1] != null)
                        RoomStats[x, y].DestroyWallRequest(DIRECTION.SOUTH);

                    //destroy corners
                    if (RoomStats[x, y].Phase == PHASE.TURN)
                    {
                        if (RoomStats[x - 1, y + 1] != null && RoomStats[x - 1, y] != null && RoomStats[x, y + 1] != null)
                            RoomStats[x, y].DestroyCorner(BaseRoomStats.CORNERS.TOPLEFT);
                        if (RoomStats[x + 1, y + 1] != null && RoomStats[x, y + 1] != null && RoomStats[x + 1, y] != null)
                            RoomStats[x, y].DestroyCorner(BaseRoomStats.CORNERS.TOPRIGHT);
                        if (RoomStats[x - 1, y - 1] != null && RoomStats[x, y - 1] != null && RoomStats[x - 1, y] != null)
                            RoomStats[x, y].DestroyCorner(BaseRoomStats.CORNERS.BOTTOMLEFT);
                        if (RoomStats[x + 1, y - 1] != null && RoomStats[x, y - 1] != null && RoomStats[x + 1, y] != null)
                            RoomStats[x, y].DestroyCorner(BaseRoomStats.CORNERS.BOTTOMRIGHT);
                    }

                }
            }
        }
    }


    void EnemySpawner(Object type, Vector3 pos, int count)
    {
        for (int i = 0; i < count; ++i)
            Instantiate(type, new Vector3(pos.x * 10f, pos.y * 10f, 0.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
