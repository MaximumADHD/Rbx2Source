$collisionjoints "CollisionModel.smd"
{
	$mass 120.00
	$inertia 2.00
	$damping 0.01
	$rotdamping 0.40
	$concave

	$jointconstrain	"UpperTorso"	x limit -45.00  20.00  0.00
	$jointconstrain	"UpperTorso"	y limit -5.00   5.00   0.00
	$jointconstrain	"UpperTorso"	z limit -5.00   5.00   0.00

	$jointconstrain	"Head"			x limit -30.00  45.00  0.00
	$jointconstrain	"Head"			y limit -60.00  60.00  0.00
	$jointconstrain	"Head"			z limit -35.00  35.00  0.00
	
	$jointconstrain	"LeftUpperArm"	x limit -60.00  180.00 0.00
	$jointconstrain	"LeftUpperArm"	y limit -60.00  60.00  0.00
	$jointconstrain	"LeftUpperArm"	z limit -120.00 20.00 0.00
	
	$jointconstrain	"LeftLowerArm"	x limit	-5.00   120.00 0.00
	$jointconstrain	"LeftLowerArm"	y limit -30.00  30.00  0.00
	$jointconstrain	"LeftLowerArm"	z limit -5.00   5.00   0.00

	$jointconstrain	"LeftHand"		x limit -25.00	25.00  0.00
	$jointconstrain	"LeftHand"		y limit -30.00	30.00  0.00
	$jointconstrain	"LeftHand"		z limit -20.00	90.00  0.00

	$jointconstrain	"RightUpperArm"	x limit -60.00  180.00 0.00
	$jointconstrain	"RightUpperArm"	y limit -60.00  60.00  0.00
	$jointconstrain	"RightUpperArm"	z limit -20.00  120.00 0.00
	
	$jointconstrain	"RightLowerArm"	x limit	-5.00   120.00 0.00
	$jointconstrain	"RightLowerArm"	y limit -30.00  30.00  0.00
	$jointconstrain	"RightLowerArm"	z limit -5.00   5.00   0.00

	$jointconstrain	"RightHand"		x limit -25.00	25.00  0.00
	$jointconstrain	"RightHand"		y limit -30.00	30.00  0.00
	$jointconstrain	"RightHand"		z limit -90.00	20.00  0.00

	$jointconstrain	"LeftUpperLeg"	x limit -20.00	90.00  0.00
	$jointconstrain	"LeftUpperLeg"	y limit -10.00  10.00  0.00
	$jointconstrain	"LeftUpperLeg"	z limit -30.00  10.00  0.00

	$jointconstrain	"LeftLowerLeg"	x limit -120.00 0.00   0.00
	$jointconstrain	"LeftLowerLeg"	y limit -10.00  10.00  0.00 
	$jointconstrain	"LeftLowerLeg"	z fixed

	$jointconstrain	"LeftFoot"		x limit -45.00  20.00  0.00
	$jointconstrain	"LeftFoot"		y limit -5.00	5.00   0.00
	$jointconstrain	"LeftFoot"		z limit -10.00  10.00  0.00
	
	$jointconstrain	"RightUpperLeg"	x limit -20.00	90.00  0.00
	$jointconstrain	"RightUpperLeg"	y limit -10.00  10.00  0.00
	$jointconstrain	"RightUpperLeg"	z limit -10.00  30.00  0.00

	$jointconstrain	"RightLowerLeg"	x limit -120.00 0.00   0.00
	$jointconstrain	"RightLowerLeg"	y limit -10.00  10.00  0.00 
	$jointconstrain	"RightLowerLeg"	z fixed

	$jointconstrain	"RightFoot"		x limit -20.00  45.00  0.00
	$jointconstrain	"RightFoot"		y limit -5.00	5.00   0.00
	$jointconstrain	"RightFoot"		z limit -10.00  10.00  0.00
}