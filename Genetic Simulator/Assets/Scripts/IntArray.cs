using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Text;


public class IntArray
{
    public int[] items;
    public int size;
    public bool ordered;

    /** @param ordered If false, methods that remove elements may change the order of other elements in the array, which avoids a
     *           memory copy.
     * @param capacity Any elements Added beyond this will cause the backing array to be grown. */
    public IntArray(bool ordered, int capacity)
    {
        this.ordered = ordered;
        items = new int[capacity];
    }

    /** Creates an ordered array with a capacity of 16. */
    public IntArray() : this(true, 16)
    {
    }

    /** Creates an ordered array with the specified capacity. */
    public IntArray(int capacity) : this(true, capacity)
    {
    }



    /** Creates a new array containing the elements in the specific array. The new array will be ordered if the specific array is
     * ordered. The capacity is set to the number of elements, so any subsequent elements Added will cause the backing array to be
     * grown. */
    public IntArray(IntArray array)
    {
        this.ordered = array.ordered;
        size = array.size;
        items = new int[size];
        Array.Copy(array.items, 0, items, 0, size);
    }

    /** Creates a new ordered array containing the elements in the specified array. The capacity is set to the number of elements,
     * so any subsequent elements Added will cause the backing array to be grown. */
    public IntArray(int[] array) : this(true, array, 0, array.Length)
    {
    }

    /** Creates a new array containing the elements in the specified array. The capacity is set to the number of elements, so any
     * subsequent elements Added will cause the backing array to be grown.
     * @param ordered If false, methods that remove elements may change the order of other elements in the array, which avoids a
     *           memory copy. */
    public IntArray(bool ordered, int[] array, int startIndex, int count) : this(ordered, count)
    {
        size = count;
        Array.Copy(array, startIndex, items, 0, count);
    }

    public void Add(int value)
    {
        int[] items = this.items;
        if (size == items.Length) items = resize(Math.Max(8, (int)(size * 1.75f)));
        items[size++] = value;
    }

    public void Add(int value1, int value2)
    {
        int[] items = this.items;
        if (size + 1 >= items.Length) items = resize(Math.Max(8, (int)(size * 1.75f)));
        items[size] = value1;
        items[size + 1] = value2;
        size += 2;
    }

    public void Add(int value1, int value2, int value3)
    {
        int[] items = this.items;
        if (size + 2 >= items.Length) items = resize(Math.Max(8, (int)(size * 1.75f)));
        items[size] = value1;
        items[size + 1] = value2;
        items[size + 2] = value3;
        size += 3;
    }

    public void Add(int value1, int value2, int value3, int value4)
    {
        int[] items = this.items;
        if (size + 3 >= items.Length) items = resize(Math.Max(8, (int)(size * 1.8f))); // 1.75 isn't enough when size=5.
        items[size] = value1;
        items[size + 1] = value2;
        items[size + 2] = value3;
        items[size + 3] = value4;
        size += 4;
    }

    public void AddAll(IntArray array)
    {
        AddAll(array, 0, array.size);
    }

    public void AddAll(IntArray array, int offset, int Length)
    {
        if (offset + Length > array.size)
            throw new Exception("offset + Length must be <= size: " + offset + " + " + Length + " <= " + array.size);
        AddAll(array.items, offset, Length);
    }



    public void AddAll(int[] array, int offset, int Length)
    {
        int[] items = this.items;
        int sizeNeeded = size + Length;
        if (sizeNeeded > items.Length) items = resize(Math.Max(8, (int)(sizeNeeded * 1.75f)));
        Array.Copy(array, offset, items, size, Length);
        size += Length;
    }

    public int get(int index)
    {
        if (index >= size) throw new IndexOutOfRangeException("index can't be >= size: " + index + " >= " + size);
        return items[index];
    }

    public void set(int index, int value)
    {
        if (index >= size) throw new IndexOutOfRangeException("index can't be >= size: " + index + " >= " + size);
        items[index] = value;
    }

