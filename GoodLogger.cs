using System;
using System.IO;

public class GoodLogger
{
#if !UNITY_WEBGL
    private string filename;
    private StreamWriter writter;
    private FileStream stream;
    private object threadlock;
#endif

    public const byte TYPE_DEFAULT = 1;
    public const byte TYPE_WARNING = 2;
    public const byte TYPE_ERROR = 3;

    public GoodLogger(string name)
    {
#if UNITY_WEBGL
        UnityEngine.Debug.Log("Good log " + name + " was virtually instantiated");
#else
        threadlock = new object();
        filename = name;
        stream = new FileStream(name, FileMode.Create);
        writter = new StreamWriter(stream);
        Write("Log created on " + DateTime.Now.ToLongDateString());
#endif
    }

    public void Write(object text, byte logType = TYPE_DEFAULT)
    {
#if UNITY_WEBGL || UNITY_EDITOR
        if (logType == TYPE_DEFAULT)
            UnityEngine.Debug.Log(text);
        else if (logType == TYPE_ERROR)
            UnityEngine.Debug.LogError(text);
        else
            UnityEngine.Debug.LogWarning(text);
#endif
#if !UNITY_WEBGL
        try
        {
            lock (threadlock)
            {
                if (logType == TYPE_DEFAULT)
                    writter.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text.ToString());
                else if (logType == TYPE_ERROR)
                    writter.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] ERROR: " + text.ToString());
                else
                    writter.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] WARNING: " + text.ToString());
                writter.Flush();
            }
        }
        catch (Exception e)
        {
            // very bad case
            stream = new FileStream(filename + ".part2", FileMode.Create);
            writter = new StreamWriter(stream);
            Write("The second part was created because the first part of the log has crashed due " + e.Message);
        }
#endif
    }

    public void Stop()
    {
#if UNITY_WEBGL
        UnityEngine.Debug.Log("GoodLogger was stopped.");
#else
        if (writter != null)
        {
            writter.Flush();
            writter = null;
        }            
        if (stream != null)
        {
            stream.Close();
            stream = null;
        }     
#endif      
    }
}