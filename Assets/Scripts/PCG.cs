using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PCG : MonoBehaviour
{
	public float GridSize = 5.0f; //Size of floor and wall tiles in units
	public int MaxMapSize = 41; //Maximum height and width of tile map
	private	Dictionary<string, GameObject> Prefabs; //Dictionary of all PCG prefabs
	private	GameObject[] TileMap; //Tilemap array to make sure we don't put walls over floors
	private	int TileMapMidPoint; //The 0,0 point of the tile map array
	private System.Random RNG;
	
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
		Prefabs["silverdoor"].transform.localScale = new Vector3(GridSize/2.0f, 1.0f, 1.0f); //Scale the door properly
		Prefabs.Add("golddoor", Resources.Load<GameObject>("Prefabs/GoldDoor"));
		Prefabs["golddoor"].transform.localScale = new Vector3(GridSize/2.0f, 1.0f, 1.0f); //Scale the door properly

        //Delete everything visible except the hero when reloading       
		var objsToDelete = FindObjectsOfType<SpriteRenderer>();
		int totalObjs = objsToDelete.Length;
		for (int i = 0; i < totalObjs; i++)
		{
			if (objsToDelete[i].gameObject.ToString().StartsWith("Hero") == false)
				UnityEngine.Object.DestroyImmediate(objsToDelete[i].gameObject);
		}
			
		//Create the tile map
		TileMap = new GameObject[MaxMapSize*MaxMapSize];
		TileMapMidPoint = (MaxMapSize*MaxMapSize)/2;
		RNG = new System.Random();

		//Create the starting tile
		SpawnTile(0, 0);

        //Add more PCG logic here...
    }
	
	//Get a tile object (only walls and floors, currently)
	GameObject GetTile(int x, int y)
	{
		if (Math.Abs(x) > MaxMapSize/2 || Math.Abs(y) > MaxMapSize/2)
			return Prefabs["wall"];
		return TileMap[(y * MaxMapSize) + x + TileMapMidPoint];
	}

	//Spawn a tile object if one isn't already there
	void SpawnTile(int x, int y)
	{
		if (GetTile(x,y) != null)
			return;
		TileMap[(y * MaxMapSize) + x + TileMapMidPoint] = Spawn("floor", x, y);
	}

	//Spawn any object
	GameObject Spawn(string obj, float x, float y)
	{
		return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.identity);
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
