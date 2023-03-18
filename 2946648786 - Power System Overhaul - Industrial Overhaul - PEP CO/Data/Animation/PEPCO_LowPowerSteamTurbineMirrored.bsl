@BlockID "PEPCO_LowPowerSteamTurbineMirrored"
@Version 2
@Author UnicornConsulting



#---Declarations---
using Subpart1 as subpart("EngineShaftM")
using Emitter1 as Emitter("emitter_SparksM")

#---Functions---

func turn() {
	Subpart1.spin([0,0,1], 45, 60)
	Emitter1.playParticle("ExhaustElectric", 0.125, 1.1, [0,0,0 ], 10, 10, 10).delay(60).stopparticle()
}



#---Actions---
action block() {

    working() {		
		api.startLoop("turn", 60, -1)
	}
	notworking() {
		api.stoploop("turn")
	}
}