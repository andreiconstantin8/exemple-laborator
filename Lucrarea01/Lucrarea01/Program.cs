using System;
using Lucrarea01.Domain;
using System.Collections.Generic;
using static Lucrarea01.Domain.Cart;

namespace Lucrarea01
{
    class Program
    {

        static void Main(string[] args)
        {

            string answer = ReadValue("Start shopping . Press Y/N: ");
            if (answer.Contains("Y"))
            {
                var listOfProduct = ReadProduse().ToArray();
                var cartDetails = ReadDetails();

                UnvalidatedCos unvalidatedCos = new(listOfProduct, cartDetails);

                ICos result = CheckCos(unvalidatedCos);
                result.Match(
                    whenUnvalidatedCos: unvalidatedCos => unvalidatedCos,
                    whenGolCos: invalidResult => invalidResult,
                    whenInvalidatedCos: invalidResult => invalidResult,
                    whenValidatedCos: validatedCos => CosPlatit(validatedCos, cartDetails,DateTime.Now),
                    whenCosPlatit: cosPlatit => cosPlatit
                );

                Console.WriteLine(result);

            }
            else Console.WriteLine("You pressed N . Exit!");

        }
        private static ICos CheckCos(UnvalidatedCos unvalidatedCos) =>
           ( (unvalidatedCos.ProduseList.Count == 0) ? new GolCos(new List<UnvalidatedProducts>(), "empty cart")
                : ((string.IsNullOrEmpty(unvalidatedCos.CosDetails.PaymentAddress.Value))? new InvalidatedCos(new List<UnvalidatedProducts>(), "Cos Invalid")
                      :( (unvalidatedCos.CosDetails.PaymentState.Value == 0) ? new ValidatedCos(new List<ValidatedProducts>(), unvalidatedCos.CosDetails)
                             :new CosPlatit(new List<ValidatedProducts>(), unvalidatedCos.CosDetails, DateTime.Now))));
        
        private static ICos CosPlatit(ValidatedCos validatedResult, CartDetails cosDetails, DateTime PublishedDate) =>
                new CosPlatit(new List<ValidatedProducts>(), cosDetails, DateTime.Now);

        private static List<UnvalidatedProducts> ReadProduse()
        {
            List<UnvalidatedProducts> listOfProduse = new();
            object answer = null;
            do
            {
                answer = ReadValue("Add product .Press Y/N: ");

                if (answer.Equals("Y"))
                {
                    var ProdusID = ReadValue("ProductID: ");
                    if (string.IsNullOrEmpty(ProdusID))
                    {
                        break;
                    }

                    var ProdusCantitate = ReadValue("ProductQty: ");
                    if (string.IsNullOrEmpty(ProdusCantitate))
                    {
                        break;
                    }
                    UnvalidatedProducts toAdd = new(ProdusID, ProdusCantitate);
                    listOfProduse.Add(toAdd);
                }

            } while (!answer.Equals("N"));
            
            return listOfProduse;
        }

        public static CartDetails ReadDetails()
        {
            PaymentState paymentState;
            PaymentAddress paymentAddress;
            CartDetails cosDetails;

            string answer = ReadValue("Finish Press Y/N: ");

            if (answer.Contains("Y"))
            {

                var Address = ReadValue("Adress: ");
                if (string.IsNullOrEmpty(Address))
                {
                    paymentAddress = new PaymentAddress("NONE");
                }
                else
                {
                    paymentAddress = new PaymentAddress(Address);
                }
                var payment = ReadValue("Pay? Press Y/N: ");
                if (payment.Contains("Y"))
                {
                    paymentState = new PaymentState(1);
                }
                else
                {
                    paymentState = new PaymentState(0);
                }
            }
            else
            {
                paymentAddress = new PaymentAddress("NONE");
                paymentState = new PaymentState(0);
            }
            cosDetails = new CartDetails(paymentAddress, paymentState);
            return cosDetails;
         }

        private static string? ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

    }
}
