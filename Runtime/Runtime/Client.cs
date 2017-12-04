using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;

namespace OpenXP.Runtime
{
    class Client : GameWindow
    {
        // the actual resolution - i.e. 640x480
        public System.Drawing.Size BaseResolution { get; set; }
        // the primary viewport for the centered/letterboxed/scaled surface
        public System.Drawing.Rectangle RenderRect { get; set; }
        // the actual viewport for the entire client display, used for screen-wide effects
        public System.Drawing.Size ClientResolution { get; set; }

        //debug graphics for debug builds, otherwise compat
        private static OpenTK.Graphics.GraphicsContextFlags GetInitialGraphicsContextFlags()
        {
            #if DEBUG
                return OpenTK.Graphics.GraphicsContextFlags.Debug;
            #else
                return OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible;
            #endif
        }

        //a chance to intercept graphics mode prior to window initialization
        private static OpenTK.Graphics.GraphicsMode GetInitialGraphicsMode()
        {
            OpenTK.Graphics.GraphicsMode gm = OpenTK.Graphics.GraphicsMode.Default;
            //todo: tinker here
            //gm.Samples = 0;
            return gm;
        }

        private static GameWindowFlags GetInitialGameWindowFlags()
        {
            if (Program.Configuration.Fullscreen) return GameWindowFlags.Fullscreen;
            else if (Program.Configuration.AllowResize) return GameWindowFlags.Default;
            return GameWindowFlags.FixedWindow;
        }

        private static DisplayDevice GetInitialDisplayDevice()
        {
            return DisplayDevice.Default;
        }

        private static int GetInitialWidth()
        {
            return Program.Configuration.GraphicsWidth * Program.Configuration.BaseScale;
        }

        private static int GetInitialHeight()
        {
            return Program.Configuration.GraphicsHeight * Program.Configuration.BaseScale;
        }

        public Client() : base(
            GetInitialWidth(), GetInitialHeight(),
            GetInitialGraphicsMode(), Program.Configuration.GameTitle,
            GetInitialGameWindowFlags(), GetInitialDisplayDevice(),
            4, 0, GetInitialGraphicsContextFlags())
        {
            //setup details from Configuration
            Location = new System.Drawing.Point(Program.Configuration.LocationX, Program.Configuration.LocationY);
            if(Program.Configuration.Borderless) WindowBorder = WindowBorder.Hidden;
            
            
            //hook the closing event to store configuration variables
            Closing += Client_Closing;
        }

        private void Client_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //store window configuration
            Program.Configuration.LocationX = Location.X;
            Program.Configuration.SetValue("Graphics", "LocationX", Location.X.ToString());
            Program.Configuration.LocationY = Location.Y;
            Program.Configuration.SetValue("Graphics", "LocationY", Location.Y.ToString());
        }
    }
}
