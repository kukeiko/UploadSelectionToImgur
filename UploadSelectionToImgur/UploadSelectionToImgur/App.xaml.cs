using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UploadSelectionToImgur
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum Mode
        {
            Idle,
            UsingTool
        }

        public enum Tool
        {
            Snipper,
            Resizer,
            Mover
        }

        [Flags]
        public enum Directions
        {
            None = 0x0,
            Top = 0x1,
            Right = 0x2,
            Bottom = 0x4,
            Left = 0x8
        }
    }
}
