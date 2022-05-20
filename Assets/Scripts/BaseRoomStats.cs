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

    // Start is called before the first frame update
    public void Start()
    {
        SpriteRenderer floorRenderer = Floor.GetComponent<SpriteRenderer>();

        switch (Phase)
        {
            case PHASE.SETUP:
                {
                    floorRenderer.color = Color.red;
                    break;
                }
            case PHASE.HOOK:
                {
                    floorRenderer.color = Color.magenta;
                    break;
                }
            case PHASE.DEVELOPMENT:
                {
                    floorRenderer.color = Color.yellow;
                    break;
                }
            case PHASE.TURN:
                {
                    floorRenderer.color = Color.blue;
                    break;
                }
            case PHASE.RESOLUTION:
                {
                    floorRenderer.color = Color.green;
                    break;
                }
            default:
                break;
        }
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
            DestroyWall(dir);
        }
        if (Phase == PHASE.TURN)
        {
            DestroyDoorway(dir);
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


    // Update is called once per frame
    public void Update()
    {
        
    }
}
