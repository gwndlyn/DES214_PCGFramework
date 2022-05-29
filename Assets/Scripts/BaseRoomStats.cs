using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoomStats : MonoBehaviour
{
    public PHASE Phase = PHASE.SETUP;
    public int RoomNumber = 0;

    public GameObject Floor;
    public GameObject NorthWall;
    public GameObject SouthWall;
    public GameObject WestWall;
    public GameObject EastWall;

    public GameObject NorthDoorway;
    public GameObject SouthDoorway;
    public GameObject WestDoorway;
    public GameObject EastDoorway;

    public GameObject TopLeftCorner;
    public GameObject TopRightCorner;
    public GameObject BottomLeftCorner;
    public GameObject BottomRightCorner;

    public enum CORNERS
    {
        TOPLEFT,
        TOPRIGHT,
        BOTTOMLEFT,
        BOTTOMRIGHT
    };

    // Start is called before the first frame update
    public void Start()
    {
        SpriteRenderer floorRenderer = Floor.GetComponent<SpriteRenderer>();

        Vector4 tempColor = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0.6f);
        floorRenderer.color = tempColor;
    }

    public void DestroyWallRequest(DIRECTION dir)
    {
        if (Phase == PHASE.SETUP)
        {
            DestroyWall(dir);
        }
        if (Phase == PHASE.HOOK)
        {
            DestroyDoorway(dir);
        }
        if (Phase == PHASE.DEVELOPMENT)
        {
            bool wallOrDoorway = Random.Range(0, 4) == 0;
            if (wallOrDoorway)
                DestroyDoorway(dir);
            else
                DestroyWall(dir);
        }
        if (Phase == PHASE.TURN)
        {
            DestroyWall(dir);
        }
        if (Phase == PHASE.RESOLUTION)
        {
            DestroyDoorway(dir);
        }
    }

    public void DestroyDoorway(DIRECTION dir)
    {
        switch (dir)
        {
            case DIRECTION.NORTH:
                {
                    Destroy(NorthDoorway);
                    break;
                }
            case DIRECTION.SOUTH:
                {
                    Destroy(SouthDoorway);
                    break;
                }
            case DIRECTION.EAST:
                {
                    Destroy(EastDoorway);
                    break;
                }
            case DIRECTION.WEST:
                {
                    Destroy(WestDoorway);
                    break;
                }
        }
    }

    public void DestroyWall(DIRECTION dir)
    {
        switch (dir)
        {
            case DIRECTION.NORTH:
                {
                    Destroy(NorthWall);
                    break;
                }
            case DIRECTION.SOUTH:
                {
                    Destroy(SouthWall);
                    break;
                }
            case DIRECTION.EAST:
                {
                    Destroy(EastWall);
                    break;
                }
            case DIRECTION.WEST:
                {
                    Destroy(WestWall);
                    break;
                }
        }
    }

    public void DestroyCorner(CORNERS corner)
    {
        if (corner == CORNERS.TOPLEFT)
            Destroy(TopLeftCorner);
        if (corner == CORNERS.TOPRIGHT)
            Destroy(TopRightCorner);
        if (corner == CORNERS.BOTTOMLEFT)
            Destroy(BottomLeftCorner);
        if (corner == CORNERS.BOTTOMRIGHT)
            Destroy(BottomRightCorner);
    }


    // Update is called once per frame
    public void Update()
    {

    }

}
