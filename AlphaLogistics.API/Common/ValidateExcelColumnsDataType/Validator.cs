using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic.FileIO;
using OfficeOpenXml;
using System.Globalization;

namespace WALMS.API.Common.ValidateExcelColumnsDataType
{
	public static class Validator
	{
		private static readonly Dictionary<string, Type> _expectedColumnTypes = new Dictionary<string, Type>
		{
			{ "Trading Name/Retailer Name", typeof(string) },
			{ "Order Number", typeof(string) },
			{ "Recipient Reference", typeof(string) },
			{ "Recipient Name", typeof(string) },
			{ "Timed Service", typeof(string) },
			{ "Address 1", typeof(string) },
			{ "Delivery Instructions", typeof(string) },
			{ "Other Instructions", typeof(string) },
			{ "Home Number", typeof(string) },
			{ "Work Number", typeof(string) },
			{ "Mobile Number", typeof(string) },
			{ "Email Address", typeof(string) },
			{ "CONFIRMED DELIVERY DATE", typeof(string) },
			{ "Town/City", typeof(string) },
			{ "Country", typeof(string) },
			{ "Postcode", typeof(string) },
			{ "QTY", typeof(int) },
			{ "Delivery Charge", typeof(decimal) },
			{ "Weight (KG)", typeof(decimal) },
			{ "Parts", typeof(int) },
			{ "Product Category", typeof(string) },
			{ "Cube", typeof(decimal) },
			//{ "ConsignmentNumber", typeof(string) }
		};

		public static bool ValidateColumnTypes(ExcelWorksheet worksheet, out List<string> errors)
		{
			errors = new List<string>();

			foreach (var column in _expectedColumnTypes)
			{
				string columnName = column.Key;
				Type expectedType = column.Value;

				int columnIndex;
				try
				{
					columnIndex = GetColumnIndex(worksheet, columnName);
				}
				catch (Exception ex)
				{
					errors.Add(ex.Message);
					continue;
				}

				for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // Skip header row
				{
					string cellValue = worksheet.Cells[row, columnIndex].Text;

					if (!IsValidType(cellValue, expectedType))
					{
						errors.Add($"Column '{columnName}' expects {expectedType.Name} but found invalid data at row {row}.");
						break; // Stop further validation for this column
					}
				}
			}

			return !errors.Any();
		}

		private static bool IsValidType(string value, Type expectedType)
		{
			if (string.IsNullOrWhiteSpace(value))
				return true; // Allow empty values

			try
			{
				if (expectedType == typeof(string))
				{
					return true;
				}
				else if (expectedType == typeof(decimal))
				{
					// Handle cases where commas are used as decimal separator
					//cellValue = cellValue.Replace(",", ".");

					// Try to parse the value as decimal
					return decimal.TryParse(value, out _);
				}
				else if (expectedType == typeof(DateTime))
				{
					DateTime.Parse(value);
				}
				else if (expectedType == typeof(int))
				{
					int.Parse(value);
				}
				else if (expectedType == typeof(double))
				{
					double.Parse(value);
				}
				else
				{
					throw new NotSupportedException($"Validation for type {expectedType.Name} is not implemented.");
				}
			}
			catch
			{
				return false;
			}

			return true;
		}
		public static int GetColumnIndex(ExcelWorksheet worksheet, string columnName)
		{
			for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
			{
				if (worksheet.Cells[1, col].Text == columnName)
				{
					return col;
				}
			}
			throw new Exception($"Column {columnName} not found in Excel sheet.");
		}
        public static bool ValidateCSV(IFormFile file, out List<string> errors)
        {
            errors = new List<string>();

            if (file == null || file.Length == 0)
            {
                errors.Add("File is empty or not found.");
                return false;
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csvParser = new TextFieldParser(reader))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                if (csvParser.EndOfData)
                {
                    errors.Add("CSV file must contain at least a header row and one data row.");
                    return false;
                }

                string[] headers = csvParser.ReadFields(); // Read header row

                if (headers == null)
                {
                    errors.Add("CSV file does not contain a valid header.");
                    return false;
                }

                headers = headers.Select(h => h.Trim()).ToArray();

                // Validate headers
                foreach (var expectedColumn in _expectedColumnTypes.Keys)
                {
                    if (!headers.Contains(expectedColumn))
                    {
                        errors.Add($"Missing required column: {expectedColumn}");
                    }
                }

                if (errors.Any())
                    return false;

                int rowIndex = 1;
                while (!csvParser.EndOfData)
                {
                    string[] values = csvParser.ReadFields(); // Read row

                    if (values == null || values.Length != headers.Length)
                    {
                        errors.Add($"Row {rowIndex + 1} has an incorrect number of columns.");
                        continue;
                    }

                    for (int colIndex = 0; colIndex < headers.Length; colIndex++)
                    {
                        string columnName = headers[colIndex];
                        string cellValue = values[colIndex];

                        if (_expectedColumnTypes.ContainsKey(columnName))
                        {
                            Type expectedType = _expectedColumnTypes[columnName];

                            if (!IsValidType(cellValue, expectedType))
                            {
                                errors.Add($"Column '{columnName}' expects {expectedType.Name} but found invalid data at row {rowIndex + 1}.");
                            }
                        }
                    }

                    rowIndex++;
                }
            }

            return !errors.Any();
        }
    }
}
