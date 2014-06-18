using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Drawing;

namespace UploadSelectionToImgur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Imgur Imgur { get; set; }
        public App.Mode ActiveMode { get; private set; }
        public App.Tool ActiveTool { get; private set; }
        public App.Directions ResizeDirections { get; private set; }
        public System.Windows.Point ToolStartMousePosition { get; private set; }
        public System.Windows.Point LastMousePosition { get; private set; }

        #region SelectionLeft : int
        public int SelectionLeft
        {
            get { return (int)GetValue(SelectionLeftProperty); }
            set { SetValue(SelectionLeftProperty, value); }
        }

        public static readonly DependencyProperty SelectionLeftProperty =
            DependencyProperty.Register("SelectionLeft", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        #endregion

        #region SelectionTop : int
        public int SelectionTop
        {
            get { return (int)GetValue(SelectionTopProperty); }
            set { SetValue(SelectionTopProperty, value); }
        }

        public static readonly DependencyProperty SelectionTopProperty =
            DependencyProperty.Register("SelectionTop", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        #endregion

        #region SelectionWidth : int
        public int SelectionWidth
        {
            get { return (int)GetValue(SelectionWidthProperty); }
            set { SetValue(SelectionWidthProperty, value); }
        }

        public static readonly DependencyProperty SelectionWidthProperty =
            DependencyProperty.Register("SelectionWidth", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        #endregion

        #region SelectionHeight : int
        public int SelectionHeight
        {
            get { return (int)GetValue(SelectionHeightProperty); }
            set { SetValue(SelectionHeightProperty, value); }
        }

        public static readonly DependencyProperty SelectionHeightProperty =
            DependencyProperty.Register("SelectionHeight", typeof(int), typeof(MainWindow), new PropertyMetadata(0));
        #endregion

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();

            this.Imgur = new Imgur("64497c5d8fbf01a");

            this.ActiveMode = App.Mode.Idle;
            this.ActiveTool = App.Tool.Snipper;
            this.ResizeDirections = App.Directions.None;

            this.Left = SystemParameters.VirtualScreenLeft;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Height = SystemParameters.VirtualScreenHeight;

            this.MouseMove += MouseMoved;
            this.MouseDown += MouseDowned;
            this.MouseUp += MouseUpped;
            this.KeyDown += KeyDowned;
            this.KeyUp += KeyUpped;
        }

        async Task<string> UploadCurrentSelection()
        {
            using (Bitmap bitmap = new Bitmap(this.SelectionWidth, this.SelectionHeight))
            {
                var virtualLeft = this.SelectionLeft + (int)SystemParameters.VirtualScreenLeft;
                var virtualTop = this.SelectionTop + (int)SystemParameters.VirtualScreenTop;

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    this.Hide();
                    graphics.CopyFromScreen(virtualLeft, virtualTop, 0, 0, bitmap.Size);
                    //this.Show();
                }

                return await this.Imgur.Upload(bitmap);
            }
        }

        async void KeyDowned(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // can only switch tool if user is not using a tool
            if (this.ActiveMode == App.Mode.UsingTool) return;

            switch (key)
            {
                case Key.Space:
                    this.ActiveTool = App.Tool.Mover;
                    break;

                case Key.R:
                    this.ActiveTool = App.Tool.Resizer;
                    break;

                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        void KeyUpped(object sender, KeyEventArgs e)
        {
            this.ActiveTool = App.Tool.Snipper;
			
			var key = e.Key == Key.System ? e.SystemKey : e.Key;
			
			switch (key)
            {
                case Key.Enter:
                    var imageId = await this.UploadCurrentSelection();
                    this.Imgur.OpenImageInBrowser(imageId);
                    this.Close();
                    break;
            }
        }

        void MouseMoved(object sender, MouseEventArgs e)
        {
            var currentMousePosition = e.GetPosition(this);
            var delta = currentMousePosition - this.LastMousePosition;

            if (this.ActiveMode == App.Mode.UsingTool)
            {
                switch (this.ActiveTool)
                {
                    case App.Tool.Snipper:
                        if (this.ToolStartMousePosition.X > currentMousePosition.X)
                        {
                            this.SelectionLeft = (int)currentMousePosition.X;
                            this.SelectionWidth = Math.Abs((int)(this.ToolStartMousePosition.X - e.GetPosition(this).X));
                        }
                        else
                        {
                            this.SelectionLeft = (int)this.ToolStartMousePosition.X;
                            this.SelectionWidth = Math.Abs((int)(e.GetPosition(this).X - this.ToolStartMousePosition.X));
                        }

                        if (this.ToolStartMousePosition.Y > currentMousePosition.Y)
                        {
                            this.SelectionTop = (int)currentMousePosition.Y;
                            this.SelectionHeight = Math.Abs((int)(this.ToolStartMousePosition.Y - e.GetPosition(this).Y));
                        }
                        else
                        {
                            this.SelectionTop = (int)this.ToolStartMousePosition.Y;
                            this.SelectionHeight = Math.Abs((int)(e.GetPosition(this).Y - this.ToolStartMousePosition.Y));
                        }
                        break;

                    case App.Tool.Mover:
                        this.SelectionLeft += (int)delta.X;
                        this.SelectionTop += (int)delta.Y;

                        if (this.SelectionLeft <= 0)
                        {
                            this.SelectionLeft = 0;
                        }
                        else if (this.SelectionLeft + this.SelectionWidth > SystemParameters.VirtualScreenWidth)
                        {
                            this.SelectionLeft = (int)SystemParameters.VirtualScreenWidth - this.SelectionWidth;
                        }

                        if (this.SelectionTop <= 0)
                        {
                            this.SelectionTop = 0;
                        }
                        else if (this.SelectionTop + this.SelectionHeight > SystemParameters.VirtualScreenHeight)
                        {
                            this.SelectionTop = (int)SystemParameters.VirtualScreenHeight - this.SelectionHeight;
                        }
                        break;

                    case App.Tool.Resizer:
                        if (this.ResizeDirections.HasFlag(App.Directions.Top))
                        {
                            this.SelectionTop += (int)delta.Y;
                            this.SelectionHeight -= (int)delta.Y;
                        }
                        else if (this.ResizeDirections.HasFlag(App.Directions.Bottom))
                        {
                            this.SelectionHeight += (int)delta.Y;
                        }

                        if (this.ResizeDirections.HasFlag(App.Directions.Left))
                        {
                            this.SelectionLeft += (int)delta.X;
                            this.SelectionWidth -= (int)delta.X;
                        }
                        else if (this.ResizeDirections.HasFlag(App.Directions.Right))
                        {
                            this.SelectionWidth += (int)delta.X;
                        }
                        break;
                }
            }

            this.LastMousePosition = currentMousePosition;
        }

        void MouseDowned(object sender, MouseButtonEventArgs e)
        {
            this.ToolStartMousePosition = e.GetPosition(this);
            this.LastMousePosition = this.ToolStartMousePosition;
            this.ActiveMode = App.Mode.UsingTool;

            switch (this.ActiveTool)
            {
                case App.Tool.Snipper:
                    this.SelectionLeft = (int)this.ToolStartMousePosition.X;
                    this.SelectionTop = (int)this.ToolStartMousePosition.Y;
                    this.SelectionWidth = 0;
                    this.SelectionHeight = 0;
                    break;

                case App.Tool.Resizer:
                    if (this.ToolStartMousePosition.X < this.SelectionLeft)
                    {
                        this.ResizeDirections |= App.Directions.Left;
                    }
                    else if (this.ToolStartMousePosition.X > this.SelectionLeft + this.SelectionWidth)
                    {
                        this.ResizeDirections |= App.Directions.Right;
                    }

                    if (this.ToolStartMousePosition.Y < this.SelectionTop)
                    {
                        this.ResizeDirections |= App.Directions.Top;
                    }
                    else if (this.ToolStartMousePosition.Y > this.SelectionTop + this.SelectionHeight)
                    {
                        this.ResizeDirections |= App.Directions.Bottom;
                    }
                    break;
            }
        }

        void MouseUpped(object sender, MouseButtonEventArgs e)
        {
            this.ActiveMode = App.Mode.Idle;
            this.ResizeDirections = App.Directions.None;
        }
    }
}
