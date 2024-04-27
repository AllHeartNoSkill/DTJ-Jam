using Godot;
using System;
using System.Collections.Generic;
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

	[ExportGroup("Building Parameters")]
	[Export] private float _buildingRadiusMultiplier = 3f;
	[Export] private int _buildingRejectionSamples = 30;
	[Export] private MeshInstance3D _temporaryPoint;
	[Export] private Array<LevelObjectData> _buildingPrefabs = new Array<LevelObjectData>();

	private Vector2 _regionSize = new Vector2();
	private int[,] _levelGrid;
	private float _gridCellSize;
		
	private List<LevelGrid> _spawnedGrids = new List<LevelGrid>();
	private Node3D _levelPivot;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void GenerateLevel(Node3D levelPivot)
	{
		_levelPivot = levelPivot;
		
		GenerateGround();
		GenerateBuilding();
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
		Godot.Collections.Dictionary<LevelObject, Vector2> objectsToSpawn = PoissonDiscSampling.GenerateLevelObjects(
			_buildingPrefabs,
			_regionSize,
			_levelGrid,
			_gridCellSize,
			_buildingRadiusMultiplier,
			_buildingRejectionSamples);

		foreach (var objectSpawnData in objectsToSpawn)
		{
			AddChild(objectSpawnData.Key);
			objectSpawnData.Key.Position = new Vector3(objectSpawnData.Value.X, 0f, objectSpawnData.Value.Y);
		}
	}

	private void PlaceBuilding(PackedScene buildingPrefab, int posX, int posY)
	{
		
	}
}
