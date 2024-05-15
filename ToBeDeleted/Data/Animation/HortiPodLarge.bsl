@BlockID "HortiPodLarge"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Emissive as emissive("Emissive4")
using Emitter as emitter("emitter_Smoke")




#---Functions---
func on() {
	Emissive.setcolor(255, 255, 255, 100, true)
	Emitter.playparticle("OxyVent",[1,1,1],200)
}






#---Actions---
action Block() {
    create() {
		on()
    }
}
