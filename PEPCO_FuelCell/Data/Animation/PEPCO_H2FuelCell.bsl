@BlockID "PEP-CO H2 fuel cell"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Emissive1 as emissive("EmissiveColorable")




#---Functions---
func on() {
	Emissive1.setcolor(0, 0, 255, 100, true)
}
func turn() {
	Emissive1.tocolor(255, 165, 0, 100, true, 200, Linear).tocolor(0, 0, 255, 100, true, 200, Linear)
	
}





#---Actions---
action Block() {
    create() {
		on()
		api.startloop("turn",400,-1)
    }
}
