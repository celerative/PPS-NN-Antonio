using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;



class TrackGenerator : MonoBehaviour
{
    static TrackGenerator SINGLETON = null;
    const int MAXDESPLAZAMIENTO = 250;
    const int n2 = 10 * EvolutionManager.CarCount;
    const int n = 10 * EvolutionManager.CarCount;
    public static GameObject[] PA = new GameObject[n];
    public static GameObject[] PR = new GameObject[n2];
    public static Vector3[] posIni = new Vector3[EvolutionManager.CarCount];
    public IList<GameObject> LPA = new List<GameObject>();
    public IList<GameObject> LPR = new List<GameObject>();
    public IList<GameObject> LCH = new List<GameObject>();
    public IList<GameObject> LO = new List<GameObject>();
    public IList<GameObject> LS = new List<GameObject>();
    public IList<GameObject> LPU = new List<GameObject>();

    public GameObject suelo;
    public GameObject pared;
    public GameObject paredRoja;
    public GameObject checkpoint;
    public GameObject obstaculo;
    public GameObject powerUp;
    public static bool PistaCreada;
    public bool isObstaculos;
    public bool isDeforme;

    static void imprimir(string a)
    {
        UnityEngine.Debug.Log(a);
    }
    public void borrarListas(ref IList<GameObject> LPA, ref IList<GameObject> LPR, ref IList<GameObject> LCH, ref IList<GameObject> LO, ref IList<GameObject> LS)
    {
        foreach (var item in LPA)
        {
            Destroy(item);
        }
        foreach (var item in LPU)
        {
            Destroy(item);
        }
        foreach (var item in LPR)
        {
            Destroy(item);
        }
        foreach (var item in LCH)
        {
            Destroy(item);
        }
        foreach (var item in LO)
        {
            Destroy(item);
        }
        foreach (var item in LS)
        {
            Destroy(item);
        }
        LPA.Clear();
        LCH.Clear();
        LPR.Clear();
        LO.Clear();
        LS.Clear();
        LPU.Clear();
    }

    static void imprimir(int a)
    {
        imprimir(a.ToString());
    }

    public void Awake()
    {
        CrearPista(Vector3.zero);
        PistaCreada = true;
    }
    public void Update()
    {

        if (EvolutionManager.finalizo)
        {
            PistaCreada = false;
            borrarListas(ref LPA, ref LPR, ref LCH, ref LO, ref LS);
            CrearPista(Vector3.zero);
            PistaCreada = true;
            EvolutionManager.finalizo = true;
        }
    }
    public static void separarPuntos(ref Punto[] p)
    {
        Punto prox = p[1];
        Punto ant = p[p.Length - 1];
        float dif; float min = 9999;
        for (int i = 0; i < p.Length - 1; i++)
        {
            dif = p[i + 1].argumento - p[i].argumento;
            if (dif < min)
                min = dif;
        }
        for (int i = 0; i < p.Length; i++)
        {
            dif = prox.argumento - p[i].argumento;
            if (dif < 4 * min)
            {
                p[i].argumento = (prox.argumento + ant.argumento) / 2;
            }
            prox = p[(i + 1) % p.Length];
            ant = p[(i - 1) < 0 ? p.Length - 1 : i - 1];
        }
    }

    public static float getDerivada(Punto p1, Punto p2)
    {
        return (p2.y - p1.y) / (p2.x - p2.x);
    }

    public static float getDerivada(float x1, float y1, float x2, float y2)
    {
        return (y2 - y1) / (x2 - x1);
    }

    public static void desglosarFloatArrayEnVectoresNormales(FloatArray fa, out float[] xx, out float[] yy)
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

