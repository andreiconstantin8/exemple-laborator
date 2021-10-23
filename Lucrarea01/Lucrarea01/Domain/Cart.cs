using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea01.Domain
{
    [AsChoice]
    public static partial class Cart
    {
        public interface ICos
        {   }

        public record UnvalidatedCos(IReadOnlyCollection<UnvalidatedProducts> ProduseList, CartDetails CosDetails) : ICos;

        public record InvalidatedCos(IReadOnlyCollection<UnvalidatedProducts> ProduseList, string reason) : ICos;

        public record GolCos(IReadOnlyCollection<UnvalidatedProducts> ProduseList, string reason) : ICos;
        
        public record ValidatedCos(IReadOnlyCollection<ValidatedProducts> ProduseList, CartDetails CosDetails) : ICos;

        public record CosPlatit(IReadOnlyCollection<ValidatedProducts> ProduseList, CartDetails CosDetails, DateTime PublishedDate) : ICos;

    }
}
