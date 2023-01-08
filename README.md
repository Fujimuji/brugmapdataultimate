# Info
This project was made for making Doomfist Parkour (HAX's framework) map data creation easier in the absence of inspector.  

Both of the versions have the same features like adding/editing/deleting cps, effects, missions.  
But the OCR version has an extra feature that can extract coordinates from a screenshot. That's why the file size is big for that one.  
OCR version will only work for Windows 10.0.19041.0 and above. The default version should work for Windows 7 and above. (Both of the versions need .NET 6.0)  

The link for KNEAT : https://pastebin.com/0D7ssz9J (thanks to yoshi, neb, dreadowl) 

If you think the executables are malicious, you can build it from the source code yourself :agreege:

# Tips for OCR
For OCR to work you should only take a screenshot of the coordinates part and make sure that the screenshots are in .png format. (You can use something like ShareX)  
OCR will work most of the time but it might not be able to extract the coordinates if the screenshot has a very busy background.  
If you want to change the coordinates display color, find "rule ("display")" in the kneat file, and edit Color(Aqua) part to whatever you want it to be. (Aqua is the one that worked the best for me)
