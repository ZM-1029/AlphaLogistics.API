using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using WALMS.API.DTO.Inventory;
using WALMS.API.Models;
using Windows.System;

namespace WALMS.API.Common
{
    public class warehouseUtility
    {
        //public string GetBlock(List<WareHouseArea> wareHouseAreas)
        //{
        //    string Block = "";
        //    foreach (var item in wareHouseAreas)
        //    {
        //        Block = Block + "," +item.Name;

        //    }
        //    var temp = Block.Remove(0, 1);
        //    return temp;
        //}
        public long GetRows(List<GridWareHouseAreaDTO> wareHouseAreas)
        {
            long TotalRow = 0;
            foreach (var item in wareHouseAreas)
            {
                TotalRow += item.Rows ?? 0;
            }
            return TotalRow;
        }

        public long GetBays(List<GridWareHouseAreaDTO> wareHouseAreas)
        {
            long TotalBay = 0;
            foreach (var item in wareHouseAreas)
            {
                TotalBay += item.Rows * item.Bays ?? 0;
            }
            return TotalBay;
        }
        public string GetRFId(string Area, long Rows, long Bays, long Levels, string Position)
        {
            string RFId = "";
            RFId = Area + "-" + Rows + "-" + Bays + "-" + Levels + "-" + Position;
            return RFId;
        }

        public IEnumerable<AreaExcelDTO> ReadExcelFile(MemoryStream fileStream)
        {
            List<AreaExcelDTO> areas = new List<AreaExcelDTO>();
            using (ExcelPackage package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var headerRow = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.Start.Row, worksheet.Dimension.End.Column]
                 .Select(cell => cell.Text)
                 .ToList();

                var rows = new List<Dictionary<string, object>>();


                for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    // Check if the row is completely empty
                    bool isRowEmpty = true;
                    for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                    {
                        if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                        {
                            isRowEmpty = false;
                            break;
                        }
                    }

                    if (isRowEmpty)
                        continue;
                    try
                    {
                        //var blockValue = worksheet.Cells[row, 2].Text;

                        //if (long.TryParse(blockValue, out _))
                        //{
                        //    throw new Exception($"Invalid Block at row {row}: Block cannot be only numeric.");
                        //}

                        var area = new AreaExcelDTO()
                        {
                            WareHouseCode = worksheet.Cells[row, 1].Text,
                            // Address = worksheet.Cells[row, 2].Text,
                            Block = worksheet.Cells[row, 2].Text,
                            Rows = long.TryParse(worksheet.Cells[row, 3].Text, out long raws) ? raws : 0,
                            Bays = long.TryParse(worksheet.Cells[row, 4].Text, out long age) ? age : 0,
                            Levels = long.TryParse(worksheet.Cells[row, 5].Text, out long levels) ? levels : 0,
                            Position = worksheet.Cells[row, 6].Text,
                            CBM = int.Parse(worksheet.Cells[row, 7].Text)
                            //LocationId = worksheet.Cells[row, 8].Text,
                        };
                        areas.Add(area);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error Processing row {row}:{ex.Message}");
                    }
                }

                return areas;
            }
        }
        public IEnumerable<AreaExcelDTO> ReadCsvFile(Stream fileStream)
        {
            try
            {
                List<AreaExcelDTO> areas = new List<AreaExcelDTO>();
                using (var reader = new StreamReader(fileStream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<AreaExcelDTO>();
                   areas = records.ToList();
                    return areas;
                }
            }
            catch (HeaderValidationException ex)
            {
                // Specific exception for header issues
                throw new ApplicationException("CSV file header is invalid.", ex);
            }
            catch (TypeConverterException ex)
            {
                // Specific exception for type conversion issues
                throw new ApplicationException("CSV file contains invalid data format.");
            }
            catch (Exception ex)
            {
                // General exception for other issues
                throw new ApplicationException("Error reading CSV file", ex);
            }
        }

        //public bool isWareHouseExist(int wareHouseId, List<WareHouseArea> wareHousesAreas)
        //{
        //    for (int i = 0; i < wareHousesAreas.Count(); i++)
        //    {
        //        if (wareHousesAreas[i].WareHouseId != wareHouseId)
        //        {
        //            return false;
        //        }
        //    };
        //    return true;
        //}
        public bool isValid(List<AreaExcelDTO> dTOs)
        {
            //var msg = "";
            //List<string> properties = new List<string>();
            //int count = 0;

            var data = dTOs.ToList();
            if(data.Count()<=0)
            {
                return false;
            }
            for (int i = 0; i < data.Count(); i++)
            {
                if (data[i].WareHouseCode == "")
                {
                    //properties.Add("WareHouseCode");
                    //count++;
                    return false;
                }

               /* if (data[i].Address == "")
                {
                    //properties.Add("Address");
                    //count++;
                    return false;
                }*/

                if (data[i].Block == "")
                {
                    //properties.Add("Area");
                    //count++;
                    return false;
                }

                if (data[i].Rows == 0)
                {
                    //properties.Add("Rows");
                    //count++;
                    return false;
                }

                if (data[i].Bays == 0)
                {
                    //properties.Add("Bays");
                    //count++;
                    return false;
                }

                if (data[i].Levels == 0)
                {
                    //properties.Add("Levels");
                    //count++;
                    return false;
                }

                if (data[i].Position == "")
                {
                    //properties.Add("Position");
                    //count++;
                    return false;
                }
                
            }
            return true;
            
        }
    }
}
