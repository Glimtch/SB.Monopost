using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Extensions;

namespace SB.Parcel
{
    public class PreCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var service = serviceProvider.GetOrganizationService(context.UserId);

            try
            {

                var target = (Entity)context.InputParameters["Target"];
                var orderRef = target.GetAttributeValue<EntityReference>("sb_orderid");

                var CollSet = new ColumnSet("sb_shipto_apcid");
                var order = service.Retrieve("sb_order", orderRef.Id, CollSet);

                var apcRef = (EntityReference)order["sb_shipto_apcid"];
                var apcCollSet = new ColumnSet("sb_apccelltypeid");
                var apc = service.Retrieve("sb_automatedpostalcenter", apcRef.Id, apcCollSet);

                var apcCellRef = (EntityReference)apc["sb_apccelltypeid"];
                var apcCell = service.Retrieve("sb_apccelltype", apcCellRef.Id, new ColumnSet(true));

                string[] param = new string[] { "sb_length", "sb_height", "sb_weight", "sb_width" };

                for (int i = 0; i < param.Length; i++)
                {
                    if ((decimal)target[param[i]] > (decimal)apcCell[param[i]])
                    {
                        Exception exp = new Exception(
                            String.Format("Parcel options are not possible. Cell size : Length {0}, Height {1}, Weight {2}, With {3}", param[0], param[1], param[2], param[3])
                            );
                        throw exp;
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message, e);
            }
        }

    }
}
