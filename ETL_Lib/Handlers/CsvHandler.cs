using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace ETL_Lib.Handlers;
public static class CsvHandler
{
    /// <summary>
    /// Returns a DataTable containing the values of a csv file.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="encoding"></param>
    /// <param name="hasHeader"></param>
    /// <returns></returns>
    public static DataTable LoadTableFromCsv(string path, Encoding encoding, bool hasHeader = true)
    {
        DataTable result = new DataTable();

        //Load result with data from file from path
        string content = FileHandler.ToString(path, encoding);

        string linebreak = content.Contains("\r\n") ? "\r\n" : "\n";

        List<string> csvLines = content.Split(linebreak).ToList();

        //Create Headers
        List<string> headers = csvLines[0].Split(',').ToList();
        int headerCount = 1;

        foreach (string header in headers)
        {
            string headerName = hasHeader ? header : "Column" + headerCount;
            result.Columns.Add(headerName.Replace("\"", ""));
            headerCount++;
        }

        //Remove Header row if applicable
        if (hasHeader)
        {
            csvLines.Remove(csvLines[0]);
        }

        //Add Rows to Datatable
        DataRow workRow;
        List<string> columnList;
        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");


        for (int i = 0; i < csvLines.Count; i++)
        {
            //Split fields using regular expression
            columnList = CSVParser.Split(csvLines[i]).ToList();

            //Create new workrow
            workRow = result.NewRow();

            //Fill in each field to workrow
            for (int j = 0; j < columnList.Count; j++)
            {
                workRow[j] = columnList[j].Replace("\"", "");
            }

            //Add workrow to result Data Table
            result.Rows.Add(workRow);
        }

        return result;
    }

    /// <summary>
    /// Writes a csv file to the path provided that contains the values in the DataTable provided.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="encoding"></param>
    /// <param name="hasHeader"></param>
    public static void WriteTableToCsv(string filePath, DataTable table, Encoding encoding, bool hasHeader = true)
    {
        string fieldDelimeter = ",";
        string encapsulationDelimeter = "\"";

        List<string> fileContent = new List<string>();

        //Add Headers
        var cols = table.Columns.Cast<DataColumn>().Select(x => encapsulationDelimeter + x.ColumnName + encapsulationDelimeter);
        var header = string.Join(fieldDelimeter, cols);
        fileContent.Add(header);

        //Add Row Content
        fileContent.AddRange(table.Rows.Cast<DataRow>().Select(val =>
        {
            return string.Join(
                fieldDelimeter,
                val.ItemArray.Select(v => encapsulationDelimeter + v.ToString().Replace("\"", "\"\"") + encapsulationDelimeter)
            );
        }));

        File.WriteAllLines(filePath, fileContent, encoding);
    }
}
