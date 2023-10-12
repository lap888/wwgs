using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Gs.Application.Utils
{
    /// <summary>
    /// 微信Xml
    /// </summary>
    public class WeXmlDoc : XmlDocument
    {
        public WeXmlDoc()
        {
            XmlElement rootElement = this.CreateElement("xml");
            this.AppendChild(rootElement);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            string strVal = null;
            if (value == null)
            {
                strVal = null;
            }
            XmlNode root = this.SelectSingleNode("xml");
            XmlElement node = this.CreateElement(key);
            if (value is string)
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                {
                    XmlCDataSection cdata = this.CreateCDataSection((string)value);
                    node.AppendChild(cdata);
                    root.AppendChild(node);
                }
                return;
            }
            if (value is Nullable<Int32>)
            {
                strVal = (value as Nullable<Int32>).Value.ToString();
            }
            if (value is Nullable<Int64>)
            {
                strVal = (value as Nullable<Int64>).Value.ToString();
            }
            if (!String.IsNullOrWhiteSpace(strVal))
            {
                node.InnerText = strVal;
                root.AppendChild(node);
            }
        }
        /// <summary>
        /// 转成字符串
        /// </summary>
        /// <returns></returns>
        public string ToXmlStr()
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            this.Save(writer);
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return xmlString;
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public string GetSign(string key)
        {
            XmlNode rootXml = this.SelectSingleNode("xml");
            XmlNodeList xmlList = rootXml.ChildNodes;
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            foreach (XmlNode item in xmlList)
            {
                dic.Add(item.Name, item.InnerText);
            }
            StringBuilder signStr = new StringBuilder();
            dic.Aggregate(signStr, (s, i) => s.Append($"{i.Key}={i.Value.ToString()}&"));
            signStr.Append("key=");
            signStr.Append(key);
            return Security.MD5(signStr.ToString());
        }
    }
}
