using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odoo.Integration.Service.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]


    public class OdooModelName : Attribute
    {
        public string modelName { get; set; }
        
        public OdooModelName(string modelName)
        {
            this.modelName = modelName;
        }
    }
}
