extends Control

var pictures_menu: Control
var textures: Array
var mission_nodes: Array

var menu_ready = false

# Called when the node enters the scene tree for the first time.
func _ready():
	get_node("../SubViewport").generate_texture_signal.connect(_print_textures)
	pictures_menu = get_node(".")
	textures = []

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (!menu_ready):
		return
	if (Input.is_action_just_pressed("pictures")):
		_check_pictures()
		pictures_menu.show()
	if (Input.is_action_just_released(("pictures"))):
		pictures_menu.hide()

func _print_textures():
	for i in range(5):
		mission_nodes.append(get_node("../MissionSystem/Photo Position "+ str(i)))
		var textureNode = get_node("MarginContainer/VBoxContainer/GridContainer/TextureRect"+ str(i+1))
		var image = Image.load_from_file(("res://GeneratedTextures/texture" + str(i) + ".jpg"))
		textureNode.texture = ImageTexture.create_from_image(image)
		textures.append(textureNode)
	menu_ready = true

func _check_pictures():
	for i in range(5):
		print(mission_nodes[i])
