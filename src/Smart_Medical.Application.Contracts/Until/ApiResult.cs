using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Until
{
    public enum ResultCode
    {
        Success = 200,
        Error = 500,
        NotFound = 300,
        ValidationError = 400,
        AlreadyExists = 409
    }
    public class ApiResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuc { get; private set; }

        /// <summary>
        /// 接口请求结果的状态码
        /// </summary>
        public ResultCode Code { get; private set; }

        /// <summary>
        /// 返回的信息
        /// </summary>
        public string Msg { get; private set; }

        /// <summary>
        /// 私有构造函数，防止直接实例化
        /// </summary>
        /// <param name="isSuc"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        protected internal ApiResult(bool isSuc, ResultCode code, string msg)
        {
            IsSuc = isSuc;
            Code = code;
            Msg = msg;
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="reason">失败原因</param>
        /// <param name="code">错误码</param>
        /// <returns>ApiResult</returns>
        public static ApiResult Fail(string reason, ResultCode code)
        {
            return new ApiResult(false, code, reason);
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="code">状态码</param>
        /// <returns>ApiResult</returns>
        public static ApiResult Success( ResultCode code)
        {
            return new ApiResult(true, code, "操作成功");
        }
    }

    /// <summary>
    /// 封装返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 数据
        /// </summary>
        public T? Data { get; private set; }

        // 私有构造函数，防止直接实例化
        private ApiResult(bool isSuc, ResultCode code, string msg, T? data)
            : base(isSuc, code, msg)
        {
            Data = data;
        }

        /// <summary>
        /// 创建带数据的成功结果
        /// </summary>
        /// <param name="data">返回的数据</param>
        /// <param name="code"></param>
        /// <returns>ApiResult</returns>
        public static ApiResult<T> Success(T data, ResultCode code)
        {
            return new ApiResult<T>(true, code, "操作成功", data);
        }

        /// <summary>
        /// 创建带数据的失败结果
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public new static ApiResult<T> Fail(string reason, ResultCode code)
        {
            return new ApiResult<T>(false, code, reason, default);
        }
    }
}
