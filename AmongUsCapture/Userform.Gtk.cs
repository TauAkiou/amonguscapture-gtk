using System.Diagnostics.CodeAnalysis;
using System.Text;
using Gtk;

namespace AmongUsCapture
{
    public partial class UserForm
    {
        // Menubar
        private VBox _primaryWindowContainer;
        private MenuBar _primaryWindowMenuBar;
        private MenuItem _primaryMenuItemFile;
        private MenuItem _primaryMenuItemAbout;
        
        // Menu
        private Menu _primaryWindowMenuFile;
        private MenuItem _primaryWindowMenuQuitItem;
        
        
        // Top level windows
        private HPaned _primaryWindowPane;
        private VBox _primaryWindowLeftContainer;

        // UserSettings (Left Side)
        private VBox _userSettingsParentContainer;
        private Frame _userSettingsParentFrame;

        private Frame _gameInfoParentFrame;
        private VBox _gameInfoParentContainer;
        
        // GameCode objects
        private Frame _gameCodeParentFrame;
        private Box _gameCodeLayoutContainer;
        private Entry _gameCodeEntryField;
        private Button _gameCodeCopyButton;

        // Websocket/Host Control
        private Frame _hostControlFrame;
        private VBox _hostControlLayoutContainer;
        
        private Frame _urlHostEntryFrame;
        private HBox _urlHostEntryLayoutContainer;
        private Entry _urlHostEntryField;
        
        // Connect Code
        private Frame _connectCodeParentFrame;
        private HBox _connectCodeLayoutContainer;
        private Button _connectCodeSubmitButton;
        private Entry _connectCodeEntryField;
        
        
        // Right Side Text Console

        private Frame _consoleParentFrame;
        private VBox _consoleLayoutContainer;
        private ScrolledWindow _consoleScrolledWindow;
        private TextView _consoleTextView;


        private Box _currentStateContainer;
        private Frame _currentStateFrame;
        private Label _currentStateLabel;





