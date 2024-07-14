import PIL
import os
import sys
from PIL import ImageTk,Image
from collections import Counter


import time
from tkinter import *
from tkinter import filedialog, Text
import Setup

print('Hello, CptArthur here')
print('If you have any problems please join the discord server')
print('https://discord.gg/ZNGNu6E')
print('')
print('keep in mind that matboy is a sensitive program, let it load, if it freezes that is normal')
print('Also, remember when saving you will need to add .png manually : ''yourfilename.png''')
root = Tk()
root.title('Matboy')
root.configure(background='grey')
global noob
noob = 0
global rloc
global gloc
global bloc
rloc = 2
gloc = 2
bloc = 2

def open():
    global Mloc
    Mloc = filedialog.askopenfilename(filetypes=(("png files", "*.png"),("all files", "*.*")))
    mat_btn.configure(text = os.path.split(Mloc)[1])
def Ropen():
    global rloc
    rloc = filedialog.askopenfilename(filetypes=(("png files", "*.png"),("all files", "*.*")))
    if rloc:
        red_btn.configure(text = os.path.split(rloc)[1])
        print(rloc)
    else:
        print("Nothing Chosen")
def Gopen():
    global gloc
    gloc = filedialog.askopenfilename(filetypes=(("png files", "*.png"),("all files", "*.*")))
    green_btn.configure(text= os.path.split(gloc)[1])
    print(gloc)
def Bopen():
    global bloc
    bloc = filedialog.askopenfilename(filetypes=(("png files", "*.png"),("all files", "*.*")))
    print(bloc)
    blue_btn.configure(text= os.path.split(bloc)[1])
def Clear():
    global rloc
    global gloc
    global bloc
    rloc = 2
    gloc = 2
    bloc = 2
    red_btn.configure(text = 'Red channel')
    green_btn.configure(text='Green channel')
    blue_btn.configure(text='Blue channel')
    mat_btn.configure(text = 'Mat file')
def show():
    checklabelR = Label(root, text=varR.get())
    checklabelR.grid(row=3, column=0)
    checklabelG = Label(root, text=varG.get())
    checklabelG.grid(row=3, column=1)
    checklabelB = Label(root, text=varB.get())
    checklabelB.grid(row=3, column=2)

def run():

    # 0 = on
    # 1 = off
    Red = 0
    green = 0
    Blue = 0
    MAT = PIL.Image.open(Mloc)
    rgb_MAT = MAT.convert('RGB')



#red channel
    if varR.get() == 'on':
       print('fixing the corners on the red map')
       im = PIL.Image.open(rloc)
       im.load()

       x = 0
       y = 0
       for z in range(0, 2048*2048):
           if x == 0:
               colode = im.getpixel((x+1, y))
               im.putpixel((x,y),(colode))
           if x == 2047:
               colode = im.getpixel((x-1, y))
               im.putpixel((x,y),(colode))
           if y == 0:
               colode = im.getpixel((x, y+1))
               im.putpixel((x,y),(colode))
           if y == 2047:
               colode = im.getpixel((x, y-1))
               im.putpixel((x,y),(colode))

           x = x + 1
           if x == 2048:
               y = y + 1
               x = 0

       im.putpixel((0,0),(im.getpixel((1, 1))))
       im.putpixel((0,2047),(im.getpixel((1, 2046))))
       im.putpixel((2047,0),(im.getpixel((2046, 1))))
       im.putpixel((2047,2047),(im.getpixel((2046, 2046))))

       im.save(rloc)
#Green channel
    if varG.get() == 'on':
       print('fixing the corners on the green map')
       im = PIL.Image.open(gloc)
       im.load()

       x = 0
       y = 0
       for z in range(0, 2048*2048):
           if x == 0:
               colode = im.getpixel((x+1, y))
               im.putpixel((x,y),(colode))
           if x == 2047:
               colode = im.getpixel((x-1, y))
               im.putpixel((x,y),(colode))
           if y == 0:
               colode = im.getpixel((x, y+1))
               im.putpixel((x,y),(colode))
           if y == 2047:
               colode = im.getpixel((x, y-1))
               im.putpixel((x,y),(colode))

           x = x + 1
           if x == 2048:
               y = y + 1
               x = 0

       im.putpixel((0,0),(im.getpixel((1, 1))))
       im.putpixel((0,2047),(im.getpixel((1, 2046))))
       im.putpixel((2047,0),(im.getpixel((2046, 1))))
       im.putpixel((2047,2047),(im.getpixel((2046, 2046))))

       im.save(gloc)

