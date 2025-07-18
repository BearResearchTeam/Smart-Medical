using System;
using System.Collections.Generic;

namespace Smart_Medical.KouZiAI.Dtos
{
    /// <summary>
    /// 聊天消息DTO - 单条消息的完整信息载体
    /// 
    /// 此DTO表示对话中的单条消息，包含消息的所有元数据：
    /// 
    /// 消息类型：
    /// - 用户消息：用户发送给AI的问题或请求
    /// - AI回复：智能体生成的回答或响应
    /// - 系统消息：系统自动生成的提示或状态信息
    /// 
    /// 数据完整性：
    /// - 包含消息的完整内容和元数据
    /// - 支持消息的时间序列分析
    /// - 提供消息类型和角色的明确标识
    /// - 保留消息的原始格式和内容类型
    /// 
    /// 使用场景：
    /// - 对话历史展示
    /// - 消息内容分析
    /// - 上下文重建
    /// - 会话数据导出
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// 消息唯一标识符
        /// 
        /// 特征：
        /// - 全局唯一：在整个扣子AI平台中唯一
        /// - 自动生成：由平台在消息创建时自动分配
        /// - 不可变性：消息创建后ID永不改变
        /// - 引用标识：用于消息的精确定位和引用
        /// 
        /// 用途：
        /// - 消息去重：避免重复显示相同消息
        /// - 精确定位：快速找到特定消息
        /// - 关联分析：建立消息间的关联关系
        /// - 增量更新：支持消息列表的增量同步
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// 消息角色标识符 - 消息发送者的类型
        /// 
        /// 角色类型定义：
        /// - "user": 用户角色，表示人类用户发送的消息
        /// - "assistant": 助手角色，表示AI智能体的回复
        /// - "system": 系统角色，表示系统生成的提示或状态信息
        /// 
        /// 业务意义：
        /// - 消息分类：区分不同来源的消息便于处理
        /// - 显示样式：客户端根据角色设置不同的UI样式
        /// - 权限控制：不同角色的消息可能有不同的处理权限
        /// - 统计分析：分析用户与AI的交互模式
        /// 
        /// 技术实现：
        /// - 字符串枚举：使用预定义的字符串值
        /// - 大小写敏感：通常使用小写字母
        /// - 标准化：遵循OpenAI等主流AI平台的角色定义
        /// - 扩展性：预留扩展其他角色类型的可能性
        /// </summary>
        public string? Role { get; set; }
        
        /// <summary>
        /// 消息类型标识符 - 消息的具体分类
        /// 
        /// 常见类型：
        /// - "question": 用户提出的问题
        /// - "answer": AI给出的回答
        /// - "instruction": 用户给出的指令
        /// - "response": AI的响应或确认
        /// - "error": 错误信息
        /// - "notification": 通知消息
        /// 
        /// 与Role的关系：
        /// - Role表示"谁"发送了消息
        /// - Type表示消息的"性质"或"目的"
        /// - 两者结合提供消息的完整分类
        /// 
        /// 应用价值：
        /// - 内容过滤：按类型筛选特定的消息
        /// - 统计分析：分析不同类型消息的分布
        /// - 处理策略：针对不同类型采用不同的处理逻辑
        /// - 用户体验：为不同类型提供差异化的展示效果
        /// </summary>
        public string? Type { get; set; }
        
