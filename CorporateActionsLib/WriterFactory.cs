using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateActionsLib
{
    public class WriterFactory
    {
        private WriterFactory() { }

        public static ICorporateActionsWriter GetWriter(string provider)
        {
            switch (provider)
            {
                case "Log":
                    return new LoggingCorporateActionsWriter();

                case "Hugo":
                    return new HugoCorporateActionsWriter();

                default:
                    throw new ApplicationException(String.Format("Provider {0} is not supported by CorporateActionsLib", provider));
            }
        }
    }
}
