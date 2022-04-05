using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Vival
{
    /// <summary>
    /// Vivals main canvas.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        // * Most of these variables are only intended to be used from events and cached.
        /// <summary>
        /// Cached position value.
        /// </summary>
        private static PixelPoint staticPosition;
        #endregion

        /// <summary>
        /// Initialize all of the components before making {Binding *} available.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            viewModel.StartLoop();
            staticPosition = Position;
        }

        /// <summary>
        /// If the client tries to change the bars position during runtime, reset it back to <see cref="staticPosition"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e) => Position = staticPosition;
    }

    /// <summary>
    /// This class is responsible for grabbing property values, changing them, etc.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Fired whenever a property has been changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
        #region Variables
        /// <summary>
        /// Current Date and Time.
        /// </summary>
        private string currentDate = DateTime.Now.ToString(" ddd MMM dd  HH:mm");

        /// <summary>
        /// The hosts username.
        /// </summary>
        private readonly string username = Environment.UserName;

        /// <summary>
        /// The background color to be used.
        /// </summary>
        private const string backgroundColor = "#0A0A0A";

        /// <summary>
        /// Name of the current workspace.
        /// </summary>
        private string currentWorkspace = string.Empty;

        /// <summary>
        /// Name of the focused window.
        /// </summary>
        private string currentWindow = string.Empty;

        /// <summary>
        /// First active Graphics Card.
        /// </summary>
        private string activeGPU = string.Empty;

        /// <summary>
        /// Vivals height (current, min, max).
        /// <para>Default is <c>21</c>.</para>
        /// </summary>
        private const int height = 21;

        /// <summary>
        /// Vivals width (current, min, max).
        /// <para>Default is <c>1920 - 2</c>.</para>
        /// </summary>
        private const int width = 1920;
        #endregion

        #region Bindings
        /// <summary>
        /// <inheritdoc cref="currentDate"/>
        /// </summary>
        public string CurrentDate
        {
            get => currentDate;
            set
            {
                currentDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDate)));
            }
        }

        /// <summary>
        /// <inheritdoc cref="currentWorkspace"/>
        /// </summary>
        public string CurrentWorkspace
        {
            get => currentWorkspace;
            set
            {
                currentWorkspace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWorkspace)));
            }
        }

        /// <summary>
        /// <inheritdoc cref="activeGPU"/>
        /// </summary>
        public string ActiveGPU
        {
            get => activeGPU;
            set
            {
                activeGPU = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveGPU)));
            }
        }

        /// <summary>
        /// <inheritdoc cref="currentWindow"/>
        /// </summary>
        public string CurrentWindow
        {
            get => currentWindow;
            set
            {
                currentWindow = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentWindow)));
            }
        }

        /// <summary>
        /// <inheritdoc cref="backgroundColor"/>
        /// </summary>
        public string BackgroundColor => backgroundColor;

        /// <summary>
        /// <inheritdoc cref="username"/>
        /// </summary>
        public string Username => username;

        /// <summary>
        /// <inheritdoc cref="height"/>
        /// </summary>
        public int Height => height;

        /// <summary>
        /// <inheritdoc cref="width"/>
        /// </summary>
        public int Width => width;
        #endregion

        /// <summary>
        /// Starts the loop.
        /// </summary>
        public async void StartLoop()
        {
            const int wait = 200;
            while (true)
            {
                UpdateDate();
                UpdateWorkspace();
                UpdateGPU();
                UpdateWindow();
                await Task.Delay(wait);
            }
        }

        /// <summary>
        /// Updates the Date and Time.
        /// </summary>
        private void UpdateDate() => CurrentDate = DateTime.Now.ToString("ddd MMM dd  HH:mm");

        /// <summary>
        /// Updates the value of <see cref="CurrentWorkspace"/>.
        /// </summary>
        private async void UpdateWorkspace() => CurrentWorkspace = await LinuxUtils.GetActiveWorkspace();

        /// <summary>
        /// Updates the value of <see cref="ActiveGPU"/>.
        /// </summary>
        private async void UpdateGPU() => ActiveGPU = await LinuxUtils.GetGPUName();

        /// <summary>
        /// Updates the value of <see cref="CurrentWindow"/>.
        /// </summary>
        private async void UpdateWindow() => CurrentWindow = await LinuxUtils.GetActiveWindowTitle();
    }
}