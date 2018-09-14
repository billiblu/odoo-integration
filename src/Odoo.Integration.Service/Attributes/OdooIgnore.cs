using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odoo.Integration.Service.Attributes
{

    public class OdooIgnore : Attribute
    {
        public bool IgnoreProperty { get; set; }

        public OdooIgnore()
        {
            this.IgnoreProperty = true;
        }
    }
}
