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
   
-- Data Format

Each frame sends a JSON message like:
json
{
  "points": [
    {"id": 0, "x": 102, "y": 250},
    {"id": 1, "x": 105, "y": 300},
    ...
  ]
}

Total Points: 7

If 1 point (typically head) is missing, it is interpolated.


-- How It Works
  -Python (frame_processor.py)
  -Detects blobs using cv2.findContours()
  -Calculates centroids with image moments
  -Sends coordinates via socket every 1/30 seconds

 -- Unity (PointReceiver.cs)
  -Uses TcpClient to connect and read data
  -Deserializes JSON to get point list
  -Instantiates prefabs and LineRenderer connections

 -- Configuration
  -Parameter	Default	Description
  -THRESHOLD	60	Image binarization threshold
  -FPS	30	Frame rate of simulation
  -PORT	2002	TCP socket port
  -TOTAL_POINTS	7	Expected number of body keypoints

 --Folder Structure

project-root/
├── back_sault/              # Input frames (.jpg)
├── frame_processor.py       # Python OpenCV + socket script
├── UnityProject/            # Unity project folder
│   └── Assets/
│       └── Scripts/
│           └── PointReceiver.cs

 -- Notes
  -Make sure Python and Unity are running simultaneously.
  -Restart Python server if Unity throws a broken pipe error.
  -This system does not require internet, it runs locally.

 -- Preview


<img width="828" height="373" alt="image" src="https://github.com/user-attachments/assets/3ff27eb6-dcb8-426d-99d9-8b1d95f29bba" />
https://github.com/user-attachments/assets/417154a8-9354-4105-9fa1-8cd9840d2585



Yavuz Selim Kaya
Back Sault Simulation using OpenCV and Unity
2025
