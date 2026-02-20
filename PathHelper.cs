using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OCC.UI.TestingFramework.Utility
{
	public class PathHelper
	{
		public static string GetProjectPathWithStackTrace(int frameIndex)
		{
			var frame = new StackTrace().GetFrame(frameIndex);
			var testAssembly = frame.GetMethod().DeclaringType.Assembly;
			var workingDirectory = Path.GetDirectoryName(testAssembly.Location.Substring("file:///".Length));
			var parts = workingDirectory.Split(Path.DirectorySeparatorChar).ToList();
			var projectName = testAssembly.GetName().Name;
			var rootParts = parts.TakeWhile(p => p != projectName).ToList();
			rootParts.Add(projectName);
			var result = string.Join(Path.DirectorySeparatorChar.ToString(), rootParts.ToArray());
			return result;
		}

		public static string GetProjectWorkingDirectory()
		{
			var frame = new StackTrace().GetFrame(1);
			var testAssembly = frame.GetMethod().DeclaringType.Assembly;
			var workingDirectory = Path.GetDirectoryName(testAssembly.Location.Substring("file:///".Length));
			return workingDirectory;
		}

		public static string HackPath(string path)
		{
			var parts = path.Split(@"\/".ToCharArray());
			var stack = new Stack<string>();
			foreach (var part in parts)
			{
				if (part == "..")
					stack.Pop();
				else
					stack.Push(part);
			}
			return string.Join("\\", stack.Reverse());
		}

		public static string Combine(string string1, string string2)
		{
			return Path.Combine(string1, string2.Substring(0, 1) == @"\" ? string2.Substring(1, string2.Length - 1) : string2);
		}
	}
}
