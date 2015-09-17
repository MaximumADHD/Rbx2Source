$upaxis y
$origin 0 -30 0 270
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
	activity ACT_DIESIMPLE 1
	fps 1 
	loop
}

$sequence "idle" "idle_anim.smd" {
	activity ACT_IDLE 1
	fps 60
	loop
}

$sequence "walk" "walk_anim.smd" {
	activity ACT_WALK 1
	fps 60
	loop
}

$sequence "jump" "jump_anim.smd" {
	fps 20
	activity ACT_JUMP 1
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
