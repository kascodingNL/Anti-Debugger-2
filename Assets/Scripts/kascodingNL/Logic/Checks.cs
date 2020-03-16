
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public abstract class Checks : MonoBehaviour
{
    public Checks(int toScene = -1)
    {
        ToSceneId = toScene;
    }

    public Checks(SocketClient socket, int toScene = -1)
    {
        this.client = socket;
    }

    #region Low level imports
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool IsDebuggerPresent();
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetCurrentThread();
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int NtSetInformationThread(IntPtr threadHandle, int threadInformationClass, IntPtr threadInformation, int threadInformationLength);
    [DllImport("ntdll.dll", SetLastError = true)]
    static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, IntPtr processInformation, uint processInformationLength, IntPtr returnLength);
    #endregion

    #region Variables
    private SocketClient client;

    private bool Dflag;
    private IntPtr NoDebugInherit = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UInt32)));
    private long status;
    private long status2;
    private IntPtr hDebugObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));

    private string filePath;
    private bool flagged;

    public int timeDiff = 0;
    int previousTime = 0;
    int realTime = 0;
    float gameTime = 0;
    bool detected = false;

    public int ToSceneId;

    bool CheckDebugger;

    public float SpeedPenalty = 0;
    public float MaxSpeedPenalty = 20;

    private float SecondDelay;
    #endregion

    #region Unity built in methods

    void Awake()
    {
        
    }

    void Start()
    {
        #region Set variables
        Dflag = false;
        filePath = Application.persistentDataPath + "/logs/latest.log";

        previousTime = DateTime.Now.Second;
        gameTime = 1;

        Debug.Log(CreateMd5("Test"));

        #region CheckDebugger setter
#if UNITY_EDITOR
        CheckDebugger = false;
#endif

#if !UNITY_DEBUGGER
        CheckDebugger = true;
#endif
        #endregion

        #endregion

    }

    void FixedUpdate()
    {
        SecondDelay += Time.fixedDeltaTime;

        if (previousTime != DateTime.Now.Second)
        {
            realTime++;
            previousTime = DateTime.Now.Second;

            timeDiff = (int)gameTime - realTime;
            if (timeDiff > 7)
            {
                if (SecondDelay >= .5)
                {
                    SecondDelay = 0;
                    detected = true;
                    SpeedupDetected(timeDiff);
                    ClockDesync(timeDiff);
                }
            }
            else
            {
                detected = false;
            }

            InterUpdate(timeDiff);
        }
        gameTime += Time.deltaTime;
    }

    void Update()
    {
        if (!flagged && Dflag)
        {
            flagged = true;
            WriteToFileAndDebug(string.Format("Debugger: {0}, status1: {1}, status2: {2},", Dflag, status, status2), true);
            WriteToFileAndDebug(string.Format("NoDebugInherit: {0}, hDebugObject: {1}",
                ((uint)Marshal.PtrToStructure(NoDebugInherit, typeof(uint))).ToString(),
                ((IntPtr)Marshal.PtrToStructure(hDebugObject, typeof(IntPtr))).ToString()), true);
        }

        if (SpeedPenalty > 20)
        {
            WriteToFileAndDebug(string.Format("[Anticheat] Speedup detected! MD5: {0}", CreateMd5(SpeedPenalty.ToString())), true);
        }
    }

    private void LateUpdate()
    {
        if (CheckDebugger)
        {
            RequestDebugCheck();
        }

        if(Dflag && ToSceneId != -1)
        {
            if(SceneManager.GetSceneByBuildIndex(ToSceneId) != null)
            {
                SceneManager.LoadSceneAsync(ToSceneId);
            }
            else
            {
                throw new Exception("[Anti-Debugger] The ToSceneId integer refers to " + ToSceneId + " but the scene does not exist!");
            }
        }
    }
    #endregion

    #region In-house function
    void RequestDebugCheck()
    {
        #region Debugger detections
        #region Method 1
        NtSetInformationThread(GetCurrentThread(), 0x11, IntPtr.Zero, 0);
        #endregion

        #region Method 2
        bool isDebuggerPresent = false;
        CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);

        if (isDebuggerPresent || IsDebuggerPresent())
        {
            DebuggerFound(DateTime.UtcNow, 0);
            Dflag = true;
        }
        #endregion

        #region Method 3
        if (System.Diagnostics.Debugger.IsAttached)
        {
            Dflag = true;
            DebuggerFound(DateTime.UtcNow, 1);
        }
        #endregion

        #region Method 4
        status = NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 0x1f, NoDebugInherit, 4, IntPtr.Zero);
        if (((uint)Marshal.PtrToStructure(NoDebugInherit, typeof(uint))) == 0)
        {
            Dflag = true;
            DebuggerFound(DateTime.UtcNow, 2);
        }

        status2 = NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 0x1e, hDebugObject, 4, IntPtr.Zero);
        if (status2 == 0)
        {
            DebuggerFound(DateTime.UtcNow, 3);
            Dflag = true;
        }

        #endregion
        #endregion
    }

    void SpeedupDetected(int TimeDifference)
    {
        float penaltyToAdd = Time.timeScale == 1f ? TimeDifference * .5f : 0f;
        SpeedPenalty += penaltyToAdd;
    }

    public string CreateMd5(string toMd5)
    {
        using (MD5 md5 = MD5.Create())
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(toMd5)))
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }

    void WriteToFileAndDebug(string message, bool writeToDebug)
    {
        try
        {
            StreamWriter fileWriter = new StreamWriter(filePath, true);
            fileWriter.WriteLine(message);
            fileWriter.Close();
            if (writeToDebug)
            {
                Debug.Log(message);
            }
        }
        catch (IOException e)
        {
            Debug.LogError(e.Message);
        }
    }
    #endregion

    #region Requesting checks.
    public void RequestDebuggerCheck()
    {
        RequestDebugCheck();
    }
    #endregion

    #region Abstracts
    public abstract void ClockDesync(int TimeDiff);
    public abstract void InterUpdate(int TimeDiff);
    public abstract void DebuggerFound(DateTime timeStamp, int methodId);
    #endregion
}