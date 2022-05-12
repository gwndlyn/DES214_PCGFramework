﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PCG : MonoBehaviour
{
    public float GridSize = 5.0f; //Size of floor and wall tiles in units
    public Vector2Int MaxMapSize = new Vector2Int(50, 50); //Maximum width and height of tile map

    public int RoomsToSpawn = 8;

    private Dictionary<string, GameObject> Prefabs; //Dictionary of all PCG prefabs
    private GameObject[,] TileMap; //Tilemap array to make sure we don't put walls over floors
    private int TileMapMidPoint; //The 0,0 point of the tile map array
    private System.Random RNG;

    private enum PHASE
    {
        STARTING,
        SETUP,
        HOOK,
        DEVELOPMENT,
        TURN,
        RESOLUTION
    };
    private PHASE Phase = PHASE.STARTING;

    public class RoomInfo
    {
        public Vector2Int NextTilePos;
        public bool IsVertical;

        public RoomInfo(Vector2Int nextTilePos, bool isVertical)
        {
            NextTilePos = nextTilePos;
            IsVertical = isVertical;
        }
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
        TileMapMidPoint = (MaxMapSize.x * MaxMapSize.y) / 2;
        RNG = new System.Random();

        SpawnEdgeWalls();

        RoomInfo currRoom = new RoomInfo(new Vector2Int(3, 3), true);
        for (int i = 0; i < RoomsToSpawn; ++i)
        {
            currRoom = (i < RoomsToSpawn - 1) 
                ? SpawnRoom(currRoom, true) 
                : SpawnRoom(currRoom, false);
        }

        FillInWalls();

    }

    // PCG Room Helper Functions ------------------------------------------------------------------------------

    RoomInfo SpawnRoom(RoomInfo roomInfo, bool hasCorridor)
    {
        //get room size
        int roomSizeX = RNG.Next(1, 3) * 2 + 1;
        int roomSizeY = RNG.Next(1, 3) * 2 + 1;

        //push starting to lower left
        Vector2Int roomTilePos = roomInfo.NextTilePos;
        if (roomInfo.IsVertical)
            roomTilePos.x -= (roomSizeX - 1) / 2;
        else
            roomTilePos.y -= (roomSizeY - 1) / 2;

        //create floor tile
        for (int x = 0; x < roomSizeX; ++x)
            for (int y = 0; y < roomSizeY; ++y)
                SpawnFloorTile(new Vector2Int(roomTilePos.x + x, roomTilePos.y + y));

        //get corridor info
        bool isVertical = RNG.Next(0, 2) == 0;

        if (isVertical)
        {
            roomTilePos.x += ((roomSizeX - 1) / 2);
            roomTilePos.y += roomSizeY;
            if(!hasCorridor)
                Spawn("portal", roomTilePos);
            SpawnFloorTile(roomTilePos);
            ++roomTilePos.y;
        }
        else
        {
            roomTilePos.x += roomSizeX;
            roomTilePos.y += ((roomSizeY - 1) / 2);
            if (!hasCorridor)
                Spawn("portal", roomTilePos);
            SpawnFloorTile(roomTilePos);
            ++roomTilePos.x;
        }
        return new RoomInfo(roomTilePos, isVertical);

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