        /// <summary>
        /// 消息文本内容 - 消息的核心载体
        /// 
        /// 内容特征：
        /// - 纯文本：主要支持纯文本格式（可能包含Markdown）
        /// - 多语言：支持中文、英文等多种语言
        /// - 长度变化：从简短回复到长篇解答差异很大
        /// - 格式保留：保持原始的换行、空格等格式
        /// 
        /// 内容来源：
        /// - 用户输入：用户在客户端输入的原始文本
        /// - AI生成：智能体生成的回复内容
        /// - 系统生成：系统自动生成的提示信息
        /// - 处理结果：经过格式化或清理后的内容
        /// 
        /// 处理建议：
        /// - 安全检查：防止XSS等安全风险
        /// - 格式渲染：支持Markdown等富文本格式
        /// - 长度限制：考虑显示界面的空间限制
        /// - 敏感信息：过滤或脱敏敏感内容
        /// 
        /// 技术注意：
        /// - 编码：使用UTF-8编码支持多语言
        /// - 转义：正确处理特殊字符的转义
        /// - 压缩：对于长内容考虑压缩存储
        /// - 索引：为内容搜索建立合适的索引
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// 内容类型标识符 - 描述内容的格式和结构
        /// 
        /// 支持的类型：
        /// - "text": 纯文本内容（最常见）
        /// - "markdown": Markdown格式文本
        /// - "html": HTML格式内容
        /// - "image": 图片内容（URL或Base64）
        /// - "file": 文件附件
        /// - "code": 代码片段
        /// 
        /// 处理指导：
        /// - 渲染选择：客户端根据类型选择合适的渲染方式
        /// - 安全控制：不同类型需要不同的安全检查
        /// - 功能支持：某些类型可能需要特殊的功能支持
        /// - 兼容性：确保客户端支持相应的内容类型
        /// 
        /// 扩展性：
        /// - 预留扩展：为未来的内容类型预留扩展空间
        /// - 版本兼容：保持与旧版本客户端的兼容性
        /// - 标准化：遵循MIME类型等标准规范
        /// - 降级处理：不支持的类型的降级显示策略
        /// </summary>
        public string? ContentType { get; set; }
        
        /// <summary>
        /// 消息创建时间 - 消息生成的精确时间戳
        /// 
        /// 时间意义：
        /// - 时序标记：确定消息在对话中的时间顺序
        /// - 性能指标：分析AI响应速度和用户活跃度
        /// - 数据分析：支持基于时间的统计和分析
        /// - 用户体验：显示消息的发送时间信息
        /// 
        /// 技术规范：
        /// - UTC时间：统一使用UTC时间避免时区问题
        /// - 高精度：支持毫秒级精度的时间记录
        /// - 标准格式：ISO 8601格式便于标准化处理
        /// - 自动设置：由系统自动设置，确保准确性
        /// 
        /// 应用场景：
        /// - 排序显示：按时间顺序展示对话历史
        /// - 过期处理：根据时间判断消息的有效性
        /// - 同步机制：支持多端数据同步
        /// - 统计报告：生成基于时间的使用报告
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 消息更新时间 - 消息最后修改的时间戳
        /// 
        /// 更新场景：
        /// - 内容修正：修正AI回复中的错误或不准确信息
        /// - 格式调整：优化消息的显示格式
        /// - 状态变更：更新消息的处理状态
        /// - 补充信息：添加额外的元数据或标记
        /// 
        /// 版本管理：
        /// - 变更追踪：记录消息的修改历史
        /// - 一致性检查：确保分布式环境下的数据一致性
        /// - 冲突解决：处理并发修改的冲突情况
        /// - 回滚支持：支持将消息回滚到之前的版本
        /// 
        /// 业务价值：
        /// - 质量保证：支持内容质量的持续改进
        /// - 审计合规：满足审计要求的变更记录
        /// - 用户体验：让用户了解内容的更新情况
        /// - 系统维护：便于系统维护和问题排查
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 查询聊天记录响应结果DTO - 完整的会话信息载体
    /// 
    /// 此DTO封装了从扣子AI平台查询到的完整会话信息：
    /// 
    /// 核心功能：
    /// - 会话状态展示：提供会话的当前状态和处理进度
    /// - 消息历史重现：完整还原对话的历史记录
    /// - 元数据提供：包含丰富的会话元数据信息
    /// - 便捷访问：提供快速访问关键信息的字段
    /// 
    /// 数据完整性：
    /// - 状态信息：会话的处理状态和时间节点
    /// - 消息列表：按时间顺序排列的完整消息历史
    /// - 快速访问：最后AI回复的直接访问接口
    /// - 错误处理：完善的错误信息和状态反馈
    /// 
    /// 使用场景：
    /// - 历史回顾：用户查看过往对话记录
    /// - 状态监控：检查AI处理的当前状态
    /// - 上下文恢复：为继续对话提供上下文
    /// - 数据分析：分析对话模式和用户行为
    /// - 问题排查：技术支持和问题诊断
    /// 
    /// 性能优化：
    /// - 分页支持：对于长对话支持分页加载
    /// - 增量更新：支持只获取新增的消息
    /// - 缓存友好：设计考虑客户端缓存策略
    /// - 压缩传输：大量数据时考虑压缩传输
    /// </summary>
    public class ChatRetrieveResponseDto
    {
        /// <summary>
        /// 聊天会话的唯一标识符
        /// 
        /// 与请求中ChatId的对应关系：
        /// - 一致性验证：确保响应对应正确的请求
        /// - 数据完整性：验证查询结果的准确性
        /// - 客户端验证：客户端可以验证响应的有效性
        /// - 日志追踪：便于日志关联和问题排查
        /// 
        /// 业务应用：
        /// - 身份确认：确认查询的是正确的会话
        /// - 缓存键值：作为客户端缓存的键值
        /// - 关联查询：支持相关数据的关联查询
        /// - 状态同步：支持多端状态同步
        /// </summary>
        public string? ChatId { get; set; }
        
