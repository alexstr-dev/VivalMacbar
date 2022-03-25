using System.Diagnostics;

namespace Vival;

internal static class LinuxUtils
{
    /// <summary>
    ///     Setup our custom process which will be used to send commands.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public static void SetupProcess()
    {
        customProcess =
            Process.Start(executeCommand) ?? throw new NullReferenceException("CUSTOMPROCESS IS NULL");
    }

    /// <summary>
    ///     Get the output from a command.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string GetConsoleOut(string cmd, string args)
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
    ///     Executes a command.
    /// </summary>
    /// <param name="cmd"></param>
    public static void ExecuteCommand(string cmd)
    {
        customProcess?.StandardInput.WriteLine($"bash -c \"{cmd}\"");
    }

    /// <summary>
    ///     Retrieves the currently active window title.
    /// </summary>
    /// <returns></returns>
    public static string GetActiveWindowTitle()
    {
        return GetConsoleOut("xdotool", "getwindowfocus getwindowname");
    }

    #region Variables
    /// <summary>
    ///     <see cref="ProcessStartInfo" /> info for <see cref="GetConsoleOut" /> and <see cref="ExecuteCommand" />.
    /// </summary>
    private static readonly ProcessStartInfo executeCommand = new()
    {
        FileName = "/usr/bin/bash",
        CreateNoWindow = true,
        RedirectStandardInput = true,
        UseShellExecute = false
    };

    /// <summary>
    ///     Custom bash process for executing stdin commands.
    /// </summary>
    private static Process? customProcess;

    /// <summary>
    ///     New line character.
    /// </summary>
    private const char split = '\n';
    #endregion
}