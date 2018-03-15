using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class Punto : IComparable
{
    public float x;
    public float y;
    public float modulo;
    public float argumento;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        Punto p = (Punto)obj;
        bool b = (this.x == p.x) && (this.y == p.y);
        return b;
    }

    static float mod(float a, float b)
    {
        return Mathf.Sqrt(a * a + b * b);
    }

    public Punto(float a, float b)
    {
        x = a;
        y = b;
        argumento = Mathf.Atan2(b, a);
        modulo = mod(a, b);
    }

    public Punto(Vector3 v)
    {
        x = v.x;
        y = v.z;
        argumento = Mathf.Atan2(v.y,v.x);
        modulo = mod(v.x, v.y);
    }


    public Punto() { }


    /// <summary>
    /// ///O(n²) pero son 10 o 20 puntos
    /// </summary>
    /// <param name="puntos"></param>
    /// <returns></returns>
    public static IList<Punto> ordernarPorDistancia(Punto[] puntos)
    {
        IList<Punto> yaEvauluados = new List<Punto>();
        IList<Punto> resultado = new List<Punto>();
        resultado.Add(puntos[0]);
        yaEvauluados.Add(puntos[0]);
        float min = float.MaxValue;
        int ant = 0;
        Punto candidato = new Punto();
        for (int i = 1; i < puntos.Length; i++)
        {
            for (int j = 1; j < puntos.Length; j++)
            {
                if ((Punto.distancia(puntos[ant], puntos[j]) < min) && (!yaEvauluados.Contains(puntos[j])))
                {
                    candidato = puntos[j];
                    min = Punto.distancia(puntos[ant], puntos[j]);
                }
                yaEvauluados.Add(puntos[j]);
            }
            min = float.MaxValue;
            yaEvauluados.Clear();
            foreach (var item in resultado)
            {
                yaEvauluados.Add(item);
            }
            resultado.Add(candidato);
            ant++;
        }
        resultado.Add(puntos[0]);
        //Console.Out.WriteLine("{0}    {1}", resultado.Count, puntos.Length);
        //Console.ReadKey();
        return resultado;

    }

    private static float distancia(Punto p1, Punto p2)
    {
        return Mathf.Sqrt((p2.x-p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));    }


    
    public Punto(float modu, float angl, int a)//       angulo en radianes
    {
        modulo = modu;
        argumento = angl;
        if (angl < Mathf.PI)
        {
            x = modu * Mathf.Cos(angl);
            y = modu * Mathf.Sin(angl);
        }
        else if (angl > Mathf.PI)
        {
            x = modu * Mathf.Cos(angl);
            y = modu * Mathf.Sin(angl);

        }

    }

  

    public static float[] getAbscisas(Punto[] puntos)
    {
        float[] vec = new float[puntos.Length];
        for (int i = 0; i < puntos.Length; i++)
        {
            vec[i] = puntos[i].x;
        }
        return vec;
    }


    public static float[] getOrdenadas(Punto[] puntos)
    {
        float[] vec = new float[puntos.Length];
        for (int i = 0; i < puntos.Length; i++)
        {
            vec[i] = puntos[i].y;
        }
        return vec;
    }


    public override string ToString()
    {
        return string.Format("(x: {0}\ty:{1} \t Modulo: {2}\tArgumento:{3})", x, y,modulo, 180 * argumento /Mathf.PI);
    }

    public static Punto[] CrearArregloDesdeArreglos(float[] a, float[] b)
    {
        Punto[] res = new Punto[a.Length];

        if (a.Length != b.Length)
            throw new Exception("cagaste");
        else
        {
            for (int i = 0; i < a.Length; i++)
            {
                res[i] = new Punto(a[i],b[i]);
            }
        }
        return res;
    }

    public Punto(Punto p)
    {
        this.x = p.x;
        this.y = p.y;
        argumento = Mathf.Atan2(p.y, -p.x);
        modulo = mod(p.x, p.y);

    }

    public static Punto[] getArreglo(float[] x, float[] y)
    {
        Punto[] Puntos = new Punto[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            Puntos[i] = new Punto(mod(x[i],y[i]), Mathf.Atan2(y[i], -x[i]), 1);
        }
        return Puntos;
    }

    


    int IComparable.CompareTo(object obj)
    {
        Punto c = (Punto)obj;
        return (c.x == this.x) ? 0 : (c.x > this.x) ? 1 : -1;
    }

    public static class PuntosSorter
    {
        

        private static float Pow2(float x)
        {
            return x * x;
        }

        private static float Distance2(Punto p1, Punto p2)
        {
            return Pow2(p2.x - p1.x) + Pow2(p2.y - p1.y);
        }



        private class compararPorAnguloHelper : IComparer
        {
            public float Rad2Deg = 180.0f / Mathf.PI;
            public float Deg2Rad = Mathf.PI / 180.0f;
            int IComparer.Compare(object a, object b)
            {
                Punto p1 = (Punto)a;
                Punto p2 = (Punto)b;
                float res = p2.argumento - p1.argumento;
                if (res > 0)
                    return 1;

                if (res < 0)
                    return -1;

                else return 0;
            }
        }

        private class compararPorAlturaHelper : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                float tolerancia = 1e-20f;
                Punto p1 = (Punto)x; Punto p2 = (Punto)y;
                if (Mathf.Abs(p1.y - p2.y) < tolerancia)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class compararPorAbsisaHelper : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                Punto p1 = (Punto)x; Punto p2 = (Punto)y;
                if (p1.x > p2.x) return 1;
                else if (p1.x == p2.x)
                {
                    if (p1.y > p2.y)
                        return 1;
                    else if (p2.y > p1.y)
                        return -1;
                    else return 0;
                }
                else return -1;
            }
        }
        public static IComparer compararPorAltura()
        {
            return (IComparer)new compararPorAlturaHelper();
        }
        public static IComparer compararPorAbsisa()
        {
            return (IComparer)new compararPorAbsisaHelper();
        }
        public static IComparer compararPorAngulo()
        {
            return (IComparer)new compararPorAnguloHelper();
        }


    }

}
