using Godot;
using System;
using System.Collections.Generic;

public partial class LevelScene : Node3D
{
	[Export] private LevelGenerator _levelGenerator;
	[Export] private Node3D _levelPivot;

	private Node3D _playerNode;
	public Node3D PlayerSpawnNode { get; private set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Root.Ready += Start;
		_playerNode = GetNode<Node3D>("%Player");
	}

	private void Start()
	{
		GameEvents.Instance.OnStartGame += OnStartGame;
	}

	public override void _ExitTree()
	{
		GameEvents.Instance.OnStartGame -= OnStartGame;
	}

	private void OnStartGame()
	{
		GenerateLevelThenSpawnPlayer();
	}

	private async void GenerateLevelThenSpawnPlayer()
	{
		await _levelGenerator.GenerateLevel(_levelPivot);
		_playerNode.Position = PlayerSpawnNode.Position;
		_playerNode.ProcessMode = ProcessModeEnum.Inherit;
	}

	public void SetPlayerPositionNode(Node3D point)
	{
		AddChild(point);
		PlayerSpawnNode = point;
	}
}
