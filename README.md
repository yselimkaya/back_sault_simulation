# Back Sault Simulation (OpenCV + Unity)

This project simulates the real-time movement of **7 blob points** extracted from a video of a back sault using OpenCV in Python, and visualizes them in Unity via TCP socket communication.

---

## Key Features
- Real-time blob detection using OpenCV  
- TCP socket communication between Python and Unity  
- Frame-by-frame synchronization with original video  
- Skeleton visualization in Unity  
- Interpolation of missing head point from shoulder and hip positions  
- Adjustable threshold and frame rate  

---

## System Requirements
| Component        | Requirement                     |
|------------------|---------------------------------|
| Operating System | Windows 10 or later             |
| Python           | 3.10+                            |
| Python Libraries | `opencv-python`, `json`, `socket`, `time` |
| Unity            | 2022.3 or newer                  |
| RAM              | Minimum 4 GB                     |

---

## System Architecture

**Python Side**
1. Processes frames from `back_sault/*.jpg`  
2. Detects contours and extracts centroids  
3. Sends JSON data over TCP in the format:  
   ```json
   {
     "points": [
       {"id": 0, "x": 123, "y": 456},
       ...
     ]
   }
   ```

**Unity Side**
1. Connects to Python server at `127.0.0.1:2002`  
2. Receives JSON data via TCP  
3. Updates prefab objects and LineRenderers  
4. Displays moving points and skeleton in real-time  

---

## How to Run
1. **Run the Python script**  
   ```bash
   python back_sault_sender.py
   ```
2. **Start the Unity project**  
3. Observe the real-time simulation  

---

## Preview
*(Add image or GIF here)*  
- Video without image processing  
- Real-time Unity visualization  

---

## License
This project is licensed under the MIT License.
