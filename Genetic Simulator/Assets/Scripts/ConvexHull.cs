using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class ConvexHull
{

    private IntArray quicksortStack = new IntArray();
    private float[] sortedPoints;
    private FloatArray hull = new FloatArray();
    private IntArray indices = new IntArray();
    private ShortArray originalIndices = new ShortArray(false, 0);

    /** @see #computePolygon(float[], int, int, bool) */
    public FloatArray computePolygon(FloatArray points, bool sorted)
    {
        return computePolygon(points.items, 0, points.size, sorted);
    }

    /** @see #computePolygon(float[], int, int, bool) */
    public FloatArray computePolygon(float[] polygon, bool sorted)
    {
        return computePolygon(polygon, 0, polygon.Length, sorted);
    }

    /** Returns a list of points on the convex hull in counter-clockwise order. Note: the last point in the returned list is the
     * same as the first one. */
    /** Returns the convex hull polygon for the given point cloud.
     * @param points x,y pairs describing points. Duplicate points will result in undefined behavior.
     * @param sorted If false, the points will be sorted by the x coordinate then the y coordinate, which is required by the convex
     *           hull algorithm. If sorting is done the input array is not modified and count Additional working memory is needed.
     * @return pairs of coordinates that describe the convex hull polygon in counterclockwise order. Note the returned array is
     *         reused for later calls to the same method. */
    public static Punto[] arriba, abajo;
    public FloatArray computePolygon(float[] points, int offset, int count, bool sorted)
    {
        int end = offset + count;

        if (!sorted)
        {
            if (sortedPoints == null || sortedPoints.Length < count) sortedPoints = new float[count];
            Array.Copy(points, offset, sortedPoints, 0, count);
            points = sortedPoints;
            offset = 0;
            sort(points, count);
        }

        FloatArray hull = this.hull;
        hull.clear();

        // Lower hull.
        for (int i = offset; i < end; i += 2)
        {
            float x = points[i];
            float y = points[i + 1];
            while (hull.size >= 4 && ccw(x, y) <= 0)
                hull.size -= 2;
            hull.Add(x);
            hull.Add(y);
        }
        List<float> l1 = new List<float>();
        List<float> l2 = new List<float>();
        float[] xx = new float[hull.size];
        float[] yy = new float[hull.size];
        l1 = hull.toArray().ToList();
        l2 = hull.toArray().ToList();
        for (int i = 0; i < l1.Count; i++)
        {
            if (i % 2 == 0)
                xx[i / 2] = l1.ElementAt(i);
            else
                yy[(i - 1) / 2] = l1.ElementAt(i);
        }
        abajo = Punto.getArreglo(xx, yy);

        FloatArray hullAUX = new FloatArray();
        // Upper hull.
        for (int i = end - 4, t = hull.size + 2; i >= offset; i -= 2)
        {
            float x = points[i];
            float y = points[i + 1];
            while (hull.size >= t && ccw(x, y) <= 0)
                hull.size -= 2;
            hull.Add(x);
            hull.Add(y);
        }

        for (int i = end - 4, t = hullAUX.size + 2; i >= offset; i -= 2)
        {
            float x = points[i];
            float y = points[i + 1];
            while (hullAUX.size >= t && ccw(x, y) <= 0)
                hullAUX.size -= 2;
            hullAUX.Add(x);
            hullAUX.Add(y);
        }

        l1 = hullAUX.toArray().ToList();
        l2 = hullAUX.toArray().ToList();
        for (int i = 0; i < l1.Count; i++)
        {
            if (i % 2 == 0)
                xx[i / 2] = l1.ElementAt(i);
            else
                yy[(i - 1) / 2] = l1.ElementAt(i);
        }
        arriba = Punto.getArreglo(xx, yy);
        
        Array.Sort(arriba, ConvexHull.compararPorAbsisa());
        return hull;
    }

    /** @see #computeIndices(float[], int, int, bool, bool) */
    public IntArray computeIndices(FloatArray points, bool sorted, bool yDown)
    {
        return computeIndices(points.items, 0, points.size, sorted, yDown);
    }

    /** @see #computeIndices(float[], int, int, bool, bool) */
    public IntArray computeIndices(float[] polygon, bool sorted, bool yDown)
    {
        return computeIndices(polygon, 0, polygon.Length, sorted, yDown);
    }

    /** Computes a hull the same as {@link #computePolygon(float[], int, int, bool)} but returns indices of the specified points. */
    public IntArray computeIndices(float[] points, int offset, int count, bool sorted, bool yDown)
    {
        int end = offset + count;

        if (!sorted)
        {
            if (sortedPoints == null || sortedPoints.Length < count) sortedPoints = new float[count];
            Array.Copy(points, offset, sortedPoints, 0, count);
            points = sortedPoints;
            offset = 0;
            sortWithIndices(points, count, yDown);
        }

        IntArray indices = this.indices;
        indices.clear();

        FloatArray hull = this.hull;
        hull.clear();

        // Lower hull.
        for (int i = offset, index = i / 2; i < end; i += 2, index++)
        {
            float x = points[i];
            float y = points[i + 1];
            while (hull.size >= 4 && ccw(x, y) <= 0)
            {
                hull.size -= 2;
                indices.size--;
            }
            hull.Add(x);
            hull.Add(y);
            indices.Add(index);
        }

        // Upper hull.
        for (int i = end - 4, index = i / 2, t = hull.size + 2; i >= offset; i -= 2, index--)
        {
            float x = points[i];
            float y = points[i + 1];
            while (hull.size >= t && ccw(x, y) <= 0)
            {
                hull.size -= 2;
                indices.size--;
            }
            hull.Add(x);
            hull.Add(y);
            indices.Add(index);
        }

        // Convert sorted to unsorted indices.
        if (!sorted)
        {
            short[] originalIndicesArray = originalIndices.items;
            int[] indicesArray = indices.items;
            for (int i = 0, n = indices.size; i < n; i++)
                indicesArray[i] = originalIndicesArray[indicesArray[i]];
        }

        return indices;
    }

    /** Returns > 0 if the points are a counterclockwise turn, < 0 if clockwise, and 0 if colinear. */
    private float ccw(float p3x, float p3y)
    {
        FloatArray hull = this.hull;
        int size = hull.size;
        float p1x = hull.get(size - 4);
        float p1y = hull.get(size - 3);
        float p2x = hull.get(size - 2);
        float p2y = hull.peek();
        return (p2x - p1x) * (p3y - p1y) - (p2y - p1y) * (p3x - p1x);
    }

    /** Sorts x,y pairs of values by the x value, then the y value.
     * @param count Number of indices, must be even. */
    private void sort(float[] values, int count)
    {
        int lower = 0;
        int upper = count - 1;
        IntArray stack = quicksortStack;
        stack.Add(lower);
        stack.Add(upper - 1);
        while (stack.size > 0)
        {
            upper = stack.pop();
            lower = stack.pop();
            if (upper <= lower) continue;
            int i = quicksortPartition(values, lower, upper);
            if (i - lower > upper - i)
            {
                stack.Add(lower);
                stack.Add(i - 2);
            }
            stack.Add(i + 2);
            stack.Add(upper);
            if (upper - i >= i - lower)
            {
                stack.Add(lower);
                stack.Add(i - 2);
            }
        }
    }

    private int quicksortPartition(float[] values, int lower, int upper)
    {
        float x = values[lower];
        float y = values[lower + 1];
        int up = upper;
        int down = lower;
        float temp;

        while (down < up)
        {
            while (down < up && values[down] <= x)
                down = down + 2;
            while (values[up] > x || (values[up] == x && values[up + 1] < y))
                up = up - 2;
            if (down < up)
            {
                temp = values[down];
                values[down] = values[up];
                values[up] = temp;

                temp = values[down + 1];
                values[down + 1] = values[up + 1];
                values[up + 1] = temp;
            }
        }
        values[lower] = values[up];
        values[up] = x;

        values[lower + 1] = values[up + 1];
        values[up + 1] = y;

        return up;
    }

    /** Sorts x,y pairs of values by the x value, then the y value and stores unsorted original indices.
     * @param count Number of indices, must be even. */
    private void sortWithIndices(float[] values, int count, bool yDown)
    {
        int pointCount = count / 2;
        originalIndices.clear();
        originalIndices.ensureCapacity(pointCount);
        short[] originalIndicesArray = originalIndices.items;
        for (short i = 0; i < pointCount; i++)
            originalIndicesArray[i] = i;

        int lower = 0;
        int upper = count - 1;
        IntArray stack = quicksortStack;
        stack.Add(lower);
        stack.Add(upper - 1);
        while (stack.size > 0)
        {
            upper = stack.pop();
            lower = stack.pop();
            if (upper <= lower) continue;
            int i = quicksortPartitionWithIndices(values, lower, upper, yDown, originalIndicesArray);
            if (i - lower > upper - i)
            {
                stack.Add(lower);
                stack.Add(i - 2);
            }
            stack.Add(i + 2);
            stack.Add(upper);
            if (upper - i >= i - lower)
            {
                stack.Add(lower);
                stack.Add(i - 2);
            }
        }
    }

    private int quicksortPartitionWithIndices(float[] values, int lower, int upper, bool yDown, short[] originalIndices)
    {
        float x = values[lower];
        float y = values[lower + 1];
        int up = upper;
        int down = lower;
        float temp;
        short tempIndex;
        while (down < up)
        {
            while (down < up && values[down] <= x)
                down = down + 2;
            if (yDown)
            {
                while (values[up] > x || (values[up] == x && values[up + 1] < y))
                    up = up - 2;
            }
            else
            {
                while (values[up] > x || (values[up] == x && values[up + 1] > y))
                    up = up - 2;
            }
            if (down < up)
            {
                temp = values[down];
                values[down] = values[up];
                values[up] = temp;

                temp = values[down + 1];
                values[down + 1] = values[up + 1];
                values[up + 1] = temp;

                tempIndex = originalIndices[down / 2];
                originalIndices[down / 2] = originalIndices[up / 2];
                originalIndices[up / 2] = tempIndex;
            }
        }
        values[lower] = values[up];
        values[up] = x;

        values[lower + 1] = values[up + 1];
        values[up + 1] = y;

        tempIndex = originalIndices[lower / 2];
        originalIndices[lower / 2] = originalIndices[up / 2];
        originalIndices[up / 2] = tempIndex;

        return up;
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
    public static IComparer compararPorAbsisa()
    {
        return (IComparer)new compararPorAbsisaHelper();
    }
}
