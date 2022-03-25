using System.Diagnostics;

namespace Vival;

internal static class LinuxUtils
{
    #region Variables
    /// <summary>
    /// <see cref="ProcessStartInfo"/> info for <see cref="GetConsoleOut"/> and <see cref="ExecuteCommand"/>.
    /// </summary>
    private static readonly ProcessStartInfo executeCommand = new()
    {
        FileName = "/usr/bin/bash",
        CreateNoWindow = true,
        RedirectStandardInput = true,
        UseShellExecute = false
    };

    /// <summary>
    /// Custom bash process for executing stdin commands.
    /// </summary>
    private static Process? customProcess;

    /// <summary>
    /// The amount of available workspaces.
    /// </summary>
    public static int workspacesCount;

    /// <summary>
    /// New line character.
    /// </summary>
    private const char split = '\n';
    #endregion

    /// <summary>
    /// Setup our custom process which will be used to send commands.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public static void SetupProcess()
    {
        customProcess = Process.Start(executeCommand) ?? throw new NullReferenceException("CUSTOMPROCESS IS NULL");
        workspacesCount = Convert.ToInt32(GetConsoleOut("xdotool", "get_num_desktops"));
    }

    /// <summary>
    /// Get the output from a command.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string GetConsoleOut(string cmd, string args)
    {
        executeCommand.FileName = cmd;
        executeCommand.Arguments = args;
        executeCommand.CreateNoWindow = true;
        executeCommand.RedirectStandardOutput = true;
        executeCommand.UseShellExecute = false;
        using var proc = Process.Start(executeCommand);
        var stdOut = proc?.StandardOutput.ReadToEnd();
        proc?.StandardOutput.Close();
        return stdOut?.Split(split)[0] ?? string.Empty;
    }

    /// <summary>
    /// Executes a command.
    /// </summary>
    /// <param name="cmd"></param>
    public static void ExecuteCommand(string cmd) => customProcess?.StandardInput.WriteLine($"bash -c \"{cmd}\"");

    /// <summary>
    /// Retrieves the current kernel.
    /// </summary>
    /// <returns></returns>
    public static string GetKernel() => GetConsoleOut("uname", "-r");

    /// <summary>
    /// Retrieves the currently active workspace.
    /// </summary>
    /// <param name="startFromOne">Start counting from 1 instead of 0</param>
    /// <returns></returns>
    public static int GetActiveWorkspace(bool startFromOne = true)
    {
        var result = Convert.ToInt32(GetConsoleOut("xdotool", "get_desktop"));
        return startFromOne ? result + 1 : result;
    }

    /// <summary>
    /// Try and retrieve the active GPU name.
    /// </summary>
    /// <returns>"Unknown Graphics Device" if no name was found.</returns>
    public static string GetGPUName()
    {
        var result = GetConsoleOut("/usr/bin/bash",
            "-c \"lspci | grep VGA | cut -d \":\" -f3 | cut -d \"[\" -f2 | cut -d \"]\" -f1\"");
        return result is {Length: < 2} ? "Unknown Graphics Device" : result;
    }

    /// <summary>
    /// Retrieves the currently active window title.
    /// </summary>
    /// <returns></returns>
    public static string GetActiveWindowTitle() => GetConsoleOut("xdotool", "getwindowfocus getwindowname");
}