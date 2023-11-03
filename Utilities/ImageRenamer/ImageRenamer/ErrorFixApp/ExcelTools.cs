using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
namespace ImageToXlsx
{
    public class ExcelTools
    {
         public static ImagePartType GetImagePartTypeByBitmap(Bitmap image)
        {
            if (ImageFormat.Bmp.Equals(image.RawFormat))
                return ImagePartType.Bmp;
            else if (ImageFormat.Gif.Equals(image.RawFormat))
                return ImagePartType.Gif;
            else if (ImageFormat.Png.Equals(image.RawFormat))
                return ImagePartType.Png;
            else if (ImageFormat.Tiff.Equals(image.RawFormat))
                return ImagePartType.Tiff;
            else if (ImageFormat.Icon.Equals(image.RawFormat))
                return ImagePartType.Icon;
            else if (ImageFormat.Jpeg.Equals(image.RawFormat))
                return ImagePartType.Jpeg;
            else if (ImageFormat.Emf.Equals(image.RawFormat))
                return ImagePartType.Emf;
            else if (ImageFormat.Wmf.Equals(image.RawFormat))
                return ImagePartType.Wmf;
            else
                throw new Exception("Image type could not be determined.");
        }

        public static WorksheetPart GetWorksheetPartByName(SpreadsheetDocument document, string sheetName)
        {
            if (document == null) return null;
            if (document.WorkbookPart != null)
            {
                
                    IEnumerable<Sheet> sheets =
                        document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>()
                            .Where(s => s.Name == sheetName);

                    if (!sheets.Any())
                    {
                        // The specified worksheet does not exist
                        return null;
                    }

                    string relationshipId = sheets.First().Id.Value;
                    return (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                
            }
            return null;
        }

        public static SpreadsheetDocument OpenDocument(string excelFile, string sheetName,  out WorkbookPart workBookPart, out WorksheetPart worksheetPart)
        {
            if (File.Exists(excelFile))
            {
                File.Delete(excelFile);  
            }
            SpreadsheetDocument spreadsheetDocument = null;
            worksheetPart = null;
            
            // Create a spreadsheet document by supplying the filepath
            spreadsheetDocument = SpreadsheetDocument.Create(excelFile, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document
            workBookPart = spreadsheetDocument.AddWorkbookPart();
            workBookPart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart
            worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook
            if (spreadsheetDocument.WorkbookPart != null)
            {
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = sheetName
                };
                sheets.Append(sheet);
            }

            return spreadsheetDocument;
        }

        public static void CloseDocument(SpreadsheetDocument document, WorksheetPart worksheetPart)
        {
            worksheetPart?.Worksheet.Save();
            document?.Close();
            document = null;
        }

        public static void AddImage(bool createFile, string excelFile, string sheetName,
                                    string imageFileName, string imgDesc,
                                    int colNumber, int rowNumber)
        {
            using (var imageStream = new FileStream(imageFileName, FileMode.Open))
            {
                AddImage(createFile, excelFile, sheetName, imageStream, imgDesc, colNumber, rowNumber);
            }
        }

        public static void AddImage(WorksheetPart worksheetPart,
                                    string imageFileName, string imgDesc,
                                    int colNumber, int rowNumber)
        {
            using (var imageStream = new FileStream(imageFileName, FileMode.Open))
            {
                AddImage(worksheetPart, imageStream, imgDesc, colNumber, rowNumber);
            }
        }

        public static void AddImage(bool createFile, string excelFile, string sheetName,
                                    Stream imageStream, string imgDesc,
                                    int colNumber, int rowNumber)
        {
            SpreadsheetDocument spreadsheetDocument = null;
            WorksheetPart worksheetPart = null;
            if (createFile && !File.Exists(excelFile))
            {
                // Create a spreadsheet document by supplying the filepath
                spreadsheetDocument = SpreadsheetDocument.Create(excelFile, SpreadsheetDocumentType.Workbook);

                // Add a WorkbookPart to the document
                WorkbookPart workBookPart = spreadsheetDocument.AddWorkbookPart();
                workBookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart
                worksheetPart = workBookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook
                if (spreadsheetDocument.WorkbookPart != null)
                {
                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    // Append a new worksheet and associate it with the workbook
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = sheetName
                    };
                    sheets.Append(sheet);
                }
            }
            else
            {
                // Open spreadsheet
                spreadsheetDocument = SpreadsheetDocument.Open(excelFile, true);

                // Get WorksheetPart
                worksheetPart = GetWorksheetPartByName(spreadsheetDocument, sheetName);

                if (worksheetPart == null)
                {
                    WorkbookPart workBookPart = spreadsheetDocument.WorkbookPart;
                    
                    Sheet sheet = new Sheet()
                    {
                        Name = sheetName
                    };

                    workBookPart.Workbook.Sheets.AddChild(sheet);
                    worksheetPart = GetWorksheetPartByName(spreadsheetDocument, sheetName);
                   
                }
            }

            AddImage(worksheetPart, imageStream, imgDesc, colNumber, rowNumber);

            worksheetPart?.Worksheet.Save();

            spreadsheetDocument.Close();
        }

        

