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
	[Export] private bool _rotateBuilding = false;
	
	[ExportGroup("Misc Object Parameters")]
	[Export] private float _miscObjectRadiusMultiplier = 3f;
	[Export] private int _miscObjectRejectionSamples = 30;
	[Export] private Array<LevelObjectData> _miscObjectDatas = new Array<LevelObjectData>();
	[Export(PropertyHint.Layers3DPhysics)] private uint _miscObjectRayMask;
	[Export] private bool _rotateMiscObject = false;

	[ExportGroup("Point Generation")]
	[Export] private int _photoPointCount = 5;
	[Export] private float _playerHeight = 1.5f;
	[Export(PropertyHint.Layers3DPhysics)] private uint _groundRayMask;

	private Vector2 _regionSize = new Vector2();
	private int[,] _levelGrid;
	private float _gridCellSize;
		
	private List<LevelGrid> _spawnedGrids = new List<LevelGrid>();
	private Node3D _levelPivot;
	
	private PhysicsDirectSpaceState3D _worldSpace;
	private PhysicsRayQueryParameters3D _rayParams = new PhysicsRayQueryParameters3D();

	private LevelScene _levelScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_worldSpace = GetWorld3D().DirectSpaceState;
		_levelScene = GetParent<LevelScene>();
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
		await Task.Delay(1000);
		GenerateImportantPoints();
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
		List<Vector2> spawnedPoints = PoissonDiscSampling.GenerateLevelObjects(
			_buildingDatas,
			_regionSize,
			_levelGrid,
			_gridCellSize,
			_buildingRadiusMultiplier,
			_buildingRejectionSamples,
			out var objectsToSpawn,
			out var rotationList);

		PlaceObjects(objectsToSpawn, spawnedPoints, rotationList, _rotateBuilding, _buildingRayMask);
	}

	private void GenerateMiscObject()
	{
		_levelGrid = new int[_levelGridSize.X, _levelGridSize.Y];
		List<Vector2> spawnedPoints = PoissonDiscSampling.GenerateLevelObjects(
			_miscObjectDatas,
			_regionSize,
			_levelGrid,
			_gridCellSize,
			_miscObjectRadiusMultiplier,
			_miscObjectRejectionSamples,
			out var objectsToSpawn,
			out var rotationList);

		PlaceObjects(objectsToSpawn, spawnedPoints, rotationList, _rotateMiscObject, _miscObjectRayMask);
	}

	private void GenerateImportantPoints()
	{
		List<Vector2> spawnedPoints = PoissonDiscSampling.GeneratePoints(
			10f,
			_regionSize,
			30);
		
		var random = new RandomNumberGenerator();
		random.Randomize();

		Vector3 playerSpawnPosition = new Vector3();
		List<Vector3> photoPositions = new List<Vector3>();
		ChoosePlayerSpawnPosition();
		ChoosePhotoPosition();

		Node3D playerSpawnNode = new Node3D();
		playerSpawnNode.Name = "Player Spawn Position Node";
		_levelScene.SetPlayerPositionNode(playerSpawnNode);
		playerSpawnNode.GlobalPosition = playerSpawnPosition;
		
		GeneratePhotoPosition();

		void ChoosePlayerSpawnPosition()
		{
			foreach (var point in spawnedPoints)
			{
				if (TestPoint(point, out playerSpawnPosition))
				{
					GD.Print($"PlayerSpawnPosition: {playerSpawnPosition}");
					spawnedPoints.Remove(point);
					return;
				}
			}
		}

		void ChoosePhotoPosition()
		{
			for (int i = 0; i < _photoPointCount; i++)
			{
				bool foundOne = false;
				int counter = 0;
				while (!foundOne && counter < 10)
				{
					int chosenIndex = random.RandiRange(0, spawnedPoints.Count - 1);
					if (TestPoint(spawnedPoints[chosenIndex], out Vector3 chosenPoint))
					{
						GD.Print($"Photo Position ke-{i}: {chosenPoint}");
						photoPositions.Add(chosenPoint);
						spawnedPoints.RemoveAt(chosenIndex);
						foundOne = true;
					}

					counter += 1;
				}
			}
		}

		bool TestPoint(Vector2 point, out Vector3 worldPos)
		{
			Vector3 startRay = new Vector3(point.X, _skyHeight, point.Y);
			Vector3 targetRay = new Vector3(point.X, -1f, point.Y);
			_rayParams.From = startRay;
			_rayParams.To = targetRay;
			_rayParams.CollisionMask = _groundRayMask;
			_rayParams.CollideWithBodies = true;
			var result = _worldSpace.IntersectRay(_rayParams);
			
			if (result.Count == 0)
			{
				worldPos = Vector3.Zero;
				return false;
			}

			worldPos = result["position"].AsVector3();
			Node colliderOther = (Node)result["collider"].AsGodotObject();
			
			if (colliderOther.IsInGroup("Ground"))
			{
				return true;
			}

			return false;
		}

		void GeneratePhotoPosition()
		{
			for (int i = 0; i < photoPositions.Count; i++)
			{
				Node3D node = new Node3D();
				node.Name = $"Photo Position {i}";
				MissionSystem.Instance.SetPhotoPosition(node);
				node.GlobalPosition = photoPositions[i] + (Vector3.Up * _playerHeight);
				RotateToFarthestDirection(node);
			}
		}

		void RotateToFarthestDirection(Node3D point)
		{
			float farthestDistance = -1f;
			Vector3 lookDirection = new Vector3();
			float angleIncrement = 10f;
			for (int i = 0; i < 36; i++)
			{
				Vector3 startRay = point.Position;
				Vector3 direction =
					new Vector3(Mathf.Sin(angleIncrement * i), 0, Mathf.Cos(angleIncrement * i));
				Vector3 targetRay = startRay + direction * _regionSize.X;
				_rayParams.From = startRay;
				_rayParams.To = targetRay;
				_rayParams.CollisionMask = _groundRayMask;
				_rayParams.CollideWithBodies = true;
				var result = _worldSpace.IntersectRay(_rayParams);
			
				if (result.Count > 0)
				{
					Vector3 worldPos = result["position"].AsVector3();
					float currDist = (worldPos - startRay).LengthSquared();
					if (currDist > farthestDistance)
					{
						farthestDistance = currDist;
						lookDirection = worldPos;
					}
				}
				else
				{
					farthestDistance = _regionSize.X;
					lookDirection = targetRay;
				}
			}
			GD.Print($"{point.Name} Farthest distance: {farthestDistance} Look Direction: {lookDirection}");
			point.LookAt(lookDirection, Vector3.Up);
		}
	}

	private void PlaceObjects(List<LevelObject> objectsToSpawn, List<Vector2> spawnedPoints, List<Vector2> rotationList, bool rotateObject, uint collisionMask, bool collideWithBodies = true)
	{
		for (int i = 0; i < objectsToSpawn.Count; i++)
		{
			Vector2 desiredPos = spawnedPoints[i];
			LevelObject levelObject = objectsToSpawn[i];
			
			Vector3 startRay = new Vector3(desiredPos.X, _skyHeight, desiredPos.Y);
			Vector3 targetRay = new Vector3(desiredPos.X, -1f, desiredPos.Y);
			_rayParams.From = startRay;
			_rayParams.To = targetRay;
			_rayParams.CollisionMask = collisionMask;
			_rayParams.CollideWithBodies = collideWithBodies;
			var result = _worldSpace.IntersectRay(_rayParams);

			if (result.Count == 0)
			{
				GD.PrintErr("No Object Below To Place!");
				return;
			}
			
			AddChild(levelObject);
			Vector3 objectPosition = result["position"].AsVector3();
			levelObject.Position = objectPosition;

			if (rotateObject)
			{
				Vector2 rotationVec = rotationList[i];
				Vector3 lookVector = new Vector3(rotationVec.X, 0, rotationVec.Y);
				levelObject.LookAt(lookVector + objectPosition, Vector3.Up);
			}
		}
	}
}
