using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;


namespace SB.Country
{
    public class PostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var service = serviceProvider.GetOrganizationService(context.UserId);
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                var query = new QueryExpression("sb_country")
                {
                    ColumnSet = new ColumnSet(true),
                };

                var countries = service.RetrieveMultiple(query).Entities;

                var target = (Entity)context.InputParameters["Target"];

                string s = "";

                if (countries.Count() < 10) s = "0" + countries.Count();
                else s = countries.Count().ToString();
                
                target["sb_idcountry"] = s;

                service.Update(target);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }
    }
}
