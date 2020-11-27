using System.ComponentModel;
using Gtk;

namespace AUCapture_GTK.GTK
{
    public partial class PrimaryWindow
    {
        private Box _baseContainer;
        private HeaderBar _headerBar;
        
        // Application Menu
        private MenuButton _appMenuButton;
        private Image _appMenuButtonImage;
        
        private Popover _appMenuPopover;
        private Box _appMenuPopoverContainer;
        
        private Button _appMenuSettingsButton;
        private Image _appMenuSettingsButtonImage;

        private Button _appMenuAboutButton;
        private Image _appMenuAboutButtonImage;
        
        // Capture Status
        private Box _controlContainer;
        
        // Game Status Frame
        private Frame _gameStatusFrame;
        private Box _gameStatusContainer;
        
        private Frame _gameStatusStateFrame;
        private Box _gameStatusStateContainer;
        private Label _gameStatusStateLabel;

        private Frame _gameStatusCodeFrame;
        private Box _gameStatusCodeContainer;
        private Entry _gameStatusCodeEntry;

        private Frame _botConnectionFrame;
        private Box _botConnectionContainer;
        private Box _botConnectionFieldContainer;
        private Frame _botConnectionFieldHostnameFrame;
        private Entry _botConnectionFieldHostnameEntry;
        private Frame _botConnectionFieldCodeFrame;
        private Entry _botConnectionFieldCodeEntry;
        private Box _botConnectionButtonContainer;
        private Button _botConnectionButtonSubmit;

        private Separator _baseContainerSeparator;
        

        // Capture Console
        private Expander _consoleExpander;
        private ScrolledWindow _consoleScrolledWindow;
        private TextView _consoleTextView;
    }
}