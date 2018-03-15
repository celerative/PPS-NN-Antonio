using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

using UnityEngine;
using System.IO;
using System.Linq;



class Program : MonoBehaviour
{
    const int MAXDESPLAZAMIENTO = 250;
    static void Main(string[] args)
    {

        CrearPista();
    }

    public static void desglosar(FloatArray fa, out float[] xx, out float[] yy)
    {
        xx = new float[fa.size / 2];
        yy = new float[fa.size / 2];


        List<float> l4 = fa.toArray().ToList();

        for (int i = 0; i < l4.Count; i++)
        {
            if (i % 2 == 0)
                xx[i / 2] = l4.ElementAt(i);
            else
                yy[(i - 1) / 2] = l4.ElementAt(i);
        }
    }

    public static void arreglarAngulos(ref Punto[] Puntos)
    {
        for (int i = 0; i < Puntos.Length; ++i)
        {
            int previous = (i - 1 < 0) ? Puntos.Length - 1 : i - 1;
            int next = (i + 1) % Puntos.Length;
            float px = Puntos[i].x - Puntos[previous].x;
            float py = Puntos[i].y - Puntos[previous].y;
            float pl = (px != 0) ? (float)Math.Sqrt(px * px + py * py) : 1;
            px /= pl;
            py /= pl;

            float nx = Puntos[i].x - Puntos[next].x;
            float ny = Puntos[i].y - Puntos[next].y;
            nx = -nx;
            ny = -ny;
            float nl = (nx != 0) ? (float)Math.Sqrt(nx * nx + ny * ny) : 1;
            nx /= nl;
            ny /= nl;
            //I got a vector going to the next and to the previous points, normalised.  

            float a = (float)MathUtils.Sin.atan2(px * ny - py * nx, px * nx + py * ny); // perp dot product between the previous and next point. Google it you should learn about it!  

            if (Math.Abs(a * MathUtils.radDeg) <= 100) continue;

            float nA = 100 * Math.Sign(a) * MathUtils.degRad;
            float diff = nA - a;
            float cos = (float)Math.Cos(diff);
            float sin = (float)Math.Sin(diff);
            float newX = nx * cos - ny * sin;
            float newY = nx * sin + ny * cos;
            newX *= nl;
            newY *= nl;
            Puntos[next].x = Puntos[i].x + newX;
            Puntos[next].y = Puntos[i].y + newY;
            //I got the difference between the current angle and 100degrees, and built a new vector that puts the next point at 100 degrees.  
        }
        Puntos[Puntos.Length - 1] = Puntos[0];
    }

    public static void swap(ref float a, ref float b)
    {
        float aux = a;
        a = b;
        b = aux;
    }
    private static void deformar(ref Punto[] p)
    {
        var r = new System.Random();
        for (int i = 0; i < p.Length; i++)
        {
            if (r.Next(2) == 0)
            {
                swap(ref p[i].y, ref p[(i + 1) % p.Length].y);
                //swap(ref p[i].x, ref p[Math.Abs(i - 1) % p.Length].x);
            }


        }
        p[p.Length - 1] = new Punto(p[0]);
        arreglarAngulos(ref p);

    }

    private static void CrearPista()
    {
        var r = new System.Random();
        int m = 500;



        Punto[] puntos = new Punto[m];
        for (int i = 0; i < m; i++)
        {
            //COORDENADAS POLARES
            puntos[i] = new Punto(Convert.ToSingle(r.NextDouble() * 125), Convert.ToSingle(r.NextDouble() * 2 * Math.PI), 1);
        }
        List<Punto> l = new List<Punto>();
        for (int i = 0; i < puntos.Length; i++)
        {
            l.Add(puntos[i]);
        }
        l.Add(puntos[0]);
        puntos = l.ToArray();
        ConvexHull ch = new ConvexHull();
        List<float> ll = new List<float>();
        for (int i = 0; i < puntos.Length; i++)
        {
            ll.Add(puntos[i].x);
            ll.Add(puntos[i].y);
        }
        FloatArray fa = new FloatArray(ll.ToArray());

        FloatArray contorno = ch.computePolygon(fa, false);


        float[] xx;
        float[] yy;
        float[] xs, ys;
        int n = 1000;

        desglosar(contorno, out xx, out yy);

        Punto[] convexo = Punto.CrearArregloDesdeArreglos(xx, yy);
        CubicSpline.FitParametric(xx, yy, n, out xs, out ys, 1, -1, 1, -1);


        ////////////////////////////////////// AGREGAR DIFICULTAD

        //agregarDificultad(ref convexo, 30);
        deformar(ref convexo);
        xx = Punto.getAbscisas(convexo);
        yy = Punto.getOrdenadas(convexo);
        CubicSpline.FitParametric(xx, yy, n, out xs, out ys);
    }


    public static float getDerivada(Punto p1, Punto p2)
    {
        return (p2.y - p1.y) / (p2.x - p2.x);
    }




    public static TriDiagonalMatrixF TestTdm()
    {
        TriDiagonalMatrixF m = new TriDiagonalMatrixF(10);

        for (int i = 0; i < m.N; i++)
        {
            m.A[i] = 1.111111f;
            m.B[i] = 2.222222f;
            m.C[i] = 3.333333f;
        }

        Console.WriteLine("Matrix:\n{0}", m.ToDisplayString(",4:0.00", "    "));

        for (int i = 0; i < m.N; i++)
        {
            m[i, i] = 4.4444f;
        }

        Console.WriteLine("Matrix:\n{0}", m.ToDisplayString(",4:0.00", "    "));

        // Solve
        var rand = new System.Random(1);
        float[] d = new float[m.N];

        for (int i = 0; i < d.Length; i++)
        {
            d[i] = (float)rand.NextDouble();
        }

        float[] x = m.Solve(d);

        Console.WriteLine("Solve returned: ");

        for (int i = 0; i < x.Length; i++)
        {
            Console.Write("{0:0.0000}, ", x[i]);
        }

        Console.WriteLine();
        return m;
    }


}
