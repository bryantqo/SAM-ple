using com.timmons.Stitch.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace API.Controllers.api
{
    [Route("api/App/[action]")]
    [ApiController]
    [Authorize]
    public class AppController : ControllerBase
    {
        private readonly IConnection con;
        private readonly IAppConfigMapper mapper;
        private readonly IHostApplicationLifetime applicationLifetime;
        public AppController(IConnection con, IAppConfigMapper mapper, IHostApplicationLifetime appLifetime)
        {
            this.con = con;
            this.mapper = mapper;
            this.applicationLifetime = appLifetime;
        }

        [ActionName("Reload")]
        public double Reload()
        {
            applicationLifetime.StopApplication();
            return (new Random()).NextDouble();
        }

        private static List<string> allowedConfigs = new List<string>
            {
                "Project:Status:Map",
                "Project:Type:Map",
                "Project:Subtype:Map",
                "Project:ChecklistType:Map",
                "Project:Region:Map",
                "Property:Type:Map",
                "Property:County:Map",
                "Property:AccessType:Map",
                "Instrument:Category:Map",
                "Instrument:DeedType:Map",
                "Instrument:Status:Map"
            };

        [ActionName("GetConfig")]
        public object GetConfig(string configname)
        {
            if(configname == "All")
            {
                Dictionary<string, object> ret = new Dictionary<string, object>();
                foreach (var itm in allowedConfigs)
                    ret[itm] = GetConfig(itm);
                return ret;
            }

            try
            { 

                if (allowedConfigs.Contains(configname))
                return mapper.Get(configname);
            } catch
            {
                //IGnore if the config doesnt exist and return an empty config
            }

            return new { };
        }

        [ActionName("GetVersion")]
        public string getVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
