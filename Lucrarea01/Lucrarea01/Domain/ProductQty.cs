using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lucrarea01.Domain
{
    public record  ProductQty
    {
        private static readonly Regex ValidPattern = new("^LM[0-9]{5}$");
        public string Value { get; }

        private ProductQty(string value)
        {
            if (ValidPattern.IsMatch(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidProdusCantitate("WRONG INPUT");
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
