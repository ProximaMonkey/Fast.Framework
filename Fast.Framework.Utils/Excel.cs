using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;


namespace Fast.Framework.Utils
{

    /// <summary>
    /// Excel工具类
    /// </summary>
    public static class Excel
    {

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="sheetName">工作表名称</param>
        /// <param name="title">标题</param>
        /// <param name="details">明细</param>
        /// <param name="split">拆分</param>
        public static async Task Write(string fileName, string sheetName, Dictionary<string, string> title, List<Dictionary<string, object>> details, int split = 60000)
        {
            try
            {
                await Task.Run(() =>
                {
                    #region 参数校验
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        throw new ArgumentNullException("文件名称不能为空");
                    }
                    #endregion

                    IWorkbook workbook;//工作簿
                    ISheet sheet;//工作表
                    IRow row;//行
                    ICell cell;//单元格
                    IDataFormat dataformat;//数据格式化
                    ICellStyle cellStyle;//单元格样式
                    int sheetIndex = 1;//工作表索引
                    int rowIndex = 0;//行索引
                    int cellIndex = 0;//单元格索引
                    int detailsIndex = 0;//明细索引
                    int count = details.Count;//计数

                    if (string.IsNullOrWhiteSpace(sheetName))
                    {
                        sheetName = "Sheet";
                    }

                    if (fileName.EndsWith(".xls"))
                    {
                        workbook = new HSSFWorkbook();
                    }
                    else if (fileName.EndsWith(".xlsx"))
                    {
                        workbook = new XSSFWorkbook();
                    }
                    else
                    {
                        fileName += ".xlsx";
                        workbook = new XSSFWorkbook();
                    }
                    while (true)
                    {
                        //创建工作表
                        sheet = workbook.CreateSheet($"{sheetName}{sheetIndex}");

                        //创建数据格式化
                        dataformat = workbook.CreateDataFormat();

                        //创建单元格样式
                        cellStyle = workbook.CreateCellStyle();
                        cellStyle.DataFormat = dataformat.GetFormat("@");
                        cellStyle.Alignment = HorizontalAlignment.Center;//水平对齐
                        cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐

                        row = sheet.CreateRow(rowIndex);
                        //循环写入标题
                        foreach (var key in title.Keys)
                        {
                            sheet.SetColumnWidth(cellIndex, title[key].Length * 5 * 256);
                            cell = row.CreateCell(cellIndex);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(title[key]);
                            cellIndex++;
                        }
                        rowIndex++;
                        for (int i = 0; i < (count <= split ? count : split); i++)
                        {
                            row = sheet.CreateRow(rowIndex);
                            cellIndex = 0;
                            //循环写入明细
                            foreach (var key in title.Keys)
                            {
                                if (details[i].ContainsKey(key))
                                {
                                    var obj = details[detailsIndex][key];
                                    if (obj == null)
                                    {
                                        cellIndex++;
                                    }
                                    else
                                    {
                                        cell = row.CreateCell(cellIndex);
                                        cell.CellStyle = cellStyle;
                                        cell.SetCellValue(obj.GetType().Equals(typeof(DateTime)) ? Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss") : obj.ToString());
                                        cellIndex++;
                                    }
                                }
                                else
                                {
                                    cellIndex++;
                                }
                            }
                            rowIndex++;
                            detailsIndex++;
                        }
                        count -= split;
                        if (count <= 0)
                        {
                            break;
                        }
                        sheetIndex++;
                        rowIndex = 0;
                        cellIndex = 0;
                    }
                    FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    workbook.Write(fs);
                    fs.Close();
                    workbook.Close();
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"写入Excel发生异常:{ex.Message}");
            }
        }
    }
}
