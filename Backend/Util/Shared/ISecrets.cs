using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.timmons.Stitch.Shared
{
    public interface ISecrets
    {
        Dictionary<String, String> Secrets { get; }
    }

    public class DockerSecrets : ISecrets
    {
        public Dictionary<String, String> Secrets
        {
            get
            {
                const String SECRET_PATH = "/run/secrets/";
                Dictionary<String, String> ret = new Dictionary<String, String>();
                if (Directory.Exists(SECRET_PATH))
                {
                    IFileProvider provider = new PhysicalFileProvider(SECRET_PATH);
                    IDirectoryContents files = provider.GetDirectoryContents(".");
                    foreach (var file in files)
                    {
                        try
                        {
                            using (var stream = file.CreateReadStream())
                            using (var streamReader = new StreamReader(stream))
                            {
                                ret[file.Name] = streamReader.ReadToEnd();
                            }
                        }
                        catch (IOException ex)
                        {
                            ret[file.Name] = "Unable to read secret";
                        }
                    }

                }

                return ret;
            }
        }
    }

    public class EnvSecrets : ISecrets
    {
        private IConfiguration config;
        private Dictionary<String, String> secrets;

        public EnvSecrets(IConfiguration conf)
        {

            this.config = conf;
            this.secrets = new Dictionary<String, String>();
            var secrets = config.GetSection("secrets");
            foreach (var key in secrets.GetChildren())
            {
                this.secrets[key.Key] = secrets.GetValue<string>(key.Key);
            }

        }

        public Dictionary<String, String> Secrets => this.secrets;
    }

    public class DotNetSecrets : ISecrets
    {
        private IConfiguration config;
        private Dictionary<String, String> secrets;

        public DotNetSecrets(IConfiguration conf)
        {

            this.config = conf;
            this.secrets = new Dictionary<String, String>();
            var secrets = config.GetSection("secrets");
            foreach (var key in secrets.GetChildren())
            {
                this.secrets[key.Key] = secrets.GetValue<string>(key.Key);
            }

        }

        public Dictionary<String, String> Secrets => this.secrets;
    }
}
