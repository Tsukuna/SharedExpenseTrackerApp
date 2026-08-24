using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharedExpenseTrackerApp.Domain.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsError { get { return !IsSuccess; } }
        private EnumResType Type { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set;  }

        public EnumResType GetEnumType() => Type;

        public static Result<T> Success(T? data, string message = "Success")
        {
            return new Result<T>()
            {
                IsSuccess = true,
                Type = EnumResType.Success,
                Data = data,
                Message = message
            };
        }

        public static Result<T> DeleteSuccess(string message = "Delete success")
        {
            return new Result<T>()
            {
                IsSuccess = true,
                Type = EnumResType.Success,
                Message = message
            };
        }

        public static Result<T> ValidationError(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.ValidationError,
                Data = data,
                Message = message
            };
        }

        public static Result<T> SystemError(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.SystemError,
                Data = data,
                Message = message
            };
        }


        public static Result<T> NotFound(string message, T? data = default)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Type = EnumResType.NotFound,
                Data = data,
                Message = message
            };
        }
    }

    public enum EnumResType
    {
        None,
        Success,
        ValidationError,
        SystemError,
        NotFound
    }
}
