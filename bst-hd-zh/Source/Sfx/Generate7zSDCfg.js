Main(WScript.Arguments);

function Main(args)
{
	if (args.Length != 1)
		Usage();

	var executeFilePath = args.Item(0);
	var fileNamePos = executeFilePath.lastIndexOf("\\");
	if (fileNamePos == -1)
		fileNamePos = 0;
	else
		fileNamePos += 1;
	var executeFile = executeFilePath.substring(fileNamePos);

	while (!WScript.StdIn.AtEndOfStream) {
		var line = WScript.StdIn.ReadLine();
		line = line.replace("@@EXECUTE_FILE@@", executeFile);
		WScript.Echo(line);
	}
}

function Usage()
{
	WScript.StdErr.WriteLine("Usage: " + WScript.ScriptName +
	    " <execute file>");
	WScript.Exit(1);
}
