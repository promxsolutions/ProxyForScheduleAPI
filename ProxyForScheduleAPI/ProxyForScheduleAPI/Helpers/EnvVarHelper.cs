using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ProxyForScheduleAPI.Wellknown;
using System.Linq;
using static ProxyForScheduleAPI.Helpers.LoggerDelegate;

namespace ProxyForScheduleAPI.Helpers
{
   public class EnvVarHelper
   {
      private IOrganizationService service = null;
      private DxLogger logger = null;

      public EnvVarHelper(IOrganizationService service, DxLogger externalLogger)
      {
         this.service = service;
         this.logger = externalLogger.GetNestedLogger(this.GetType().Name);
      }


      public string GetValueByName(string name)
      {
         logger($"GetValueByName({name}):");

         var query = new QueryExpression(EnvironmentVariableDefinition.LogicalName);
         query.ColumnSet = new ColumnSet(
            EnvironmentVariableDefinition.DefaultValue,
            EnvironmentVariableDefinition.SchemaName,
            EnvironmentVariableDefinition.DisplayName,
            EnvironmentVariableDefinition.Id);
         query.Criteria.AddCondition(EnvironmentVariableDefinition.SchemaName, ConditionOperator.Equal, name);

         var linkToValues = query.AddLink(
            EnvironmentVariableValue.LogicalName,
            EnvironmentVariableDefinition.Id,
            EnvironmentVariableValue.EnvironmentVariableDefinitionId,
            JoinOperator.LeftOuter);
         linkToValues.Columns = new ColumnSet(EnvironmentVariableValue.Value);

         const string alias = "link";
         linkToValues.EntityAlias = alias;

         var collection = service.RetrieveMultiple(query);
         var parsed = collection.Entities
            .Select(entity => new
            {
               value = entity.GetAttributeValue<AliasedValue>($"{alias}.{EnvironmentVariableValue.Value}")?.Value as string,
               defaultValue = entity.GetAttributeValue<string>(EnvironmentVariableDefinition.DefaultValue),
               schemaName = entity.GetAttributeValue<string>(EnvironmentVariableDefinition.SchemaName),
               displayName = entity.GetAttributeValue<string>(EnvironmentVariableDefinition.DisplayName)
            })
            .ToList();
         var first = parsed.FirstOrDefault();

         logger($"defaultValue = '{first.defaultValue}', value = '{first.value}'");

         return first?.value ?? first?.defaultValue;
      }

   }
}
