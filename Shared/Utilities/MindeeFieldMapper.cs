namespace Shared.Utilities
{
    public static class MindeeFieldMapper
    {
        private const float MinConfidenceThreshold = 0.85f;

        private static readonly Dictionary<string, Dictionary<string, string>> _mappingsByType = new()
        {
            {
               "passport", new Dictionary<string, string>
               {
                   { "passport_number_card_no", "Passport Number" },
                   { "date_of_birth", "Date of Birth" },
                   { "date_of_expire", "Expiry Date" },
                   { "name", "Name" },
                   { "surname", "Surname" },
                   { "nationality", "Nationality" },
                   { "sex", "Sex" },
                   { "personal_no", "Personal No" }
               }
            },
            { 
                "car_registration", new Dictionary<string, string>
                {
                    { "registration_number", "Car Plate" },
                    { "vin", "VIN Code" },
                    { "make", "Make" },
                    { "yearofmanufacture", "Year of Manufacture" },
                    { "body_type", "Body Type" },
                    { "color", "Color" },
                    { "ownername", "Owner Name" },
                    { "fuel_type", "Fuel Type" },
                    { "date_of_issue", "Date of Issue" },
                    { "country_of_issue", "Country of Issue" },
                    { "legal_document_number", "Document No" },
                    { "ptsnumber", "PTS Number" },
                    { "category", "Category" }
                }
            }
        };

        public static string? MapField(string rawField, float confidence, string fileType)
        {
            if (confidence < MinConfidenceThreshold)
                return null;

            if (_mappingsByType.TryGetValue(fileType.ToLower(), out var typeMap))
            {
                return typeMap.TryGetValue(rawField, out var mapped) ? mapped : null;
            }

            return null;
        }
    }
}
