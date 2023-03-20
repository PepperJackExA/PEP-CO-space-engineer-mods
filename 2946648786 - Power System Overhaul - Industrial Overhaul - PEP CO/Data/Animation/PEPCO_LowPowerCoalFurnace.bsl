@BlockID "PEPCO_LowPowerCoalFurnace"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Emitter1 as Emitter("emitter_Smoke1")
using Emitter2 as Emitter("emitter_Smoke2")
using Emitter3 as Emitter("emitter_Steam")
using Emissive1 as Emissive ("Coal")

#---Functions---

func smoke() {
	Emitter1.playParticle("OxyVent", 0.2, 2, [0,0,0 ], 255, 255, 255)
	Emitter2.playParticle("OxyVent", 0.2, 2, [0,0,0 ], 255, 255, 255)
	Emitter3.playParticle("OxyVent", 0.5, 1, [0,0,0 ], 255, 255, 255)
}

func burn() {
	var randomv1 = math.random();
	var randomv2 = math.random();
	var randomv3 = 75.0 + 25.0 * math.random();
	var red1 = 255.0 - 11.0 * randomv1
	var yellow1 = 44.0 * randomv2
	api.log(red1)
	Emissive1.setcolor(red1, yellow1, 0, randomv3, false)
}
func off(){
	Emissive1.setcolor(0, 0, 0, 0, false)
	Emitter1.delay(200).stopparticle()
	Emitter2.delay(200).stopparticle()
	Emitter3.delay(200).stopparticle()
}


#---Actions---
action block() {

    working() {		
		smoke()
		api.startLoop("burn", 6, -1)
	}
	notworking() {
		api.stoploop("burn")
		off()
		
	}
}