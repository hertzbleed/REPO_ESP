![image](https://github.com/user-attachments/assets/273a4e14-2d20-4949-a5d1-4036d93c9166)

### REPO_ESP
Simple repo mono C# ESP i made in 2 hours after reversing this unity game
Currently it does the following:
----------------------------------------------------------------------------------------------
- DRAWS 2D BOUNDING-BOX AROUND ALL ENEMIES. (size of box is relative to the enemy hitbox)
  

- DRAWS TRACER LINES FROM YOU TO ALL ENEMIES. (helps with tracking them while looting)
  

- DRAWS 2D BOUNDING-BOX AROUND TEAMATES.
  

- WORKS ON ALL LEVELS AND REFRESHES ENEMY CHECKLIST AFTER EVERY NEW LEVEL AUTOMATICALLY.
  

- DECENTLY OPTIMIZED SO YOU SHOULD NOT EXPERIENCE ANY NOTICABLE FRAME DROPS.


----------------------------------------------------------------------------------------------
- COMPILE USING VS2022
----------- 
###HOW TO INJECT:

1. open cmd and run:
2. cd C:\Users\YOUR USER\Desktop\smi.exe inject -p REPO -a "RSEsp.dll" -n MyESP -c Loader -m Init
