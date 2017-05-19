using System;
using System.IO;
using static Finals;

public class LogHelper
{

    
    public LogHelper()
	{
	}

    public static void onLog(string source, string text)
    {
        FileStream filestream = new FileStream(LOG_FILES_PATH + "Consoleout.txt", FileMode.Append, FileAccess.Write);
        StreamWriter streamwriter = new StreamWriter(filestream);
        Console.SetOut(streamwriter);
        Console.SetError(streamwriter);
        //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")+"|Error source: "+source+"|" + e);
        Console.WriteLine("\n");
        Console.WriteLine("*********************************************************************");
        Console.WriteLine(DateTime.Now.ToString("u"));
        Console.WriteLine("Log source: " + source + "|" +text);
        Console.WriteLine("---------------------------------------------------------------------");
        streamwriter.Close();
    }
    public static void onFailureLog(string source, Exception e, string parname = "Unknown")
    {
        FileStream filestream = new FileStream(LOG_FILES_PATH + "ConsoleErrorOut.txt", FileMode.Append, FileAccess.Write);
        StreamWriter streamwriter = new StreamWriter(filestream);
        Console.SetOut(streamwriter);
        Console.SetError(streamwriter);
        //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")+"|Error source: "+source+"|" + e);
        Console.WriteLine("\n");
        Console.WriteLine("*********************************************************************");
        Console.WriteLine(DateTime.Now.ToString("u"));
        Console.WriteLine("Error source: " + source + "|" + parname + "|" + e);
        Console.WriteLine("---------------------------------------------------------------------");
        streamwriter.Close();
    }
}