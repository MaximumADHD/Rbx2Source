$collisionjoints "physics_mdl.smd" 
{
	$automass
	$inertia 10
	$damping 0.01
	$rotdamping 1.5
	$rootbone "Torso"
	$concave
	
	$jointconstrain "Right Shoulder" x free 
	$jointconstrain "Right Shoulder" y limit -10 10 0
	$jointconstrain "Right Shoulder" z limit -10 10 0

	$jointconstrain "Left Hip" x limit  -80 80 0
	$jointconstrain "Left Hip" y limit  -10 10 0
	$jointconstrain "Left Hip" z limit  -10 10 0

	$jointconstrain "Right Hip" x limit  -80 80 0
	$jointconstrain "Right Hip" y limit  -10 10 0
	$jointconstrain "Right Hip" z limit  -10 10 0

	$jointconstrain "Neck" x limit  -20 20 0
	$jointconstrain "Neck" y limit  -50 50 0
	$jointconstrain "Neck" z limit  -20 20 0

	$jointconstrain "Left Shoulder" x free 
	$jointconstrain "Left Shoulder" y limit  -10 10 0
	$jointconstrain "Left Shoulder" z limit  -10 10 0
}
