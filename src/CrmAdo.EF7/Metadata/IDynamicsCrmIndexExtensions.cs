﻿using Microsoft.Data.Entity.Relational.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Data.Entity.DynamicsCrm.Metadata
{

    public interface IDynamicsCrmIndexExtensions : IRelationalIndexExtensions
    {        
        bool? IsClustered { get; }
    }

}