$upaxis y
$origin 0 -30 0
$surfaceprop "flesh"
$contents "solid"

$weightlist toolup_seq {
	"Torso" 0
	"Left Shoulder" 0
	"Right Shoulder" 1
	"Left Hip" 0
	"Right Hip" 0
	"Neck" 0
}

$sequence "ref" "reference_anim.smd" {
	fps 1 
	loop
}

$sequence "idle" "idle_anim.smd" {
	fps 60
	loop
}

$sequence "walk" "walk_anim.smd" {
	fps 60
	loop
}

$sequence "jump" "jump_anim.smd" {
	fps 20
}

$sequence "falling" "falling_anim.smd" {
	fps 60
	loop
}

$sequence "toolup" "toolup_anim.smd" {
	fps 60
	weightlist toolup_seq
	loop
}

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
