using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PointReceiver2D : MonoBehaviour
{
    [Header("Socket Settings")]
    public string host = "127.0.0.1";
    public int port = 2002;

    [Header("Image Resolution (px)")]
    public int imageWidth = 1920;   // Python tarafındaki frame genişliği
    public int imageHeight = 1080;  // Python tarafındaki frame yüksekliği

    [Header("Visual")]
    public GameObject boxPrefab;    // Kare sprite prefab’ı

    // --- Dahili değişkenler ---
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private readonly Queue<FrameData> frameQueue = new Queue<FrameData>();
    private readonly Dictionary<int, GameObject> boxesById = new Dictionary<int, GameObject>();
    private readonly object queueLock = new object();

    // ========= JSON Modelleri =========
    [Serializable] public class Point { public int id; public int x; public int y; }
    [Serializable] public class FrameData { public List<Point> points; }

    void Start()
    {
        // Bağlantı işini arka threade at
        receiveThread = new Thread(SocketListen) { IsBackground = true };
        receiveThread.Start();
    }

    void Update()
    {
        // Ana thread: Kuyruktaki en güncel frame’i uygula
        FrameData latestFrame = null;

        lock (queueLock)
        {
            while (frameQueue.Count > 0) latestFrame = frameQueue.Dequeue();
        }

        if (latestFrame != null) RenderFrame(latestFrame);
    }

    private void RenderFrame(FrameData frame)
    {
        foreach (var p in frame.points)
        {
            // Box var mı? Yoksa oluştur
            if (!boxesById.TryGetValue(p.id, out var box))
            {
                box = Instantiate(boxPrefab, transform);
                box.name = $"Box_{p.id}";
                boxesById[p.id] = box;
            }

            // Pixel → ScreenPoint → WorldPoint
            float screenX = p.x;                         // (0,0) sol-üst varsayılıyor
            float screenY = imageHeight - p.y;           // Y eksenini tersine çevir
            Vector3 screenPos = new Vector3(screenX, screenY, 0);

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;                              // 2D düzlemde tut
            box.transform.position = worldPos;
        }
    }

    private void SocketListen()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            Debug.Log("<color=#90ee90>Socket connected</color>");

            var sb = new StringBuilder();
            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) break;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Satır sonu “\n” ile frame ayır
                string data = sb.ToString();
                int newlineIdx;
                while ((newlineIdx = data.IndexOf('\n')) >= 0)
                {
                    string jsonLine = data.Substring(0, newlineIdx);
                    data = data.Substring(newlineIdx + 1);

                    var frame = JsonUtility.FromJson<FrameData>(jsonLine.Trim());
                    lock (queueLock) frameQueue.Enqueue(frame);
                }
                sb = new StringBuilder(data); // elde kalan parçayı sakla
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket error: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        try { receiveThread?.Abort(); } catch { }
        stream?.Close();
        client?.Close();
    }
}
