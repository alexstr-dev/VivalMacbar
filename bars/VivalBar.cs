using Eto.Drawing;
using Eto.Forms;

namespace Vival.bars;

public sealed class VivalBar : Form
{
    #region Variables
    /// <summary>
    /// The date label.
    /// </summary>
    // ? Labels that should update after startup should all be assigned their own variable.
    // ? This is because you can change the value of it directly, only redrawing the single component rather than the entire cell-set which is much more expensive.
    private readonly Label date = new()
    {
        Text = $"{DateTime.Now:D} ({DateTime.Now:HH:m:s})", TextColor = Colors.White,
        TextAlignment =
            TextAlignment.Right // ! <= Don't try and rely on this alignment logic, it hardly even works as it should.
    };

    /// <summary>
    /// The workspaces label (current / max).
    /// </summary>
    private readonly Label workspaces = new()
    {
        Text = $"{LinuxUtils.GetActiveWorkspace()} / {LinuxUtils.workspacesCount}",
        TextColor = Colors.Gray
    };

    /// <summary>
    /// The currently active window.
    /// </summary>
    private readonly Label activeWindow = new()
    {
        Text = LinuxUtils.GetActiveWindowTitle(),
        TextColor = Colors.White
    };

    /// <summary>
    /// Padding for <see cref="DrawContent"/>.
    /// </summary>
    private readonly Padding contentPadding = new(10, 1, 0, 0);

    /// <summary>
    /// Padding for <see cref="DrawDate"/>.
    /// </summary>
    private readonly Padding datePadding = new(0, 0, 10, 0);

    /// <summary>
    /// Max and Min size.
    /// </summary>
    // * Replace 1920 with your screens width and make proper adjustments if needed.
    // * 1920 can also (in theory, not tested) be extended to your total resolution (all monitors combined) to stretch across them.
    private readonly Size defaultSize = new(1920, 22);

    /// <summary>
    /// Spacing for <see cref="DrawContent"/>.
    /// </summary>
    private readonly Size contentSpacing = new(5, 5);

    /// <summary>
    /// This is the position that's used for the bar.
    /// <para>0, 0 by default which makes it spawn on your first monitor.</para>
    /// </summary>
    private readonly Point position = new(0, 0);

    /// <summary>
    /// Symbol used in <see cref="Separator"/>.
    /// </summary>
    private const string separator = "|";
    #endregion

    /// <summary>
    /// Begin drawing the bar.
    /// </summary>
    public VivalBar()
    {
        Title = "Vival";
        DrawContent();
        // * Not like xmonad gives the slightest shit about MinimumSize but yeah.
        Size = MinimumSize = defaultSize;
        AutoSize = true;
        Resizable = Topmost = false;
        BackgroundColor = Color.FromArgb(10, 10, 10);
        HackSyncSize();
        UpdateBar();
        SyncWorkspace();
    }

    /// <summary>
    /// Creates a new gray "|" separator.
    /// </summary>
    /// <returns></returns>
    private TableCell Separator() => new(new Label {Text = separator, TextColor = Colors.White});

    /// <summary>
    /// Draws all of the content.
    /// </summary>
    private void DrawContent() =>
        // * Begin drawing the cells.
        Content = new TableLayout
        {
            Spacing = contentSpacing,
            Padding = contentPadding,
            Rows =
            {
                new TableRow(DrawText(" ", Colors.White, null, 0, 2),
                    activeWindow,
                    //Separator(),
                    //DrawText(" ", Colors.White, null, 5, 3),
                    //DrawText(LinuxUtils.GetGPUName(), Colors.White),
                    //Separator(),
                    //DrawText(" ", Colors.White, null, 5, 3),
                    DrawDate())
            }
        };

    /// <summary>
    /// Draws text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="textColor"></param>
    /// <param name="callback"></param>
    /// <param name="leftOffset"></param>
    /// <param name="yOffset"></param>
    /// <returns></returns>
    private TableCell DrawText(string text, Color textColor, EventHandler<MouseEventArgs>? callback = null,
        int leftOffset = 0, int yOffset = 0)
    {
        var label = new Label {Text = text, TextColor = textColor};
        if (callback is not null) label.MouseDown += callback;
        var layout = new TableLayout {Rows = {label}};
        var cell = new TableCell(layout);
        if (leftOffset + yOffset is 0) return cell;
        layout.Padding = new Padding(leftOffset, yOffset);
        return cell;
    }

    /// <summary>
    /// Draws the current date and time.
    /// </summary>
    /// <returns></returns>
    private TableCell DrawDate() =>
        new(new TableLayout
        {
            Padding = datePadding,
            Rows = {date}
        });

    /// <summary>
    /// Shitty way of trying to keep the size consistent.
    /// <para>Should be reworked in the future if possible.</para>
    /// </summary>
    private async void HackSyncSize()
    {
        const int wait = 50;
        while (true)
        {
            Size = defaultSize;
            Location = position;
            await Task.Delay(wait);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    /// Syncs the bar to the currently active workspace.
    /// <para>Thanks nukistan for helping with this.</para>
    /// </summary>
    private void SyncWorkspace() =>
        new Thread(() =>
        {
            const int wait = 100;
            while (true)
            {
                LinuxUtils.ExecuteCommand(
                    "xdotool search --class Vival set_desktop_for_window %@ $(xdotool get_desktop)");
                Thread.Sleep(wait);
            }
        }) {Priority = ThreadPriority.Lowest, IsBackground = true}.Start();

    /// <summary>
    /// Updates the bars content.
    /// </summary>
    private void UpdateBar() =>
        new Thread(() =>
        {
            const int wait = 360;
            while (true)
            {
                Application.Instance.Invoke(() =>
                {
                    date.Text = DateTime.Now.ToString("ddd MMM dd  HH:mm");
                    workspaces.Text = LinuxUtils.GetConsoleOut("/usr/bin/bash",
                        "-c \"wmctrl -d | grep -w '*' | cut -d ' ' -f13\"");
                    //workspaces.Text = $"{LinuxUtils.GetActiveWorkspace()} / {LinuxUtils.workspacesCount}";
                    activeWindow.Text = LinuxUtils.GetActiveWindowTitle();
                });
                Thread.Sleep(wait);
                // ! GC.Collect() is needed mainly because of the allocation issues from redrawing components.
                GC.Collect();
            }
        }) {Priority = ThreadPriority.Lowest, IsBackground = true}.Start();
}