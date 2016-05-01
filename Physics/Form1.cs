﻿using System;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using Physics.Engine;
using Timer = System.Timers.Timer;

namespace Physics
{
    public partial class Form1 : Form
    {
        private readonly Simulator m_Simulator = new Simulator();
        private Timer m_Timer = new Timer(15);
        private Bitmap m_Buffer;
        private Bitmap m_Display;
        private int m_Width;
        private int m_Height;

        public Form1()
        {
            InitializeComponent();
            pictureBox.Paint += PictureBoxOnPaint;
            CreateBitmap();
        }

        private void CreateBitmap()
        {
            m_Width = pictureBox.Size.Width;
            m_Height = pictureBox.Size.Height;
            if (m_Buffer != null)
            {
                lock (m_Buffer)
                {
                    m_Buffer.Dispose();
                    m_Buffer = new Bitmap(m_Width, m_Height);
                }
            }
            else
            {
                m_Buffer = new Bitmap(m_Width, m_Height);
            }
        }

        private float MapX(float x)
        {
            return x + m_Width / 2;
        }

        private float MapY(float y)
        {
            return y + m_Height / 2;
        }

        private void PictureBoxOnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            lock (m_Buffer)
            paintEventArgs.Graphics.DrawImage(m_Buffer, Point.Empty);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CollisionTest(m_Simulator);

            m_Simulator.TickInterval = 15;
            m_Simulator.Start();
            m_Timer.Elapsed += Draw;
            m_Timer.Start();
        }

        private void CollisionTest(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;

            simulator.Particles.Add(new FixedBody(new Vector3(0, 0, 0), 1000));
            for (int i = 1; i <= 20; i++)
            {
                var r = i*10 + 50;
                var v = Math.Sqrt(simulator.GravityConstant*(1000)/r);
                var p = new Particle(new Vector3(0,-r,0),new Vector3(v,0,0),5);
                simulator.Particles.Add(p);
            }
        }

        private void BinaryWithPlanet(Simulator simulator)
        {
            simulator.GravityConstant = 2;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(-1.3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(1.3, 0, 0), 100));

            simulator.Particles.Add(new Particle(new Vector3(0, -200, 0), new Vector3(1, 0, 0), 1));
        }

        private void FixedStar(Simulator simulator)
        {
            simulator.GravityConstant = 15;

            simulator.Particles.Add(new FixedBody(new Vector3(0, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 200, 0), new Vector3(-2.2, 0, 0), 1));
            simulator.Particles.Add(new Particle(new Vector3(0, 150, 0), new Vector3(-1.5, 0, 0), 1));
            simulator.Particles.Add(new Particle(new Vector3(0, 100, 0), new Vector3(-3, 0, 0), 1));
        }

        private void FixedStarWithPlanetAndMoon(Simulator simulator)
        {
            simulator.GravityConstant = 2;
            simulator.Particles.Add(new FixedBody(new Vector3(0, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 220, 0), new Vector3(-1, 0, 0), 50));
            simulator.Particles.Add(new Particle(new Vector3(0, 250, 0), new Vector3(-2.5, 0, 0), 1));
        }

        private void BinaryWithFourSatellites(Simulator simulator)
        {
            simulator.GravityConstant = 15;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(-3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(3, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 0, -200), new Vector3(-3, 0, 0), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, 0, 200), new Vector3(3, 0, 0), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, -200, 0), new Vector3(0, 0, -3), 20));
            simulator.Particles.Add(new Particle(new Vector3(0, 200, 0), new Vector3(0, 0, 3), 20));
        }

        private void Draw(object sender, ElapsedEventArgs e)
        {
            lock (m_Buffer)
            {
                using (var g = Graphics.FromImage(m_Buffer))
                {
                    g.Clear(Color.White);
                    var particles = m_Simulator.Particles.OrderBy(p => p.Position.Z).ToArray();
                    foreach (var particle in particles)
                    {
                        var zModifier = Math.Pow(2, particle.Position.Z / 80);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier);
                        //g.FillRectangle(Brushes.DarkCyan, (float)particle.Position.X, (float)particle.Position.Y, dim, dim);
                        Brush b = particle.Mass > 50 ? Brushes.Goldenrod : Brushes.Blue;
                        DrawCircleAt(g, b, MapX((float)particle.Position.X), MapY((float)particle.Position.Y), dim);
                    }
                }
            }
            pictureBox.Invalidate();
        }

        private void DrawCircleAt(Graphics g, Brush brush, float x, float y, float r)
        {
            var radius = r > 500 ? 500 : r;
            if (radius < 1) radius = 1;
            g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_Simulator.Stop();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            CreateBitmap();
        }
    }
}