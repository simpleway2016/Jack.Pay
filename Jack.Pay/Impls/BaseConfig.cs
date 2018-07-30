using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Xml.XPath;
namespace Jack.Pay.Impls
{
    class BaseConfig
    {
        public BaseConfig(string xmlOrJson)
        {
            if (xmlOrJson.StartsWith("{"))
            {
                //json格式
                var jsonObj = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(xmlOrJson);
                var property = (Newtonsoft.Json.Linq.JProperty)jsonObj.First;
                var typeInfo = this.GetType().GetTypeInfo();
                while (property != null)
                {
                    var field = typeInfo.GetField(property.Name);
                    if (field != null)
                    {
                        field.SetValue(this, property.Value.ToString());
                    }
                    property = (Newtonsoft.Json.Linq.JProperty)property.Next;
                }
            }
            else
            {
                XDocument xmldoc = XDocument.Parse(xmlOrJson);
                var fields = this.GetType().GetTypeInfo().GetFields();
                foreach (var field in fields)
                {
                    var element = xmldoc.Root.XPathSelectElement(field.Name);
                    if (element != null)
                    {
                        field.SetValue(this, element.Value);
                    }
                }
            }
        }
    }
}
