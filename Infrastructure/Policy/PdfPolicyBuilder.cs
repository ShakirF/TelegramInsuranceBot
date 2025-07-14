using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Policy
{
    public class PdfPolicyBuilder : IPolicyBuilder
    {
        private readonly IPdfGenerator _pdfGenerator;
        private User _user = null!;
        private string _summary = "";
        private string _gptText = "";
        private decimal _price;

        public PdfPolicyBuilder(IPdfGenerator pdfGenerator)
        {
            _pdfGenerator = pdfGenerator;
        }

        public IPolicyBuilder WithUser(User user) { _user = user; return this; }
        public IPolicyBuilder WithSummary(string extracted) { _summary = extracted; return this; }
        public IPolicyBuilder WithGptText(string text) { _gptText = text; return this; }
        public IPolicyBuilder WithPrice(decimal price) { _price = price; return this; }

        public Task<byte[]> BuildPdfAsync() =>
            _pdfGenerator.GeneratePolicyPdf(_user, _summary, _gptText, _price);
    }

}
