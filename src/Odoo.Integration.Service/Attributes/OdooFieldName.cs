using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odoo.Integration.Service.Attributes
{
    public class OdooFieldName : Attribute
    {
        public string fieldName { get; set; }

        public OdooFieldName(string fieldName)
        {
            this.fieldName = fieldName;
        }

    }
}
