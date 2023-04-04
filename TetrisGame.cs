using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace Tetris
{
    public class TetrisGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D grid_40, pixel;

        private int W = 10, H = 20, TILE = 45;
        private Rectangle[] GRID;

        private int[,,] figure_pos;
        private Rectangle[,] figures;
        private Rectangle figure_rect;
        private Rectangle[] figure;
        private int fig_type;

        private bool[,] field;

        public Song tetrisTheme;


        private int dx;
        private int anim_count, anim_speed, anim_limit;

        //windowSize
        public int WINDOW_WIDTH, WINDOW_HEIGHT;

        // keyboard state
        public KeyboardState keyboardState, previousKeyboardState;



        public TetrisGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;


            


        }

        protected override void Initialize()
        {
            WINDOW_WIDTH = W * TILE;
            WINDOW_HEIGHT = H * TILE;

            // window size
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();


            GRID = new Rectangle[W * H];

            // setup grid
            for (int x = 0; x < W; ++x) 
            {
                for (int y = 0; y < H * 10; y += 10)
                {
                    GRID[x+y] = new Rectangle(x * TILE, y/10 * TILE, TILE, TILE);  
                }
            }

            // setup pieces

            figure_pos = new int[,,] {
                    { { -1, 0 }, { -2, 0 }, { 0, 0 }, { 1, 0 } },
                    { { 0, -1 }, { -1, -1 }, { -1, 0 }, { 0, 0 } },
                    { { -1, 0 }, { -1, 1 }, { 0, 0 }, { 0, -1 } },
                    { { 0, 0 }, { -1, 0 }, { 0, 1 }, { -1, -1 } },
                    { { 0, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 } },
                    { { 0, 0 }, { 0, -1 }, { 0, 1 }, { 1, -1 } },
                    { { 0, 0 }, { 0, -1 }, { 0, 1 }, { -1, 0 } }
                                      };

            int figure_num = 7;


            figures = new Rectangle[figure_num,4];

            for (int i = 0; i < figure_num; ++i) 
            {
                for (int j = 0; j < 4; ++j)
                {

                    figures[i, j] = new Rectangle(figure_pos[i, j, 0] + (W/2), figure_pos[i, j, 1] + 1, 1, 1);

                                    
                }
            }

            figure_rect = new Rectangle(0, 0, TILE - 2, TILE - 2);

            Random rnd = new Random();
            fig_type = rnd.Next(7);

            figure = new Rectangle[4];

            new_figure();


            // setup animation
            anim_count = 0;
            anim_speed = 60;
            anim_limit = 2000;

            field = new bool[W, H];

            // setup field
            for (int i = 0; i < W; ++i)
            {
                for (int j = 0; j < H; ++j)
                    field[i,j] = false;
            }
            

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // loading sprites
            grid_40 = Content.Load<Texture2D>(@"Sprites\40grid");
            pixel = Content.Load<Texture2D>(@"Sprites\pixel");

            //Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            //pixel.SetData(new[] { Color.White }); // so that we can draw whatever color we want on top of it 

            // loading sounds
            tetrisTheme = Content.Load<Song>(@"Sounds\Tetris");


            // playing music
            MediaPlayer.IsRepeating = true;
            ChangeMusic(tetrisTheme);

            // TODO: use this.Content to load your game content here


        }



        protected override void Update(GameTime gameTime)
        {
            
            // some xbox thing for exiting the game?
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here


            // keyboard state
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();


            // move x
            if (keyboardState.IsKeyDown(Keys.A))
                dx = -1;
            else if (keyboardState.IsKeyDown(Keys.D))
                dx = 1;
            else
                dx = 0;


            Rectangle[] figure_old = copy_figure(figure);
            
            for (int i = 0; i < 4; ++i)
            {
                figure[i].X += dx;
                if (figure[i].X < 0 || figure[i].X > W - 1)
                { 
                    figure = figure_old;
                    dx = 0;
                }
            }

            //move y

            

            if (keyboardState.IsKeyDown(Keys.S))
                anim_limit = 100;
            else
                anim_limit = 2000;

            anim_count += anim_speed;
            if (anim_count > anim_limit)
            {
                
                anim_count = 0;
                figure_old = copy_figure(figure);
                for (int i = 0; i < 4; ++i)
                {                    
                    figure[i].Y += 1;
                    if (figure[i].Y > H - 1 || field[figure[i].X, figure[i].Y])
                    {
                        add_to_field(figure_old);
                        new_figure();
                        break;
                    }
                }

            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // clear canvas
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // begin rendering
            _spriteBatch.Begin();

            foreach (Rectangle rect in  GRID)
            {
                _spriteBatch.Draw(grid_40, rect, Color.Black);
            }


            // draw figure

            for (int i = 0; i < 4; ++i)
            {
                figure_rect.X = figure[i].X * TILE + 1;
                figure_rect.Y = figure[i].Y * TILE + 1;
                _spriteBatch.Draw(pixel, figure_rect, Color.Blue);
            }

            // draw field

            for (int i = 0; i < W; ++i)
            {
                for (int j = 0; j < H; ++j)
                {
                    if (field[i, j] == true)
                        _spriteBatch.Draw(pixel, new Rectangle(i * TILE, j * TILE, TILE - 2, TILE - 2), Color.White);
                }
                
            }


            // end rendering
            _spriteBatch.End();

            // calling components rendering
            base.Draw(gameTime);
        }




        public void ChangeMusic(Song song)
        {
            // Isn't the same song already playing?
            if (MediaPlayer.Queue.ActiveSong != song)
                MediaPlayer.Play(song);
        }

        public bool NewKey(Keys key)
        {
            return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }


        public Rectangle[] copy_figure(Rectangle[] rect)
        {
            Rectangle[] copy = new Rectangle[4];
            for (int i = 0; i < 4; ++i)
                copy[i] = new Rectangle(rect[i].X, rect[i].Y, 1, 1);

            return copy;
        }

        public void new_figure()
        {
            Random rnd = new Random();
            fig_type = rnd.Next(6);

            for (int i = 0; i < 4; ++i)
                figure[i] = new Rectangle(figures[fig_type, i].X, figures[fig_type, i].Y, 1, 1);
        }

        public void add_to_field(Rectangle[] old)
        {
            for (int i = 0; i < 4; ++i)
                field[old[i].X, old[i].Y] = true;
        }
    }
}