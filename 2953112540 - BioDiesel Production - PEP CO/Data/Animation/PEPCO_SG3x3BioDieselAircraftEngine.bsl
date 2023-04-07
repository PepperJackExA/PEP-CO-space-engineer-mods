@BlockID "PEPCO_SG3x3BioDieselAircraftEngine"
@Version 2
@Author UnicornConsulting


#---Declarations---
using AESmoke1 as Emitter("emitter_AESmoke1")
using AESmoke2 as Emitter("emitter_AESmoke2")
using AESmoke3 as Emitter("emitter_AESmoke3")
using AESmoke4 as Emitter("emitter_AESmoke4")

#---Functions---
func burn() {
	var temp = 0.1 + (0.1 * block.currentthrustpercent())
	AESmoke1.playParticle("OxyVent", temp, 2, [0,0,0 ], 100,100,100).delay(200).stopparticle()
	AESmoke2.playParticle("OxyVent", temp, 2, [0,0,0 ], 100,100,100).delay(200).stopparticle()
	AESmoke3.playParticle("OxyVent", temp, 2, [0,0,0 ], 100,100,100).delay(200).stopparticle()
	AESmoke4.playParticle("OxyVent", temp, 2, [0,0,0 ], 100,100,100).delay(200).stopparticle()
}

func off(){
	AESmoke1.delay(200).stopparticle()
	AESmoke2.delay(200).stopparticle()
	AESmoke3.delay(200).stopparticle()
	AESmoke4.delay(200).stopparticle()
}


#---Actions---
action block() {
    working() {
		api.startLoop("burn", 20, -1)
		#burn()
	}
	notworking() {
		api.stoploop("burn")
		off()
	}
}