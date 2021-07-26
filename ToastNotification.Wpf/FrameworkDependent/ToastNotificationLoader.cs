using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Notification.FrameworkDependent
{
    public class ToastNotificationLoader : DependencyObject
    {
        #region DependencyProperties
        public static readonly DependencyProperty AppNameProperty =
            DependencyProperty.Register(
                "AppName", typeof(string), typeof(ToastNotificationLoader)
                );
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title", typeof(string), typeof(ToastNotificationLoader)
                );
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                "Description", typeof(string), typeof(ToastNotificationLoader)
                );
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(
                "Time", typeof(string), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register(
                "PrimaryButtonText", typeof(string), typeof(ToastNotificationLoader)
                );
        public static readonly DependencyProperty SecondaryButtonTextProperty =
            DependencyProperty.Register(
                "SecondaryButtonText", typeof(string), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty ContentImageSourceProperty =
            DependencyProperty.Register(
                "ContentImageSource", typeof(ImageSource), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty AppIconImageSourceProperty =
            DependencyProperty.Register(
                "AppIconImageSource", typeof(ImageSource), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty PrimaryButtonClickCommandProperty =
            DependencyProperty.Register(
                "PrimaryButtonClickCommand", typeof(RelayCommand), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty SecondaryButtonClickCommandProperty =
            DependencyProperty.Register(
                "SecondaryButtonClickCommand", typeof(RelayCommand), typeof(ToastNotificationLoader)
                );

        public static readonly DependencyProperty CloseButtonClickCommandProperty =
            DependencyProperty.Register(
                "CloseButtonClickCommand", typeof(RelayCommand), typeof(ToastNotificationLoader)
                );
        #endregion

        #region Attached DependencyProperties
        public static readonly DependencyProperty WasPressedProperty =
            DependencyProperty.Register(
                "Uno.Extras.ToastNotification.WasPressed", typeof(bool), typeof(Button)
                );
        #endregion

        #region Properties
        public string AppName
        {
            get => (string)GetValue(AppNameProperty);
            set => SetValue(AppNameProperty, value);
        }
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public string Time
        {
            get => (string)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public string PrimaryButtonText
        {
            get => (string)GetValue(PrimaryButtonTextProperty);
            set
            {
                SetValue(PrimaryButtonTextProperty, value);
                RelayoutButtons();
            }
        }
        public string SecondaryButtonText
        {
            get => (string)GetValue(SecondaryButtonTextProperty);
            set
            {
                SetValue(SecondaryButtonTextProperty, value);
                RelayoutButtons();
            }
        }

        public ImageSource ContentImageSource
        {
            get => (ImageSource)GetValue(ContentImageSourceProperty);
            set => SetValue(ContentImageSourceProperty, value);
        }

        public ImageSource AppIconImageSource
        {
            get => (ImageSource)GetValue(AppIconImageSourceProperty);
            set => SetValue(AppIconImageSourceProperty, value);
        }

        public RelayCommand PrimaryButtonClickCommand
        {
            get => (RelayCommand)GetValue(PrimaryButtonClickCommandProperty);
            private set => SetValue(PrimaryButtonClickCommandProperty, value);
        }

        public RelayCommand SecondaryButtonClickCommand
        {
            get => (RelayCommand)GetValue(SecondaryButtonClickCommandProperty);
            private set => SetValue(SecondaryButtonClickCommandProperty, value);
        }

        public RelayCommand CloseButtonClickCommand
        {
            get => (RelayCommand)GetValue(CloseButtonClickCommandProperty);
            private set => SetValue(CloseButtonClickCommandProperty, value);
        }
        #endregion

        #region XamlControls
        private UserControl _control;
        private SolidColorBrush _buttonBorderBrush;
        private SolidColorBrush _buttonBackgroundBrush;
        private SolidColorBrush _buttonPressedBackgroundBrush;

        private Button _primaryButton;
        private Button _secondaryButton;

        private Rectangle _backgroundRect;
        #endregion

        public ToastNotificationLoader()
        {
            Time = DateTime.Now.ToString("h:mm tt");
            InitializeComponent();
            _control.DataContext = this;
            _control.MouseLeftButtonDown += ToastNotification_MouseLeftButtonDown;
            _control.MouseLeftButtonUp += ToastNotification_MouseLeftButtonUp;
            _control.MouseMove += ToastNotification_MouseMove;

            PrimaryButtonClickCommand = new RelayCommand((o) => _control.IsEnabled, (o) => PrimaryButton_Click(this, null));
            SecondaryButtonClickCommand = new RelayCommand((o) => _control.IsEnabled, (o) => SecondaryButton_Click(this, null));
            CloseButtonClickCommand = new RelayCommand(o => _control.IsEnabled, (o) => CloseButton_MouseLeftButtonDown(this, null));
        }

        private void InitializeComponent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Notification.FrameworkDependent.ToastNotificationXaml.xaml";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                _control = XamlReader.Load(stream) as UserControl;
            }

            _buttonBorderBrush = _control.FindResource("Win10NotificationButtonBorderBrush") as SolidColorBrush;
            _buttonBackgroundBrush = _control.FindResource("Win10NotificationButtonBackground") as SolidColorBrush;
            _buttonPressedBackgroundBrush = _control.FindResource("Win10NotificationButtonPressedBackground") as SolidColorBrush;

            _primaryButton = _control.FindName("PrimaryButton") as Button;
            _secondaryButton = _control.FindName("SecondaryButton") as Button;

            _primaryButton.MouseEnter += ActionButton_MouseAction;
            _primaryButton.MouseLeave += ActionButton_MouseAction;
            _primaryButton.PreviewMouseDown += ActionButton_MouseAction;
            _primaryButton.PreviewMouseUp += PrimaryButton_MouseUp;
            _primaryButton.PreviewMouseUp += ActionButton_MouseAction;
            _primaryButton.GotMouseCapture += ActionButton_GotMouseCapture;

            _secondaryButton.MouseEnter += ActionButton_MouseAction;
            _secondaryButton.MouseLeave += ActionButton_MouseAction;
            _secondaryButton.PreviewMouseDown += ActionButton_MouseAction;
            _secondaryButton.PreviewMouseUp += SecondaryButton_MouseUp;
            _secondaryButton.PreviewMouseUp += ActionButton_MouseAction;
            _secondaryButton.Click += SecondaryButton_Click;
            _secondaryButton.GotMouseCapture += ActionButton_GotMouseCapture;

            _backgroundRect = _control.FindName("BackgroundRect") as Rectangle;

            RelayoutButtons();
        }

        #region Events
        public event EventHandler CloseRequested;
        public event EventHandler PrimaryButtonClick;
        public event EventHandler SecondaryButtonClick;

        public event EventHandler NotificationClick;

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseRequested?.Invoke(this, null);
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryButtonClick?.Invoke(this, null);
            CloseRequested?.Invoke(this, null);
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryButtonClick?.Invoke(this, null);
            CloseRequested?.Invoke(this, null);
        }
        #endregion

        #region Action Buttons
        private void RelayoutButtons()
        {
            var primaryShown = PrimaryButtonText != null;
            var secondaryShown = SecondaryButtonText != null;

            if (primaryShown && secondaryShown)
            {
                _primaryButton.Visibility = Visibility.Visible;
                _secondaryButton.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(_primaryButton, 1);
                Grid.SetColumn(_primaryButton, 0);
                Grid.SetColumnSpan(_secondaryButton, 1);
                Grid.SetColumn(_secondaryButton, 2);
            }
            else if (primaryShown)
            {
                _primaryButton.Visibility = Visibility.Visible;
                _secondaryButton.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(_primaryButton, 3);
                Grid.SetColumn(_primaryButton, 0);
            }
            else if (secondaryShown)
            {
                _primaryButton.Visibility = Visibility.Collapsed;
                _secondaryButton.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(_secondaryButton, 3);
                Grid.SetColumn(_secondaryButton, 0);
            }
            else
            {
                _primaryButton.Visibility = Visibility.Collapsed;
                _secondaryButton.Visibility = Visibility.Collapsed;
            }
        }

        private void PrimaryButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            if ((bool)button.GetValue(WasPressedProperty))
            {
                PrimaryButton_Click(sender, null);
                button.SetValue(WasPressedProperty, false);
            }
        }

        private void SecondaryButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            if ((bool)button.GetValue(WasPressedProperty))
            {
                SecondaryButton_Click(sender, null);
                button.SetValue(WasPressedProperty, false);
            }
        }

        private void ActionButton_GotMouseCapture(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            button.ReleaseMouseCapture();
        }

        private void ActionButton_MouseAction(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (!button.IsEnabled)
            {
                return;
            }

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                button.SetValue(WasPressedProperty, true);
            }

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed && button.IsMouseOver)
            {
                button.BorderThickness = new Thickness(0);
                button.Background = _buttonPressedBackgroundBrush;
            }
            else if (button.IsMouseOver)
            {
                button.BorderThickness = new Thickness(3);
                button.Background = _buttonBackgroundBrush;
            }
            else
            {
                button.BorderThickness = new Thickness(0);
                button.Background = _buttonBackgroundBrush;
            }
        }
        #endregion

        #region Toast Notification Mouse Controls
        private double _initialXPosition;
        private double _deltaX;
        private bool _mouseWasDown = false;

        private void ToastNotification_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lock (_locker)
            {
                if (_popup == null)
                {
                    return;
                }
                var rect = ScreenInterop.GetWorkArea();
                _popup.HorizontalOffset = rect.Right + _control.ActualWidth;
                _popup.VerticalOffset = rect.Bottom - _control.ActualHeight - 10;
            }

            _mouseWasDown = true;
            _control.CaptureMouse();

            _control.BeginAnimation(FrameworkElement.MarginProperty, null);
            _control.Margin = new Thickness(0, 0, 10 - _deltaX, 0);

            var point = ScreenInterop.GetCursorPosition();
            _initialXPosition = point.X;
            _deltaX = 0;
        }

        private void ToastNotification_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseWasDown)
            {
                return;
            }

            var point = ScreenInterop.GetCursorPosition();

            _deltaX = Math.Max(0, point.X - _initialXPosition);

            if (_deltaX > 50)
            {
                _backgroundRect.Opacity = 0.5;
            }
            else
            {
                _backgroundRect.Opacity = 0.9;
            }

            _control.BeginAnimation(FrameworkElement.MarginProperty, null);
            _control.Margin = new Thickness(0, 0, 10 - _deltaX, 0);
        }

        private void ToastNotification_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseWasDown)
            {
                _mouseWasDown = false;
                _control.ReleaseMouseCapture();

                var isMouseOver = false;
                VisualTreeHelper.HitTest(_control, d =>
                {
                    if (d == _control)
                    {
                        isMouseOver = true;
                        return HitTestFilterBehavior.Stop;
                    }
                    else
                    {
                        return HitTestFilterBehavior.Continue;
                    }
                },
                    ht => HitTestResultBehavior.Stop,
                    new PointHitTestParameters(Mouse.GetPosition(_control)));

                if (isMouseOver && _deltaX < 2)
                {
                    NotificationClick?.Invoke(this, null);
                    CloseRequested?.Invoke(this, null);
                    return;
                }

                var rect = ScreenInterop.GetWorkArea();
                _popup.HorizontalOffset = rect.Right - _control.ActualWidth - 10;
                _popup.VerticalOffset = rect.Bottom - _control.ActualHeight - 10;

                _control.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimationUsingKeyFrames()
                {
                    Duration = TimeSpan.Zero,
                    KeyFrames = new ThicknessKeyFrameCollection()
                    {
                        new DiscreteThicknessKeyFrame()
                        {
                            Value = new Thickness(0, 0, 0, 0)
                        }
                    }
                });

                if (_deltaX >= 50)
                {
                    CloseRequested?.Invoke(this, null);
                }

                _deltaX = 0;
            }
        }
        #endregion

        #region Show and Hide
        private Popup _popup;
        private object _locker = new object();

        public Task ShowAsync()
        {
            lock (_locker)
            {
                if (_popup != null)
                {
                    return Task.CompletedTask;
                }

                _popup = new Popup()
                {
                    AllowsTransparency = true,
                    Child = _control,
                    Placement = PlacementMode.Absolute,
                    StaysOpen = true,
                };
            }

            var contentImage = _control.FindName("ContentImage") as Image;
            var contentTextPanel = _control.FindName("ContentTextPanel") as UIElement;
            var contentGrid = _control.FindName("ContentGrid") as Grid;

            if (ContentImageSource == null)
            {
                contentImage.Visibility = Visibility.Collapsed;
                Grid.SetColumn(contentTextPanel, 0);
                Grid.SetColumnSpan(contentTextPanel, 3);
            }
            else
            {
                contentImage.Visibility = Visibility.Visible;
                Grid.SetColumn(contentTextPanel, 2);
                Grid.SetColumnSpan(contentTextPanel, 1);
            }

            var window = Application.Current.MainWindow;
            var wih = new WindowInteropHelper(window);
            var hWnd = wih.Handle;

            AppIconImageSource = Imaging.CreateBitmapSourceFromHIcon(
                IconInterop.GetSmallWindowIcon(hWnd),
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (AppName == null)
            {
                AppName = Assembly.GetEntryAssembly().GetName().Name;
            }

            var tcs = new TaskCompletionSource<object>();

            var rect = ScreenInterop.GetWorkArea();

            _control.Measure(new Size { Height = rect.Height, Width = rect.Width });

            // Just need a very big value for horizontal.
            _popup.HorizontalOffset = rect.Right + _control.DesiredSize.Width;
            _popup.VerticalOffset = rect.Bottom - _control.DesiredSize.Height - 10;

            Debug.WriteLine(_popup.HorizontalOffset);
            Debug.WriteLine(_popup.VerticalOffset);

            _popup.IsOpen = true;

            _backgroundRect.Opacity = 0.9;

            // This seems to work, because:
            // When setting Margin to a huge negative size, the whole Control 
            // becomes invisible. This makes the Popup accept a big offet.
            // When the Margin increases, the visible portion does, too,
            // and the Popup responds by shifting everything left.
            _control.Margin = new Thickness(0, 0, -_control.DesiredSize.Width, 0);
            var thicknessAnimation = new ThicknessAnimation
            {
                From = _control.Margin,
                To = new Thickness(0, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                FillBehavior = FillBehavior.Stop
            };

            thicknessAnimation.Completed += AnimationComplete;
            _control.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);

            return tcs.Task;

            async void AnimationComplete(object sender, EventArgs args)
            {
                thicknessAnimation.Completed -= AnimationComplete;
                _control.Visibility = Visibility.Visible;
                _control.BeginAnimation(FrameworkElement.MarginProperty, null);
                _control.Margin = new Thickness(0, 0, 10, 0);
                // LMAO dunno why.
                await Task.Delay(100);
                _popup.HorizontalOffset = rect.Right - _control.ActualWidth - 10;
                _popup.VerticalOffset = rect.Bottom - _control.ActualHeight - 10;

                tcs.SetResult(null);
            };
        }

        public Task HideAsync()
        {
            lock (_locker)
            {
                if (_popup != null)
                {
                    // We must nullify this BEFORE releasing the lock.
                    var popup = _popup;
                    _popup = null;

                    var tcs = new TaskCompletionSource<object>();
                    var rect = ScreenInterop.GetWorkArea();
                    popup.HorizontalOffset = rect.Right + _control.DesiredSize.Width + 100;
                    _backgroundRect.Opacity = 0.5;
                    var thicknessAnimation = new ThicknessAnimation
                    {
                        From = _control.Margin,
                        To = new Thickness(0, 0, -_control.Width, 0),
                        Duration = new Duration(TimeSpan.FromSeconds(0.2))
                    };

                    thicknessAnimation.Completed += AnimationComplete;
                    _control.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);

                    // Lock is released here, after that code runs asynchronously.
                    return tcs.Task;

                    void AnimationComplete(object sender, EventArgs args)
                    {
                        thicknessAnimation.Completed -= AnimationComplete;
                        _control.Visibility = Visibility.Collapsed;
                        _control.ReleaseMouseCapture();
                        popup.IsOpen = false;
                        popup.Child = null;
                        popup = null;
                        tcs.SetResult(null);
                    }
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
        }
        #endregion

        public void SetImageSource(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            ContentImageSource = image;
        }

        public void CleanEvents()
        {
            Helpers.CleanEvent(PrimaryButtonClick);
            Helpers.CleanEvent(SecondaryButtonClick);
            Helpers.CleanEvent(NotificationClick);
            Helpers.CleanEvent(CloseRequested);
        }

        public class RelayCommand : ICommand
        {
            private readonly Predicate<object> _canExecute;
            private readonly Action<object> _execute;

            public RelayCommand(Predicate<object> canExecute, Action<object> execute)
            {
                _canExecute = canExecute;
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }
    }
}
