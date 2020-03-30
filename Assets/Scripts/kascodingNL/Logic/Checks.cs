using Assets.Scripts.kascodingNL;
using Assets.Scripts.kascodingNL.Logic.Utils;
using System;
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
        shoulduseSocket = (socket != null);
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

    public GameObject networkObject;

    public string SocketMD5Hash = string.Empty;

    //CommandExecutor Authorication
    private ISender sender;

    private SocketClient client;
    private bool shoulduseSocket = false;

    private bool Dflag;
    private IntPtr NoDebugInherit = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UInt32)));
    private long status;
    private long status2;
    private IntPtr hDebugObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));

    private string filePath = string.Empty;
    private bool flagged;

    public int timeDiff = 0;
    private int previousTime = 0;
    private int realTime = 0;
    private float gameTime = 0f;
    private bool detected = false;

    public int ToSceneId;

    private bool CheckDebugger;

    public float SpeedPenalty = 0f;
    public float MaxSpeedPenalty = 20f;

    private float SecondDelay;

    public SocketClient socketClient;

    #endregion

    #region Unity built in methods

    void Start()
    {
        #region Set variables
        if (socketClient != null && shoulduseSocket)
        {
            socketClient = networkObject.GetComponent<SocketClient>();
            socketClient.neededSender = sender;

            var crypt = new Cryptography();

            Tuple<string, string> keypair = crypt.CreateKeyPair();
            var key = keypair.Item1;
            sender = new ISender(crypt.Encrypt(SocketMD5Hash, key));
            Debug.Log("RSA Auth Key: " + key);
        }

        Dflag = false;
        filePath = Application.persistentDataPath + "/logs/latest.log";

        previousTime = DateTime.Now.Second;
        gameTime = 1f;

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
        UpdateMouse(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        if (!flagged && Dflag)
        {
            flagged = true;
            WriteToFileAndDebug($"Debugger: {Dflag}, status1: {status}, status2: {status2},", false);
            WriteToFileAndDebug(string.Format("NoDebugInherit: {0}, hDebugObject: {1}",
                ((uint)Marshal.PtrToStructure(NoDebugInherit, typeof(uint))).ToString(),
                ((IntPtr)Marshal.PtrToStructure(hDebugObject, typeof(IntPtr))).ToString()), true);
        }

        if (SpeedPenalty > 20f)
        {
            WriteToFileAndDebug(string.Format("[Anticheat] Speedup detected! MD5: {0}", CreateMd5(SpeedPenalty.ToString())), true);
        }
    }

    private float checkDelay;

    private void LateUpdate()
    {
        checkDelay += Time.deltaTime;
        if (CheckDebugger && checkDelay >= 1f)
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
            if(shoulduseSocket)
            {
                client.SendData("FlagDebug", true);
            }
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

    public float lastMouseX;
    public float lastMouseY;

    public float mouseX;
    public float mouseY;

    #region Verbose values

    private Verbose smoothVerbose = new Verbose(50);

    #endregion

    public void UpdateMouse(float x, float y)
    {
        lastMouseX = mouseX;
        lastMouseY = mouseY;

        mouseX += x;
        mouseY += y;

        float limit = .03f;
        float minusLimit = -.03f;

        Vector2 delta = new Vector2(mouseX, mouseY) - new Vector2(lastMouseX, lastMouseY);

        bool deltaXWithinLimits = delta.x >= minusLimit && delta.x <= limit;
        bool deltaYWithinLimits = delta.y >= minusLimit && delta.y <= limit;

        bool deltaXNotNull = delta.x != 0;
        bool deltaYNotNull = delta.y != 0;

        if ((!deltaXWithinLimits || !deltaYWithinLimits) && deltaXNotNull && deltaYNotNull && smoothVerbose.flag(1))
        {
            SmoothAim(delta);
        }
    }


    void WriteToFileAndDebug(string message, bool writeToDebug)
    {
        try
        {

            if (writeToDebug)
            {
                Debug.Log(message);
            }
        }
        catch (IOException e)
        {
            throw e;
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
    public abstract void SmoothAim(Vector2 delta);
    #endregion
}