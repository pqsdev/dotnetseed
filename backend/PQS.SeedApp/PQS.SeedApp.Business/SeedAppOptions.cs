using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PQS.SeedApp.Business
{
    public class SeedAppOptions
    {
        /// <summary>
        /// String de conexion a la base de datos
        /// </summary>
        public string ConnectionString { get; set; }
        public SeedAppMessagingOptions Messaging { get; set; }
    }

    public class SeedAppMessagingOptions : Confluent.Kafka.ClientConfig
    {
        public string MessagePrefix { get; set; }

        public IDictionary<string, string> ExtraProperties { get { return this.properties; }  set { this.properties = value; } }
    }
}
