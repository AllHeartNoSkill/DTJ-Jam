extends Node3D

# Called when the node enters the scene tree for the first time.
func _ready():

	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("capture")):
		await _capture_image()
		print("Captured image")
	pass

func _capture_image():
	var viewport = get_viewport()
	print(viewport)
	var image = viewport.get_texture().get_image()
	print(image)

	image.save_jpg("texture.jpg")
