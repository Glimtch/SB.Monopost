using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;


namespace SB.City
{
    public class PostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var service = serviceProvider.GetOrganizationService(context.UserId);

            try
            {
                var target = (Entity)context.InputParameters["Target"];

                var query = new QueryExpression("sb_city")
                {
                    ColumnSet = new ColumnSet(true),
                };

                query.Criteria.AddCondition("sb_countryid", ConditionOperator.Equal, ((EntityReference)target["sb_countryid"]).Id);

                var cities = service.RetrieveMultiple(query).Entities;


                string s = "";

                if (cities.Count() < 10) s = "0" + cities.Count();
                else s = cities.Count().ToString();

                target["sb_idcity"] = s;

                service.Update(target);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }
    }
}
