using Godot;
using System;

public partial class GameEvents : Node3D
{
	public static GameEvents Instance;

	public event Action OnStartGame;
	[Signal]
	public delegate void StartGameSignalEventHandler();
	public void StartGame()
	{
		EmitSignal("StartGameSignal");
		OnStartGame?.Invoke();
	}
	
	public event Action OnEndGame;
	[Signal]
	public delegate void EndGameSignalEventHandler();
	public void EndGame()
	{
		EmitSignal("EndGameSignal");
		OnEndGame?.Invoke();
	}
	
	public event Action OnLevelLoaded;
	[Signal]
	public delegate void LevelLoadedSignalEventHandler();
	public void LevelLoaded()
	{
		EmitSignal("LevelLoadedSignal");
		OnLevelLoaded?.Invoke();
	}
	
	public event Action<int> OnMissionComplete;
	[Signal]
	public delegate void MissionCompleteSignalEventHandler(int index);
	public void MissionComplete(int index)
	{
		EmitSignal("MissionCompleteSignal", index);
		OnMissionComplete?.Invoke(index);
	}
	
	public override void _Ready()
	{
		Instance = this;
	}
	
}
