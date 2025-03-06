# Usage
There are two parts to adding in-game dialogue: copying the text into a `DialogueHolder`
and triggering the dialogue at some point in the game.

## Storing Dialogue
Create a new child `GameObject` on `LevelManager > Dialog` and attach a `DialogueHolder` component to it (it technically doesn't need to be a child of `LevelManager > Dialog` but it is much nicer for organization and makes sure things don't get lost.)

`DialogueHolder` has two fields of interest: 

![Preview](img/DialogueHolderInEditor.png)

**`Dialog System`**: The dialog system the text should be displayed on (i.e. bottom
of screen or in a text bubble). The default system is the global system (bottom
of screen) and will be used when no other system is provided. Just search `DialogSystem` in the Hierarchy if you want to grab the speech bubble system.

**`Segments`**: List where the dialogue is stored. Each "Segment" in this list
corresponds to the speach of a single character and contains a list of lines
that the character should say. Only one line is displayed in the dialog box at
a time. Something which is a bit confusing is that 
- `Title` in a dialogue situation is the character name. 
- `Font` will set the font the text is written in. Note that `.ttf` and `.otf` font files won't work here as fonts must first be converted into SDFs for use with TMP. This can be done by navigating to `Window > TextMeshPro > Font Asset Creator`, dragging the `.ttf` or `.otf` into `Source Font` and clicking `Generate Font Atlas`. 
- `Auto Continue` will make is so after every line in the segment the dialog system will automatically move to the next.

## Triggering Dialogue
There are many ways to do this. The simplest is:

Create a new `GameObject` and attach a `TriggerArea` and a collider (preferably `BoxCollider2D` or `CircleCollider2D` cause they're cheap). Make sure the collider `Is Trigger` is on and set `Trigger Layers` to `Player` on the `TriggerArea`. Then add a `Trigger Event` and drag and drop the `DialogueHolder` GameObject. For the function choose `DialogueHolder.PlayDialogue` and now the dialogue will play whenever the player runs into the collider. 

