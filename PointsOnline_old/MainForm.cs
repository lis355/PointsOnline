using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PointsOnlineProject
{
    public partial class MainForm : Form
    {
        bool FullScreen = false;
        Point FormLocation;
        Size FormSize;

        Game G;

        public MainForm()
        {
            InitializeComponent();
            G = new Game(true, 100, 10);
            //GameRender.Initialize(MainPicture,G.Info);
            //GameRender.Draw(G);

            Render.MapScaleChanged += ( s, e ) => { Status.Text = Render.MapScale.ToString(); };
            Render.ViewportChanged += ( s, e ) => { Status.Text = Render.ViewPort.ToString(); };
        }

        private void MenuPointsColors_Click(object sender, EventArgs e)
        {
            OptionsPointsColors Q = new OptionsPointsColors();
            Q.ShowDialog();
        }

        private void FullScreenButton_Click(object sender, EventArgs e)
        {
            if (!FullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                FormLocation = Location;
                Location = new Point(0, 0);
                FormSize = Size;
                Size = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width,
                    Screen.PrimaryScreen.WorkingArea.Height);
                MainMenu.Visible = false;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                Location = FormLocation;
                Size = FormSize;
                MainMenu.Visible = true;
            }
            FullScreen = !FullScreen;//обращаем флаг
           // GameRender.Draw(G);//перерисовываем
        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            G = new Game(true, 50, 10);
         //   GameRender.Draw(G);
        }
    }
}
