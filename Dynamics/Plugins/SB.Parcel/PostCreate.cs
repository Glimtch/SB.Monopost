using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;

namespace SB.Parcel
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
                var orderRef = target.GetAttributeValue<EntityReference>("sb_orderid");

                var order = service.Retrieve("sb_order", orderRef.Id, new ColumnSet(true));
                

                decimal cell_cost;
                cell_cost = (decimal)target["sb_length"] * (decimal)target["sb_height"] * (decimal)target["sb_weight"] * (decimal)target["sb_width"] * (decimal)0.25;

                var cityPostOffice = (EntityReference)order["sb_shipfrom_cityid"];
                var cityAPC = (EntityReference)order["sb_shipto_cityid"];
                if (cityAPC.Id != cityPostOffice.Id) cell_cost = cell_cost * 3;


                var countryPostOffice = (EntityReference)order["sb_shipfrom_countryid"];
                var countryAPC = (EntityReference)order["sb_shipto_countryid"];
                if (countryPostOffice.Id != countryPostOffice.Id) cell_cost = cell_cost * 3;

                Money cost = new Money(cell_cost);

                Random rand = new Random();
                string id = rand.Next(0, 10000).ToString();
                
                while (id.Length != 4)
                {
                    id = "0" + id;
                }

                

                string number = (string)order["sb_name"] + "-" + id;

                Guid passGuid = Guid.NewGuid();

                string pass = (passGuid.ToString()).Substring(0, 8);

                var delivery = new Entity("sb_delivery")
                {
                    ["sb_name"] = number,
                    ["sb_password"] = pass,
                    ["sb_orderid"] = orderRef,
                    ["sb_parcelid"] = new EntityReference("sb_parcel", target.Id),
                    ["sb_deliverycost"] = cost
                };
                

                service.Create(delivery);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }
    }
}
