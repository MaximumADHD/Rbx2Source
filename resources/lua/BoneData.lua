----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Max G, 2015
-- Bone Data
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
																																					--[[
HOW THIS STUFF WORKS:

There are 5 mesh groups in a Roblox character:
	Torso1, LeftArm1, RightArm1, LeftLeg1, RightLeg1

Each table representing each mesh group has a list of tables representing bones in those mesh groups.
	Each bone has the following information:
		* Name: 
			* The name of the bone.
		* Link: 
			* An integer which serves as a unique identifier for that bone. Required in the SMD file structure.
		* Parent(OPTIONAL)
			* An integer which parents this bone to another bone.
			  The integer should refer to another bones Link.
			* If you parent a hand bone to an arm bone and the arm bone moves, 
			  the hand will move with it.
			* If excluded, the bone will be parented to the world and cannot be moved by other bones.
		* Offset (OPTIONAL): 
			* An XYZ offset relative to the parent bone. 
			  Defaults to 0,0,0 if undefined.
		* Heat (OPTIONAL)
			* A number which sets a maximum range for a vert to be controlled.
			* Example: If a bone has a heat of 2 units, and a vert is within 1 unit
			  of the bone, its unique heat is 0.5, or 50%. This means that the bone
			  associated with it can only move it half as much as a bone 
			  with a heat of 100%
			* If undefined, all verts associated with the mesh group will have 
			  a heat of 100% for that bone.
																																					--]]
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

bones =
{
	Torso1 = 
	{
		{
			Name = "Torso";
			Link = 0;
		}
	},
	LeftArm1 = 
	{
		[1] = 
		{
			Name = "Left Shoulder";
			Parent = 0;
			Link = 1;
			Offset = Vector3.new(1,0.5,0);
		},
		[2] = 
		{
			Name = "Left Elbow";
			Offset = Vector3.new(-0.5,0,0);
			Link = 6;
			Parent = 1;
			Heat = 1.25;
		},
		[3] = 
		{
			Name = "Left Hand";
			Offset = Vector3.new(-0.5,0,0);
			Link = 7;
			Parent = 6;
			Heat = 0.75;
		}
	},
	RightArm1 = 
	{
		{
			Name = "Right Shoulder";
			Offset = Vector3.new(-1,0.5,0);
			Link = 2;
		},
		{
			Name = "Right Elbow";
			Offset = Vector3.new(-0.5,0,0);
			Link = 8;
			Parent = 2;
			Heat = 1.25;
		},
		{
			Name = "Right Hand";
			Offset = Vector3.new(-0.5,0,0);
			Link = 9;
			Parent = 8;
			Heat = 0.75;
		}
	},
	LeftLeg1 = 
	{
		{
			Name = "Left Hip";
			Offset = Vector3.new(1,-1,0);
			Link = 3;
		},
		{
			Name = "Left Knee";
			Offset = Vector3.new(-0.5,0,0);
			Link = 10;
			Parent = 3;
			Heat = 1.25;
		},
		{
			Name = "Right Hand";
			Offset = Vector3.new(-0.5,0,0);
			Link = 11;
			Parent = 10;
			Heat = 0.75;
		}
	},
	RightLeg1 = 
	{
		{
			Name = "Right Hip";
			Offset = Vector3.new(-1,-1,0);
			Link = 4;
		},
		{
			Name = "Right Knee";
			Offset = Vector3.new(-0.5,0,0);
			Link = 12;
			Parent = 4;
			Heat = 1.25;
		},
		{
			Name = "Right Hand";
			Offset = Vector3.new(-0.5,0,0);
			Link = 13;
			Parent = 12;
			Heat = 0.75;
		}
	},
	Head1 = 
	{
		{
			Name = "Neck";
			Offset = Vector3.new(0,1,0);
			Link = 5;
		}
	}
}
