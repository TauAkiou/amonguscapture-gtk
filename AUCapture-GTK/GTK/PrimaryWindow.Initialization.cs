using System;
using Gtk;

namespace AUCapture_GTK.GTK
{
    public partial class PrimaryWindow
    {
        private void window_initialize()
        {
            _baseContainer = new Box(Orientation.Vertical, 5);
            _headerBar = new HeaderBar();
            _appMenuButton = new MenuButton();
            _appMenuButtonImage = Image.NewFromIconName("format-justify-fill", IconSize.Button);
            
            _appMenuPopover = new Popover(_appMenuButton);
            _appMenuPopoverContainer = new Box(Orientation.Vertical, 5);

            _appMenuSettingsButton = new Button();
            _appMenuSettingsButtonImage = Image.NewFromIconName("preferences-other", IconSize.Button);
            
            _appMenuAboutButton = new Button();
            _appMenuAboutButtonImage = Image.NewFromIconName("help-about", IconSize.Button);

            _controlContainer = new Box(Orientation.Horizontal, 5);
            _gameStatusFrame = new Frame("Game Status");
            _gameStatusContainer = new Box(Orientation.Vertical, 5);

            _gameStatusStateFrame = new Frame("State");
            _gameStatusStateContainer = new Box(Orientation.Vertical, 5);
            _gameStatusStateLabel = new Label("Disconnected");
            
            _gameStatusCodeFrame = new Frame("Game Code");
            _gameStatusCodeContainer = new Box(Orientation.Vertical, 5);
            _gameStatusCodeEntry = new Entry();
            
            _botConnectionFrame = new Frame("Bot Connection");
            _botConnectionContainer = new Box(Orientation.Horizontal, 5);
            _botConnectionFieldContainer = new Box(Orientation.Vertical, 5);
            _botConnectionFieldHostnameFrame = new Frame("Hostname");
            _botConnectionFieldHostnameEntry = new Entry();
            _botConnectionFieldCodeFrame = new Frame("Connection Code");
            _botConnectionFieldCodeEntry = new Entry();
            _botConnectionButtonContainer = new Box(Orientation.Horizontal, 5);
            _botConnectionButtonSubmit = new Button();
            
            _baseContainerSeparator = new Separator(Orientation.Horizontal);
            
            _consoleExpander = new Expander("Console");
            _consoleScrolledWindow = new ScrolledWindow();
            _consoleTextView = new TextView();

            Expand = true;
            
            _baseContainer.Name = "_primaryWindowBaseContainer";
            _baseContainer.Hexpand = true;
            _baseContainer.Add(_controlContainer);
            _baseContainer.Add(_baseContainerSeparator);
            _baseContainer.Add(_consoleExpander);

            _headerBar.Name = "_primaryWindowHeaderBar";
            _headerBar.ShowCloseButton = true;
            _headerBar.Title = "Among Us Capture - GTK";
            _headerBar.PackEnd(_appMenuButton);

            _appMenuButton.Name = "_primaryWindowAppMenuButton";
            _appMenuButton.Popover = _appMenuPopover;
            _appMenuButton.Image = _appMenuButtonImage;

            _appMenuPopover.Name = "_primaryWindowAppMenuPopover";
            _appMenuPopover.Add(_appMenuPopoverContainer);

            _appMenuSettingsButton.Name = "_primaryWindowAppMenuSettingsButton";
            _appMenuSettingsButton.Label = "Settings";
            _appMenuSettingsButton.Image = _appMenuSettingsButtonImage;
            _appMenuSettingsButton.Relief = ReliefStyle.None;
            _appMenuSettingsButton.AlwaysShowImage = true;

            _appMenuAboutButton.Name = "_primaryWindowAppMenuAboutButton";
            _appMenuAboutButton.Label = "About";
            _appMenuAboutButton.Image = _appMenuAboutButtonImage;
            _appMenuAboutButton.Relief = ReliefStyle.None;
            _appMenuAboutButton.AlwaysShowImage = true;

            _appMenuPopoverContainer.Name = "_primaryWindowAppMenuPopoverContainer";
            _appMenuPopoverContainer.Margin = 5;
            _appMenuPopoverContainer.Add(_appMenuSettingsButton);
            _appMenuPopoverContainer.Add(_appMenuAboutButton);
            _appMenuPopoverContainer.ShowAll();

            _controlContainer.Name = "_primaryWindowControlContainer";
            _controlContainer.Hexpand = true;
            _controlContainer.Margin = 5;
            _controlContainer.Add(_gameStatusFrame);
            _controlContainer.Add(_botConnectionFrame);

            _gameStatusFrame.Name = "_primaryWindowGameStatusFrame";
            _gameStatusFrame.Margin = 5;
            _gameStatusFrame.Hexpand = true;
            _gameStatusFrame.ShadowType = ShadowType.In;
            _gameStatusFrame.Add(_gameStatusContainer);

            _gameStatusContainer.Name = "_primaryWindowGameStatusContainer";
            _gameStatusContainer.Hexpand = true;
            _gameStatusContainer.Add(_gameStatusStateFrame);
            _gameStatusContainer.Add(_gameStatusCodeFrame);

            _gameStatusStateFrame.Name = "_primaryWindowGameStatusStateFrame";
            _gameStatusStateFrame.Hexpand = true;
            _gameStatusStateFrame.ShadowType = ShadowType.None;
            _gameStatusStateFrame.Add(_gameStatusStateContainer);
            _gameStatusStateFrame.Margin = 5;

            _gameStatusStateContainer.Name = "_primaryWindowGameStatusStateContainer";
            _gameStatusStateContainer.Margin = 5;
            _gameStatusStateContainer.Hexpand = true;
            _gameStatusStateContainer.Add(_gameStatusStateLabel);

            _gameStatusStateLabel.Name = "_primaryWindowGameStatusStateLabel";
            _gameStatusStateLabel.Margin = 5;
            _gameStatusStateLabel.Hexpand = true;

            _gameStatusCodeFrame.Name = "_primaryWindowGameStatusCodeFrame";
            _gameStatusCodeFrame.Margin = 5;
            _gameStatusCodeFrame.Add(_gameStatusCodeContainer);
            _gameStatusCodeFrame.ShadowType = ShadowType.None;
            _gameStatusCodeFrame.Hexpand = true;

            _gameStatusCodeContainer.Name = "_primaryWindowGameStatusCodeContainer";
            _gameStatusCodeContainer.Add(_gameStatusCodeEntry);
            _gameStatusCodeContainer.Margin = 5;
            _gameStatusCodeContainer.Hexpand = true;

            _gameStatusCodeEntry.Name = "_primaryWindowGameStatusCodeEntry";
            _gameStatusCodeEntry.IsEditable = false;
            _gameStatusCodeEntry.Hexpand = true;
            _gameStatusCodeEntry.Margin = 5;

            _botConnectionFrame.Name = "_primaryWindowBotConnectionFrame";
            _botConnectionFrame.ShadowType = ShadowType.EtchedOut;
            _botConnectionFrame.Margin = 5;
            _botConnectionFrame.Add(_botConnectionContainer);
            _botConnectionFrame.Hexpand = true;

            _botConnectionContainer.Name = "_primaryWindowBotConnectionContainer";
            _botConnectionContainer.Hexpand = true;
            _botConnectionContainer.Margin = 5;
            _botConnectionContainer.Add(_botConnectionFieldContainer);
            _botConnectionContainer.Add(_botConnectionButtonContainer);

            _botConnectionFieldContainer.Name = "_primaryWindowConnectionFieldContainer";
            _botConnectionFieldContainer.Hexpand = true;
            _botConnectionFieldContainer.Add(_botConnectionFieldHostnameFrame);
            _botConnectionFieldContainer.Add(_botConnectionFieldCodeFrame);

            _botConnectionFieldHostnameFrame.Name = "_primaryWindowConnectionFieldFrame";
            _botConnectionFieldHostnameFrame.Hexpand = true;
            _botConnectionFieldHostnameFrame.Add(_botConnectionFieldHostnameEntry);
            _botConnectionFieldHostnameFrame.Margin = 5;
            _botConnectionFieldHostnameFrame.ShadowType = ShadowType.None;

            _botConnectionFieldCodeFrame.Name = "_primaryWindowBotConnectionFieldCodeFrame";
            _botConnectionFieldCodeFrame.Hexpand = true;
            _botConnectionFieldCodeFrame.Add(_botConnectionFieldCodeEntry);
            _botConnectionFieldCodeFrame.Margin = 5;
            _botConnectionFieldCodeFrame.ShadowType = ShadowType.None;
            
            _botConnectionFieldHostnameEntry.Name = "_primaryWindowBotConnectionFieldHostnameEntry";
            _botConnectionFieldHostnameEntry.Hexpand = true;
            _botConnectionFieldHostnameEntry.MaxLength = 8;
            _botConnectionFieldHostnameEntry.Margin = 5;

            _botConnectionFieldCodeEntry.Name = "_primaryWindowBotConnectionFieldCodeEntry";
            _botConnectionFieldCodeEntry.Hexpand = true;
            _botConnectionFieldCodeEntry.MaxLength = 8;
            _botConnectionFieldCodeEntry.Margin = 5;

            _botConnectionButtonContainer.Name = "_primaryWindowBotConnectionButtonContainer";
            _botConnectionButtonContainer.Hexpand = true;
            _botConnectionButtonContainer.Margin = 5;
            _botConnectionButtonContainer.Add(_botConnectionButtonSubmit);

            _botConnectionButtonSubmit.Name = "_primaryWindowBotConnectionButtonSubmit";
            _botConnectionButtonSubmit.Margin = 5;
            _botConnectionButtonSubmit.Label = "Submit";
            _botConnectionButtonSubmit.Hexpand = true;

            _consoleExpander.Name = "_primaryWindowConsoleExpander";
            _consoleExpander.Hexpand = true;
            _consoleExpander.Vexpand = true;
            _consoleExpander.Margin = 5;
            _consoleExpander.Add(_consoleScrolledWindow);
            
            _consoleScrolledWindow.Name = "_primaryWindowConsoleScrolledWindow";
            _consoleScrolledWindow.Hexpand = true;
            _consoleScrolledWindow.Vexpand = true;
            _consoleScrolledWindow.Add(_consoleTextView);
            _consoleScrolledWindow.HeightRequest = 300;
            _consoleScrolledWindow.Margin = 3;
            
            _consoleTextView.Name = "_primaryWindowConsoleTextView";
            _consoleTextView.Hexpand = true;
            _consoleTextView.Vexpand = true;


            Resizable = false;
            Titlebar = _headerBar;
            // SetDefaultSize(800, 600);
            Add(_baseContainer);
        }
    }
}