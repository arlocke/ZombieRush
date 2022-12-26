extends CanvasLayer


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func initDialogue(dialogueResource):
	var dialogueLine = yield(DialogueManager.get_next_dialogue_line("succy_talk", dialogueResource), "completed")
	$Dialogue.dialogue = dialogueLine
	$Dialogue.type_out()
