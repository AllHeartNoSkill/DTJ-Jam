using Godot;
using System;
using System.Collections.Generic;

public partial class LevelScene : Node3D
{
	[Export] private LevelGenerator _levelGenerator;
	[Export] private Node3D _levelPivot;
	
	public Node3D PlayerSpawnNode { get; private set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_levelGenerator.GenerateLevel(_levelPivot);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetPlayerPositionNode(Node3D point)
	{
		AddChild(point);
		PlayerSpawnNode = point;
	}
}
