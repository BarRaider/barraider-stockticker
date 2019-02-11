using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarRaider.StockTicker
{
    public class StockManager
    {
        private StockComm connection;
        private static StockManager instance = null;
        private static readonly object objLock = new object();

        #region Constructors

        public static StockManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new StockManager();
                    }
                    return instance;
                }
            }
        }

        private StockManager()
        {
            connection = new StockComm();
        }

        #endregion

        public async Task<SymbolData> GetSymbol(string stockSymbol)
        {
            return await connection.GetSymbol(stockSymbol);
        }

    }
}
