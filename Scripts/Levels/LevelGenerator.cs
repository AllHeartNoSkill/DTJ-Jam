using Godot;
using System;
using System.Collections.Generic;
using DTJJam.Scripts.Levels;

public partial class LevelGenerator : Node3D
{
	[ExportGroup("Level Parameters")] 
	[Export] private Vector2 _levelGridSize = new Vector2();
	[Export] private PackedScene _levelGridPrefab;
	[Export] private float _levelPrefabSize = 25f;

	[ExportGroup("Building Parameters")]
	[Export] private float _buildingRadius = 1f;
	[Export] private int _buildingRejectionSamples = 30;
	[Export] private MeshInstance3D _temporaryPoint;

	private Vector2 _regionSize = new Vector2();
		
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
		
		for (int row = 0; row < _levelGridSize.Y; row++)
		{
			for (int col = 0; col < _levelGridSize.X; col++)
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

		_regionSize.X = _levelPrefabSize * (_levelGridSize.X);
		_regionSize.Y = _levelPrefabSize * (_levelGridSize.Y);
	}

	private void GenerateBuilding()
	{
		List<Vector2> points = PoissonDiscSampling.GeneratePoints(_buildingRadius, _regionSize, _buildingRejectionSamples);
		foreach (var point in points)
		{
			MeshInstance3D myMesh = (MeshInstance3D)_temporaryPoint.Duplicate();
			AddChild(myMesh);
			myMesh.Visible = true;
			myMesh.Position = new Vector3(point.X, 0f, point.Y);
		}
	}
}
