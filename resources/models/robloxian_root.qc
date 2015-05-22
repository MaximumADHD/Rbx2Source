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