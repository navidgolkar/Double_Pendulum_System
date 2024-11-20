using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace Pendulum1
{
    public partial class Form1 : Form
    {
        private const int pend_num = 6; //Number of double pendulums
        private const float ratio = 200f;
        private const double G = 9.81; // Acceleration due to gravity (m/s^2)
        private const double M1 = 1.0; // Mass of the first pendulum bob (kg)
        private const double M2 = 1.0; // Mass of the second pendulum bob (kg)
        private const double L1 = 0.5; // Length of the first pendulum arm (m)
        private const double L2 = 0.5; // Length of the second pendulum arm (m)

        private Color[] colors = {Color.Red, Color.Green, Color.Blue, Color.Cyan, Color.Magenta, Color.Yellow}; // Colors of second pendulum bobs
        private double[] angle1 = new double[pend_num]; // Initial angle of the first pendulum (rad)
        private double[] angle2 = new double[pend_num]; // Initial angle of the second pendulum (rad)
        private double[] omega1 = new double[pend_num]; // Initial angular velocity of the first pendulum (rad/s)
        private double[] omega2 = new double[pend_num]; // Initial angular velocity of the second pendulum (rad/s)

        private WinFormsTimer timer = new WinFormsTimer();

        private const int interval = 16;

        private List<PointF>[] secondBobPositions = new List<PointF>[pend_num];

        public Form1()
        {
            for (int i=0; i<pend_num; i++)
            {
                secondBobPositions[i] = new List<PointF>();
                //A little change in the initial condition of the double pendulums
                angle1[i] = 2*Math.PI / 3 + i * (0.1*Math.PI/180);
                angle2[i] = Math.PI / 2;
                omega1[i] = 0;
                omega2[i] = 0;
            }
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = interval; // Update every ~16 milliseconds (approximately 60 FPS)
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdatePendulum();
            Invalidate(); // Redraw the form
        }

        private void UpdatePendulum()
        {
            const double deltaT = (double)interval/1000; // Time step (s)

            for (int i=0; i<pend_num; i++)
            {
                // Compute angular accelerations using Lagrange's equations
                double num1 = -G * (2 * M1 + M2) * Math.Sin(angle1[i]);
                double num2 = -M2 * G * Math.Sin(angle1[i] - 2 * angle2[i]);
                double num3 = -2 * Math.Sin(angle1[i] - angle2[i]) * M2;
                double num4 = omega2[i] * omega2[i] * L2 + omega1[i] * omega1[i] * L1 * Math.Cos(angle1[i] - angle2[i]);
                double den1 = L1 * (2 * M1 + M2 - M2 * Math.Cos(2 * angle1[i] - 2 * angle2[i]));
                double den2 = L2 * (2 * M1 + M2 - M2 * Math.Cos(2 * angle1[i] - 2 * angle2[i]));
                double alpha1 = (num1 + num2 + num3 * num4) / den1;
                double alpha2 = (2 * Math.Sin(angle1[i] - angle2[i]) * (omega1[i] * omega1[i] * L1 * (M1 + M2) + G * (M1 + M2) * Math.Cos(angle1[i]) + omega2[i] * omega2[i] * L2 * M2 * Math.Cos(angle1[i] - angle2[i]))) / den2;

                // Update angular velocities
                omega1[i] += alpha1 * deltaT;
                omega2[i] += alpha2 * deltaT;

                // Update angles
                angle1[i] += omega1[i] * deltaT;
                angle2[i] += omega2[i] * deltaT;

                // Calculate the position of the second pendulum bob
                PointF secondBobPosition = new PointF();
                secondBobPosition.X = ClientSize.Width / 2 + (int)(((float)(L1 * Math.Sin(angle1[i]) + L2 * Math.Sin(angle2[i]))) * ratio);
                secondBobPosition.Y = ClientSize.Height / 2 + (int)(((float)(L1 * Math.Cos(angle1[i]) + L2 * Math.Cos(angle2[i]))) * ratio);

                // Store the position
                secondBobPositions[i].Add(secondBobPosition);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            for (int i = 0; i < pend_num;  i++)
            {
                Brush b = new SolidBrush(colors[i]);
                // Draw the line representing the motion of the second pendulum bob
                /*if (secondBobPositions[i].Count > 1)
                {
                    Pen p = new Pen(colors[i]);
                    g.DrawLines(p, secondBobPositions[i].ToArray());
                }*/

                // Draw the first pendulum arm
                double x1 = L1 * Math.Sin(angle1[i]);
                double y1 = L1 * Math.Cos(angle1[i]);
                g.DrawLine(Pens.Black, ClientSize.Width / 2, ClientSize.Height / 2, ClientSize.Width / 2 + (int)(x1 * ratio), ClientSize.Height / 2 + (int)(y1 * ratio));

                // Draw the first pendulum bob
                g.FillEllipse(Brushes.Black, ClientSize.Width / 2 + (int)(x1 * ratio) - 5, ClientSize.Height / 2 + (int)(y1 * ratio) - 5, 10, 10);

                // Draw the second pendulum arm
                double x2 = L2 * Math.Sin(angle2[i]);
                double y2 = L2 * Math.Cos(angle2[i]);
                g.DrawLine(Pens.Black, ClientSize.Width / 2 + (int)(x1 * ratio), ClientSize.Height / 2 + (int)(y1 * ratio), ClientSize.Width / 2 + (int)((x1 + x2) * ratio), ClientSize.Height / 2 + (int)((y1 + y2) * ratio));

                // Draw the second pendulum bob
                g.FillEllipse(b, ClientSize.Width / 2 + (int)((x1 + x2) * ratio) - 5, ClientSize.Height / 2 + (int)((y1 + y2) * ratio) - 5, 10, 10);
                // Change Brushes.Black to b to change the color of the second bob for each double pendulum
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Capture the click coordinates
            int mouseX = e.X;
            int mouseY = e.Y;

            // Create a bitmap to contain only the paths
            Bitmap bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                for (int i = 0; i < pend_num; i++)
                {
                    // Draw the line representing the motion of the second pendulum bob
                    if (secondBobPositions[i].Count > 1)
                    {
                        g.DrawLines(new Pen(colors[i]), secondBobPositions[i].ToArray());
                    }
                }
            }

            // Save the bitmap as a .png image
            string fileName = "pendulum_paths.png";
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
            MessageBox.Show("Pendulum paths saved as " + fileName, "Save Successful");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
