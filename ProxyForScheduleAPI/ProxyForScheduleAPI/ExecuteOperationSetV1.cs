using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using ProxyForScheduleAPI.Helpers;
using System;
using System.Activities;
using System.Linq;
using static ProxyForScheduleAPI.Helpers.LoggerDelegate;

namespace ProxyForScheduleAPI
{
   public class ExecuteOperationSetV1 : CodeActivity
   {
      [Input("OperationSetId"), ArgumentRequired]
      public InArgument<string> OperationSetId { get; set; }

      private const string COperationSetResponseKey = "OperationSetResponse";

      [Output(COperationSetResponseKey)]
      public OutArgument<string> OperationSetResponse { get; set; }

      // "Proxy Solution: Proxy UserId"
      private const string CEnvVarProxyUser = "promx_ProxySolutionProxyUserId";

      protected override void Execute(CodeActivityContext executionContext)
      {
         var context = executionContext.GetExtension<IWorkflowContext>();

         var factory = executionContext.GetExtension<IOrganizationServiceFactory>();
         var serviceMain = factory.CreateOrganizationService(context.UserId);
         var tracer = executionContext.GetExtension<ITracingService>();
         var logger = GetLogger(tracer);

         var operationSetId = OperationSetId.Get<string>(executionContext);
         logger($"Action was triggered with parameter OperationSetId = '{operationSetId}'");

         var proxyUserRef = GetUserToImpersonate(serviceMain, logger);
         var service = factory.CreateOrganizationService(proxyUserRef.Id);
         logger($"Created impersonated service object.");

         var response = CallExecuteOperationSetAction(service, operationSetId);
         logger($"Impersonated action call (msdyn_ExecuteOperationSetV1) was successful!");

         var operationSetResponse = response.Results
            .Where(x => x.Key == COperationSetResponseKey)
            .Select(x => x.Value as string)
            .FirstOrDefault();
         logger($"{COperationSetResponseKey} = '{operationSetResponse}'.");

         OperationSetResponse.Set(executionContext, operationSetResponse);
         logger("Done.");
      }

      private DxLogger GetLogger(ITracingService tracer)
         => (message) => tracer.Trace(message);

      private string GetProxyUserId(IOrganizationService service, DxLogger logger) 
         => new EnvVarHelper(service, logger).GetValueByName(CEnvVarProxyUser);

      private EntityReference GetUserToImpersonate(IOrganizationService service, DxLogger logger)
      {
         var proxyUserIdStr = GetProxyUserId(service, logger);
         logger($"Value of '{CEnvVarProxyUser}' environment variable = '{proxyUserIdStr}'");

         if (false == Guid.TryParse(proxyUserIdStr, out var proxyUserId))
            throw new Exception($"Failed to parse '{proxyUserIdStr}' as GUID.");

         var proxyUser = service.Retrieve("systemuser", proxyUserId, new ColumnSet("fullname"));
         var fullName = proxyUser.GetAttributeValue<string>("fullname");
         logger($"Found user to impersonate: '{fullName}'.");

         return proxyUser.ToEntityReference();
      }

      static private OrganizationResponse CallExecuteOperationSetAction(
         IOrganizationService service, 
         string operationSetId)
      {
         var operationSetRequest = new OrganizationRequest("msdyn_ExecuteOperationSetV1");
         operationSetRequest["OperationSetId"] = operationSetId;
         return service.Execute(operationSetRequest);
      }
   }
}
