using Smart_Medical.KouZiAI.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Smart_Medical.KouZiAI
{
    /// <summary>
    /// 扣子空间智能体服务接口
    /// 
    /// 此接口定义了与扣子AI平台交互的所有核心功能，包括：
    /// 1. 发送消息（支持流式和非流式响应）
    /// 2. 查询聊天记录
    /// 3. 健康检查
    /// 
    /// ABP框架会自动将此接口转换为HTTP API，基础路径为: /api/app/kou-zi-ai/
    /// 所有方法都会自动生成对应的RESTful API端点
    /// 
    /// API认证：需要配置Bearer Token认证
    /// API限流：建议配置合适的限流策略
    /// 错误处理：所有方法都包含完整的异常处理和错误响应
    /// </summary>
    public interface IKouZiAIService : IApplicationService
    {
        /// <summary>
        /// 发送消息给扣子空间智能体（非流式响应）
        /// 
        /// 此方法使用非流式模式与扣子AI进行交互，特点：
        /// - 等待完整响应后一次性返回结果
        /// - 适合需要完整回答的场景
        /// - 响应时间相对较长，但内容完整
        /// - 自动处理轮询等待机制
        /// 
        /// 使用场景：
        /// - 批处理任务
        /// - 需要完整回答的问答场景
        /// - 对实时性要求不高的应用
        /// 
        /// 自动生成的API: POST /api/app/kou-zi-ai/send-message
        /// 
        /// 请求示例：
        /// POST /api/app/kou-zi-ai/send-message
        /// {
        ///   "content": "如何预防流感",
        ///   "userId": "user123",
        ///   "botId": "7524702072735367168",
        ///   "stream": false,
        ///   "autoSaveHistory": true
        /// }
        /// 
        /// 响应示例：
        /// {
        ///   "content": "预防流感的方法包括...",
        ///   "conversationId": "7525814599107788800",
        ///   "success": true,
        ///   "createdAt": "2024-01-01T10:00:00Z"
        /// }
        /// </summary>
        /// <param name="request">请求参数，包含消息内容、用户ID、智能体配置等</param>
        /// <returns>智能体的完整响应结果</returns>
        /// <exception cref="System.ArgumentException">当请求参数无效时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当API调用失败时抛出</exception>
        Task<KouZiAIResponseDto> SendMessageAsync(KouZiAIRequestDto request);
        
        /// <summary>
        /// 发送消息给扣子空间智能体（流式响应）
        /// 
        /// 此方法使用流式模式与扣子AI进行交互，特点：
        /// - 实时流式返回响应片段
        /// - 用户可以看到逐步生成的回答
        /// - 响应速度快，用户体验好
        /// - 返回所有流式事件的完整列表
        /// 
        /// 流式事件类型：
        /// - conversation.message.delta: 增量消息内容
        /// - conversation.message.completed: 消息完成事件
        /// - error: 错误事件
        /// - done: 流式响应结束事件
        /// 
        /// 使用场景：
        /// - 实时聊天应用
        /// - 需要即时反馈的场景
        /// - 提升用户体验的交互式应用
        /// 
        /// 自动生成的API: POST /api/app/kou-zi-ai/send-stream-message
        /// 
        /// 请求示例：
        /// POST /api/app/kou-zi-ai/send-stream-message
        /// {
        ///   "content": "介绍一下人工智能",
        ///   "userId": "user123",
        ///   "stream": true
        /// }
        /// 
        /// 响应示例：
        /// [
        ///   {
        ///     "event": "conversation.message.delta",
        ///     "content": "人工智能",
        ///     "isCompleted": false,
        ///     "timestamp": "2024-01-01T10:00:00Z"
        ///   },
        ///   {
        ///     "event": "conversation.message.delta", 
        ///     "content": "是一门",
        ///     "isCompleted": false,
        ///     "timestamp": "2024-01-01T10:00:01Z"
        ///   }
        /// ]
        /// </summary>
        /// <param name="request">请求参数，自动设置为流式模式</param>
        /// <returns>包含所有流式响应事件的列表</returns>
        /// <exception cref="System.ArgumentException">当请求参数无效时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当流式连接失败时抛出</exception>
        Task<List<KouZiAIStreamResponseDto>> SendStreamMessageAsync(KouZiAIRequestDto request);
        
        /// <summary>
        /// 发送消息给扣子空间智能体（流式响应，返回合并内容）
        /// 
        /// 此方法结合了流式和非流式的优点：
        /// - 内部使用流式获取数据（速度快）
        /// - 自动合并所有流式片段
        /// - 返回完整的合并结果（使用简单）
        /// - 提供最佳的性能和用户体验平衡
        /// 
        /// 工作流程：
        /// 1. 发起流式请求到扣子AI
        /// 2. 接收所有流式响应片段
        /// 3. 智能合并所有内容片段
        /// 4. 清理和格式化最终内容
        /// 5. 返回完整的响应结果
        /// 
        /// 内容合并策略：
        /// - 优先合并conversation.message.delta事件
        /// - 按时间戳排序确保内容顺序
        /// - 清理多余空白字符和换行
        /// - 错误处理和容错机制
        /// 
        /// 使用场景：
        /// - 需要完整内容但要求快速响应
        /// - 后端API调用
        /// - 批量处理场景
        /// 
        /// 自动生成的API: POST /api/app/kou-zi-ai/send-stream-merged
        /// </summary>
        /// <param name="request">请求参数，内部会自动设置为流式模式</param>
        /// <returns>合并后的完整响应内容</returns>
        /// <exception cref="System.ArgumentException">当请求参数无效时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当流式处理失败时抛出</exception>
        Task<KouZiAIResponseDto> SendStreamMergedAsync(KouZiAIRequestDto request);
        
        /// <summary>
        /// 快速发送消息（使用默认配置）
        /// 
        /// 这是一个便捷方法，简化了常见的AI对话场景：
        /// - 使用系统默认的智能体配置
        /// - 自动处理用户ID（如果未提供）
        /// - 使用非流式模式确保稳定性
        /// - 适合快速集成和原型开发
        /// 
        /// 默认配置：
        /// - 智能体ID：从配置文件读取
        /// - 用户ID：123456（如果未提供）
        /// - 流式模式：false（稳定响应）
        /// - 历史记录：true（自动保存）
        /// 
        /// 使用场景：
        /// - 快速原型开发
        /// - 简单的问答功能
        /// - 不需要复杂配置的场景
        /// - 测试和演示
        /// 
        /// 自动生成的API: POST /api/app/kou-zi-ai/quick-send
        /// 
        /// 请求示例：
        /// POST /api/app/kou-zi-ai/quick-send
        /// {
        ///   "content": "你好",
        ///   "userId": "optional_user_id"
        /// }
        /// </summary>
        /// <param name="content">要发送的消息内容，不能为空</param>
        /// <param name="userId">用户ID，可选，默认为"123456"</param>
        /// <returns>智能体的响应结果</returns>
        /// <exception cref="System.ArgumentNullException">当content为空时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当默认配置缺失时抛出</exception>
        Task<KouZiAIResponseDto> QuickSendAsync(string content, string? userId = null);
        
        /// <summary>
        /// 查询聊天记录
        /// 
        /// 此方法用于获取指定聊天会话的详细信息：
        /// - 聊天状态（进行中/已完成/失败/需要操作）
        /// - 完整的消息历史记录
        /// - 时间戳和元数据信息
        /// - 便捷的最后回复内容提取
        /// 
        /// 聊天状态说明：
        /// - in_progress: 智能体正在处理中
        /// - completed: 处理完成，有完整回复
        /// - failed: 处理失败，包含错误信息
        /// - requires_action: 需要用户进一步操作
        /// 
        /// 消息角色类型：
        /// - user: 用户发送的消息
        /// - assistant: 智能体的回复
        /// - system: 系统消息
        /// 
        /// 使用场景：
        /// - 查看历史对话记录
        /// - 检查消息处理状态
        /// - 获取特定会话的详细信息
        /// - 调试和问题排查
        /// 
        /// 自动生成的API: GET /api/app/kou-zi-ai/retrieve-chat
        /// 
        /// 请求示例：
        /// GET /api/app/kou-zi-ai/retrieve-chat?chatId=7525814600055390248&conversationId=7525814599107788800
        /// 
        /// 响应示例：
        /// {
        ///   "chatId": "7525814600055390248",
        ///   "conversationId": "7525814599107788800",
        ///   "status": "completed",
        ///   "messages": [...],
        ///   "lastAssistantMessage": "这是最后的AI回复",
        ///   "success": true
        /// }
        /// </summary>
        /// <param name="request">查询请求参数，包含chatId和conversationId</param>
        /// <returns>包含聊天记录详情的响应对象</returns>
        /// <exception cref="System.ArgumentException">当chatId或conversationId无效时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当查询API调用失败时抛出</exception>
        Task<ChatRetrieveResponseDto> RetrieveChatAsync(ChatRetrieveRequestDto request);
        
        /// <summary>
        /// 提交工具执行结果给扣子空间智能体
        /// 
        /// 此方法是工具调用流程的关键环节，用于将客户端执行工具后的结果提交回AI：
        /// 
        /// 工具调用完整流程：
        /// 1. 用户发送消息 → AI分析需要调用工具
        /// 2. AI返回requires_action状态 → 包含工具调用请求
        /// 3. 客户端执行工具操作 → 获取执行结果
        /// 4. 【此方法】提交工具结果 → AI基于结果继续处理
        /// 5. AI生成最终回复 → 完成整个交互流程
        /// 
        /// 业务场景示例：
        /// 
        /// IoT设备控制：
        /// - AI: "请帮我打开客厅的灯"
        /// - 工具调用: 控制智能灯具API
        /// - 执行结果: "客厅灯已成功打开，亮度设为80%"
        /// - AI继续: "好的，客厅灯已经为您打开了，亮度设置为80%。还需要其他帮助吗？"
        /// 
        /// 文件处理：
        /// - AI: "请生成一个销售报告"
        /// - 工具调用: 生成PDF报告文件
        /// - 执行结果: {"file": "12345", "filename": "sales_report.pdf", "size": 1024000}
        /// - AI继续: "销售报告已生成完成，您可以下载查看。报告包含了本月的详细销售数据。"
        /// 
        /// 外部API集成：
        /// - AI: "查询今天的天气情况"
        /// - 工具调用: 调用天气API
        /// - 执行结果: {"city": "北京", "temperature": "25°C", "weather": "晴朗"}
        /// - AI继续: "今天北京的天气很不错，气温25°C，天气晴朗，适合户外活动。"
        /// 
        /// 数据库操作：
        /// - AI: "查询用户张三的订单信息"
        /// - 工具调用: 查询数据库
        /// - 执行结果: {"total": 5, "orders": [...], "last_order": "2024-01-15"}
        /// - AI继续: "张三总共有5个订单，最近一次下单时间是2024年1月15日..."
        /// 
        /// 技术特性：
        /// - 批量提交：支持一次提交多个工具的执行结果
        /// - 流式支持：可选择流式或非流式的后续响应
        /// - 状态恢复：让AI从中断状态恢复到正常对话流程
        /// - 错误处理：支持工具执行失败的情况处理
        /// - 超时管理：合理的超时设置避免长时间等待
        /// 
        /// 数据安全：
        /// - 结果过滤：自动过滤敏感信息
        /// - 权限验证：验证工具调用的合法性
        /// - 数据校验：验证提交数据的格式和完整性
        /// - 审计日志：记录工具调用和结果提交的完整日志
        /// 
        /// 性能优化：
        /// - 异步处理：支持高并发的工具结果提交
        /// - 连接复用：高效的HTTP连接管理
        /// - 响应压缩：减少网络传输开销
        /// - 缓存策略：合理的缓存提升响应速度
        /// 
        /// 使用场景：
        /// - 智能家居控制系统
        /// - 企业办公自动化
        /// - 数据分析和报告生成
        /// - 第三方服务集成
        /// - 工作流自动化
        /// 
        /// 自动生成的API: POST /api/app/kou-zi-ai/submit-tool-outputs
        /// 
        /// 请求示例：
        /// POST /api/app/kou-zi-ai/submit-tool-outputs
        /// {
        ///   "chatId": "7525814600055390248",
        ///   "conversationId": "7525814599107788800",
        ///   "toolOutputs": [
        ///     {
        ///       "toolCallId": "BUJJF0dAQ0NAEBVeQkVKEV5HFURFXhFCEhFeFxdHShcSQEtFSxY****",
        ///       "output": "设备控制成功，客厅灯已打开"
        ///     }
        ///   ],
        ///   "stream": true
        /// }
        /// 
        /// 响应示例（成功）：
        /// {
        ///   "chatId": "7525814600055390248",
        ///   "conversationId": "7525814599107788800",
        ///   "status": "completed",
        ///   "completedAt": "2024-01-01T10:05:00Z",
        ///   "success": true,
        ///   "createdAt": "2024-01-01T10:05:00Z"
        /// }
        /// 
        /// 响应示例（需要进一步操作）：
        /// {
        ///   "chatId": "7525814600055390248",
        ///   "conversationId": "7525814599107788800", 
        ///   "status": "requires_action",
        ///   "success": true,
        ///   "createdAt": "2024-01-01T10:05:00Z"
        /// }
        /// </summary>
        /// <param name="request">
        /// 工具执行结果提交请求，包含：
        /// - ChatId: 聊天会话标识符（必填）
        /// - ConversationId: 对话上下文标识符（必填）
        /// - ToolOutputs: 工具执行结果列表（必填，至少一个）
        /// - Stream: 是否使用流式响应（可选，默认true）
        /// </param>
        /// <returns>
        /// 工具结果提交响应，包含：
        /// - 会话状态更新信息
        /// - 处理时间戳
        /// - 成功/失败状态
        /// - 错误信息（如果有）
        /// - Token使用统计（如果有）
        /// </returns>
        /// <exception cref="System.ArgumentNullException">当request参数为null时抛出</exception>
        /// <exception cref="System.ArgumentException">当必填字段无效或工具调用ID不存在时抛出</exception>
        /// <exception cref="System.InvalidOperationException">当会话不在requires_action状态或API调用失败时抛出</exception>
        /// <exception cref="System.UnauthorizedAccessException">当没有权限提交该会话的工具结果时抛出</exception>
        /// <exception cref="System.TimeoutException">当工具调用已超时无法接收结果时抛出</exception>
        Task<SubmitToolOutputsResponseDto> SubmitToolOutputsAsync(SubmitToolOutputsRequestDto request);
        
        /// <summary>
        /// 检查服务健康状态
        /// 
        /// 此方法用于监控扣子AI服务的可用性：
        /// - 验证API连接是否正常
        /// - 检查认证配置是否有效
        /// - 测试基本的AI响应功能
        /// - 提供服务状态指标
        /// 
        /// 健康检查流程：
        /// 1. 发送预定义的测试消息
        /// 2. 验证是否能正常获得响应
        /// 3. 检查响应格式和内容
        /// 4. 返回布尔值表示服务状态
        /// 
        /// 使用场景：
        /// - 系统监控和告警
        /// - 负载均衡健康检查
        /// - 服务部署后的验证
        /// - 定期的服务状态检查
        /// 
        /// 监控建议：
        /// - 建议每分钟检查一次
        /// - 连续失败3次后触发告警
        /// - 结合日志分析定位问题
        /// 
        /// 自动生成的API: GET /api/app/kou-zi-ai/health-check
        /// 
        /// 请求示例：
        /// GET /api/app/kou-zi-ai/health-check
        /// 
        /// 响应示例：
        /// true  // 服务正常
        /// false // 服务异常
        /// </summary>
        /// <returns>true表示服务正常，false表示服务异常</returns>
        /// <exception cref="System.Exception">当健康检查过程中发生任何异常时返回false</exception>
        Task<bool> HealthCheckAsync();
    }
}