        public static void AddImage(WorksheetPart worksheetPart,
                                    Stream imageStream, string imgDesc,
                                    int colNumber, int rowNumber)
        {
            // We need the image stream more than once, thus we create a memory copy
            MemoryStream imageMemStream = new MemoryStream();
            imageStream.Position = 0;
            imageStream.CopyTo(imageMemStream);
            imageStream.Position = 0;

            var drawingsPart = worksheetPart.DrawingsPart;
            if (drawingsPart == null)
                drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();

            if (!worksheetPart.Worksheet.ChildElements.OfType<Drawing>().Any())
            {
                worksheetPart.Worksheet.Append(new Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) });
            }

            if (drawingsPart.WorksheetDrawing == null)
            {
                drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing();
            }

            var worksheetDrawing = drawingsPart.WorksheetDrawing;

            Bitmap bm = new Bitmap(imageMemStream);
            var imagePart = drawingsPart.AddImagePart(GetImagePartTypeByBitmap(bm));
            imagePart.FeedData(imageStream);
           

            A.Extents extents = new A.Extents(); 
            //var extentsCx = bm.Width * (long)(914400 / bm.HorizontalResolution);
            //var extentsCy = bm.Height * (long)(914400 / bm.VerticalResolution);
            var extentsCx = bm.Width * (long)(228600 / bm.HorizontalResolution);
            var extentsCy = bm.Height * (long)(228600 / bm.VerticalResolution);
            bm.Dispose();

            var colOffset = 0;
            var rowOffset = 0;

            var nvps = worksheetDrawing.Descendants<Xdr.NonVisualDrawingProperties>();
            var nvpId = nvps.Count() > 0
                ? (UInt32Value)worksheetDrawing.Descendants<Xdr.NonVisualDrawingProperties>().Max(p => p.Id.Value) + 1
                : 1U;

            var oneCellAnchor = new Xdr.OneCellAnchor(
                new Xdr.FromMarker
                {
                    ColumnId = new Xdr.ColumnId((colNumber - 1).ToString()),
                    RowId = new Xdr.RowId((rowNumber - 1).ToString()),
                    ColumnOffset = new Xdr.ColumnOffset(colOffset.ToString()),
                    RowOffset = new Xdr.RowOffset(rowOffset.ToString())
                },
                new Xdr.Extent { Cx = extentsCx, Cy = extentsCy },
                new Xdr.Picture(
                    new Xdr.NonVisualPictureProperties(
                        new Xdr.NonVisualDrawingProperties { Id = nvpId, Name = "Picture " + nvpId, Description = imgDesc },
                        new Xdr.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true })
                    ),
                    new Xdr.BlipFill(
                        new A.Blip { Embed = drawingsPart.GetIdOfPart(imagePart), CompressionState = A.BlipCompressionValues.Print },
                        new A.Stretch(new A.FillRectangle())
                    ),
                    new Xdr.ShapeProperties(
                        new A.Transform2D(
                            new A.Offset { X = 0, Y = 0 },
                            new A.Extents { Cx = extentsCx, Cy = extentsCy }
                        ),
                        new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle }
                    )
                ),
                new Xdr.ClientData()
            );

            worksheetDrawing.Append(oneCellAnchor);
        }
        
        public static void InsertText(WorkbookPart workbookPart, WorksheetPart worksheetPart, string text, string colName, uint rowIndex)
        {
                SharedStringTablePart shareStringPart;
                if (workbookPart?.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
                }
                
                // Insert cell A1 into the new worksheet.
                int index = InsertSharedStringItem(text, shareStringPart);
                Cell cell = InsertCellInWorksheet(colName, rowIndex, worksheetPart);

                // Set the value of cell A1.
                cell.CellValue = new CellValue(index.ToString());
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
        }
        
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            //shareStringPart.SharedStringTable.Save();

            return i;
        }
        
        private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart)
        {
            // Add a new worksheet part to the workbook.
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new sheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            string sheetName = "Sheet" + sheetId;

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);
            workbookPart.Workbook.Save();

            return newWorksheetPart;
        }
        
        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (cell.CellReference.Value.Length == cellReference.Length)
                    {
                        if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                        {
                            refCell = cell;
                            break;
                        }
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                //worksheet.Save();
                return newCell;
            }
        }
    }
}