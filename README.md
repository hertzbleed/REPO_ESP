# REPO_ESP
Simple repo mono C# ESP i made in 2 hours after reversing this unity game
Currently it does the following:
----------------------------------------------------------------------------------------------
-DRAWS 2D BOUNDING-BOX AROUND ALL ENEMIES. (size of box is relative to the enemy hitbox)
-DRAWS TRACER LINES FROM YOU TO ALL ENEMIES. (helps with tracking them while looting)
-DRAWS 2D BOUNDING-BOX AROUND TEAMATES.
-WORKS ON ALL LEVELS AND REFRESHES ENEMY CHECKLIST AFTER EVERY NEW LEVEL AUTOMATICALLY.
-DECENTLY OPTIMIZED AND INTENRAL SO YOU SHOULD NOT EXPERIENCE ANY NOTICABLE FRAME DROPS.
----------------------------------------------------------------------------------------------
--COMPILE USING VS2022--
HOW TO INJECT:
open cmd and run:
cd C:\Users\YOUR USER\Desktop\smi.exe inject -p REPO -a "RSEsp.dll" -n MyESP -c Loader -m Init
