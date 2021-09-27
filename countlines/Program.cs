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
				"\t[--help|-h]   --> displays help text\n" +
				"\t[file ending] --> specifies which file ending to look for\n" +
				"\t[--f|--final] --> specifies whether to recursively look for files or to stay within the current directory";

		static bool isExiting = false;																																				// Flag to coordinate exit on program interrupt.

		static string ending;																																						// Keep track of file ending.
		static SearchOption searchOption;																																			// Keep track of if to do it recursively or not.

		static void showHelp()																																						// Show help text to user.
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(HELP_TEXT);
			Console.ResetColor();
			Environment.Exit(0);
		}

		static void throwError(string message)																																		// Show red error message to user.
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("ERROR: " + message);
			Console.ResetColor();
			Environment.Exit(0);
		}

		static bool manageArgs(string[] args)
		{
			switch (args.Length)
			{
				case 0: searchOption = SearchOption.AllDirectories;  return false;																									// In the case of 0 arguments, use last file ending and do it recursively.
				case 1:																																								// In case of 1 argument, either help, final or new file ending.
					if (args[0] == "--help" || args[0] == "-h") { showHelp(); }
					if (args[0] == "--f" || args[0] == "--final") { searchOption = SearchOption.TopDirectoryOnly; return false; }
					if (args[0][0] != '.') { args[0] = '.' + args[0]; }
					searchOption = SearchOption.AllDirectories;
					return true;
				case 2:																																								// In case of 2 arguments, help doesn't work anywhere. Final is only option.
					if ((args[1] == "--f" || args[1] == "--final") && (args[0] != "--f" && args[0] != "--final" && args[0] != "--help" && args[0] != "-h"))
					{
						if (args[0][0] != '.') { args[0] = '.' + args[0]; }
						searchOption = SearchOption.TopDirectoryOnly;
						return true;
					}
					throwError("invalid arguments");
					return false;
				default: throwError("too many arguments"); return false;
			}
		}

		static long getDirFileLines()
		{
			string[] files = null;
			try { files = Directory.GetFiles(".", "*", searchOption); }																												// Get array of all files in dir. Use searchOption specified by user (recursive or non-recursive).
			catch (UnauthorizedAccessException) { throwError("unauthorized access to one or more objects in dir"); }
			long lineCount = 0;
			Console.WriteLine();																																					// Write initial line so that the cursor placement algorithm doesn't overwrite unwanted text.
			for (int i = 0; i < files.Length; i++)
			{
				if (isExiting) { return -1; }																																		// Make sure to exit ASAP if interrupt flag is set.
				if (files[i].EndsWith(ending))
				{
					IEnumerable<string> lines = null;																																// Get lines in file, count them, display line count for that file, add file line count to total line count.
					try { lines = File.ReadLines(files[i]); }
					catch (UnauthorizedAccessException) { throwError("unauthorized access to one or more objects in dir"); }
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
			Console.CancelKeyPress += exitHandler;																																	// Add event handler to handle program interrupt.
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "countlines_file_ending.txt");										// Find the file that stores the file ending for countlines.
			if (manageArgs(args))																																					// If the file ending needs to be updated, do that. If it doesn't leave it alone and just read it from file.
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
				catch (FileNotFoundException) { throwError("\"countlines_file_ending.txt\" couldn't be found in %appdata% folder, specify line ending to create file"); }
				ending = f.ReadLine();
				f.Dispose();
			}
			long lineCount = getDirFileLines();																																		// Get the lines in the directory.
			if (lineCount == -1) { return; }																																		// If interrupt was triggered, exit wordlessly.
			Console.WriteLine(lineCount);																																			// Report line count to user if everything went according to plan.
		}

		static void exitHandler(object sender, ConsoleCancelEventArgs args)
		{
			isExiting = true;																																						// Set isExiting flag to notify ongoing line counting algorithm to stop.
			args.Cancel = true;																																						// Don't terminate after processing interrupt event. Give this program time to terminate itself.
		}
	}
}