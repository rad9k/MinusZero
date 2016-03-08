using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.UIWpf
{
    public class Line2D
    {
        public double A;
        public double B;
        public double C;
    }

    public class Geometry2D
    {
        public static Point FindLineCross(Line2D one, Line2D two)
        {
            Point p = new Point();

            double W = (one.A * two.B) - (two.A * one.B);
            double Wx = (-one.C * two.B) - (-two.C * one.B);
            double Wy = (one.A * -two.C) - (two.A * -one.C);

            p.X = Wx / W;
            p.Y = Wy / W;


            return p;
        }

        public static Line2D GetLine2DFromPoints(double X1, double Y1, double X2, double Y2)
        {
            if ((X1 - X2) == 0)
                X1 += 0.0001;

            if ((X1 - X2) != 0)
            {
                double a1 = (Y1 - Y2);
                double a2 = (X1 - X2);

                Line2D l = new Line2D();

                double a = (a1 / a2);
                double b = Y2 - (a * X2);

                l.A = a;
                l.B = -1;
                l.C = b;

                return l;
            }
            else
            {
                Line2D l = new Line2D();
                
                l.A = 1;
                l.B = 0;
                l.C = 0;

                return l;
            }
        }
    }
}