    public static float anguloEnGrados(Punto p1, Punto p2)
    {
        return 180 * Mathf.Atan2(p2.y - p1.y, -p2.x + p1.x) / Mathf.PI;
    }
    public static void deformar(ref Punto[] p)
    {

        for (int i = 0; i < p.Length; i++)
        {

            p[i] = new Punto(p[i].modulo + UnityEngine.Random.Range(-1, 1) * 30f, p[i].argumento, 1);

        }
        p[p.Length - 1] = new Punto(p[0]);
        arreglarAngulos(ref p);
    }
    Vector3 ant = new Vector3();
    public void CrearPista(Vector3 a)
    {
        var r = new System.Random();
        int m = 500;
        Punto[] puntos = new Punto[m];
        Punto Centro = new Punto(a.x, a.z);


        for (int i = 0; i < m; i++)
        {
            //COORDENADAS POLARES argumentos en radianes
            puntos[i] = new Punto(125 * UnityEngine.Random.value, UnityEngine.Random.value * 2 * Mathf.PI, 1);
            puntos[i].x += Centro.x;
            puntos[i].y += Centro.y;
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

        desglosarFloatArrayEnVectoresNormales(contorno, out xx, out yy);
        Punto[] convexo = Punto.CrearArregloDesdeArreglos(xx, yy);




        ////////////////////////////////////// AGREGAR DIFICULTAD
        float ancho = UnityEngine.Random.Range(5f, 9f);

        Punto[] convexo2 = new Punto[convexo.Length * 2];
        if (isDeforme)
            deformar(ref convexo);

        separarPuntos(ref convexo);

        xx = Punto.getAbscisas(convexo);

        yy = Punto.getOrdenadas(convexo);

        CubicSpline.FitParametric(xx, yy, n, out xs, out ys, 1, -1, 1, -1);

        /////////////////////////////////////////////////////////////////////////
        /*
         en xs y en ys estan las posiciones x,y de cada punto de la pista
         */



        LS.Add(Instantiate(this.suelo, new Vector3(0, -1, 0), Quaternion.Euler(Vector3.zero), this.transform));
        float[] angulosEnGrados = new float[n];

        for (int i = 0; i < xs.Length - 1; i++)
        {
            angulosEnGrados[i] = 180 * Mathf.Atan2(ys[i + 1] - ys[i], -xs[i + 1] + xs[i]) / Mathf.PI;
        }

        IList<Punto> listaRoja = new List<Punto>();
        IList<Punto> listaCheckpoint = new List<Punto>();
        Punto[] puntosInteriores;
        int offset = r.Next();
        Vector3 pos, pos2 = new Vector3();
        Quaternion rot;
        for (int i = 0; i < xs.Length - 1; i++)
        {
            pos = new Vector3(xs[i], 0, ys[i]);
            rot = Quaternion.Euler(0, angulosEnGrados[i], 0);
            pared.transform.position = pos;
            pared.transform.rotation = rot;

            LPA.Add(Instantiate(pared, pos, rot, this.transform));
            if (i % 10 == 0)
            {
                pared.transform.Translate(ancho / 2 * -Vector3.back, Space.Self);
                pos2 = pared.transform.position;
                pared.transform.Translate(ancho / 2 * -Vector3.back, Space.Self);
                listaRoja.Add(new Punto(pared.transform.position));
            }
            else
            {
                pared.transform.Translate((ancho) * -Vector3.back, Space.Self);
                listaRoja.Add(new Punto(pared.transform.position));
            }

            if (i % 5 == 0)
            {
                pared.transform.Translate(ancho / 2 * Vector3.back, Space.Self);
                checkpoint.transform.rotation = Quaternion.Euler(0, angulosEnGrados[i], 0);
                checkpoint.transform.position = pared.transform.position;
                checkpoint.transform.localScale = new Vector3(0.1f, checkpoint.transform.localScale.y, ancho);
                LCH.Add(Instantiate(checkpoint, pared.transform.position, Quaternion.Euler(0, angulosEnGrados[i], 0), this.transform));
            }
            if (UnityEngine.Random.value > 0.93f)
            {
                powerUp.transform.position = pos;
                powerUp.transform.rotation = rot;
                powerUp.transform.Translate(UnityEngine.Random.Range(1, ancho - 1) * Vector3.forward, Space.Self);
                LPU.Add(Instantiate(powerUp, this.transform));
            }
            if (isObstaculos)
                if (UnityEngine.Random.value > 0.988f)
                {
                    obstaculo.transform.position = pos;
                    obstaculo.transform.rotation = rot;
                    obstaculo.transform.Translate(UnityEngine.Random.Range(1, ancho - 1) * Vector3.forward, Space.Self);
                    LO.Add(Instantiate(obstaculo, this.transform));
                }
            posIni[i / (EvolutionManager.CarCount / 10)] = pos2;
            EvolutionManager.ini = posIni[i / (EvolutionManager.CarCount / 10)];
        }


        puntosInteriores = listaRoja.ToArray();
        puntosInteriores[puntosInteriores.Length - 1] = puntosInteriores[0];
        arreglarAngulos(ref puntosInteriores);

        Punto[] PC = listaCheckpoint.ToArray();
        float[] xx2;
        float[] yy2;
        float[] xs2, ys2;
        float[] angulosEnGrados2 = new float[n2];
        float[] angulosEnGrados3 = new float[n / 10];
        xx2 = Punto.getAbscisas(puntosInteriores);
        yy2 = Punto.getOrdenadas(puntosInteriores);
        CubicSpline.FitParametric(xx2, yy2, n2, out xs2, out ys2, 1, -1, 1, -1);

        for (int i = 0; i < xs2.Length - 1; i++)
        {
            angulosEnGrados2[i] = 180 * Mathf.Atan2(ys2[i + 1] - ys2[i], -xs2[i + 1] + xs2[i]) / Mathf.PI;
        }

        for (int i = 0; i < xs2.Length - 1; i++)
        {
            paredRoja.transform.rotation = Quaternion.Euler(0, angulosEnGrados2[i], 0);
            paredRoja.transform.position = new Vector3(xs2[i], 0, ys2[i]);
            LPR.Add(Instantiate(paredRoja, new Vector3(xs2[i], 0, ys2[i]), Quaternion.Euler(0, angulosEnGrados2[i], 0), this.transform));
        }

        ////////////////////////////////////7
    }

}
