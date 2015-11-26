using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace PointsOnline
{
    class MainWindowViewModel
    {
        public MainWindow View { get; set; }

        #region Commands

        ICommand _onInitializeCommand;
        public ICommand OnInitializeCommand
        {
            get
            {
                if (_onInitializeCommand == null)
                {
                    _onInitializeCommand = new SimpleCommand(OnInitialize);
                }

                return _onInitializeCommand;
            }
        }

        void OnInitialize(object parameter)
        {
            View = parameter as MainWindow;

            //SaveDataManager.Instance.Save( new SaveData() );
            //SaveDataManager.Instance.Load();
        }

        ICommand _onLoadCommand;
        public ICommand OnLoadCommand
        {
            get
            {
                if (_onLoadCommand == null)
                {
                    _onLoadCommand = new SimpleCommand(OnLoad);
                }

                return _onLoadCommand;
            }
        }

        void OnLoad(object parameter)
        {
            var first = new Splash();
            View.GameStatePanel.Children.Add(first);
            first.SplashWorkedOutDelegate = () =>
            {
                var second = new Game();
                CrossFadeGameState(first, second);
            };
        }

        #endregion
        
        void CrossFadeGameState( UIElement first, UIElement second )
        {
            const double durationInSeconds = 1;

            DoubleAnimation firstAnimation = new DoubleAnimation();
            firstAnimation.From = 1;
            firstAnimation.To = 0;
            firstAnimation.Duration = TimeSpan.FromSeconds(durationInSeconds);
            firstAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn };

            first.IsHitTestVisible = false;
            second.IsHitTestVisible = false;

            View.GameStatePanel.Children.Remove(first);
            View.GameStatePanel.Children.Add(second);
            View.GameStatePanel.Children.Add(first);

            firstAnimation.Completed += (s, ea) =>
            {
                View.GameStatePanel.Children.Remove(first);
            };

            firstAnimation.Freeze();

            DoubleAnimation secondAnimation = new DoubleAnimation();
            secondAnimation.From = 0;
            secondAnimation.To = 1;
            secondAnimation.Duration = TimeSpan.FromSeconds(durationInSeconds);
            secondAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut };

            secondAnimation.Completed += (s, ea) =>
            {
                second.IsHitTestVisible = true;
            };

            secondAnimation.Freeze();

            first.BeginAnimation(UIElement.OpacityProperty, firstAnimation);
            second.BeginAnimation(UIElement.OpacityProperty, secondAnimation);
        }
    }
}
