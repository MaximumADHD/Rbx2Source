$collisionjoints "CollisionModel.smd" 
{
	$mass 120.00
	$inertia 10
	$damping 0.01
	$rotdamping 1.5
	$concave
	
	$jointconstrain "Right Arm" x limit -10 10 0
	$jointconstrain "Right Arm" y limit -10 10 0
	$jointconstrain "Right Arm" z free 

	$jointconstrain "Left Leg" x limit  -10 10 0
	$jointconstrain "Left Leg" y limit  -10 10 0
	$jointconstrain "Left Leg" z limit  -80 80 0

	$jointconstrain "Right Leg" x limit  -10 10 0
	$jointconstrain "Right Leg" y limit  -10 10 0
	$jointconstrain "Right Leg" z limit  -80 80 0

	$jointconstrain "Head" x limit  -20 20 0
	$jointconstrain "Head" y limit  -50 50 0
	$jointconstrain "Head" z limit  -20 20 0

	$jointconstrain "Left Arm" x limit  -10 10 0
	$jointconstrain "Left Arm" y limit  -10 10 0
	$jointconstrain "Left Arm" z free 
}