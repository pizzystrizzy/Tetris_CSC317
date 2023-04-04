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

        private Texture2D grid_40, pixel, golden_eagle;

        private int W = 10, H = 20, TILE = 45;
        private Rectangle[] GRID;

        private int[,,] figure_pos;
        private Rectangle[,] figures;
        private Rectangle figure_rect;
        private Rectangle[] figure;
        private int fig_type;
        private Color current_figure_color;

        private bool[,] field;
        private Color[,] color_field;

        public Song tetrisTheme;


        private int dx;
        private bool rotate;
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


            // window size
            WINDOW_WIDTH = W * TILE;
            WINDOW_HEIGHT = H * TILE;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();




            // set up grid

            GRID = new Rectangle[W * H];

            for (int x = 0; x < W; ++x)
            {
                for (int y = 0; y < H * 10; y += 10)
                {
                    GRID[x + y] = new Rectangle(x * TILE, y / 10 * TILE, TILE, TILE);
                }
            }

            // set up pieces

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


            figures = new Rectangle[figure_num, 4];

            for (int i = 0; i < figure_num; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {

                    figures[i, j] = new Rectangle(figure_pos[i, j, 0] + (W / 2), figure_pos[i, j, 1] + 1, 1, 1);


                }
            }

            figure_rect = new Rectangle(0, 0, TILE - 2, TILE - 2);


            figure = new Rectangle[4];
            New_figure();
            current_figure_color = New_Color();


            // set up animation
            anim_count = 0;
            anim_speed = 60;
            anim_limit = 2000;



            // set up field
            field = new bool[W, H];
            color_field = new Color[W, H];

            for (int i = 0; i < W; ++i)
            {
                for (int j = 0; j < H; ++j)
                    field[i, j] = false;
            }


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // loading sprites
            grid_40 = Content.Load<Texture2D>(@"Sprites\40grid");
            pixel = Content.Load<Texture2D>(@"Sprites\pixel");
            golden_eagle = Content.Load<Texture2D>(@"Sprites\golden_eagle_background");

            //Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            //pixel.SetData(new[] { Color.White }); // so that we can draw whatever color we want on top of it 

            // loading sounds
            tetrisTheme = Content.Load<Song>(@"Sounds\Tetris");


            // playing music
            MediaPlayer.IsRepeating = true;
            ChangeMusic(tetrisTheme);

           


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
            if (keyboardState.IsKeyDown(Keys.A) && anim_count % 100 == 0)
                dx = -1;
            else if (keyboardState.IsKeyDown(Keys.D) && anim_count % 100 == 0)
                dx = 1;
            else
                dx = 0;


            Rectangle[] figure_old = Copy_figure(figure);

            for (int i = 0; i < 4; ++i)
            {
                figure[i].X += dx;
                if (figure[i].X < 0 || figure[i].X > W - 1 || field[figure[i].X, figure[i].Y])
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
                figure_old = Copy_figure(figure);
                for (int i = 0; i < 4; ++i)
                {
                    figure[i].Y += 1;
                    if (figure[i].Y > H - 1 || field[figure[i].X, figure[i].Y])
                    {
                        Add_to_field(figure_old);
                        New_figure();
                        current_figure_color = New_Color();
                        break;
                    }
                }

            }

            // rotation

            if (NewKey(Keys.W))
            {
                rotate = true;
            }
            else
                rotate = false;

            Rectangle center = figure[0];


            if (rotate)
            {
                figure_old = Copy_figure(figure);
                for (int i = 0; i < 4; ++i)
                {
                    int x = figure[i].Y - center.Y;
                    int y = figure[i].X - center.X;
                    figure[i].X = center.X - x;
                    figure[i].Y = center.Y + y;

                    if (figure[i].Y > H - 1 || figure[i].X < 0 || figure[i].X > W - 1 || field[figure[i].X, figure[i].Y])
                    {
                        figure = figure_old;
                        break;
                    }
                }
                
            }


            // check lines
            
            int line = H - 1;
            for (int row = line; row >= 0; --row)
            {
                int count = 0;
                for (int x = 0; x < W; ++x)
                {
                    if (field[x,row])
                        count++;
                    field[x,line] = field[x,row];
                }
                if (count < W)
                    line--;
            }
            

            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // clear canvas
            GraphicsDevice.Clear(Color.White);

            // begin rendering
            _spriteBatch.Begin();


            //_spriteBatch.Draw(golden_eagle, new Rectangle(0,0, W * TILE, H * TILE), Color.White);  //doesn't work, ugly

            foreach (Rectangle rect in GRID)
            {
                _spriteBatch.Draw(grid_40, rect, Color.Black);
            }


            // draw figure

            for (int i = 0; i < 4; ++i)
            {
                figure_rect.X = figure[i].X * TILE + 1;
                figure_rect.Y = figure[i].Y * TILE + 1;
                _spriteBatch.Draw(pixel, figure_rect, current_figure_color);
            }

            // draw field

            for (int i = 0; i < W; ++i)
            {
                for (int j = 0; j < H; ++j)
                {
                    if (field[i, j] == true)
                        _spriteBatch.Draw(pixel, new Rectangle(i * TILE + 1, j * TILE + 1, TILE - 2, TILE - 2), color_field[i,j]);
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


        public Rectangle[] Copy_figure(Rectangle[] rect)
        {
            Rectangle[] copy = new Rectangle[4];
            for (int i = 0; i < 4; ++i)
                copy[i] = new Rectangle(rect[i].X, rect[i].Y, 1, 1);

            return copy;
        }

        public void New_figure()
        {
            Random rnd = new Random();
            fig_type = rnd.Next(6);

            for (int i = 0; i < 4; ++i)
                figure[i] = new Rectangle(figures[fig_type, i].X, figures[fig_type, i].Y, 1, 1);
        }

        public void Add_to_field(Rectangle[] old)
        {
            for (int i = 0; i < 4; ++i)
            {
                field[old[i].X, old[i].Y] = true;
                color_field[old[i].X, old[i].Y] = current_figure_color;
            }
                
        }

        public Color New_Color()
        {
            Random rnd = new Random();
            int R = rnd.Next(1, 256);
            int G = rnd.Next(1, 256);
            int B = rnd.Next(1, 256);

            return new Color(R, G, B);
        }

            
    }
}