    public void incr(int index, int value)
    {
        if (index >= size) throw new IndexOutOfRangeException("index can't be >= size: " + index + " >= " + size);
        items[index] += value;
    }

    public void mul(int index, int value)
    {
        if (index >= size) throw new IndexOutOfRangeException("index can't be >= size: " + index + " >= " + size);
        items[index] *= value;
    }

    public void insert(int index, int value)
    {
        if (index > size) throw new IndexOutOfRangeException("index can't be > size: " + index + " > " + size);
        int[] items = this.items;
        if (size == items.Length) items = resize(Math.Max(8, (int)(size * 1.75f)));
        if (ordered)
            Array.Copy(items, index, items, index + 1, size - index);
        else
            items[size] = items[index];
        size++;
        items[index] = value;
    }

    public void swap(int first, int second)
    {
        if (first >= size) throw new IndexOutOfRangeException("first can't be >= size: " + first + " >= " + size);
        if (second >= size) throw new IndexOutOfRangeException("second can't be >= size: " + second + " >= " + size);
        int[] items = this.items;
        int firstValue = items[first];
        items[first] = items[second];
        items[second] = firstValue;
    }

    public bool contains(int value)
    {
        int i = size - 1;
        int[] items = this.items;
        while (i >= 0)
            if (items[i--] == value) return true;
        return false;
    }

    public int indexOf(int value)
    {
        int[] items = this.items;
        for (int i = 0, n = size; i < n; i++)
            if (items[i] == value) return i;
        return -1;
    }

    public int lastIndexOf(int value)
    {
        int[] items = this.items;
        for (int i = size - 1; i >= 0; i--)
            if (items[i] == value) return i;
        return -1;
    }

    public bool removeValue(int value)
    {
        int[] items = this.items;
        for (int i = 0, n = size; i < n; i++)
        {
            if (items[i] == value)
            {
                removeIndex(i);
                return true;
            }
        }
        return false;
    }

    /** Removes and returns the item at the specified index. */
    public int removeIndex(int index)
    {
        if (index >= size) throw new IndexOutOfRangeException("index can't be >= size: " + index + " >= " + size);
        int[] items = this.items;
        int value = items[index];
        size--;
        if (ordered)
            Array.Copy(items, index + 1, items, index, size - index);
        else
            items[index] = items[size];
        return value;
    }

    /** Removes the items between the specified indices, inclusive. */
    public void removeRange(int start, int end)
    {
        if (end >= size) throw new IndexOutOfRangeException("end can't be >= size: " + end + " >= " + size);
        if (start > end) throw new IndexOutOfRangeException("start can't be > end: " + start + " > " + end);
        int[] items = this.items;
        int count = end - start + 1;
        if (ordered)
            Array.Copy(items, start + count, items, start, size - (start + count));
        else
        {
            int lastIndex = this.size - 1;
            for (int i = 0; i < count; i++)
                items[start + i] = items[lastIndex - i];
        }
        size -= count;
    }

    /** Removes from this array all of elements contained in the specified array.
     * @return true if this array was modified. */
    public bool removeAll(IntArray array)
    {
        int size = this.size;
        int startSize = size;
        int[] items = this.items;
        for (int i = 0, n = array.size; i < n; i++)
        {
            int item = array.get(i);
            for (int ii = 0; ii < size; ii++)
            {
                if (item == items[ii])
                {
                    removeIndex(ii);
                    size--;
                    break;
                }
            }
        }
        return size != startSize;
    }

    /** Removes and returns the last item. */
    public int pop()
    {
        return items[--size];
    }

    /** Returns the last item. */
    public int peek()
    {
        return items[size - 1];
    }

    /** Returns the first item. */
    public int first()
    {
        if (size == 0) throw new AccessViolationException("Array is empty.");
        return items[0];
    }

    public void clear()
    {
        size = 0;
    }

