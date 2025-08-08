- Back Sault Simulation using OpenCV and Unity

This project simulates the real-time movement of 7 blob points extracted from a video of a back sault, sending their positions from a Python script (OpenCV) to a Unity scene via TCP socket. 
Unity then visualizes these positions frame-by-frame in sync with the original data feed.

--  Key Features

- Real-time blob detection using OpenCV
- TCP socket communication between Python and Unity
- Visualization of moving points and skeleton lines in Unity
- Interpolation of missing head point based on shoulder and hip
- Customizable threshold and frame rate

-- System Requirements

- Operating System: Windows 10 or later  
- Python: 3.10+  
- Python Libraries: opencv-python, json, socket, time  
- Unity: 2022.3 or newer  
- RAM: Minimum 4GB  

-- System Architecture
<img width="556" height="571" alt="image" src="https://github.com/user-attachments/assets/c01fb102-4413-4d3c-99ed-442b60834374" />


1. Python Side:
   - Processes frames (back_sault/*.jpg)
   - Detects contours and extracts centroids
   - Sends {"points": [{"id": 0, "x": ..., "y": ...}, ...]} over TCP

2. Unity Side:
   - Connects to Python server on 127.0.0.1:2002
   - Receives JSON data via TCP
   - Updates prefab objects and LineRenderers to reflect current frame
   
--How to Work?
   -First, Run the Python Script
   - Secondly, Start the Unity Project

-- Preview
