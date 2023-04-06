@BlockID "PEPCO_AnaerobicDigester"
@Version 2
@Author UnicornConsulting


#---Declarations---
using Subpart1 as subpart("Meter")
using Emissive1 as emissive("Emissive4")




#---Functions---
func turn() {
	Subpart1.translate([0,-1,0], 450, Linear).delay(550).translate([0,1,0], 450, Linear)
	
}
func on() {
	Emissive1.setcolor(4, 30, 0, 10, true)
	Subpart1.translate([0,1.5,0], 45, Linear)
	Subpart1.scale([0,0.35,0],10,Linear)
}





#---Actions---
action Block() {
    create() {
        api.startloop("turn",1000,-1)
		on()
    }
}