        /// <summary>
        /// 对话上下文的标识符
        /// 
        /// 上下文价值：
        /// - 连续性保证：确保对话上下文的连续性
        /// - 后续操作：为继续对话提供必要的标识符
        /// - 关联分析：分析同一上下文中的交互模式
        /// - 数据组织：将相关消息组织为逻辑单元
        /// 
        /// 技术实现：
        /// - 持久化标识：在整个会话生命周期内保持不变
        /// - 全局唯一：避免不同会话间的标识符冲突
        /// - 传递机制：在API调用间保持一致传递
        /// - 验证机制：验证标识符的有效性和权限
        /// </summary>
        public string? ConversationId { get; set; }
        
        /// <summary>
        /// 智能体标识符 - 处理此会话的AI智能体ID
        /// 
        /// 智能体信息的价值：
        /// - 能力识别：了解处理会话的AI智能体类型和能力
        /// - 一致性保证：确保后续对话使用相同的智能体
        /// - 问题排查：定位特定智能体的问题和优化点
        /// - 性能分析：分析不同智能体的性能表现
        /// 
        /// 应用场景：
        /// - 会话继续：继续对话时使用相同的智能体
        /// - 能力匹配：根据智能体能力提供相应功能
        /// - 质量控制：监控和评估智能体的回复质量
        /// - 配置管理：管理不同智能体的配置和权限
        /// 
        /// 技术考虑：
        /// - 版本管理：智能体可能有版本更新
        /// - 兼容性：确保与不同版本智能体的兼容性
        /// - 权限验证：验证对特定智能体的访问权限
        /// - 降级策略：智能体不可用时的降级处理
        /// </summary>
        public string? BotId { get; set; }
        
        /// <summary>
        /// 会话处理状态 - 关键的状态指示器
        /// 
        /// 状态类型详解：
        /// - "in_progress": 正在处理中
        ///   * AI正在分析用户问题
        ///   * 生成回复的过程中
        ///   * 可能需要等待一段时间
        ///   * 建议客户端显示加载状态
        /// 
        /// - "completed": 处理完成
        ///   * AI已成功生成回复
        ///   * 消息列表包含完整的对话
        ///   * 可以继续发起新的对话
        ///   * 状态正常，无异常情况
        /// 
        /// - "failed": 处理失败
        ///   * AI处理过程中发生错误
        ///   * 可能是系统错误或内容问题
        ///   * 建议用户重新尝试
        ///   * 需要检查错误信息
        /// 
        /// - "requires_action": 需要用户操作
        ///   * AI需要用户提供额外信息
        ///   * 可能需要用户确认某些操作
        ///   * 需要用户输入更多详细信息
        ///   * 等待用户的下一步指示
        /// 
        /// 状态处理策略：
        /// - UI显示：根据状态显示相应的用户界面
        /// - 轮询机制：in_progress状态时可能需要轮询更新
        /// - 错误处理：failed状态时提供重试机制
        /// - 用户引导：requires_action时给出明确的操作指导
        /// 
        /// 业务逻辑：
        /// - 自动重试：某些失败情况可以自动重试
        /// - 超时处理：长时间in_progress时的超时机制
        /// - 通知机制：状态变化时的用户通知
        /// - 监控告警：异常状态的监控和告警
        /// </summary>
        public string? Status { get; set; }
        
