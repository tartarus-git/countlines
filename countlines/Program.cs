using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace countlines
{
	class Program
	{
		const string HELP_TEXT = "Usage: countlines [--help|-h]\n" +
			"Usage: countlines [file ending] [--f|--final]\n\n" +

			"Description: Recursively scans through a directory for all files of the specified type and returns the total amount of lines.\n\n" +

			"Arguments:\n" +
				"\t[--help|-h]	 --> displays help text\n" +
				"\t[file ending] --> specifies which file ending to look for\n" +
				"\t[--f|--final] --> specifies whether to recursively look for files or to stay within the current directory";

		static bool isExiting = false;

		static string ending;
		static SearchOption searchOption;

		static void ShowHelp()
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(HELP_TEXT);
			Console.ResetColor();
			Environment.Exit(0);
		}

		static void ThrowError(string Message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("ERROR: " + Message);
			Console.ResetColor();
			Environment.Exit(0);
		}

		static bool ManageArgs(string[] args)
		{
			switch (args.Length)
            {
				case 0: return false;
				case 1:
					if (args[0] == "--help" || args[0] == "-h") { ShowHelp(); }
				    if (args[0] == "--f" || args[0] == "--final") { searchOption = SearchOption.TopDirectoryOnly; return false; }
				    if (args[0][0] != '.') { args[0] = '.' + args[0]; }
					return true;
				case 2:
					if (args[0] == "--help" || args[0] == "-h" || args[1] == "--help" || args[1] == "-h") { ThrowError("invalid arguments"); }
					if (args[1] == "--f" || args[1] == "--final")
                    {
						if (args[0][0] != '.') { args[0] = '.' + args[0]; }
					    return true;
                    }
					ThrowError("invalid arguments");
					return false;
				default: ThrowError("too many arguments"); return false;
            }
		}

		static long GetDirFileLines()
		{
			string[] files = null;
			try
			{
				files = Directory.GetFiles(".", "*", searchOption);
			}
			catch (UnauthorizedAccessException)
			{
				ThrowError("unauthorized access to one or more objects in dir");
			}
			long lineCount = 0;
			Console.WriteLine();
			for (int i = 0; i < files.Length; i++)
			{
				if (isExiting) { return -1; }
				if (files[i].EndsWith(ending))
				{
					IEnumerable<string> lines = null;
					try { lines = File.ReadLines(files[i]); }
					catch (UnauthorizedAccessException) { ThrowError("unauthorized access to one or more objects in dir"); }
					long fileLineCount = lines.Count();
					string output = files[i] + " --> " + fileLineCount;
					if (output.Length < Console.BufferWidth)
					{
						int paddingLength = Console.BufferWidth - output.Length;
						for (int j = 0; j < paddingLength; j++) { output += ' '; }
					}
					Console.SetCursorPosition(0, Console.CursorTop - 1);
					Console.Write(output);
					lineCount += fileLineCount;
				}
			}
			return lineCount;
		}

		static void Main(string[] args)
		{
			Console.CancelKeyPress += exitHandler;
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "countlines_file_ending.txt");
			if (ManageArgs(args))
			{
				FileStream f = File.OpenWrite(filePath);
				byte[] buffer = Encoding.ASCII.GetBytes(args[0]);
				f.Write(buffer, 0, buffer.Length);
				f.SetLength(f.Position);
				f.Dispose();
				ending = args[0];
			}
			else
			{
				StreamReader f = null;
				try { f = new StreamReader(filePath); }
				catch (FileNotFoundException) { ThrowError("\"countlines_file_ending.txt\" couldn't be found in %appdata% folder, specify line ending to create file"); }
				ending = f.ReadLine();
				f.Dispose();
			}
			long lineCount = GetDirFileLines();
			if (lineCount == -1) { return; }
			Console.WriteLine(lineCount);
		}

		static void exitHandler(object sender, ConsoleCancelEventArgs args)
		{
			isExiting = true;
			args.Cancel = true;
		}
	}
}