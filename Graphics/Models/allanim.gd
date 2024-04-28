extends Node3D
var tree: AnimationTree

# Called when the node enters the scene tree for the first time.
func _ready():
	tree = get_node("AnimationTree")
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("capture")):
		tree.set("parameters/capture/request", AnimationNodeOneShot.ONE_SHOT_REQUEST_FIRE)
	pass
