using Application.Telegram.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Persistence.DbContext;
using Shared.Utilities;
using System.Text.Json;

namespace Application.Telegram.Handlers
{
    public class ParseOcrCommandHandler : IRequestHandler<ParseOcrCommand, Unit>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ParseOcrCommandHandler> _logger;

        public ParseOcrCommandHandler(AppDbContext context, ILogger<ParseOcrCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(ParseOcrCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _context.Documents.FindAsync(new object[] { request.DocumentId }, cancellationToken);
                if (document == null)
                    throw new Exception($"Document with ID {request.DocumentId} not found.");

                document.OcrRawJson = request.OcrJson;

                var fields = ExtractFieldsFromJson(request.OcrJson, request.DocumentId, document.FileType);

                await _context.ExtractedFields.AddRangeAsync(fields, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("📤 Extracted {Count} fields from OCR for Document ID: {Id}", fields.Count, document.Id);

                return Unit.Value;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OCR JSON");
                throw new ApplicationException("Invalid OCR data format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while parsing OCR result");
                throw;
            }
        }

        private List<ExtractedField> ExtractFieldsFromJson(string json, int documentId, string fileType)
        {
            var list = new List<ExtractedField>();
            var jObject = JObject.Parse(json);

            var fields = jObject["document"]?["inference"]?["prediction"] as JObject;
            if (fields == null) return list;

            foreach (var property in fields.Properties())
            {
                var value = property.Value?["value"]?.ToString();
                var confidenceStr = property.Value?["confidence"]?.ToString();

                if (!string.IsNullOrEmpty(value) && float.TryParse(confidenceStr, out float confidence))
                {
                    var mapped = MindeeFieldMapper.MapField(property.Name, confidence, fileType);
                    if (!string.IsNullOrWhiteSpace(mapped))
                    {
                        list.Add(new ExtractedField
                        {
                            DocumentId = documentId,
                            FieldName = mapped,
                            FieldValue = value,
                            Confidence = confidence
                        });
                    }
                }
            }

            return list;
        }
    }
}
