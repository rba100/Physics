using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Physics.Engine;

namespace Physics
{
    public partial class Form1 : Form
    {
        private readonly Simulator m_Simulator = new Simulator();
        private Bitmap m_Buffer;
        private Bitmap m_Display;
        private int m_Width;
        private int m_Height;

        private float scale = 1;
        private float offset_x = 0;
        private float offset_y = 0;


        private const int c_StarMass = 50;
        private const double c_MoonMass = 0.99;
        private const int c_NewStarFrames = 200;
        private const float c_ScrollStepSize = 1.2F;
        private const int c_ScrollStepsMax = 16;
        private float m_MaxScroll = (float)Math.Pow(c_ScrollStepSize, c_ScrollStepsMax); // (scroll step size, max scrolls)
        private List<FormingStar> m_FormingStars = new List<FormingStar>();

        private Pen[] BoomColours;

        public Form1()
        {
            InitializeComponent();
            InitColours();
            pictureBox.Paint += PictureBoxOnPaint;
            CreateBitmap();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            this.PreviewKeyDown += PictureBox_PreviewKeyDown;
        }

        private void PictureBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs args)
        {
            var amount = 100 / scale;

            if (args.KeyCode == Keys.Up)
            {
                offset_y += amount;
            }
            else if (args.KeyCode == Keys.Down)
            {
                offset_y -= amount;
            }
            else if (args.KeyCode == Keys.Right)
            {
                offset_x -= amount;
            }
            else if (args.KeyCode == Keys.Left)
            {
                offset_x += amount;
            }
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs args)
        {
            var d = args.Delta / 120;

            for (int i = 0; i < Math.Abs(d); i++)
            {
                if (d > 0) scale = Math.Min(scale * c_ScrollStepSize, m_MaxScroll);
                else scale = Math.Max(scale * 1 / c_ScrollStepSize, 1 / m_MaxScroll);
            }
        }

        private void CreateBitmap()
        {
            if (pictureBox.Size.Width < 1) return;
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
            return (x+offset_x) * scale + m_Width / 2;
        }

        private float MapY(float y)
        {
            return (y+offset_y) * scale + m_Height / 2;
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
            //Sol(m_Simulator);
            BinaryWithFourSatellites(m_Simulator);

            m_Simulator.TickInterval = 1;
            m_Simulator.ParticlesMerged += SimulatorOnParticlesMerged;
            m_Simulator.Start();
            m_Simulator.Tick += Draw;
        }

        private void Sol(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;
            var unit = Math.Sqrt(0.5);
            var sunMass = 100;
            simulator.Particles.Add(new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), sunMass));

            var mercury = CreatePlanet(sunMass, 50, 1);
            var venus = CreatePlanet(sunMass, 30, 1);
            var earth = CreatePlanet(sunMass, 70, 1);
            var moon = CreateMoon(earth, 5, earth.Mass / 6);
            var mars = CreatePlanet(sunMass, 90, 1);

            var jupiter = CreatePlanet(sunMass, 250, 20);
            var jMoon1 = CreateMoon(jupiter, 10, 0.01);
            var jMoon2 = CreateMoon(jupiter, 20, 0.01);
            var jMoon3 = CreateMoon(jupiter, 30, 0.01);
            // var saturn = CreatePlanet(sunMass, 200, 1);
            // var urasnus = CreatePlanet(sunMass, 250, 1);
            // var neptune = CreatePlanet(sunMass, 300, 1);

            simulator.Particles.Add(mercury);
            simulator.Particles.Add(venus);
            simulator.Particles.Add(earth);
            simulator.Particles.Add(moon);
            //simulator.Particles.Add(mars);
            simulator.Particles.Add(jupiter);
            simulator.Particles.Add(jMoon1);
            simulator.Particles.Add(jMoon2);
            simulator.Particles.Add(jMoon3);
            //simulator.Particles.Add(saturn);
            //simulator.Particles.Add(urasnus);
            //simulator.Particles.Add(neptune);
        }

        private void CollisionTest(Simulator simulator)
        {
            simulator.GravityConstant = 0.2;
            simulator.Collisions = true;

            simulator.Particles.Add(new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1000));

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

            simulator.Particles.Add(new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 100));
            simulator.Particles.Add(new Particle(new Vector3(0, 200, 0), new Vector3(-2.2, 0, 0), 1));
            simulator.Particles.Add(new Particle(new Vector3(0, 150, 0), new Vector3(-1.5, 0, 0), 1));
            simulator.Particles.Add(new Particle(new Vector3(0, 100, 0), new Vector3(-3, 0, 0), 1));
        }

        private void FixedStarWithPlanetAndMoon(Simulator simulator)
        {
            simulator.GravityConstant = 2;
            simulator.Particles.Add(new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 100));
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

        private void Draw(object sender, EventArgs e)
        {
            lock (m_Buffer)
            {
                using (var g = Graphics.FromImage(m_Buffer))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Black);
                    var particles = m_Simulator.Particles.ToArray().OrderBy(p => p.Position.Z);

                    foreach (var formingStar in m_FormingStars.ToArray())
                    {
                        if (formingStar.Frame >= c_NewStarFrames)
                        {
                            m_FormingStars.Remove(formingStar);
                            continue;
                        }
                        var particle = formingStar.Particle;
                        var zModifier = Math.Pow(2, particle.Position.Z / 160);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier) * scale;

                        DrawCircleAt(g, BoomColours[formingStar.Frame], MapX((float)particle.Position.X),
                            MapY((float)particle.Position.Y), dim + formingStar.Frame);

                        formingStar.Frame++;
                    }

                    foreach (var particle in particles)
                    {
                        var zModifier = Math.Pow(2, particle.Position.Z / 160);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier) * scale;
                        Brush b = particle.Mass > c_StarMass ? Brushes.Goldenrod : (particle.Mass < c_MoonMass ? Brushes.Gray : Brushes.LightBlue);
                        DrawFilledCircleAt(g, b, MapX((float)particle.Position.X), MapY((float)particle.Position.Y), dim);
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

                var red = Math.Min(255, ratio * (25 + 230));
                var green = Math.Min(255, ratio * (75 + 180));
                var blue = Math.Min(255, ratio * (105 + 150));
                colours.Add(new Pen(Color.FromArgb(255, (int)red, (int)green, (int)blue)));
            }
            colours.Reverse();
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

        private double CircularOrbitSpeed(double totalMass, double radius)
        {
            return Math.Sqrt(m_Simulator.GravityConstant * (totalMass) / radius);
        }

        private IParticle CreatePlanet(double sunMass, double altitude, double planetMass)
        {
            var planetPosition = new Vector3(altitude, 0, 0);
            var planetSpeed = CircularOrbitSpeed(sunMass + planetMass, altitude);
            var planet = new Particle(planetPosition, new Vector3(0, -planetSpeed, 0), planetMass);
            return planet;
        }

        private IParticle CreateMoon(IParticle parent, double altitude, double moonMass)
        {
            var moonPosition = new Vector3(parent.Position.Magnitude + altitude, 0, 0);
            var moonSpeed = CircularOrbitSpeed(moonMass + parent.Mass, altitude) + parent.Velocity.Magnitude;
            return new Particle(moonPosition, new Vector3(0, -moonSpeed, 0), moonMass);

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
