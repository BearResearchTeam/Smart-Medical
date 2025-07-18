using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.KouZiAI.Dtos
{
    /// <summary>
    /// 查询聊天记录请求参数DTO
    /// 
    /// 此DTO用于封装查询特定聊天会话详细信息的请求参数：
    /// 
    /// 主要功能：
    /// - 通过聊天ID和会话ID精确定位目标会话
    /// - 获取完整的对话历史和状态信息
    /// - 支持会话状态检查和问题排查
    /// - 提供会话恢复和上下文重建的数据基础
    /// 
    /// 使用场景：
    /// - 查看历史对话记录
    /// - 检查AI处理状态（进行中/完成/失败）
    /// - 会话状态监控和告警
    /// - 问题排查和调试分析
    /// - 会话数据导出和备份
    /// - 上下文恢复和会话继续
    /// 
    /// 请求特点：
    /// - GET请求方式，符合RESTful API规范
    /// - 参数简洁，只需要两个关键标识符
    /// - 数据验证严格，确保请求参数有效性
    /// - 支持高频查询，性能优化友好
    /// 
    /// 安全考虑：
    /// - 验证用户对指定会话的访问权限
    /// - 防止通过遍历ID获取他人会话信息
    /// - 记录查询操作用于审计追踪
    /// - 对敏感会话信息进行适当脱敏
    /// 
    /// API端点：GET /api/app/kou-zi-ai/retrieve-chat
    /// 查询字符串格式：?chatId=xxx&conversationId=yyy
    /// </summary>
    public class ChatRetrieveRequestDto
    {
        /// <summary>
        /// 聊天ID - 扣子AI平台生成的会话唯一标识符
        /// 
        /// 聊天ID的特征和作用：
        /// - 全局唯一性：在整个扣子AI平台中唯一标识一次聊天会话
        /// - 格式规范：通常为长整型数字字符串（如"7525814600055390248"）
        /// - 生成时机：用户发起聊天请求时由平台自动生成
        /// - 生命周期：从会话创建到自然过期或主动删除
        /// 
        /// 业务含义：
        /// - 会话定位：精确定位到特定的AI对话会话
        /// - 状态跟踪：查询AI处理的当前状态和进度
        /// - 历史查询：获取完整的对话历史记录
        /// - 问题排查：用于技术支持和问题诊断
        /// 
        /// 数据来源：
        /// - 初始响应：用户发起聊天后从响应中获取
        /// - 客户端存储：临时保存在客户端本地存储中
        /// - 服务端日志：从系统日志中提取相关信息
        /// - 用户提供：用户主动提供需要查询的ChatId
        /// 
        /// 验证规则：
        /// - 必填字段：不能为空、null或纯空白字符
        /// - 格式检查：确保符合扣子AI的ID格式规范
        /// - 长度限制：避免异常长的字符串导致系统问题
        /// - 字符集限制：通常只包含数字字符
        /// 
        /// 使用建议：
        /// - 及时获取：在聊天创建后立即保存ChatId
        /// - 安全存储：避免在不安全的地方记录ChatId
        /// - 定期清理：清理过期或无效的ChatId
        /// - 错误处理：优雅处理ChatId无效或过期的情况
        /// 
        /// 示例值："7525814600055390248"
        /// </summary>
        [Required(ErrorMessage = "聊天ID不能为空")]
        public string ChatId { get; set; }
        
        /// <summary>
        /// 会话ID - 对话上下文的连续性标识符
        /// 
        /// 会话ID的核心作用：
        /// - 上下文关联：将多轮对话关联为一个完整的会话
        /// - 连续性保证：确保AI能理解前后文的关系
        /// - 个性化记忆：维护用户在此会话中的偏好和状态
        /// - 数据组织：将分散的消息组织成逻辑对话单元
        /// 
        /// 与ChatId的关系：
        /// - 层次结构：ConversationId是更高层次的逻辑分组
        /// - 一对多关系：一个Conversation可能包含多个Chat
        /// - 生命周期：Conversation通常比单个Chat存在更久
        /// - 查询依赖：查询时需要同时提供两个ID确保精确定位
        /// 
        /// 业务价值：
        /// - 上下文查询：获取完整的对话上下文信息
        /// - 会话分析：分析用户的完整对话流程
        /// - 个性化服务：基于会话历史提供个性化回复
        /// - 问题追踪：追踪问题在整个会话中的解决过程
        /// 
        /// 技术特征：
        /// - ID格式：与ChatId类似的长整型数字字符串
        /// - 持久性：在会话生命周期内保持不变
        /// - 唯一性：在用户范围内或全局范围内唯一
        /// - 可预测性：相关的Chat通常具有相同的ConversationId
        /// 
        /// 数据流转：
        /// - 创建阶段：首次对话时由系统生成
        /// - 传递阶段：在后续请求中由客户端传递
        /// - 存储阶段：保存在客户端和服务端的会话管理中
        /// - 查询阶段：用于检索和恢复会话状态
        /// 
        /// 最佳实践：
        /// - 一致传递：在同一会话的所有请求中保持一致
        /// - 及时更新：当开始新话题时考虑使用新的ConversationId
        /// - 异常处理：处理ConversationId过期或无效的情况
        /// - 隐私保护：避免在日志中直接记录ConversationId
        /// 
        /// 示例值："7525814599107788800"
        /// </summary>
        [Required(ErrorMessage = "会话ID不能为空")]
        public string ConversationId { get; set; }
    }
}