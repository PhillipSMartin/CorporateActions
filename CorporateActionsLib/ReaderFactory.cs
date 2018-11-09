using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateActionsLib
{
    public class ReaderFactory
    {
        private ReaderFactory() { }

        public static ICorporateActionsReader GetReader(string provider)
        {
            switch (provider)
            {
                case "RealTick":
                case "Realtick":
                    return new RealTickCorporateActionsReader();

                case "TWS":
                    return new TWSCorporateActionsReader();

                default:
                    throw new ApplicationException(String.Format("Provider {0} is not supported by ReportListenerLib"));
            }
        }
    }
}
