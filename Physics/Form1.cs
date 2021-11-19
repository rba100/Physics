using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Physics.Engine;
using Physics.Scenarios;

namespace Physics
{
    public partial class Form1 : Form
    {
        private Simulator m_Simulator;
        private Bitmap m_Buffer;
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
            InitScenarios();
            pictureBox.Paint += PictureBoxOnPaint;
            CreateBitmap();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            this.PreviewKeyDown += PictureBox_PreviewKeyDown;
        }

        private void InitScenarios()
        {
            List<Scenario> scenarios = new List<Scenario>();

            scenarios.AddRange(GetType().Assembly
                                        .GetTypes()
                                        .Where(t => t.BaseType == typeof(Scenario))
                                        .Select(Activator.CreateInstance)
                                        .Cast<Scenario>()
                                        .OrderBy(s=>s.ToString()));

            cbScenarioList.DataSource = scenarios;

            cbScenarioList.SelectedIndex = 0;
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
            ResetSim(cbScenarioList.SelectedItem as Scenario);
        }

        private void ResetSim(Scenario scenario)
        {
            m_Simulator?.Dispose();

            m_Simulator = new Simulator() { TickInterval = 1 };
            m_FormingStars = new List<FormingStar>();

            m_Simulator.ParticlesMerged += SimulatorOnParticlesMerged;
            m_Simulator.Tick += Draw;

            if (scenario == null) return;
            scenario.Configure(m_Simulator);

            m_Simulator.Start();
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
            m_Simulator?.Dispose();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            CreateBitmap();
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            ResetSim(cbScenarioList.SelectedItem as Scenario);
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