        public void InitializeWindow()
        {
            // Menubar
            _primaryWindowContainer = new VBox();
            _primaryWindowMenuBar = new MenuBar();
            
            _primaryMenuItemFile = new MenuItem();
            
            _primaryWindowMenuFile = new Menu();
            _primaryWindowMenuQuitItem = new MenuItem();
            _primaryMenuItemAbout = new MenuItem();
            
            
            // Top level window pane.
            _primaryWindowPane = new HPaned();
            _primaryWindowLeftContainer = new VBox();

            // Left side User Settings Pane
            _userSettingsParentFrame = new Frame();
            _userSettingsParentContainer = new VBox();

            _gameInfoParentFrame = new Frame();
            _gameInfoParentContainer = new VBox();
            
            // Left Side Current State Field
            _currentStateFrame = new Frame();
            _currentStateContainer = new Box(Orientation.Vertical, 0);
            _currentStateLabel = new Label();

            // Left Side Game Code Fields
            _gameCodeParentFrame = new Frame();
            _gameCodeLayoutContainer = new HBox();
            _gameCodeCopyButton = new Button();
            _gameCodeEntryField = new Entry();

            // Left Side Websocket/Host Control
            _hostControlFrame = new Frame();
            _hostControlLayoutContainer = new VBox();

            _urlHostEntryFrame = new Frame();
            _urlHostEntryLayoutContainer = new HBox();
            _urlHostEntryField = new Entry();

            _connectCodeParentFrame = new Frame();
            _connectCodeLayoutContainer = new HBox();
            _connectCodeSubmitButton = new Button();
            _connectCodeEntryField = new Entry();
            

            

            // Right Side Console
            _consoleScrolledWindow = new ScrolledWindow();
            _consoleLayoutContainer = new VBox();
            _consoleParentFrame = new Frame();
            
            _consoleTextView = new TextView();
            
            //

            _primaryWindowContainer.Name = "_primaryWindowContainer";
            _primaryWindowContainer.PackStart(_primaryWindowMenuBar, false, false, 2);
            _primaryWindowContainer.PackStart(_primaryWindowPane, true, true, 0);
            
            _primaryWindowMenuBar.Name = "_primaryWindowMenuBar";
            _primaryWindowMenuBar.Append(_primaryMenuItemFile);
            _primaryWindowMenuBar.Append(_primaryMenuItemAbout);

            _primaryMenuItemFile.Name = "_primaryMenuItemFile";
            _primaryMenuItemFile.Label = "File";
            _primaryMenuItemFile.Submenu = _primaryWindowMenuFile;

            _primaryMenuItemAbout.Name = "_primaryMenuItemAbout";
            _primaryMenuItemAbout.Label = "About";
            _primaryMenuItemAbout.Activated += _primaryWindowMenuItemAbout_Activated;

            _primaryWindowMenuFile.Name = "_primaryWindowMenu";
            _primaryWindowMenuFile.Append(_primaryWindowMenuQuitItem);

            _primaryWindowMenuQuitItem.Name = "_primaryWindowMenuQuitItem";
            _primaryWindowMenuQuitItem.Label = "Quit";
            _primaryWindowMenuQuitItem.Activated += _primaryWindowMenuQuitItem_Activated;

            // _primaryWindowPane definition (splitContainer1)
            _primaryWindowPane.Name = "_primaryWindowPane";
            _primaryWindowPane.SetSizeRequest(824, 476);
            _primaryWindowPane.Position = 180;

            _primaryWindowPane.Pack1(_primaryWindowLeftContainer, true, false);
            _primaryWindowPane.Pack2(_consoleParentFrame, true, false);
            
            _primaryWindowLeftContainer.PackStart(_userSettingsParentFrame, true, true, 10);
            _primaryWindowLeftContainer.Name = "_primaryWindowLeftContainerH";
            _primaryWindowLeftContainer.Margin = 5;
            
            
            // UserSettings
            
            _userSettingsParentFrame.Label = "Settings";
            _userSettingsParentFrame.Name = "_userSettingsParentFrame";
            _userSettingsParentFrame.SetSizeRequest(276, 274);
            _userSettingsParentFrame.Add(_userSettingsParentContainer);

            _userSettingsParentContainer.Margin = 5;
            _userSettingsParentContainer.PackStart(_gameInfoParentFrame, true, false, 10);
            _userSettingsParentContainer.PackStart(_hostControlFrame, true, false, 10);
            _userSettingsParentContainer.Name = "_userSettingsParentContainer";

            _gameInfoParentFrame.Name = "_gameInfoParentFrame";
            _gameInfoParentFrame.Label = "Game Information";
            _gameInfoParentFrame.SetSizeRequest(55, 40);
            _gameInfoParentFrame.Add(_gameInfoParentContainer);

            _gameInfoParentContainer.Name = "_gameInfoParentContainer";
            _gameInfoParentContainer.PackStart(_currentStateFrame, true, false, 10);
            _gameInfoParentContainer.PackStart(_gameCodeParentFrame, true, false, 10);
            _gameInfoParentContainer.Margin = 5;
            
            // CurrentStateFrame
            _currentStateFrame.Add(_currentStateContainer);
            _currentStateFrame.Label = "Current State";
            _currentStateFrame.Name = "_currentStateFrame";
            _currentStateFrame.SetSizeRequest(55, 40);

            // CurrentStateBox
            _currentStateContainer.Name = "_currentStateContainer";
            _currentStateContainer.SetSizeRequest(55, 40);
            _currentStateContainer.PackStart(_currentStateLabel, true, false, 5);
            _currentStateContainer.Halign = Align.Center;
            _currentStateContainer.Valign = Align.Center;

            // CurrentState
            _currentStateLabel.Name = "_currentStateLabel";
            _currentStateLabel.Text = "Loading...";

            //
            // GAME CODE UI BLOCK
            //
            
            // _gameCodeParentFrame
            _gameCodeParentFrame.Add(_gameCodeLayoutContainer);
            _gameCodeParentFrame.Name = "_gameCodeParentFrame";
            _gameCodeParentFrame.Label = "Game Code";

            _gameCodeLayoutContainer.Name = "_gameCodeLayoutContainer";

            _gameCodeLayoutContainer.MarginBottom = 7;
            _gameCodeLayoutContainer.SetSizeRequest(25, 25);
            _gameCodeLayoutContainer.PackStart(_gameCodeEntryField, true, false, 10);
            _gameCodeLayoutContainer.PackStart(_gameCodeCopyButton, true, false, 10);
            
            _gameCodeCopyButton.SetSizeRequest(20, 25);
            _gameCodeCopyButton.Name = "_gameModeCopyButton";
            _gameCodeCopyButton.Label = "Copy";
            _gameCodeCopyButton.Clicked += _gameCodeCopyButton_Click;

            _gameCodeEntryField.Xalign = (float) 0.5;
            _gameCodeEntryField.SetSizeRequest(50, 20);
            _gameCodeEntryField.IsEditable = false;
           
            // CONNECT CODE UI BLOCK

            // HOST CONTROL UI BLOCK

            _hostControlFrame.Name = "_hostControlFrame";
            _hostControlFrame.Label = "Server Connection";
            _hostControlFrame.Add(_hostControlLayoutContainer);

            _hostControlLayoutContainer.Name = "_hostControlLayoutContainer";
            _hostControlLayoutContainer.Margin = 5;
            _hostControlLayoutContainer.SetSizeRequest(25, 20);
            _hostControlLayoutContainer.PackStart(_urlHostEntryFrame, true, false, 5);
            _hostControlLayoutContainer.PackStart(_connectCodeParentFrame, true, false, 5);

            _urlHostEntryFrame.Name = "_urlHostEntryFrame";
            _urlHostEntryFrame.Label = "Server URL";
            _urlHostEntryFrame.Add(_urlHostEntryLayoutContainer);

            _urlHostEntryLayoutContainer.Name = "_urlHostEntryLayoutContainer";
            _urlHostEntryLayoutContainer.SetSizeRequest(70, 20);
            _urlHostEntryLayoutContainer.PackStart(_urlHostEntryField, true, false, 5);
            _urlHostEntryLayoutContainer.MarginBottom = 5;
                
            _connectCodeParentFrame.Name = "_connectCodeParentFrame";
            _connectCodeParentFrame.Label = "Connect Code";
            _connectCodeParentFrame.Add(_connectCodeLayoutContainer);

            _connectCodeLayoutContainer.Name = "_connectCodeLayoutContainer";
            _connectCodeLayoutContainer.SetSizeRequest(25, 20);
            _connectCodeLayoutContainer.PackStart(_connectCodeEntryField, true, false, 5);
            _connectCodeLayoutContainer.PackStart(_connectCodeSubmitButton, true, false, 5);
            _connectCodeLayoutContainer.MarginBottom = 5;
            
            _connectCodeEntryField.Name = "_connectCodeEntryField";
            _connectCodeEntryField.Xalign = (float)0.5;
            _connectCodeEntryField.SetSizeRequest(70, 20);
            _connectCodeEntryField.MaxLength = 6;

            _connectCodeSubmitButton.Name = "_connectCodeSubmitButton";
            _connectCodeSubmitButton.Label = "Submit";
            _connectCodeSubmitButton.SetSizeRequest(30, 20);
            _connectCodeSubmitButton.Clicked += _connectCodeSubmitButton_Click;
            _connectCodeSubmitButton.CanDefault = true;
            
            
            
            // Right Side
            _consoleParentFrame.Name = "_consoleParentFrame";
            _consoleParentFrame.Label = "Console";
            _consoleParentFrame.Add(_consoleLayoutContainer);

            _consoleLayoutContainer.Name = "_consoleLayoutContainer";
            _consoleLayoutContainer.PackStart(_consoleScrolledWindow, true, true, 5);
            _consoleLayoutContainer.Margin = 5;

            _consoleScrolledWindow.Name = "_consoleScrolledWindow";
            _consoleScrolledWindow.Add(_consoleTextView);


            _consoleTextView.Name = "_consoleTextView";
            _consoleTextView.Editable = false;

            //_autoScrollCheckMenuItem.Name = "_autoscrollMenuItem";
            _consoleTextView.PopulatePopup += _consoleTextView_OnPopulateContextMenu;
            _consoleTextView.Buffer.Changed += _consoleTextView_BufferChanged;

            SetDefaultSize(824, 476);
            Add(_primaryWindowContainer);
           
        }
        
        
    }
}