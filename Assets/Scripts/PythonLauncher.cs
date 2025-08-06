using UnityEngine;
using System.Diagnostics;          // Process, ProcessStartInfo
using Debug = UnityEngine.Debug;   // <-- alias
using System.IO;

public class PythonLauncher : MonoBehaviour
{
    [Header("Python Ayarlar�")]
    public string pythonExePath = "python";
    public string pythonScriptRelative = "Scripts/main.py";
    public bool showConsole = false;

    private Process pythonProc;

    void Start()
    {
        string scriptFullPath = Path.Combine(Application.dataPath, pythonScriptRelative);
        if (!File.Exists(scriptFullPath))
        {
            Debug.LogError($"Python beti�i bulunamad�: {scriptFullPath}");
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = $"\"{scriptFullPath}\"",
            UseShellExecute = false,
            CreateNoWindow = !showConsole,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            pythonProc = Process.Start(psi);
            if (pythonProc == null)
            {
                Debug.LogError("Python s�reci ba�lat�lamad�.");
                return;
            }

            pythonProc.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.Log($"[PY] {e.Data}"); };
            pythonProc.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.LogError($"[PY] {e.Data}"); };
            pythonProc.BeginOutputReadLine();
            pythonProc.BeginErrorReadLine();

            Debug.Log($"<color=#90ee90>Python s�reci ba�lad� (PID {pythonProc.Id})</color>");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Python ba�lat�lamad�: {ex.Message}");
        }

        Application.quitting += OnApplicationQuit;
    }

    private void OnApplicationQuit()
    {
        if (pythonProc == null) return;

        try
        {
            if (!pythonProc.HasExited)
            {
                pythonProc.Kill();
                pythonProc.WaitForExit(2000);
            }
            pythonProc.Dispose();
            Debug.Log("<color=#ffaa00>Python s�reci sonland�r�ld�.</color>");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Python s�reci kapat�l�rken hata: {ex.Message}");
        }
    }
}
