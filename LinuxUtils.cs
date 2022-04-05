using System.Diagnostics;
using Eto.Forms;

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
    public static async Task SetupProcess()
    {
        customProcess = Process.Start(executeCommand) ?? throw new NullReferenceException("CUSTOMPROCESS IS NULL");
        workspacesCount = Convert.ToInt32(await GetConsoleOut("xdotool", "get_num_desktops"));
    }

    /// <summary>
    /// Get the output from a command.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task<string> GetConsoleOut(string cmd, string args)
    {
        executeCommand.FileName = cmd;
        executeCommand.Arguments = args;
        executeCommand.CreateNoWindow = true;
        executeCommand.RedirectStandardOutput = true;
        executeCommand.UseShellExecute = false;
        using var proc = Process.Start(executeCommand);
        var stdOut = await proc?.StandardOutput.ReadToEndAsync()!;
        proc.StandardOutput.Close();
        return stdOut.Split(split)[0];
    }

    /// <summary>
    /// Switch to the next workspace.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static async void OnRequestIncreaseWorkspace(object? sender, MouseEventArgs e) =>
        await ExecuteCommand("xdotool set_desktop --relative 1");

    /// <summary>
    /// Switch to the previous workspace.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static async void OnRequestDecreaseWorkspace(object? sender, MouseEventArgs e) =>
        await ExecuteCommand("xdotool set_desktop --relative -- -1");

    /// <summary>
    /// Executes a command.
    /// </summary>
    /// <param name="cmd"></param>
    public static async Task ExecuteCommand(string cmd) =>
        await customProcess?.StandardInput.WriteLineAsync($"bash -c \"{cmd}\"")!;

    /// <summary>
    /// Retrieves the current kernel.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetKernel() => await GetConsoleOut("uname", "-r");

    /// <summary>
    /// Retrieves the currently active workspace.
    /// </summary>
    /// <param name="startFromOne">Start counting from 1 instead of 0</param>
    /// <returns></returns>
    public static async Task<int> GetActiveWorkspace(bool startFromOne = true)
    {
        var result = Convert.ToInt32(await GetConsoleOut("xdotool", "get_desktop"));
        return startFromOne ? result + 1 : result;
    }

    /// <summary>
    /// Get the Workspace ID from a programs class name (for example, Vival).
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    public static async Task<int> GetClassWorkspace(string className)
    {
        var result = Convert.ToInt32(await GetConsoleOut("xdotool", $"search --class {className} get_desktop"));
        return result;
    }

    /// <summary>
    /// Try and retrieve the active GPU name.
    /// </summary>
    /// <returns>"Unknown Graphics Device" if no name was found.</returns>
    public static async Task<string> GetGPUName()
    {
        var result = await GetConsoleOut("/usr/bin/bash",
            "-c \"lspci | grep VGA | cut -d \":\" -f3 | cut -d \"[\" -f2 | cut -d \"]\" -f1\"");
        return result is {Length: < 2} ? "Unknown Graphics Device" : result;
    }

    /// <summary>
    /// Retrieves the currently active window title.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetActiveWindowTitle() =>
        await GetConsoleOut("xdotool", "getwindowfocus getwindowname");
}