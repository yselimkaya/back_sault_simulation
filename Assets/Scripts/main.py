import cv2
import os
import glob
import json
import socket
import time
import math

# Ayarlar
INPUT_FOLDER = 'back_sault'
IMAGE_TYPE = '*.jpg'
THRESHOLD = 60
HOST = '127.0.0.1'
PORT = 2002
FPS = 30

TOTAL_POINTS = 7
reference_coords = None  # İlk frame'den alınacak referans noktalar

def get_frame_coordinates(image_path):
    frame = cv2.imread(image_path)
    if frame is None:
        return []

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    _, thresh = cv2.threshold(gray, THRESHOLD, 255, cv2.THRESH_BINARY)
    contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    coords = []
    for contour in contours:
        M = cv2.moments(contour)
        if M['m00'] != 0:
            cX = int(M['m10'] / M['m00'])
            cY = int(M['m01'] / M['m00'])
            coords.append({"x": cX, "y": cY})
    return coords

def interpolate_missing_point(coords):
    if len(coords) == 6:
        coords = sorted(coords, key=lambda p: (p["y"], p["x"]))
        max_gap = 0
        insert_index = len(coords)
        for i in range(len(coords) - 1):
            gap = coords[i+1]["y"] - coords[i]["y"]
            if gap > max_gap:
                max_gap = gap
                insert_index = i + 1

        p1 = coords[insert_index - 1] if insert_index > 0 else coords[0]
        p2 = coords[insert_index] if insert_index < len(coords) else coords[-1]

        interp_x = (p1["x"] + p2["x"]) // 2
        interp_y = (p1["y"] + p2["y"]) // 2
        coords.insert(insert_index, {"x": interp_x, "y": interp_y})
    return coords

def euclidean(p1, p2):
    return math.sqrt((p1["x"] - p2["x"])**2 + (p1["y"] - p2["y"])**2)

def prepare_numbered_coords(coords):
    global reference_coords

    if len(coords) < 6:
        return []

    if len(coords) == 6:
        coords = interpolate_missing_point(coords)

    if len(coords) != 7 and reference_coords is None:
        return []

    # İlk 7 noktalı frame: referans olarak belirle
    if reference_coords is None and len(coords) == 7:
        coords = sorted(coords, key=lambda p: (p["y"], p["x"]))
        reference_coords = coords.copy()
        print("Reference frame set.")
        numbered = []
        for i, p in enumerate(coords):
            numbered.append({ "id": i, "x": p["x"], "y": p["y"] })
        return numbered

    # Eğer 6 nokta varsa ve kafa eksikse, otomatik tahminle ekle
    if len(coords) == 6 and reference_coords is not None:
        ref_shoulder = reference_coords[1]  # ID 1: shoulder
        ref_hip = reference_coords[2]       # ID 2: hip

        shoulder = min(coords, key=lambda p: euclidean(p, ref_shoulder))
        hip = min(coords, key=lambda p: euclidean(p, ref_hip))

        head_x = (shoulder["x"] + hip["x"]) // 2
        head_y = shoulder["y"] - abs(hip["y"] - shoulder["y"])
        coords.append({"x": head_x, "y": head_y})
        print("Head estimated and added.")

    if len(coords) != 7:
        return []

    # Noktaları referans noktalara göre eşleştir
    matched = []
    used_indices = set()

    for ref in reference_coords:
        min_dist = float('inf')
        closest = None
        closest_idx = -1

        for i, c in enumerate(coords):
            if i in used_indices:
                continue
            dist = euclidean(ref, c)
            if dist < min_dist:
                min_dist = dist
                closest = c
                closest_idx = i

        if closest is not None:
            matched.append(closest)
            used_indices.add(closest_idx)

    if len(matched) != 7:
        print("Matching error: not all points matched.")
        return []

    numbered = []
    for i, p in enumerate(matched):
        numbered.append({ "id": i, "x": p["x"], "y": p["y"] })
    return numbered

def send_frames():
    image_files = sorted(glob.glob(os.path.join(INPUT_FOLDER, IMAGE_TYPE)))
    if not image_files:
        print("Images not found")
        return

    print("Starting...")
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((HOST, PORT))
    s.listen(1)
    print("Waiting for Unity to connect...")
    conn, addr = s.accept()
    print(f"Connection established with: {addr}\n")

    try:
        for image_path in image_files:
            coords = get_frame_coordinates(image_path)
            numbered_coords = prepare_numbered_coords(coords)

            if not numbered_coords:
                print(f"Skipping frame: {os.path.basename(image_path)}")
                continue

            data = {"points": numbered_coords}
            json_str = json.dumps(data) + "\n"
            conn.sendall(json_str.encode('utf-8'))

            print(f"Sent {len(numbered_coords)} points for {os.path.basename(image_path)}")
            time.sleep(1 / FPS)

        print("All frames sent successfully.")
    except BrokenPipeError:
        print("⚠️ Unity connection lost.")
    finally:
        conn.close()
        s.close()

if __name__ == "__main__":
    send_frames()