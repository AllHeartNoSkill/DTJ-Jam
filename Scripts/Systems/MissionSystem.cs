using Godot;
using System;
using System.Collections.Generic;

public partial class MissionSystem : Node3D
{
	public static MissionSystem Instance;

	[ExportGroup("Mission Tolerance")]
	[Export] private float _distanceTolerance = 0.5f;
	[Export] private float _angleTolerance = 15f;
	
	public List<Node3D> PhotoPoints { get; private set; }

	private int _photoDoneCOunter = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		PhotoPoints = new List<Node3D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void SetPhotoPosition(Node3D point)
	{
		AddChild(point);
		PhotoPoints.Add(point);
	}

	public void CheckMission(Node3D playerNode)
	{
		int chosenIndex = 0;
		Node3D closestNode = GetClosestPointNode(out float closestDistance);
		float angleDiff = GetAngleDifference();
		
		GD.Print($"Closes Distance {closestDistance} : angle diff {angleDiff}");
		if (CheckIfComplete())
		{
			CompleteMission(closestNode, chosenIndex);
		}
		
		Node3D GetClosestPointNode(out float distance)
		{
			float smallestDistance = Mathf.Inf;
			Node3D chosenNode = PhotoPoints[0];

			for (int i = 0; i < PhotoPoints.Count; i++)
			{
				var point = PhotoPoints[i];
				if(!point.Visible) continue;
				float dist = (point.GlobalPosition - playerNode.GlobalPosition).LengthSquared();
				if (dist < smallestDistance)
				{
					chosenNode = point;
					chosenIndex = i;
					smallestDistance = dist;
				}

			}

			distance = smallestDistance;
			return chosenNode;
		}

		float GetAngleDifference()
		{
			return (playerNode.RotationDegrees - closestNode.RotationDegrees).Length();
		}

		bool CheckIfComplete()
		{
			return closestDistance <= _distanceTolerance && angleDiff <= _angleTolerance;
		}
	}

	private void CompleteMission(Node3D missionNode, int index)
	{
		GD.Print($"Mission Complete On {missionNode.Name} index {index}");
		missionNode.Hide();
		GameEvents.Instance.MissionComplete(index);
		_photoDoneCOunter += 1;

		if (PhotoPoints.Count <= _photoDoneCOunter)
		{
			GameComplete();
		}
	}

	private void GameComplete()
	{
		GameEvents.Instance.EndGame();
	}
}