    /** Reduces the size of the backing array to the size of the actual items. This is useful to release memory when many items
     * have been removed, or if it is known that more items will not be Added.
     * @return {@link #items} */
    public int[] shrink()
    {
        if (items.Length != size) resize(size);
        return items;
    }

    /** Increases the size of the backing array to accommodate the specified number of Additional items. Useful before Adding many
     * items to avoid multiple backing array resizes.
     * @return {@link #items} */
    public int[] ensureCapacity(int AdditionalCapacity)
    {
        int sizeNeeded = size + AdditionalCapacity;
        if (sizeNeeded > items.Length) resize(Math.Max(8, sizeNeeded));
        return items;
    }

    /** Sets the array size, leaving any values beyond the current size undefined.
     * @return {@link #items} */
    public int[] setSize(int newSize)
    {
        if (newSize > items.Length) resize(Math.Max(8, newSize));
        size = newSize;
        return items;
    }

    protected int[] resize(int newSize)
    {
        int[] newItems = new int[newSize];
        int[] items = this.items;
        Array.Copy(items, 0, newItems, 0, Math.Min(size, newItems.Length));
        this.items = newItems;
        return newItems;
    }

    public void sort()
    {
        Array.Sort(items, 0, size);
    }

    public void reverse()
    {
        int[] items = this.items;
        for (int i = 0, lastIndex = size - 1, n = size / 2; i < n; i++)
        {
            int ii = lastIndex - i;
            int temp = items[i];
            items[i] = items[ii];
            items[ii] = temp;
        }
    }

    public void shuffle()
    {
        int[] items = this.items;
        Random r = new Random();

        for (int i = size - 1; i >= 0; i--)
        {
            int ii = r.Next(i);
            int temp = items[i];
            items[i] = items[ii];
            items[ii] = temp;
        }
    }

    /** Reduces the size of the array to the specified size. If the array is already smaller than the specified size, no action is
     * taken. */
    public void truncate(int newSize)
    {
        if (size > newSize) size = newSize;
    }

    /** Returns a random item from the array, or zero if the array is empty. */
    public int random()
    {
        Random r = new Random();
        if (size == 0) return 0;
        return items[r.Next(0, size - 1)];
    }

    public int[] toArray()
    {
        int[] array = new int[size];
        Array.Copy(items, 0, array, 0, size);
        return array;
    }

    public int hashCode()
    {
        if (!ordered) return base.GetHashCode();
        int[] items = this.items;
        int h = 1;
        for (int i = 0, n = size; i < n; i++)
            h = h * 31 + items[i];
        return h;
    }

    public bool equals(System.Object objecto)
    {
        if (objecto.Equals(this)) return true;
        if (!ordered) return false;
        if (!(objecto is IntArray)) return false;
        IntArray array = (IntArray)objecto;
        if (!array.ordered) return false;
        int n = size;
        if (n != array.size) return false;


        for (int i = 0; i < n; i++)
            if (items[i] != array.items[i]) return false;
        return true;
    }

    public override String ToString()
    {
        if (size == 0) return "[]";
        int[] items = this.items;
        StringBuilder buffer = new StringBuilder(32);
        buffer.Append('[');
        buffer.Append(items[0]);
        for (int i = 1; i < size; i++)
        {
            buffer.Append(", ");
            buffer.Append(items[i]);
        }
        buffer.Append(']');
        return buffer.ToString();
    }

    public String ToString(String separator)
    {
        if (size == 0) return "";
        int[] items = this.items;
        StringBuilder buffer = new StringBuilder(32);
        buffer.Append(items[0]);
        for (int i = 1; i < size; i++)
        {
            buffer.Append(separator);
            buffer.Append(items[i]);
        }
        return buffer.ToString();
    }

    /** @see #IntArray(int[]) */
    static public IntArray with(IntArray array)
    {
        return new IntArray(array);
    }
}
