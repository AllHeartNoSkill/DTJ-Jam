using Godot;
using System;

public partial class CameraSystem : Node3D
{
	private Node3D _playerNode;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_playerNode = GetParent<Node3D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("capture"))
		{
			ShootCamera();
		}
	}

	private void ShootCamera()
	{
		MissionSystem.Instance.CheckMission(_playerNode);
	}
}
