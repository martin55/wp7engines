using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using xTile.Display;
using xTile;
using xTile.Dimensions;
using xTile.Format;
using xTile.Tiles;
using xTile.Layers;

// noxo 2011

namespace GolfBallAdventure
{
    public partial class GamePage : PhoneApplicationPage
    {

        ContentManager contentManager;
        GameTimer timer;
        SpriteBatch spriteBatch;
        Physics physics;
        Sprite golfball;

        Map map;
        IDisplayDevice mapDisplayDevice;
        xTile.Dimensions.Rectangle camera;

        readonly int ballScreenX = 120;

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            TouchPanel.EnabledGestures = GestureType.Flick;// | GestureType.DragComplete;


            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
        }

        // Fix the tile images to point into correct XNA content id
 
        void fixTileLocationsBug(Map map)
        {
            foreach (TileSheet ts in map.TileSheets)
            {
                // Points default to "..\TileSheet\tilename.png", we change it to "tilename" which
                // will be valid XNA content id when bitmap is imported as content resource

                int p = ts.ImageSource.LastIndexOf('\\') + 1;
                ts.ImageSource = ts.ImageSource.Substring(p, ts.ImageSource.Length - p);
                ts.ImageSource = ts.ImageSource.Substring(0, ts.ImageSource.Length - 4);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            golfball = new Sprite(contentManager.Load<Texture2D>("golfball"));
            golfball.X = ballScreenX;
            golfball.Y = 128;

            int w = SharedGraphicsDeviceManager.Current.GraphicsDevice.Viewport.Width;
            int h = SharedGraphicsDeviceManager.Current.GraphicsDevice.Viewport.Height;

            // Create xTile mandatory objects

            camera = new xTile.Dimensions.Rectangle(new xTile.Dimensions.Size(w, h));
            mapDisplayDevice = new XnaDisplayDevice(contentManager, SharedGraphicsDeviceManager.Current.GraphicsDevice);

            // Load xTile tilemap from content, some hacking required to load file without processor+importer from contentlib
            
            // Map file must have; build action set to "none", and copy to output dir
            // set to "always" to be able open this way. Also "Rebuild solution" must
            // be invoked in Visual Studio when resource is initially added.

            System.IO.Stream stream = TitleContainer.OpenStream("Content\\Map02.tbin");

            // Seems that only tbin type of maps are supported by libs

            map = FormatManager.Instance.BinaryFormat.Load(stream);
            fixTileLocationsBug(map);
            map.LoadTileSheets(mapDisplayDevice);

            // Setup Box2D physics

            physics = new Physics();

            // Add golf ball to Box2D world
            
            physics.addBallSprite(golfball);

            // Add the ground and water hit area rectables to Box2D world
            // Each tile is 16x16 pixels

            TileArray groundTiles = map.GetLayer("HitGround").Tiles;
            TileArray waterTiles = map.GetLayer("HitWater").Tiles;

            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 800; x++)
                {

                    Tile tgroup = groundTiles[x, y];
                    Tile twater = waterTiles[x, y];

                    if (tgroup != null)
                    {
                        physics.addRect(x * 16, y * 16, 16, 16, Physics.TYPE_GROUND);
                    }

                    if (twater != null)
                    {
                        physics.addRect(x * 16, y * 16, 16, 16, Physics.TYPE_WATER);
                    }

                }
            }

            // Start the timer

            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // Check touchscreen events
            processTouches();
            // Update map (animations)
            map.Update(e.ElapsedTime.Milliseconds);
            // Update Box2d world
            physics.update(e.ElapsedTime.Milliseconds);
            // Change camera/viewport location to current ball position
            camera.X = (int)golfball.X - ballScreenX;
            // To die or not
            checkGameOver();
        }

        void checkGameOver()
        {

            if (golfball.Drown ||
                golfball.Y > SharedGraphicsDeviceManager.Current.GraphicsDevice.Viewport.Height)
            {
                timer.Stop();
                String dieReason = golfball.Drown ? "splashhh..!" : "ooout of screen..!";
                NavigationService.Navigate(new Uri("/MainPage.xaml?gameover=" + dieReason, UriKind.Relative));
            }
 
        }
        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {

            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.CornflowerBlue);

            map.Draw(mapDisplayDevice, camera);

            spriteBatch.Begin();
            spriteBatch.Draw(golfball.Texture2D, new Vector2(ballScreenX, golfball.Y), Color.White);
            spriteBatch.End();
        }

        private void processTouches()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();

                if (gs.GestureType == GestureType.Flick)
                {
                    // Apply force to golf ball in Box2d world
                    physics.flickBall(gs.Delta.X, gs.Delta.Y);
                }

            }
        }

    }
}