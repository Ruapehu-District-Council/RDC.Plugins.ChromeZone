using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDC.Plugins.ChromeZone.Core.Objects;

namespace RDC.Plugins.ChromeZone.Core
{
    public static class Logic
    {
        public static string GetAttributeField(string baseString)
        {
            var fields = GetAttributeFields(baseString);

            if (fields.Count > 0)
            {
                return fields[0];
            }

            return string.Empty;
        }

        public static List<int> GetAttributeIDs(string baseString)
        {
            var attributeIDs = new List<int>();
            try
            {
                var RecordIDParts = baseString.Split(new[] { "**" }, StringSplitOptions.None);

                for (int i = 0; RecordIDParts.Length > i; i++)
                {
                    i++;
                    var attrID = RecordIDParts[i];
                    if (int.TryParse(attrID, out int result))
                    {
                        attributeIDs.Add(result);
                    }

                    i++;
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return attributeIDs;
        }

        public static List<string> GetAttributeFields(string baseString)
        {
            var Fields = new List<string>();
            try
            {
                var RecordFieldParts = baseString.Split(new[] { "**" }, StringSplitOptions.None);

                for (int i = 0; RecordFieldParts.Length > i; i++)
                {
                    i++;
                    var attrName = RecordFieldParts[i];
                    Fields.Add(attrName);
                    i++;
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return Fields;
        }

        public static string HandleFieldConversion(string fieldValue,  Interfaces.IFieldConversion conversion)
        {
            try
            {
                switch (conversion.Type)
                {
                    case "Date":
                        return DateTime.FromOADate(int.Parse(fieldValue)).AddDays(24837).ToString(conversion.Format);
                    case "Replace":
                        return fieldValue.Replace(conversion.OldValue, conversion.NewValue);
                }
            }
            catch
            {

            }

            return fieldValue;
        }

        public static bool HandleMatchFieldRules(string FieldValue, string Operator, string Value)
        {
            try
            {
                switch (Operator)
                {
                    case "=":
                        if (FieldValue == Value)
                        {
                            return true;
                        }

                        break;
                    case "!=":
                        if (FieldValue != Value)
                        {
                            return true;
                        }

                        break;
                    case ">":
                        if (int.Parse(FieldValue) > int.Parse(Value))
                        {
                            return true;
                        }

                        break;
                    case ">=":
                        if (int.Parse(FieldValue) >= int.Parse(Value))
                        {
                            return true;
                        }

                        break;
                    case "<":
                        if (int.Parse(FieldValue) < int.Parse(Value))
                        {
                            return true;
                        }

                        break;
                    case "<=":
                        if (int.Parse(FieldValue) <= int.Parse(Value))
                        {
                            return true;
                        }

                        break;
                }
            }
            catch
            {

            }

            return false;
        }


    }
}
