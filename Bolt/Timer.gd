extends Control

var _label: RichTextLabel
var _time: float = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	_label = get_node("Label")

	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	_time += delta

	var _rounded = int(floor(_time))

	var seconds: int = _rounded % 60
	var minutes: int = (_rounded - seconds) / 60

	_label.text = "%02d:%02d" % [minutes, seconds]

	pass
