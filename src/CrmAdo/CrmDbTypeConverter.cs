using System;

namespace CrmAdo
{
    public class CrmDbTypeConverter
    {
        public static object ToDbType(object val)
        {
            if (val == null)
            {
                
                return DBNull.Value;
            }
            else
            {
                return val;
            }
        }

    }
}