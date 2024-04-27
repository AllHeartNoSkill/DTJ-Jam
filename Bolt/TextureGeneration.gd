extends SubViewport

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("capture")):
		_capture_multiple()
		print("Captured image")
	pass

func _generate_texture(suffix):
	var viewport = get_viewport()
	print(viewport)
	var image = viewport.get_texture().get_image()
	print(image)

	image.save_jpg("res://Bolt/texture" + str(suffix) + ".jpg")

func random_camera_pos(max_x, max_z):
	var camera = get_node("Camera")

	print("camera pos", camera.get_position())

	var x = randf_range(0, max_x)
	var z = randf_range(0, max_z)
	camera.set_position(Vector3(x, 0, z))

	var rotation = randf_range(0, 360)
	camera.set_rotation(Vector3(0, rotation, 0))

func _capture_image(count):
	random_camera_pos(3, 3)
	# await get_tree().create_timer(0.5).timeout
	_generate_texture(count)

func _capture_multiple():
	for i in range(5):
		_capture_image(i)
		await get_tree().create_timer(0.5).timeout
