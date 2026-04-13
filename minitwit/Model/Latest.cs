using System;
using System.Collections.Generic;

namespace minitwit.Model;

public partial class Latest
{
    public int Id {get; set; } // The value should be 1, since this will be used as a singleton.

    public int Value {get; set; }
}
