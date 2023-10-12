using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Gs.Domain.Repository
{
    public class MyResult : MyResult<object>
    {

    }
    /// <summary>
    /// customer return result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MyResult<T> where T : class
    {
        protected const int SUCCESS_STASTUS = 200;
        protected const int SYSTEM_ERROR_STATUS = 500;
        protected const int CUSTOMER_ERROR_STATUS = 501;
        public static string _msg = "sucessful!";
        private T defaultData;
        private string _message;

        [Description("e.g. 200:success; 500:system error; 404:not found; 401:Unauthorized ")]
        public int Code { get; set; } = SUCCESS_STASTUS;
        [JsonIgnore]
        public bool Success
        {
            get
            {
                return Code == SUCCESS_STASTUS;
            }
        }
        [Description("response extend data")]
        public T Data
        {
            get
            {
                if (!Success)
                {
                    defaultData = default(T);
                }
                return defaultData;
            }
            set
            {
                defaultData = value;
            }
        }
        public string Message
        {
            get
            {
                if (string.IsNullOrEmpty(_message))
                {
                    return _msg;
                }
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        [Description("record's count")]
        public int? PageCount { get; set; }
        [Description("record count")]
        public int? RecordCount { get; set; }
        public void SetDefaultMessage(string message)
        {
            _msg = message;
        }
        public MyResult()
        {

        }
        public MyResult(T data)
        {
            defaultData = data;
        }
        public MyResult(int status, string message)
        {
            Code = status;
            _message = message;
        }
        public MyResult(Enum status, string message = "")
        {
            SetStatus(status, message);
        }
        public MyResult(Exception exception, bool showStackTrace = false)
        {
            SetStatus(exception, showStackTrace);
        }
        public MyResult(int status, string message, Exception ex = null)
        {
            Code = status;
            if (ex != null)
            {
                Error(ex, false);
            }
            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
            }
        }
        public virtual MyResult<T> SetError(string errorMessage)
        {
            Code = 501;
            _message = errorMessage;
            return this;
        }
        public virtual MyResult<T> Error(Exception exception, bool showStackTrace = false)
        {
            if (exception == null) throw new NullReferenceException("exception canot be null");
            Data = null;
            if (Code == 200) Code = 500;
            StringBuilder stringBuilder = new StringBuilder();
            if (exception != null)
            {
                stringBuilder.Insert(0, $"{exception.GetType().Name}:{exception.Message}-{Environment.NewLine}");
                if (showStackTrace)
                {
                    stringBuilder.Append(exception.StackTrace);
                }
            }
            Message = stringBuilder.ToString();
            return this;
        }
        public virtual MyResult<T> SetStatus(Exception exception, bool showStackTrace = false)
        {
            Code = 500;
            StringBuilder stringBuilder = new StringBuilder();
            if (exception != null)
            {
                stringBuilder.Insert(0, $"{exception.GetType().Name}:{exception.Message}-{Environment.NewLine}");
                if (showStackTrace)
                {
                    stringBuilder.Append(exception.StackTrace);
                }
            }
            Message = stringBuilder.ToString();
            return this;
        }
        public virtual MyResult<T> SetStatus(Enum status, string message = "")
        {
            Code = Convert.ToInt32(status);
            if (string.IsNullOrEmpty(message))
            {
                _message = GetEnumToString(status);
            }
            else
            {
                _message = message;
            }
            return this;
        }

        private string GetEnumToString(Enum _enum)
        {
            if (_enum == null)
            {
                return string.Empty;
            }
            Type type = _enum.GetType();
            string name = Enum.GetName(type, _enum);
            if (!string.IsNullOrEmpty(name))
            {
                DescriptionAttribute customAttribute = (type.GetField(name)).GetCustomAttribute<DescriptionAttribute>();
                if (customAttribute != null)
                {
                    return customAttribute.Description;
                }
                return name;
            }
            List<FieldInfo> list = (type.GetFields()).Where(t => t.FieldType == type).ToList();
            List<string> description = new List<string>();
            list.ForEach((FieldInfo t) =>
            {
                if (_enum.HasFlag((Enum)t.GetValue(_enum)))
                {
                    DescriptionAttribute customAttribute2 = t.GetCustomAttribute<DescriptionAttribute>();
                    if (customAttribute2 != null && !string.IsNullOrEmpty(customAttribute2.Description))
                    {
                        description.Add(customAttribute2.Description);
                    }
                    else
                    {
                        description.Add(t.Name);
                    }
                }
            });
            return string.Join(",", description);
        }
    }
}