#Blue channel
    if varB.get() == 'on':
       print('fixing the corners on the blue map')
       im = PIL.Image.open(bloc)
       im.load()

       x = 0
       y = 0
       for z in range(0, 2048*2048):
           if x == 0:
               colode = im.getpixel((x+1, y))
               im.putpixel((x,y),(colode))
           if x == 2047:
               colode = im.getpixel((x-1, y))
               im.putpixel((x,y),(colode))
           if y == 0:
               colode = im.getpixel((x, y+1))
               im.putpixel((x,y),(colode))
           if y == 2047:
               colode = im.getpixel((x, y-1))
               im.putpixel((x,y),(colode))

           x = x + 1
           if x == 2048:
               y = y + 1
               x = 0

       im.putpixel((0,0),(im.getpixel((1, 1))))
       im.putpixel((0,2047),(im.getpixel((1, 2046))))
       im.putpixel((2047,0),(im.getpixel((2046, 1))))
       im.putpixel((2047,2047),(im.getpixel((2046, 2046))))

       im.save(bloc)

    try:
        VOX = PIL.Image.open(rloc)
        rgb_im = VOX.convert('RGB')
    except:
        print('Voxel file not included')
        Red = 1
    try:
        BIOM = PIL.Image.open(gloc)
        rgb_BIOM = BIOM.convert('RGB')
    except:
        print("Biome File not inlcuded")
        green = 1
    try:
        ore = PIL.Image.open(bloc)
        rgb_ore = ore.convert('RGB')
    except:
        print("ore File not inlcuded")
        Blue = 1
    MAT.load()
    r, g, b = rgb_MAT.split()
    x = 0
    y = 0
    for z in range(0, 2048*2048):
    # Red Channel

        if Red == 0:
            Colode = rgb_im.getpixel((x, y))
            Vkeys = Setup.redlist.keys()
            if Colode in Vkeys:
                c = Setup.redlist[Colode]
                r.putpixel((x, y), (c))

    # Green Channel
        if green == 0:
            Bilode = rgb_BIOM.getpixel((x, y))
            Bkeys = Setup.greenlist.keys()
            if Bilode in Bkeys:
                d = Setup.greenlist[Bilode]
                g.putpixel((x, y), (d))
    # Blue Channel
        if Blue == 0:
            orecode = rgb_ore.getpixel((x, y))
            Okeys = Setup.bluelist.keys()
            if orecode in Okeys:
                d = Setup.bluelist[orecode]
                b.putpixel((x, y), (d))

        x = x + 1
        if x == 2048:
            y = y + 1
            x = 0



    MAT_nieuw = PIL.Image.merge( 'RGB', (r, g, b))
    #Export = filedialog.asksaveasfilename(filetypes=(("png files", "*.png"),("all files", "*.*")))
#    print(Export)
    MAT_nieuw.save(Mloc)
    print('Klaar')
#os.path.split(Mloc)[1]

mat_btn = Button(root, text='Mat file', command=open)
mat_btn.grid(row=0, column=1)
red_btn = Button(root, text='Red channel', command=Ropen)
red_btn.grid(row=1, column=0)
green_btn = Button(root, text='green channel', command=Gopen)
green_btn.grid(row=1, column=1)
blue_btn = Button(root, text='blue channel', command=Bopen)
blue_btn.grid(row=1, column=2)
clear_btn = Button(root, text='Clear', command=Clear)
clear_btn.grid(row=1, column=3)

varR = StringVar()
varG = StringVar()
varB = StringVar()
check_red = Checkbutton(root, text='corner fix', variable=varR, onvalue='on', offvalue='off', command=show)
check_red.grid(row=2, column=0)
check_red.deselect()


check_green = Checkbutton(root, text='corner fix', variable=varG, onvalue='on', offvalue='off', command=show)
check_green.grid(row=2, column=1)
check_green.deselect()

check_blue = Checkbutton(root, text='corner fix', variable=varB, onvalue='on', offvalue='off', command=show)
check_blue.grid(row=2, column=2)
check_blue.deselect()


run_btn = Button(root, text='Run', command=run)
run_btn.grid(row=4, column=1)
#Credits_btn = Button(root, text='Credits', command=open).pack()

root.mainloop()
