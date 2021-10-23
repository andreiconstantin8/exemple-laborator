using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain
{
    public record Price
    {
        public decimal Value { get; }

        public Price(decimal value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidPriceException($"{value:0.##} is an invalid amount value.");
            }
        }

        public static Price operator *(Price a, Price b) => new Price((a.Value * b.Value));

        public Price Round()
        {
            var roundedValue = Math.Round(Value);
            return new Price(roundedValue);
        }

        public override string ToString()
        {
            return $"{Value:0.##}";
        }

        public static bool TryParsePrice(string amountString, out Amount amount)
        {
            bool isValid = false;
            amount = null;
            if (decimal.TryParse(amountString, out decimal numericAmount))
            {
                if (IsValid(numericAmount))
                {
                    isValid = true;
                    amount = new(numericAmount);
                }
            }
            return isValid;
        }

        private static bool IsValid(decimal numericAmount) => numericAmount > 0 && numericAmount <= 5000;
    }
}
