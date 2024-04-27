using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace DTJJam.Scripts.Levels;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30) {
	    var random = new RandomNumberGenerator();
	    random.Randomize();
	    
		float cellSize = radius/Mathf.Sqrt(2);

		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.X/cellSize), Mathf.CeilToInt(sampleRegionSize.Y/cellSize)];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2> { sampleRegionSize/2 };

		while (spawnPoints.Count > 0) {
			int spawnIndex = random.RandiRange(0,spawnPoints.Count - 1);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = random.Randf() * Mathf.Pi * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * random.RandfRange(radius, 2*radius);
				if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) {
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.X/cellSize),(int)(candidate.Y/cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}
		}

		return points;
	}

	static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
		if (candidate.X >=0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y) {
			int cellX = (int)(candidate.X / cellSize);
			int cellY = (int)(candidate.Y / cellSize);
			int searchRadius = Mathf.CeilToInt(radius / cellSize);
			int searchStartX = Mathf.Max(0,cellX - searchRadius);
			int searchEndX = Mathf.Min(cellX + searchRadius,grid.GetLength(0) - 1);
			int searchStartY = Mathf.Max(0,cellY - searchRadius);
			int searchEndY = Mathf.Min(cellY + searchRadius,grid.GetLength(1) - 1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x, y] - 1;
					if (pointIndex != - 1) {
						float sqrDst = (candidate - points[pointIndex]).LengthSquared();
						if (sqrDst < radius * radius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
	
	public static Godot.Collections.Dictionary<LevelObject, Vector2> GenerateLevelObjects(Array<LevelObjectData> levelObjects, Vector2 sampleRegionSize, int[,] grid, float cellSize, float radiusMultiplier, int numSamplesBeforeRejection = 30) {
		var random = new RandomNumberGenerator();
		random.Randomize();

		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2> { sampleRegionSize / 2 };
		Godot.Collections.Dictionary<LevelObject, Vector2> spawnedLevelObjects =
			new Godot.Collections.Dictionary<LevelObject, Vector2>();

		while (spawnPoints.Count > 0) {
			int spawnIndex = random.RandiRange(0,spawnPoints.Count - 1);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			int objectIndex = random.RandiRange(0, levelObjects.Count - 1);
			LevelObjectData chosenObjectData = levelObjects[objectIndex];

			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = random.Randf() * Mathf.Pi * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * random.RandfRange(chosenObjectData.ObjectGridSize * radiusMultiplier, 2 * chosenObjectData.ObjectGridSize * radiusMultiplier);
				if (IsValid(candidate, sampleRegionSize, cellSize, chosenObjectData.ObjectGridSize * radiusMultiplier, points, grid)) {
					points.Add(candidate);
					spawnPoints.Add(candidate);
					LevelObject spawnedObject = (LevelObject)chosenObjectData.LevelObjectPrefab.Instantiate();
					spawnedLevelObjects.Add(spawnedObject, candidate);
					
					grid[(int)(candidate.X / cellSize),(int)(candidate.Y / cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}
		}

		return spawnedLevelObjects;
	}
}