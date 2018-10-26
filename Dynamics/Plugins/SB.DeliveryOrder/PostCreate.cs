using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;

namespace SB.DeliveryOrder
{
    public class PostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var service = serviceProvider.GetOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                var target = (Entity)context.InputParameters["Target"];

                var fromCountreyRef = (EntityReference)target["sb_shipfrom_countryid"];
                var fromCountrey = service.Retrieve("sb_country", fromCountreyRef.Id, new ColumnSet(true));

                var fromCityRef = (EntityReference)target["sb_shipfrom_cityid"];
                var fromCity = service.Retrieve("sb_city", fromCityRef.Id, new ColumnSet(true));

                var fromPostOfficeRef = (EntityReference)target["sb_shipfrom_postofficeid"];
                var fromPostOffice = service.Retrieve("sb_postoffice", fromPostOfficeRef.Id, new ColumnSet(true));

                var toCountreyRef = (EntityReference)target["sb_shipto_countryid"];
                var toCountrey = service.Retrieve("sb_country", toCountreyRef.Id, new ColumnSet(true));

                var toCityRef = (EntityReference)target["sb_shipto_cityid"];
                var toCity = service.Retrieve("sb_city", toCityRef.Id, new ColumnSet(true));

                var toAPCRef = (EntityReference)target["sb_shipto_apcid"];
                var toAPC = service.Retrieve("sb_automatedpostalcenter", toAPCRef.Id, new ColumnSet(true));



                string postOfficeNumber = "";
                if ((int)fromPostOffice["sb_number"] < 9) postOfficeNumber = "0" + (fromPostOffice["sb_number"]).ToString();
                else postOfficeNumber = (fromPostOffice["sb_number"]).ToString();

                string part1 = fromCountrey["sb_idcountry"].ToString() + fromCity["sb_idcity"].ToString() + postOfficeNumber;

                string toAPCNumber = "";
                if ((int)toAPC["sb_number"] < 9) toAPCNumber = "0" + toAPC["sb_number"].ToString();
                else toAPCNumber = toAPC["sb_number"].ToString();

                string part2 = toCountrey["sb_idcountry"].ToString() + toCity["sb_idcity"].ToString() + toAPCNumber;

                tracingService.Trace(part1);
                tracingService.Trace(part2);

                string alias = part1 + "-" + part2;

                
                target["sb_name"] = alias;

                service.Update(target);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }
    }
}
