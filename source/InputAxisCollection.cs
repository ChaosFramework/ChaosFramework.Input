using ChaosFramework.Collections;
using System.Collections;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public class InputAxisCollection : SysCol.IEnumerable<InputAxis>
    {
        LinkedList<InputAxis> lst = new LinkedList<InputAxis>();

        public InputAxis first => lst.first;
        public float value => lst.first.value;
        public InputAxis this[uint index] => lst[(int)index];

        public void Add(SysCol.IEnumerable<InputAxis> newValues) => lst.Add(newValues);

        public int IndexOf(InputAxis axis)
        {
            int i = 0;
            foreach (InputAxis a in this)
                if (a == axis)
                    return i;
                else
                    i++;

            return -1;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        SysCol.IEnumerator<InputAxis> SysCol.IEnumerable<InputAxis>.GetEnumerator() => GetEnumerator();
        public SysCol.IEnumerator<InputAxis> GetEnumerator() => ((SysCol.IEnumerable<InputAxis>)lst).GetEnumerator();
    }
}
