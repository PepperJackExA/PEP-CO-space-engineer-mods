@BlockID "PEPCO_CoalFurnace_Rebalance"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Emitter1 as Emitter("emitter_Coalfire1")
using Emitter2 as Emitter("emitter_Coalfire2")
using Emitter3 as Emitter("emitter_Smoke1")
using Emitter4 as Emitter("emitter_Smoke2")
using Emitter5 as Emitter("emitter_Steam")

#---Functions---

func burn() {
	Emitter1.playParticle("ExhaustFire", 0.175, 1.1, [0,0,0 ], 10, 10, 10)
	Emitter2.playParticle("ExhaustFire", 0.175, 1.1, [0,0,0 ], 10, 10, 10)
}
func smoke() {
	Emitter3.playParticle("Smoke_Missile", 0.1, 2, [0,0,0 ], 2, 2, 2)
	Emitter4.playParticle("Smoke_Missile", 0.1, 2, [0,0,0 ], 2, 2, 2)
	Emitter5.playParticle("OxyVent", 0.5, 1, [0,0,0 ], 255, 255, 255)
}


#---Actions---
action block() {

    working() {		
		api.startLoop("burn", 75, -1)
		api.startLoop("smoke", 500, -1)
	}
	notworking() {
		api.stoploop("burn")
		api.stoploop("smoke")
	}
}