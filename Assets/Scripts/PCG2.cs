using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PHASE
{
    SETUP,
    HOOK,
    DEVELOPMENT,
    TURN,
    RESOLUTION
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
    public GameObject RoomPrefab;
    public GameObject EnemyNormal;
    public GameObject EnemyFast;
    public GameObject EnemyBoss;
    public GameObject PortalPrefab;

    public int MaxDungeonWidth = 20;
    public int MaxDungeonHeight = 20;

    public BaseRoomStats[,] RoomStats;

    private PHASE Phase = PHASE.SETUP;

    // Start is called before the first frame update
    void Start()
    {
        RoomStats = new BaseRoomStats[MaxDungeonWidth, MaxDungeonHeight];

        CreateDungeon();

        KnockDownWalls();
    }

    void CreateDungeon()
    {
        Vector3 temp;

        temp = SpawnRoom(0, 10, PHASE.SETUP);

        temp = SpawnRoom(1, 10, PHASE.SETUP);
        EnemySpawner(EnemyNormal, temp, 1);

        temp = SpawnRoom(2, 10, PHASE.SETUP);
        EnemySpawner(EnemyNormal, temp, 2);

        temp = SpawnRoom(3, 10, PHASE.HOOK);
        EnemySpawner(EnemyNormal, temp, 4);

        int devRoomNum = Random.Range(3, 7);
        for (int i = 4; i < 4 + devRoomNum; ++i)
        {
            temp = SpawnRoom(i, 10, PHASE.DEVELOPMENT);
            EnemySpawner(EnemyNormal, temp, 2);
            EnemySpawner(EnemyFast, temp, 2);

            int sideRoomRoll = Random.Range(0, 10);
            if (sideRoomRoll < 4)
            {
                if (sideRoomRoll % 2 == 0)
                {
                    temp = SpawnRoom(i, 11, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);
                }
                else
                {
                    temp = SpawnRoom(i, 9, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);
                }
            }
            else if (sideRoomRoll < 6)
            {
                if (sideRoomRoll % 2 == 0)
                {
                    temp = SpawnRoom(i, 11, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);

                    temp = SpawnRoom(i, 12, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);
                }
                else
                {
                    temp = SpawnRoom(i, 9, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);

                    temp = SpawnRoom(i, 8, PHASE.DEVELOPMENT);
                    EnemySpawner(EnemyNormal, temp, 2);
                    EnemySpawner(EnemyFast, temp, 2);
                }
            }

        }

        temp = SpawnRoom(4 + devRoomNum, 10, PHASE.TURN);
        EnemySpawner(EnemyNormal, temp, 3);

        temp = SpawnRoom(4 + devRoomNum + 1, 10, PHASE.TURN);
        EnemySpawner(EnemyBoss, temp, 1);

        temp = SpawnRoom(4 + devRoomNum + 2, 10, PHASE.RESOLUTION);
        EnemySpawner(EnemyNormal, temp, 2);

        temp = SpawnRoom(4 + devRoomNum + 3, 10, PHASE.RESOLUTION);
        Instantiate(PortalPrefab, new Vector3(temp.x * 10f, temp.y * 10f, 0.0f), Quaternion.identity);
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

                }
            }
        }
    }


    void EnemySpawner(Object type, Vector3 pos, int count)
    {
        //Vector2Int[] spawnPosArr = new Vector2Int[count];

        for (int i = 0; i < count; ++i)
            Instantiate(type, new Vector3(pos.x * 10f, pos.y * 10f, 0.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
