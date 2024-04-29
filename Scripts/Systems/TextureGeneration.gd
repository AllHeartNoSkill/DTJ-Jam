extends SubViewport

var mission_system: Node3D
signal generate_texture_signal()
signal generated_texture(index: int, texture: Image)

# Called when the node enters the scene tree for the first time.
func _ready():
	get_node("../GameEvents").LevelLoadedSignal.connect(_capture_multiple)
	mission_system = get_node("../MissionSystem")
	# _capture_multiple()

# Called every frame. 'delta' is the elapsed time since the previous frame.

func _generate_texture(suffix):
	var viewport = get_viewport()
	print(viewport)
	var image = viewport.get_texture().get_image()
	generated_texture.emit(suffix, image)
	print(image)
	# image.save_jpg("res://GeneratedTextures/texture" + str(suffix) + ".jpg")

# TODO get parameter according to level data
# func random_camera_pos(max_x, max_z):
# 	var camera = get_node("TextureCamera")

# 	print("camera pos", camera.get_position())

# 	var x = randf_range(0, max_x)
# 	var z = randf_range(0, max_z)
# 	camera.set_position(Vector3(x, 0, z))

# 	var rotation = randf_range(0, 360)
# 	camera.set_rotation(Vector3(0, rotation, 0))

func _capture_image(point: Node3D, suffix: int=0):
	var camera = get_node("TextureCamera")
	camera.set_position(point.get_position())
	camera.set_rotation(point.get_rotation())
	await get_tree().create_timer(0.1).timeout
	_generate_texture(suffix)

func _capture_multiple():

	for i in range(5):
		var point: Node3D = mission_system.get_node("Photo Position "+ str(i))
		_capture_image(point, i)
		await get_tree().create_timer(0.5).timeout
	
	await get_tree().create_timer(0.5).timeout
	generate_texture_signal.emit()
