extends Control
signal actioned(next_id)

var is_waiting_for_input: bool = false

var dialogueLine: Dictionary 
var dialogueResource: DialogueResource

# Called when the node enters the scene tree for the first time.
func _ready():
	var _DialogueResource
	connect("actioned",Callable(self,"initDialogue"))


func initDialogue(title, newDialogueResource = dialogueResource):
	dialogueResource = newDialogueResource
	dialogueLine = await DialogueManager.get_next_dialogue_line(title, dialogueResource).completed
	$Dialogue.dialogue_line = dialogueLine
	$Dialogue.type_out()
	#initDialogue(await $Dialogue.actioned, dialogueResource)
	

func next(next_id: String) -> void:
	emit_signal("actioned", next_id)
	#queue_free()


# When there are no response options the balloon itself is the clickable thing
func _on_Balloon_gui_input(event):
	#if not is_waiting_for_input: return
	
	get_viewport().set_input_as_handled()
	
	if event is InputEventMouseButton and event.is_pressed() and event.button_index == 1:
		next(dialogueLine.next_id)
	elif event.is_action_pressed("ui_accept"):
		next(dialogueLine.next_id)


func _input(event):
	if event.is_action_pressed("ui_accept"):
		next(dialogueLine.next_id)
