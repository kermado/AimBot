﻿using System;
using System.Drawing;
using System.Threading;

using AimBot.Injectors;

namespace AimBot.Aimers
{
    public class FlickAimer : Aimer
    {
        private Thread thread;
        private double ms; // Maximum speed (in pixels per second).
        private double hs; // Horizontal sensitivity (mouse/screen).
        private double vs; // Vertical sensitivity (mouse/screen).
        private int ts; // Time-step (in milliseconds).
        private int td; // Trigger delay (in milliseconds);
        private bool trigger;
        private bool flick;

        private bool disposed;

        public double MaxSpeed
        {
            get { return ms; }
            set { ms = value; }
        }

        public double HorizontalSensitivity
        {
            get { return hs; }
            set { hs = value; }
        }

        public double VerticalSensitivity
        {
            get { return vs; }
            set { vs = value; }
        }

        public int TimeStepMs
        {
            get { return ts; }
            set { ts = value; }
        }

        public bool Trigger
        {
            get { return trigger; }
            set { trigger = value; }
        }

        public int TriggerDelayMs
        {
            get { return td; }
            set { td = value; }
        }

        public FlickAimer()
        {
            ms = 4000.0;
            hs = 0.65;
            vs = 0.65;
            ts = 10;
            td = 50;
            trigger = true;
            flick = true;
        }

        public void Clear()
        {
            flick = true;
        }

        public void Aim(MouseInjector injector, Point target, Point position, double dt)
        {
            if (flick && target.IsEmpty == false && (thread == null || thread.IsAlive == false))
            {
                flick = false; // Wait for aim key/button to be pressed again.
                thread = new Thread(() => Flick(injector, target, position, ms, hs, vs, ts, td, trigger));
                thread.Start();
            }
        }

        public void Tick(MouseInjector injector, bool aiming, double dt)
        {
            if (aiming == false)
            {
                Clear();
            }
        }

        private static void Flick(MouseInjector injector, Point target, Point position, double speed, double horizontalSensitivity, double verticalSensitivity, int timestepMs, int triggerDelayMs, bool trigger)
        {
            double ex = 0.0;
            double ey = 0.0;

            var screenx = target.X - position.X;
            var screeny = target.Y - position.Y;

            var mousex = screenx * horizontalSensitivity;
            var mousey = screeny * verticalSensitivity;

            var distance = Math.Sqrt(mousex * mousex + mousey * mousey);
            var time = distance / speed;

            var frames = (int)Math.Ceiling((time * 1000) / timestepMs);
            for (int i = 0; i < frames; ++i)
            {
                var dx = ex + (double)mousex / frames;
                var dy = ey + (double)mousey / frames;

                var px = (int)Math.Round(dx);
                var py = (int)Math.Round(dy);

                ex = dx - px;
                ey = dy - py;

                if (px != 0 || py != 0)
                {
                    injector.Move(px, py);
                }

                if (i < frames - 1)
                {
                    Thread.Sleep(timestepMs);
                }
                else if (trigger)
                {
                    Thread.Sleep(triggerDelayMs);
                    var rng = new Random();
                    injector.Click(0, rng.Next(50, 80));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed == false)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
