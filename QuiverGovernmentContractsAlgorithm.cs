/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using QuantConnect.Data;
using QuantConnect.Util;
using QuantConnect.Orders;
using QuantConnect.Algorithm;
using QuantConnect.DataSource;

namespace QuantConnect.DataLibrary.Tests
{
    /// <summary>
    /// Quiver Quantitative is a provider of alternative data.
    /// </summary>
    public class QuiverGovernmentContractsAlgorithm : QCAlgorithm
    {
        private Symbol _customDataSymbol;
        private Symbol _equitySymbol;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2020, 10, 7);  //Set Start Date
            SetEndDate(2020, 10, 11);    //Set End Date
            _equitySymbol = AddEquity("AAPL").Symbol;
            _customDataSymbol = AddData<QuiverGovernmentContracts>(_equitySymbol).Symbol;
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="slice">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice slice)
        {
            var data = slice.Get<QuiverGovernmentContracts>();
            if (!data.IsNullOrEmpty())
            {
                foreach (var govContracts in data.Values)
                {
                    Log($"{Time} {govContracts.ToString()}");
                    foreach (QuiverGovernmentContract govContract in govContracts)
                    {
                        var amount = govContract.Amount;
                        // based on the custom data property we will buy or short the underlying equity
                        if (amount >= 5m)
                        {
                            SetHoldings(_equitySymbol, 1);
                        }
                        else if (amount <= 5m)
                        {
                            SetHoldings(_equitySymbol, -1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Order fill event handler. On an order fill update the resulting information is passed to this method.
        /// </summary>
        /// <param name="orderEvent">Order event details containing details of the events</param>
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Filled)
            {
                Debug($"Purchased Stock: {orderEvent.Symbol}");
            }
        }
    }
}
