using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTJJam.Scripts.Levels;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class LevelGenerator : Node3D
{
	[ExportGroup("Level Parameters")] 
	[Export] private Vector2I _groundGridSize = new Vector2I();
	[Export] private PackedScene _levelGridPrefab;
	[Export] private float _levelPrefabSize = 25f;
	[Export] private Vector2I _levelGridSize = new Vector2I();
	[Export] private float _skyHeight = 10f;

	[ExportGroup("Building Parameters")]
	[Export] private float _buildingRadiusMultiplier = 3f;
	[Export] private int _buildingRejectionSamples = 30;
	[Export] private Array<LevelObjectData> _buildingDatas = new Array<LevelObjectData>();
	[Export(PropertyHint.Layers3DPhysics)] private uint _buildingRayMask;
	
	[ExportGroup("Misc Object Parameters")]
	[Export] private float _miscObjectRadiusMultiplier = 3f;
	[Export] private int _miscObjectRejectionSamples = 30;
	[Export] private Array<LevelObjectData> _miscObjectDatas = new Array<LevelObjectData>();
	[Export(PropertyHint.Layers3DPhysics)] private uint _miscObjectRayMask;

	private Vector2 _regionSize = new Vector2();
	private int[,] _levelGrid;
	private float _gridCellSize;
		
	private List<LevelGrid> _spawnedGrids = new List<LevelGrid>();
	private Node3D _levelPivot;
	
	private PhysicsDirectSpaceState3D _worldSpace;
	private PhysicsRayQueryParameters3D _rayParams = new PhysicsRayQueryParameters3D();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_worldSpace = GetWorld3D().DirectSpaceState;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void GenerateLevel(Node3D levelPivot)
	{
		_levelPivot = levelPivot;
		
		GenerateGround();
		await Task.Delay(1000);
		GenerateBuilding();
		await Task.Delay(1000);
		GenerateMiscObject();
	}

	private void GenerateGround()
	{
		Vector3 startPosition = new Vector3(
			_levelPrefabSize / 2f,
			0f,
			_levelPrefabSize / 2f);
		
		for (int row = 0; row < _groundGridSize.Y; row++)
		{
			for (int col = 0; col < _groundGridSize.X; col++)
			{
				LevelGrid spawnedGrid = _levelGridPrefab.Instantiate<LevelGrid>();
				_levelPivot.AddChild(spawnedGrid);

				spawnedGrid.Position = new Vector3(
					startPosition.X + _levelPrefabSize * col, 
					startPosition.Y,
					startPosition.Z + _levelPrefabSize * row);
				
				_spawnedGrids.Add(spawnedGrid);
			}
		}

		_regionSize.X = _levelPrefabSize * (_groundGridSize.X);
		_regionSize.Y = _levelPrefabSize * (_groundGridSize.Y);

		_levelGrid = new int[_levelGridSize.X, _levelGridSize.Y];
		_gridCellSize = _regionSize.X / _levelGridSize.X;
	}

	private void GenerateBuilding()
	{
		_levelGrid = new int[_levelGridSize.X, _levelGridSize.Y];
		Godot.Collections.Dictionary<LevelObject, Vector2> objectsToSpawn = PoissonDiscSampling.GenerateLevelObjects(
			_buildingDatas,
			_regionSize,
			_levelGrid,
			_gridCellSize,
			_buildingRadiusMultiplier,
			_buildingRejectionSamples);

		PlaceObjects(objectsToSpawn, false, _buildingRayMask);
	}

	private void GenerateMiscObject()
	{
		_levelGrid = new int[_levelGridSize.X, _levelGridSize.Y];
		Godot.Collections.Dictionary<LevelObject, Vector2> objectsToSpawn = PoissonDiscSampling.GenerateLevelObjects(
			_miscObjectDatas,
			_regionSize,
			_levelGrid,
			_gridCellSize,
			_miscObjectRadiusMultiplier,
			_miscObjectRejectionSamples);

		PlaceObjects(objectsToSpawn, true, _miscObjectRayMask);
	}

	private void PlaceObjects(Godot.Collections.Dictionary<LevelObject, Vector2> objectsToSpawn, bool rotateObject, uint collisionMask, bool collideWithBodies = true)
	{
		foreach (var objectSpawnData in objectsToSpawn)
		{
			Vector2 desiredPos = objectSpawnData.Value;
			LevelObject levelObject = objectSpawnData.Key;
			
			Vector3 startRay = new Vector3(desiredPos.X, _skyHeight, desiredPos.Y);
			Vector3 targetRay = new Vector3(desiredPos.X, -1f, desiredPos.Y);
			_rayParams.From = startRay;
			_rayParams.To = targetRay;
			_rayParams.CollisionMask = collisionMask;
			_rayParams.CollideWithBodies = collideWithBodies;
			var result = _worldSpace.IntersectRay(_rayParams);

			if (result.Count > 0)
			{
				AddChild(levelObject);
				levelObject.Position = result["position"].AsVector3();
			}
			
		}
	}
}
