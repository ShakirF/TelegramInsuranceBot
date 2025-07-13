using Application.Telegram.Commands;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Persistence.DbContext;
using Shared.Utilities;
using System.Text.Json;

namespace Application.Telegram.Handlers
{
    public class ParseOcrCommandHandler : IRequestHandler<ParseOcrCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ParseOcrCommandHandler> _logger;

        public ParseOcrCommandHandler(IUnitOfWork unitOfWork, ILogger<ParseOcrCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(ParseOcrCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _unitOfWork.Documents.Query()
                 .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
                if (document == null)
                    throw new Exception($"Document with ID {request.DocumentId} not found.");

                document.OcrRawJson = request.OcrJson;

                var fields = ExtractFieldsFromJson(request.OcrJson, request.DocumentId, document.FileType);

                foreach (var field in fields)
                {
                    await _unitOfWork.ExtractedFields.AddAsync(field, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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

                if (!string.IsNullOrEmpty(value) )
                {
                    var mapped = MindeeFieldMapper.MapField(property.Name, fileType);
                    if (!string.IsNullOrWhiteSpace(mapped))
                    {
                        list.Add(new ExtractedField
                        {
                            DocumentId = documentId,
                            FieldName = mapped,
                            FieldValue = value,
                            Confidence = 0.0f // Default confidence if not provided
                        });
                    }
                }
            }

            return list;
        }
    }
}
