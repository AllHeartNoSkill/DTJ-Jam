extends Control

var _is_paused = false
var _pause_menu: Control
var _timer: Control

# Called when the node enters the scene tree for the first time.
func _ready():
	%Continue.pressed.connect(_on_continue_pressed)
	_pause_menu = get_node("PauseMenu")
	_pause_menu.hide()
	_timer = get_node("Timer")
	print("READY")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Input.is_action_just_pressed("escape"):
		if _is_paused:
			_continue()
		else:
			_pause()

func _pause():
	get_tree().paused = true
	_is_paused = true
	_pause_menu.show()

func _continue():
	get_tree().paused = false
	_is_paused = false
	_pause_menu.hide()

func _on_continue_pressed():
	_continue()
	print("Continue")
	
func _on_game_events_start_game_signal():
	_timer.show()
	_timer.start_round()
	
