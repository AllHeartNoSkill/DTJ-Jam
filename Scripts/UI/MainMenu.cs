using Godot;
using System;

public partial class MainMenu : Control
{
	private Control _mainMenuNode;
	private Control _restartMenuNode;
	
	public override void _Ready()
	{
		GetTree().Root.Ready += Start;
		_mainMenuNode = GetNode<Control>("MainMenu");
		_restartMenuNode = GetNode<Control>("RestartMenu");
	}

	private void Start()
	{
		GameEvents.Instance.OnEndGame += OnEndGame;
	}

	public override void _ExitTree()
	{
		GameEvents.Instance.OnEndGame -= OnEndGame;
	}

	private void OnEndGame()
	{
		GetNode<Node3D>("%Player").QueueFree();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GD.Print(Input.MouseMode);
		this.Show();
		_restartMenuNode.Show();
	}
	
	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
			GetTree().Quit(); // default behavior
	}

	private void OnPlayButtonPressed()
	{
		GameEvents.Instance.StartGame();
		this.Hide();
		_mainMenuNode.Hide();
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
	}

	private void OnRestartButtonPressed()
	{
		GetTree().ReloadCurrentScene();
	}
}