        /// <summary>
        /// 会话创建时间 - 会话开始的时间戳
        /// 
        /// 时间价值：
        /// - 会话历史：记录会话的开始时间
        /// - 生命周期：计算会话的持续时间
        /// - 数据分析：分析用户的使用模式
        /// - 过期管理：判断会话是否需要清理
        /// 
        /// 业务应用：
        /// - 排序显示：按创建时间排序会话列表
        /// - 统计报告：生成基于时间的使用统计
        /// - 保留策略：实施数据保留和清理策略
        /// - 用户体验：显示会话的相对时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 会话完成时间 - 处理完成的时间戳（可选）
        /// 
        /// 完成时间的意义：
        /// - 性能指标：计算AI的处理速度
        /// - 状态确认：确认会话已成功完成
        /// - 时效性：判断回复的时效性
        /// - SLA监控：监控服务水平协议的达成
        /// 
        /// 数据特征：
        /// - 条件性：只有completed状态时才有值
        /// - 可空性：其他状态时为null
        /// - 精确性：精确到毫秒的完成时间
        /// - 单调性：一旦设置不再变化
        /// 
        /// 应用场景：
        /// - 性能分析：分析不同问题的处理时间
        /// - 质量评估：评估AI回复的及时性
        /// - 容量规划：预估系统处理能力
        /// - 用户反馈：向用户展示处理时长
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// 会话失败时间 - 处理失败的时间戳（可选）
        /// 
        /// 失败时间的价值：
        /// - 问题定位：精确定位问题发生的时间点
        /// - 故障分析：分析系统故障的时间模式
        /// - 恢复策略：制定故障恢复的时间策略
        /// - 监控告警：基于失败时间的告警机制
        /// 
        /// 数据用途：
        /// - 错误追踪：追踪错误发生的具体时间
        /// - 日志关联：与系统日志进行时间关联
        /// - 重试判断：判断是否需要重试处理
        /// - 用户通知：向用户说明失败的时间信息
        /// 
        /// 技术实现：
        /// - 条件设置：只在failed状态时设置
        /// - 时区统一：使用UTC时间保持一致性
        /// - 精度要求：毫秒级精度支持精确分析
        /// - 不可变性：一旦设置不再修改
        /// </summary>
        public DateTime? FailedAt { get; set; }
        
        /// <summary>
        /// 完整的消息历史记录列表 - 对话的核心数据
        /// 
        /// 消息列表特征：
        /// - 时间排序：按消息创建时间正序排列
        /// - 类型多样：包含用户消息、AI回复、系统消息
        /// - 完整性：包含会话中的所有消息记录
        /// - 结构化：每条消息都有完整的元数据信息
        /// 
        /// 数据组织：
        /// - 角色分类：user、assistant、system等不同角色
        /// - 类型标识：question、answer等不同类型
        /// - 时间序列：严格按时间顺序组织
        /// - 关联关系：问答对之间的逻辑关联
        /// 
        /// 业务应用：
        /// - 历史展示：在UI中展示完整的对话历史
        /// - 上下文分析：分析对话的上下文关系
        /// - 质量评估：评估AI回复的质量和相关性
        /// - 数据挖掘：挖掘用户需求和行为模式
        /// 
        /// 性能考虑：
        /// - 分页加载：长对话时考虑分页或懒加载
        /// - 内存管理：大量消息时的内存优化
        /// - 压缩传输：网络传输时的数据压缩
        /// - 缓存策略：客户端和服务端的缓存策略
        /// 
        /// 技术实现：
        /// - 集合初始化：默认初始化为空列表
        /// - 线程安全：考虑多线程访问的安全性
        /// - 序列化：支持JSON序列化和反序列化
        /// - 验证机制：验证消息数据的完整性
        /// </summary>
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
        
