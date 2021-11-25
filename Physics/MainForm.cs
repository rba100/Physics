using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;

using Physics.Engine;
using Physics.Scenarios;

namespace Physics
{
    [SupportedOSPlatform("windows")]
    public partial class MainForm : Form
    {
        private Simulator m_Simulator;
        private Bitmap m_Buffer;
        private int m_Width;
        private int m_Height;

        private float scale = 1;

        private const double c_MoonMass = 0.99;
        private const int c_NewStarFrames = 200;
        private const float c_ScrollStepSize = 1.2F;
        private const int c_ScrollStepsMax = 16;
        private float m_MaxScroll = (float)Math.Pow(c_ScrollStepSize, c_ScrollStepsMax); // (scroll step size, max scrolls)
        private List<FormingStar> m_FormingStars = new List<FormingStar>();

        private Pen[] BoomColours;

        public MainForm()
        {
            InitializeComponent();
            InitColours();
            InitScenarios();
            pictureBox.Paint += PictureBoxOnPaint;
            CreateBitmap();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
        }

        private void InitScenarios()
        {
            List<Scenario> scenarios = new List<Scenario>();

            scenarios.AddRange(GetType().Assembly
                                        .GetTypes()
                                        .Where(t => t.BaseType == typeof(Scenario))
                                        .Select(Activator.CreateInstance)
                                        .Cast<Scenario>()
                                        .OrderBy(s => s.ToString() != "Trisolaris")
                                        .ThenBy(s => s.ToString()));

            cbScenarioList.DataSource = scenarios;

            cbScenarioList.SelectedIndex = 0;
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
            return (x+ m_ScrollOffset.X) * scale + m_Width / 2;
        }

        private float MapY(float y)
        {
            return (y+ m_ScrollOffset.Y) * scale + m_Height / 2;
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
            if (args.A.Mass > m_Simulator.StellarIgnitionMass) return;
            if (args.B.Mass > m_Simulator.StellarIgnitionMass) return;
            if (args.Merged.Mass > m_Simulator.StellarIgnitionMass)
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
                var particles = m_Simulator.Particles.Select(Rotate).ToArray();
                
                using (var g = Graphics.FromImage(m_Buffer))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Black);
                    var orderedParticles = particles.OrderBy(p => p.Position.Z);

                    foreach (var formingStar in m_FormingStars.ToArray())
                    {
                        if (formingStar.Frame >= c_NewStarFrames)
                        {
                            m_FormingStars.Remove(formingStar);
                            continue;
                        }
                        var particle = Rotate(formingStar.Particle);
                        var zModifier = Math.Pow(2, particle.Position.Z / 160);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier) * scale;

                        DrawCircleAt(g,
                                     BoomColours[formingStar.Frame], 
                                     MapX((float)particle.Position.X),
                                     MapY((float)particle.Position.Y),
                                     dim + formingStar.Frame);

                        formingStar.Frame++;
                    }

                    foreach (var particle in orderedParticles)
                    {
                        var zModifier = Math.Pow(2, particle.Position.Z / 160 * scale);
                        var dim = (float)(Math.Max(Math.Sqrt(particle.Mass), 2) * zModifier) * scale;
                        Brush b = particle.Mass > m_Simulator.StellaCollapseMass ? 
                            Brushes.Black : particle.Mass > m_Simulator.StellarIgnitionMass ?
                                Brushes.Goldenrod : (particle.Mass < c_MoonMass ?
                                    Brushes.Gray : Brushes.LightBlue);

                        var x = MapX((float)particle.Position.X);
                        var y = MapY((float)particle.Position.Y);
                        if(particle.Mass > m_Simulator.StellaCollapseMass)
                        {
                            DrawFilledCircleAt(g, Brushes.White, x, y, dim*1.01f);
                        }
                        DrawFilledCircleAt(g, b, x, y, dim);
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

        private Particle Rotate(IParticle p)
        {
            var v = p.Position;

            var r1 = new Vector3(v.X * Math.Cos(m_ViewRotation.Item1) + v.Z * Math.Sin(m_ViewRotation.Item1),
                                v.Y,
                                -v.X * Math.Sin(m_ViewRotation.Item1) + v.Z * Math.Cos(m_ViewRotation.Item1));

            var r2 = new Vector3(r1.X,
                                 r1.Y * Math.Cos(m_ViewRotation.Item2) - r1.Z * Math.Sin(m_ViewRotation.Item2),
                                 r1.Y * Math.Sin(m_ViewRotation.Item2) + v.Z * Math.Cos(m_ViewRotation.Item2));

            

            return new Particle(r2, Vector3.Zero, p.Mass);
        }

        Point m_MouseLast;
        Point m_ScrollOffset = new Point(0,0);
        private Tuple<double, double> m_ViewRotation = new Tuple<double, double>(0, 0);
        bool m_IsMouseDown;
        bool m_IsMouseDownRotate;

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            m_MouseLast = e.Location;
            m_IsMouseDown = true;
            m_IsMouseDownRotate = e.Button == MouseButtons.Middle;
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            m_IsMouseDown = false;
            m_IsMouseDownRotate = false;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_IsMouseDown) return;
            if (m_IsMouseDownRotate)
            {
                m_ViewRotation = new Tuple<double, double>(
                    ((e.X - m_MouseLast.X) / 100.0 * Math.PI) % Math.PI * 2,
                    ((e.Y - m_MouseLast.Y) / 100.0 * Math.PI) % Math.PI * 2);
            }
            else
            {
                m_ScrollOffset = new Point((e.X - m_MouseLast.X)*10000 / (int)(scale*10000) + m_ScrollOffset.X,
                                           (e.Y - m_MouseLast.Y)*10000 / (int)(scale*10000) + m_ScrollOffset.Y);
                m_MouseLast = e.Location;
            }
        }

        private void btResetRotation_Click(object sender, EventArgs e)
        {
            m_ViewRotation = new Tuple<double, double>(0, 0);
            m_ScrollOffset.X = 0;
            m_ScrollOffset.Y = 0;
        }
    }
}
