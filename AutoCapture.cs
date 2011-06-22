// Based on ContentBlocker example extension.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fiddler;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;

[assembly: Fiddler.RequiredVersion("2.1.1.3")]
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyTitle("AutoCapture")]
[assembly: AssemblyDescription("Automatically save all content hierarchically")]
[assembly: AssemblyCompany("Vladimir Panteleev")]
[assembly: AssemblyProduct("AutoCapture")]

public class AutoCapture : IAutoTamper
{
    public void OnLoad() {
    }

    public void AutoTamperRequestBefore(Session oSession) {}
	public void AutoTamperRequestAfter(Session oSession) {}
    public void AutoTamperResponseBefore(Session oSession) {
		if (/*oSession.oResponse.headers.ExistsAndContains("Content-Type", "application/x-shockwave-flash")*/true) {
			String path = oSession.host + oSession.PathAndQuery;
			
			//path = path.Substring(0, path.IndexOf('?'));
			path = path
				.Replace("/", "\\")
				.Replace("?", "-\\")
				.Replace("&", "\\")
				.Replace(":" , "%3A")
				.Replace("*" , "%2A")
				.Replace("\"", "%22")
				.Replace("<" , "%3C")
				.Replace(">" , "%3E")
				.Replace("|" , "%7C");
			path = "C:\\Temp\\FiddlerCapture\\" + path;
			
			while (path.Contains("\\\\"))
				path = path.Replace("\\\\", "\\ \\");

			string[] segments = path.Split('\\');
			for (int j=0; j<segments.Length; j++)
				for (int i=200; i<segments[j].Length; i+=200)
					segments[j] = segments[j].Substring(0, i) + '\\' + segments[j].Substring(i);
			path = String.Join("\\", segments);

			for (int i=0; i<path.Length; i++)
				if (path[i]=='\\' && Win32File.Exists(path.Substring(0, i)))
				{
					string dirName = path.Substring(0, i);
					string tempFileName = dirName+".temp-index";
					Win32File.Move(dirName, tempFileName);
					Win32Directory.CreateDirectory(dirName);
					Win32File.Move(tempFileName, dirName+"\\index");
				}
			
			try {
				if (Win32Directory.Exists(path))
					path = path + "\\";
			} catch(Exception e) { throw new Exception("Directory.Exists failure: " + path, e); }

			if (path[path.Length-1]=='\\')
				path = path + "index";

			//String dir = Path.GetDirectoryName(path);
			String dir = path.Substring(0, path.LastIndexOf('\\'));

			try {
				//if (!Directory.Exists(dir))
				//	Directory.CreateDirectory(dir);
				Win32Directory.CreateDirectory(dir);
			} catch(Exception e) { throw new Exception("Directory.CreateDirectory failure: " + dir, e); }

			oSession.utilDecodeResponse();
			
			//oSession.SaveResponseBody(path);
			byte[] data = oSession.responseBodyBytes;
			FileStream s;
			try {
				s = Win32File.Open(path, FileMode.Create);
			} catch(Exception e) { throw new Exception("Open failure: " + path, e); }
			try {
				s.Write(data, 0, data.Length);
			} catch(Exception e) { throw new Exception("Write failure: " + path, e); }
			try {
				s.Close();
			} catch(Exception e) { throw new Exception("Close failure: " + path, e); }
		}
    }
    public void AutoTamperResponseAfter(Session oSession) {}
    public void OnBeforeReturningError(Session oSession) {}
    public void OnBeforeUnload() {}
}
