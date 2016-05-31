using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using HikariGGNPatcher.Annotations;
using HikariGGNPatcher.Misc;

namespace HikariGGNPatcher
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties & Fields

        private readonly Patcher _patcher;

        private bool _isGameDataFound;
        public bool IsGameDataFound
        {
            get { return _isGameDataFound; }
            set
            {
                _isGameDataFound = value;
                OnPropertyChanged(nameof(IsGameDataFound));
            }
        }

        private bool _isGameStartable;
        public bool IsGameStartable
        {
            get { return _patcher.IsSteamPathFound && _isGameStartable; }
            set
            {
                _isGameStartable = value;
                OnPropertyChanged(nameof(IsGameStartable));
            }
        }

        #endregion

        #region Commands

        private ActionCommand _minimizeCommand;
        public ActionCommand MinimizeCommand => _minimizeCommand ?? (_minimizeCommand = new ActionCommand(Minimize));

        private ActionCommand _closeCommand;
        public ActionCommand CloseCommand => _closeCommand ?? (_closeCommand = new ActionCommand(Close));

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        private ActionCommand _startGameCommand;
        public ActionCommand StartGameCommand => _startGameCommand ?? (_startGameCommand = new ActionCommand(StartGame));

        private ActionCommand _patchCommand;
        public ActionCommand PatchCommand => _patchCommand ?? (_patchCommand = new ActionCommand(ApplyPatch));

        private ActionCommand _browsePathCommand;
        public ActionCommand BrowsePathCommand => _browsePathCommand ?? (_browsePathCommand = new ActionCommand(BrowsePath));

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            _patcher = new Patcher();

            Initialize();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            Application.Current.Exit += (sender, args) => _patcher.Cleanup();
            _patcher.Setup();

            _patcher.InitializeSteamPath();
            IsGameDataFound = _patcher.InitializeGamePath();
        }

        private void StartGame()
        {
            _patcher.StartGame();
        }

        private void ApplyPatch()
        {
            IsGameStartable = _patcher.ApplyPatch();
        }

        private void BrowsePath()
        {
            IsGameDataFound = _patcher.AskUserForPath();
        }

        private void OpenHomepage()
        {
            Process.Start("http://hikari-translations.de");
        }

        private void Minimize()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void Close()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        //DarthAffe 15.05.2016: CallerMemberName is 4.5+ :(
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
