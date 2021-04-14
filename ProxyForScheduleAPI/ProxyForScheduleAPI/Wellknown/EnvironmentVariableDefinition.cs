namespace ProxyForScheduleAPI.Wellknown
{
   public class EnvironmentVariableDefinition
   {
      public const string LogicalName  = "environmentvariabledefinition";
      public const string Id           = "environmentvariabledefinitionid";

      public const string DisplayName  = "displayname";
      public const string SchemaName   = "schemaname";   // string
      public const string DefaultValue = "defaultvalue"; // string 

      public const string Type         = "type";         // optionset
      public class OS_Type
      {
         public const int String = 100_000_000;
      }
   }
}
