using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NTOSDev.Models
{
    public class ProjectPropertiesModel
    {

        [Category("App")]
        public string AppName { get; set; } = "MyApp";
    }
}
