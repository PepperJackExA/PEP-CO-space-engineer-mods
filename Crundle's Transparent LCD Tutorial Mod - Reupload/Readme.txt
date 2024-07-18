Transparent LCD Tutorial Mod by crundle (2020)
Do what you want with it.
#############################################
Contains:
	One Fully Functional Spherical Transparent LCD Block
	the SEUT .blend file for this Block
#############################################

## Making Transparent LCDs in a nutshell: 
	Create Block as usual, and base your .sbc on the Original Transparent LCD found in CubeBlocks_DecorativePack2.sbc. 
	Or on the one found in the Data Folder.

	Create your Screen Geometry (can be anything really, but needs reasonable UVs). 

	Duplicate all Faces (Faces, not the Object) of your Screen and flip the normals on the copied ones. 
	This is so it can be seen from both sides. If you don't want this, skip this step.

	Duplicate the Object four Times. Name them "Screen_section", "Screen_section90", ect. up to 270.  
	These are for the Screen Rotation Feature. If you don't want that, skip this step.
	The naming might not be important, but I don't actually know.

	Add another Copy, its name doesn't matter. 
	This is for Glass effects. If you don't want it, skip this step.

	Lastly, for the copies that are 90/180/270 rotate the UVs by the appropriate amount. 
	Just rotating the mesh might work but I ran into some flipped axis that way.

	Now the important part:
	You need to set the correct Materials on your Screen Geometry for the Game to recognize it.

	The first one, "TransparentScreenArea", is available in the Matlib, but the other three are not.
	Thus you need to create them yourself, with Rendermode HOLO. (check SEUT reference on how to do that)

	The Naming is extremely important here as that is the way SE identifies them.
	Name them "TransparentScreenArea90", "TransparentScreenArea180", "TransparentScreenArea270".
	And because SEUT will not export Materials with no assigned Textures, you need to add a texture to them.
	It wont be visible, so just go with whatever.

	The Glass Effect Material is "TransparentScreenArea_Outside", which is available in the MatLib.

	Set Materials as such: 
		Screen_section: "TransparentScreenArea"
		Screen_section90: "TransparentScreenArea90" (not in MatLib, needs to be created, and a random Texture assigned, Rendermode = HOLO)
		Screen_section180: "TransparentScreenArea180" (not in MatLib, needs to be created, and a random Texture assigned, Rendermode = HOLO)
		Screen_section270: "TransparentScreenArea270" (not in MatLib, needs to be created, and a random Texture assigned, Rendermode = HOLO)
		Glass Effect: "TransparentScreenArea_Outside" (this is the glass effect)

#############################################
crundle - 2020

