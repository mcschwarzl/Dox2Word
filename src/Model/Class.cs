﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dox2Word.Model
{
    public class Class
    {
        public string Name { get; set; } = null!;
        public Descriptions Descriptions { get; set; } = null!;
        public List<ClassVariable> Variables { get; } = new();
    }
}
