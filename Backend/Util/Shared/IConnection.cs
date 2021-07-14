
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace com.timmons.Stitch.Shared
{
    public interface IConnection
    {
        DbConnection Connection { get; }
        ConnectionHelper<DbConnection> Wrap();
    }

    public class AppConnection : IConnection, ConnectionHelper<DbConnection>
    {
        private readonly IConfiguration config;

        public AppConnection(IConfiguration secrets)
        {
            this.config = secrets;
        }
        public DbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(getConString());
            }
        }

        public DbConnection GetConnection()
        {
            return new NpgsqlConnection(getConString());
        }

        private string getConString()
        {
            return config.GetValue<string>("connectionString");
        }

        public ConnectionHelper<DbConnection> Wrap()
        {
            return this;
        }
    }

}