        /// <summary>
        /// 请求处理成功标志 - 查询操作的状态指示器
        /// 
        /// 成功判断标准：
        /// - true：成功查询到会话信息，数据有效可用
        /// - false：查询失败，需要检查ErrorMessage了解原因
        /// 
        /// 成功场景：
        /// - 会话存在且有权限访问
        /// - 数据完整且格式正确
        /// - 网络连接正常，API响应成功
        /// - 扣子AI平台服务正常
        /// 
        /// 失败场景：
        /// - 会话ID不存在或已过期
        /// - 没有权限访问指定会话
        /// - 网络连接问题或API服务异常
        /// - 参数格式错误或验证失败
        /// 
        /// 处理建议：
        /// - 始终检查Success字段再处理其他数据
        /// - 失败时向用户显示ErrorMessage
        /// - 提供重试机制处理临时性失败
        /// - 记录失败详情便于问题诊断
        /// </summary>
        public bool Success { get; set; } = true;
        
        /// <summary>
        /// 错误消息描述 - 详细的错误信息
        /// 
        /// 错误信息类型：
        /// - 权限错误："无权限访问该会话"
        /// - 数据错误："会话不存在或已过期"
        /// - 网络错误："网络连接失败，请稍后重试"
        /// - 服务错误："服务暂时不可用，请稍后重试"
        /// - 参数错误："请求参数无效"
        /// 
        /// 信息设计原则：
        /// - 用户友好：使用易于理解的语言
        /// - 问题明确：准确描述发生的问题
        /// - 解决导向：提供可能的解决方案
        /// - 安全考虑：不暴露敏感的系统信息
        /// 
        /// 处理策略：
        /// - 直接显示：可以直接向用户显示
        /// - 分类处理：根据错误类型采取不同措施
        /// - 日志记录：记录完整的错误上下文
        /// - 用户指导：提供具体的操作建议
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 最后的AI回复内容 - 便捷访问的快捷字段
        /// 
        /// 便捷性设计：
        /// - 快速访问：无需遍历Messages列表即可获取最新AI回复
        /// - 常用场景：大多数情况下用户最关心最后的AI回复
        /// - 性能优化：避免客户端的额外处理逻辑
        /// - 向后兼容：为不同客户端提供兼容接口
        /// 
        /// 数据来源：
        /// - 自动提取：从Messages列表中自动提取
        /// - 过滤条件：Role="assistant" 且 Type="answer"
        /// - 时间排序：选择最新时间的符合条件消息
        /// - 内容完整：包含AI回复的完整文本内容
        /// 
        /// 使用场景：
        /// - 快速预览：在列表页面快速显示最新回复
        /// - 推送通知：在通知中展示最新的AI回复
        /// - 摘要显示：在摘要界面显示关键回复
        /// - 搜索索引：为搜索功能提供内容索引
        /// 
        /// 注意事项：
        /// - 可能为空：如果没有AI回复则为null
        /// - 内容截断：如果内容过长可能需要截断
        /// - 格式处理：可能需要清理格式字符
        /// - 敏感信息：注意过滤敏感信息
        /// 
        /// 技术实现：
        /// - 自动计算：服务端自动计算并设置
        /// - 缓存友好：适合客户端缓存
        /// - 序列化：正确处理JSON序列化
        /// - 编码安全：确保文本编码的安全性
        /// </summary>
        public string? LastAssistantMessage { get; set; }
    }
}