extends Control

var pictures_menu: Control

var textures: Array

# Called when the node enters the scene tree for the first time.
func _ready():
	get_node("../SubViewport").generate_texture_signal.connect(_print_textures)
	pictures_menu = get_node(".")
	textures = []

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("pictures")):
		pictures_menu.show()
	if (Input.is_action_just_released(("pictures"))):
		pictures_menu.hide()

func _print_textures():
	for i in range(5):
		var textureNode = get_node("MarginContainer/VBoxContainer/GridContainer/TextureRect"+ str(i+1))
		var image = Image.load_from_file(("res://GeneratedTextures/texture" + str(i) + ".jpg"))
		textureNode.texture = ImageTexture.create_from_image(image)
		textures.append(textureNode)
