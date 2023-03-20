@BlockID "PEPCO_LowPowerSteamTurbine"
@Version 2
@Author UnicornConsulting



#---Declarations---
using Subpart1 as subpart("EngineShaft")
using Emitter1 as Emitter("emitter_Sparks")
using Emitter2 as Emitter("emitter_SteamTurbine")

#---Functions---

func turn() {
	Subpart1.spin([0,0,1], 45, 60)
	Emitter1.playParticle("ExhaustElectric", 0.125, 1.1, [0,0,0 ], 10, 10, 10).delay(60).stopparticle()
}
func steam() {
	Emitter2.playParticle("OxyVent", 0.85, 1, [0,0,0 ], 255, 255, 255)
}
func off() {
	Emitter2.delay(200).stopparticle()
}


#---Actions---
action block() {

    working() {		
		steam()
		api.startLoop("turn", 6, -1)
	}
	notworking() {
		api.stoploop("turn")
		off()
	}
}