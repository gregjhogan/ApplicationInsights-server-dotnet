﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalCalls.aspx.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
// ExternalCalls page to make remote dependency calls
// </summary>
// ----

namespace Aspx45
{
    using System;
    using System.Data;
    using System.Data.SqlClient;    
    using System.Net;
    using System.Runtime.InteropServices;    
    using FW40Shared;
    using FW45Shared;

    /// <summary>
    /// ExternalCalls page to make remote dependency calls
    /// </summary>
    [ComVisible(false)]
    public partial class ExternalCalls : System.Web.UI.Page
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=RDDTestDatabase;Integrated Security=True";

        /// <summary>
        /// ExternalCalls page to make remote dependency calls
        /// </summary>        
        /// <param name="sender">sender object</param>
        /// <param name="e">e object</param>        
        protected void Page_Load(object sender, EventArgs e)
        {
            var type = Request.QueryString["type"];
            var countStr = Request.QueryString["count"];
            var count = 1;
            try
            {
                count = int.Parse(countStr);
            }
            catch (Exception)
            {
                // Dont care about this           
            }  

            try
            {
                switch (type)
                {
                    case "http":
                        HttpHelper40.MakeHttpCallSync(count, "bing");
                        break;
                    case "failedhttp":
                        HttpHelper40.MakeHttpCallSyncFailed(count);
                        break;
                    case "httpasync1":
                        HttpHelper40.MakeHttpCallAsync1(count, "bing");
                        break;
                    case "failedhttpasync1":
                        HttpHelper40.MakeHttpCallAsync1Failed(count);
                        break;
                    case "httpasync2":
                        HttpHelper40.MakeHttpCallAsync2(count, "bing");
                        break;
                    case "failedhttpasync2":
                        HttpHelper40.MakeHttpCallAsync2Failed(count);
                        break;
                    case "httpasync3":
                        HttpHelper40.MakeHttpCallAsync3(count, "bing");
                        break;
                    case "failedhttpasync3":
                        HttpHelper40.MakeHttpCallAsync3Failed(count);
                        break;
                    case "httpasync4":
                        HttpHelper40.MakeHttpCallAsync4(count, "bing");
                        break;
                    case "failedhttpasync4":
                        HttpHelper40.MakeHttpCallAsync4Failed(count);
                        break;
                    case "httpasyncawait1":
                        HttpHelper45.MakeHttpCallAsyncAwait1(count, "bing");
                        break;
                    case "failedhttpasyncawait1":
                        HttpHelper45.MakeHttpCallAsyncAwait1Failed(count);
                        break;
                    case "sql":
                        this.MakeSQLCallSync(count);
                        break;
                    case "azuresdk":
                        HttpHelper40.MakeAzureSdkCalls(count);
                        break;
                    case "ExecuteReaderAsync":
                        this.ExecuteReaderAsync();
                        break;
                    case "ExecuteNonQueryAsync":
                        this.ExecuteNonQueryAsync();
                        break;
                    case "ExecuteScalarAsync":
                        this.ExecuteScalarAsync();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Request Parameter type is not mapped to an action: " + type);
                }

                this.lblResult.Text = "Requested action completed successfully.";
            }
            catch (Exception ex)
            {
                this.lblResult.Text = "The following error occured while attempting to perform requested action" + ex;
            }
        }
        
        /// <summary>
        /// Make sync SQL calls
        /// </summary>        
        /// <param name="count">no of calls to be made</param>        
        private void MakeSQLCallSync(int count)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand("apm.GetDatabases", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader rdr = cmd.ExecuteReader();
            rdr.Close();
        }

        /// <summary>
        /// Performs ExecuteReaderAsync on SQL.
        /// </summary>        
        private void ExecuteReaderAsync()
        {
            SqlCommandHelper.ExecuteReaderAsync(ConnectionString, "SELECT TOP 2 * FROM apm.[Database]");
        }

        /// <summary>
        /// Performs ExecuteReaderAsync on SQL.
        /// </summary>        
        private void ExecuteNonQueryAsync()
        {
            SqlCommandHelper.ExecuteNonQueryAsync(ConnectionString, "SELECT TOP 2 * FROM apm.[Database]");
        }

        /// <summary>
        /// Performs ExecuteScalarAsync on SQL.
        /// </summary>        
        private void ExecuteScalarAsync()
        {
            SqlCommandHelper.ExecuteScalarAsync(ConnectionString, "SELECT count(*) FROM apm.[Database]");
        }
    }
}