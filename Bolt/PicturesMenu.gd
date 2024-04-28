extends Control

var pictures_menu: Control

var textures: Array

# Called when the node enters the scene tree for the first time.
func _ready():
	pictures_menu = get_node(".")
	textures = []
	for i in range(5):
		var textureNode = get_node("MarginContainer/VBoxContainer/GridContainer/TextureRect"+ str(i+1))
		textureNode.texture = load("res://TargetTextures/texture" + str(i) + ".jpg")
		textures.append(textureNode)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("pictures")):
		pictures_menu.show()
	if (Input.is_action_just_released(("pictures"))):
		pictures_menu.hide()
	pass
