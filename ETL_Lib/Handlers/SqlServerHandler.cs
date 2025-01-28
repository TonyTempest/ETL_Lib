using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ETL_Lib.Handlers
{
    public class SQLServerHandler
    {
        private string connectionString = string.Empty;

        public SQLServerHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataTable RunQuery(string queryString)
        {
            DataTable result = new DataTable();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(queryString, connection);
            connection.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(result);
            connection.Close();
            da.Dispose();
            return result;
        }

        public async Task<DataTable> RunQueryAsync(string queryString, int timeOutInSeconds)
        {
            DataTable dataTable = new DataTable();
            SqlConnection selectConnection = new SqlConnection(connectionString);
            selectConnection.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(queryString, selectConnection);
            sqlDataAdapter.SelectCommand.CommandTimeout = timeOutInSeconds;
            sqlDataAdapter.Fill(dataTable);
            selectConnection.Close();
            selectConnection.Dispose();
            sqlDataAdapter.Dispose();
            return dataTable;
        }

        public void BulkInsertQueries(IList<string> queries,int batchSize)
        {
            StringBuilder queryBatch = new StringBuilder();
            for (int i=0; i < queries.Count; i++)
            {
                if (i % batchSize != 0 || i == 0)
                {
                    queryBatch.AppendLine(queries[i]);
                }
                else
                {
                    RunQuery(queryBatch.ToString());
                    queryBatch.Clear();
                }
            }
            if (queryBatch.Length > 0)
            {
                RunQuery(queryBatch.ToString());
            }
        }
    }
}
