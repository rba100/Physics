using System;
using System.Collections.Generic;
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

        private const int c_StarMass = 50;
        private const int c_NewStarFrames = 200;
        private List<FormingStar> m_FormingStars = new List<FormingStar>();

        private Pen[] BoomColours;

        public Form1()
        {
            InitializeComponent();
            InitColours();
            pictureBox.Paint += PictureBoxOnPaint;
            CreateBitmap();
            m_Timer.SynchronizingObject = this;
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
        
        private void SimulatorOnParticlesMerged(object sender, MergeEventArgs args)
        {
            var oldA = m_FormingStars.FirstOrDefault(f => f.Particle.Equals(args.A));
            if (oldA != null)
            {
                oldA.Particle = args.Merged;
                return;
            }
            var oldB = m_FormingStars.FirstOrDefault(f => f.Particle.Equals(args.B));
            if (oldB != null)
            {
                oldB.Particle = args.Merged;
                return;
            }
            if (args.A.Mass > c_StarMass) return;
            if (args.B.Mass > c_StarMass) return;
            if (args.Merged.Mass > c_StarMass)
            {
                m_FormingStars.Add(new FormingStar(args.Merged));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Sol(m_Simulator);

            m_Simulator.TickInterval = 15;
            m_Simulator.ParticlesMerged += SimulatorOnParticlesMerged;
            m_Simulator.Start();
            m_Timer.Elapsed += Draw;
            m_Timer.Start();
        }

        private void Sol(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;
            var unit = Math.Sqrt(0.5);
            simulator.Particles.Add(new FixedBody(new Vector3(0, 0, 0), 1000));

            for (int i = 1; i <= 10; i++)
            {
                var r = i *20 + 50;
                var v = Math.Sqrt(simulator.GravityConstant * (1000) / r);
                var p = new Particle(new Vector3(0, -r, 0), new Vector3(v, 0, 0), 10);
                simulator.Particles.Add(p);
            }
        }

        private void CollisionTest(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;

            simulator.Particles.Add(new FixedBody(new Vector3(0, 0, 0), 1000));

            for (int i = 1; i <= 20; i++)
            {
                var r = i * 10 + 50;
                var v = Math.Sqrt(simulator.GravityConstant * (1000) / r);
                var p = new Particle(new Vector3(0, -r, 0), new Vector3(v, 0, 0), 5);
                simulator.Particles.Add(p);
            }
        }

        private void StarIsBorn(Simulator simulator)
        {
            simulator.Collisions = true;
            simulator.GravityConstant = 1;

            simulator.Particles.Add(new Particle(new Vector3(0, -30, 0), new Vector3(0, 0, 0), 30));
            simulator.Particles.Add(new Particle(new Vector3(0, 30, 0), new Vector3(0, 0, 0), 30));

            simulator.Particles.Add(new Particle(new Vector3(100, 0, 0), new Vector3(0, 0, 0), 30));
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
                    var particles = m_Simulator.Particles.ToArray().OrderBy(p => p.Position.Z);
                    foreach (var particle in particles)
                    {
                        var zModifier = Math.Pow(2, particle.Position.Z / 160);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier);
                        Brush b = particle.Mass > c_StarMass ? Brushes.Goldenrod : Brushes.Blue;
                        DrawFilledCircleAt(g, b, MapX((float)particle.Position.X), MapY((float)particle.Position.Y), dim);
                    }

                    foreach (var formingStar in m_FormingStars.ToArray())
                    {
                        if (formingStar.Frame >= c_NewStarFrames)
                        {
                            m_FormingStars.Remove(formingStar);
                            continue;
                        }
                        var particle = formingStar.Particle;
                        var zModifier = Math.Pow(2, particle.Position.Z / 160);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier);

                        DrawCircleAt(g, BoomColours[formingStar.Frame], MapX((float)particle.Position.X),
                            MapY((float)particle.Position.Y), dim + formingStar.Frame);

                        formingStar.Frame++;
                    }
                }
                pictureBox.Invalidate();
            }
        }

        private void InitColours()
        {
            var colours = new List<Pen>();
            for (int i = 0; i < c_NewStarFrames; i++)
            {
                var ratio = (double)i / c_NewStarFrames;

                var red = Math.Min(255, ratio * 25 + 230);
                var green = Math.Min(255, ratio * 75 + 180);
                var blue = Math.Min(255, ratio * 105 + 150);
                colours.Add(new Pen(Color.FromArgb(255, (int)red, (int)green, (int)blue)));
            }
            BoomColours = colours.ToArray();
        }

        private void DrawFilledCircleAt(Graphics g, Brush brush, float x, float y, float r)
        {
            var radius = r > 500 ? 500 : r;
            if (radius < 1) radius = 1;
            g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
        }

        private void DrawCircleAt(Graphics g, Pen pen, float x, float y, float r)
        {
            var radius = r > 500 ? 500 : r;
            if (radius < 1) radius = 1;
            g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
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

    internal class FormingStar
    {
        public IParticle Particle;
        public int Frame = 0;

        public FormingStar(IParticle particle)
        {
            Particle = particle;
        }
    }
}
