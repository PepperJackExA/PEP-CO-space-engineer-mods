@BlockID "PEPCO_AnaerobicDigester"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Subpart1 as subpart("Meter")
using Emissive1 as emissive("Emissive4")




#---Functions---
func turn() {
	Subpart1.translate([0,-1,0], 45, Linear).delay(55).translate([0,1,0], 45, Linear)
	
}
func on() {
	Emissive1.setcolor(4, 30, 0, 1, true)
}





#---Actions---
action Block() {
    create() {
        api.startloop("turn",100,-1)
		on()
    }
}

