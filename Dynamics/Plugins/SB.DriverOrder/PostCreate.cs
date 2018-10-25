using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SB.DriverOrder
{
    public class PostCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IExecutionContext>();
            var service = serviceProvider.GetOrganizationService(context.UserId);

            try
            {
                var target = (Entity)context.InputParameters["Target"];
                var multiRequest = new ExecuteMultipleRequest()
                {
                    Requests = new OrganizationRequestCollection(),
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = false
                    }
                };

                foreach(var delivery in GetTopPriorDeliveries(service, target))
                {
                    delivery["sb_driverorderid"] = new EntityReference("sb_driverorder", target.Id);

                    multiRequest.Requests.Add(new UpdateRequest() { Target = delivery });
                }

                service.Execute(multiRequest);
            }
            catch(Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        static IEnumerable<Entity> GetTopPriorDeliveries(IOrganizationService service, Entity driverOrder)
        {
            var fromPostOfficeId = driverOrder.GetAttributeValue<EntityReference>("sb_shipfrom_postoffice").Id;
            var toCityId = driverOrder.GetAttributeValue<EntityReference>("sb_shipto_cityid").Id;

            var ordersQuery = new QueryExpression("sb_order")
            {
                ColumnSet = new ColumnSet("sb_orderid", "sb_shipfrom_postofficeid", "sb_shipto_cityid", "sb_orderstatuscode")
            };
            ordersQuery.Criteria.AddCondition("sb_shipfrom_postofficeid", ConditionOperator.Equal, fromPostOfficeId);
            ordersQuery.Criteria.AddCondition("sb_shipto_cityid", ConditionOperator.Equal, toCityId);
            ordersQuery.Criteria.AddCondition("sb_orderstatuscode", ConditionOperator.Equal, 110000001);

            var orders = service.RetrieveMultiple(ordersQuery).Entities;
            var deliveries = new List<Entity>();
            foreach(var order in orders)
            {
                var deliveryQuery = new QueryExpression("sb_delivery")
                {
                    ColumnSet = new ColumnSet(false)
                };
                deliveryQuery.Criteria.AddCondition("sb_orderid", ConditionOperator.Equal, order.Id);
                deliveryQuery.Criteria.AddCondition("sb_deliverystatuscode", ConditionOperator.Equal, 110000001);

                Task.Run(() =>
                {
                    deliveries.AddRange(service.RetrieveMultiple(deliveryQuery).Entities);
                });
            }

            var vehicleId = driverOrder.GetAttributeValue<EntityReference>("sb_deliveryvehicleid").Id;
            var vehicle = service.Retrieve("sb_deliveryvehicle", vehicleId, new ColumnSet("sb_capacity"));
            return deliveries.OrderBy(d => d.GetAttributeValue<DateTime>("sb_datecreated")).Take(vehicle.GetAttributeValue<int>("sb_capacity"));
        }
    }
}
