using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PointsOnline
{
    public partial class MainWindow
    {
        Panel _gameStatePanel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public Panel GameStatePanel
        {
            get
            {
                if (_gameStatePanel == null)
                    _gameStatePanel = (Panel)this.FindByUid("gameStatePanel");

                return _gameStatePanel;
            }
        }

        void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            var model = DataContext as MainWindowViewModel;
            if (model != null
                && model.OnInitializeCommand != null
                && model.OnInitializeCommand.CanExecute(null))
            {
                model.OnInitializeCommand.Execute(this);
            }
        }

        private void MainWindow_OnLoadCommand(object sender, RoutedEventArgs e)
        {
            var model = DataContext as MainWindowViewModel;
            if (model != null
                && model.OnLoadCommand != null
                && model.OnLoadCommand.CanExecute(null))
            {
                model.OnLoadCommand.Execute(this);
            }
        }
    }
}
