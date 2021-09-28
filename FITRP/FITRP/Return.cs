using System;
using System.Collections.Generic;
using System.Text;

namespace FITRP
{
    class Return : Exception
    {
        public readonly Object value;

        public Return(Object value)
        {
            this.value = value;
        }
    }
}
