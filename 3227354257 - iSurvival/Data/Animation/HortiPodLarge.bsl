@BlockID "HortiPodLarge"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Emissive as emissive("Emissive4")
using Emissive2 as emissive("PEPCO_Logo")
using Emitter as emitter("emitter_Smoke")




#---Functions---
func on() {
	Emissive.setcolor(255, 255, 255, 100, true)
	Emissive2.setcolor(255, 255, 255, 100, true)
}

func exhaust() {
	Emitter.playParticle("OxyVent", 0.1, 1, [0.0, 0.0, 0.0], 255, 255, 255)#.delay(250).stopParticle()
}






#---Actions---
action Block() {
    create() {
		on()
		api.startloop("exhaust",10,-1)
    }
}
