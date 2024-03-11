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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Universe Selection helper class for QuiverQuant Government Contracts dataset
    /// </summary>
    public class QuiverGovernmentContractUniverse : BaseDataCollection
    {
        private static readonly TimeSpan Period = TimeSpan.FromDays(1);

        /// <summary>
        ///     Contract description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        ///     Awarding Agency Name
        /// </summary>
        public string Agency { get; set; }

        /// <summary>
        ///     Total dollars obligated under the given contract
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Time the data became available
        /// </summary>
        public override DateTime EndTime => Time + Period;

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "quiver",
                    "governmentcontracts",
                    "universe",
                    $"{date.ToStringInvariant(DateFormat.EightCharacter)}.csv"
                ),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.FoldingCollection
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(','); 

            var amount = csv[4].IfNotNullOrEmpty<decimal?>(s => decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture));

            return new QuiverGovernmentContractUniverse
            {
                Description = csv[2],
                Agency = csv[3],
                Amount = amount,

                Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                Time = date,
                Value = amount ?? 0
            };
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Converts the instance to string
        /// </summary>
        public override string ToString()
        {
            var niceString = Data.OfType<QuiverGovernmentContractUniverse>().Select(data => $"({Symbol}) :: " +
                $"Description: {data.Description} " +
                $"Agency: {data.Agency} " +
                $"Amount: {data.Amount}");
            return $"{Time}: [{string.Join(",", niceString)}]";
        }

        /// <summary>
        /// Clones the data
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override BaseData Clone()
        {
            return new QuiverGovernmentContractUniverse
            {
                Symbol = Symbol,
                Time = Time,
                Data = Data,
                Value = Value,

                Description = Description,
                Agency = Agency,
                Amount = Amount,
            };
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }
    }
}