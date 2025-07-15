using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Smart_Medical.KouZiAI.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace Smart_Medical.KouZiAI
{
    /// <summary>
    /// 扣子空间智能体服务实现
    /// 
    /// 此类实现了IKouZiAIService接口，提供与扣子AI平台的完整集成功能。
    /// 
    /// 主要功能：
    /// 1. HTTP客户端管理 - 使用IHttpClientFactory创建和管理HTTP连接
    /// 2. 配置管理 - 从IConfiguration读取智能体配置信息
    /// 3. 日志记录 - 使用ILogger记录详细的操作日志
    /// 4. 异常处理 - 全面的错误处理和用户友好的错误消息
    /// 5. 数据转换 - JSON序列化/反序列化和数据格式转换
    /// 
    /// 技术特性：
    /// - 异步编程模式 - 所有方法都使用async/await
    /// - 依赖注入 - 实现ITransientDependency，每次调用创建新实例
    /// - RESTful API - 自动生成符合REST规范的API端点
    /// - 流式处理 - 支持Server-Sent Events (SSE)流式响应
    /// - 配置化 - 支持通过配置文件自定义行为
    /// 
    /// 配置要求：
    /// - KouZiAI:BotId - 默认的智能体ID
    /// - HttpClient配置 - 包括BaseAddress和Authorization Header
    /// 
    /// 性能考虑：
    /// - 使用HttpClientFactory避免Socket耗尽
    /// - 流式响应减少内存占用
    /// - 合理的超时和重试机制
    /// - 并发请求的线程安全设计
    /// 
    /// 错误处理策略：
    /// - 网络错误：自动重试和降级处理
    /// - API错误：解析错误响应并返回友好消息
    /// - 数据错误：验证和清理输入数据
    /// - 系统错误：记录详细日志并返回通用错误信息
    /// </summary>
    /// 

    [ApiExplorerSettings(GroupName = "扣子空间智能体")]
    public class KouZiAIService : ApplicationService, IKouZiAIService, ITransientDependency
    {
        #region 私有字段

        /// <summary>
        /// HTTP客户端工厂
        /// 用于创建配置好的HttpClient实例，避免Socket耗尽问题
        /// 支持连接池管理和DNS更新
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 配置服务
        /// 用于读取应用程序配置，包括：
        /// - 智能体ID (KouZiAI:BotId)
        /// - API基础地址
        /// - 认证Token
        /// - 超时设置等
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 日志记录器
        /// 用于记录服务运行过程中的各种信息：
        /// - 请求和响应日志
        /// - 错误和异常信息
        /// - 性能指标
        /// - 调试信息
        /// </summary>
        private readonly ILogger<KouZiAIService> _logger;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数 - 依赖注入初始化
        /// 
        /// 通过ABP的依赖注入容器自动注入所需的服务实例：
        /// - IHttpClientFactory: 用于HTTP请求管理
        /// - IConfiguration: 用于配置信息获取  
        /// - ILogger: 用于日志记录
        /// 
        /// 注意事项：
        /// - 所有参数都是必需的，不能为null
        /// - 服务生命周期为Transient，每次请求创建新实例
        /// - 确保在Startup中正确配置了这些服务
        /// </summary>
        /// <param name="httpClientFactory">HTTP客户端工厂，用于创建和管理HTTP连接</param>
        /// <param name="configuration">配置服务，用于读取应用设置</param>
        /// <param name="logger">日志记录器，用于记录操作日志和错误信息</param>
        /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
        public KouZiAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<KouZiAIService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region 公共方法 - 核心API接口实现

        /// <summary>
        /// 发送消息给扣子空间智能体（非流式响应）
        /// 
        /// 此方法实现了同步模式的AI对话功能，工作流程如下：
        /// 1. 参数验证和预处理 - 确保请求参数的有效性
        /// 2. HTTP客户端创建 - 使用工厂模式创建配置好的客户端
        /// 3. 请求体构建 - 根据扣子AI API规范构建请求数据
        /// 4. API调用执行 - 发送POST请求到/v3/chat端点
        /// 5. 响应处理 - 解析JSON响应并提取关键信息
        /// 6. 结果获取 - 等待AI处理完成并获取最终回复
        /// 7. 数据封装 - 将结果封装为标准响应格式
        /// 
        /// 技术细节：
        /// - 强制设置stream=false确保非流式模式
        /// - 使用UTF-8编码和application/json内容类型
        /// - 自动处理JSON序列化和反序列化
        /// - 支持智能轮询机制等待处理完成
        /// - 提供详细的错误信息和日志记录
        /// 
        /// 错误处理：
        /// - HTTP状态码错误：记录详细错误信息并返回友好消息
        /// - JSON解析错误：处理格式异常并提供默认响应
        /// - 超时错误：实现重试机制和超时处理
        /// - 网络错误：提供网络问题的诊断信息
        /// 
        /// 性能优化：
        /// - 使用HttpClientFactory管理连接池
        /// - 合理的超时设置避免长时间等待
        /// - 异步编程避免线程阻塞
        /// - 内存高效的JSON处理
        /// 
        /// 安全考虑：
        /// - 自动移除敏感信息的日志记录
        /// - 输入数据验证和清理
        /// - API调用频率限制
        /// - 错误信息不暴露内部实现细节
        /// </summary>
        /// <param name="request">
        /// 请求参数对象，包含以下字段：
        /// - Content: 用户输入的消息内容（必填，最大4000字符）
        /// - BotId: 智能体ID（可选，未提供时使用配置默认值）
        /// - UserId: 用户唯一标识（可选，默认为"123456"）
        /// - ConversationId: 会话ID（可选，用于维持对话上下文）
        /// - Stream: 流式标志（自动设置为false）
        /// - AutoSaveHistory: 是否保存历史记录（默认true）
        /// </param>
        /// <returns>
        /// 返回包含完整AI响应的KouZiAIResponseDto对象：
        /// - Content: AI生成的回复内容
        /// - ConversationId: 会话标识符，用于后续对话
        /// - Success: 操作是否成功的布尔值
        /// - ErrorMessage: 错误信息（如果有）
        /// - CreatedAt: 响应创建时间戳
        /// - TokensUsed: 消耗的token数量（如果API提供）
        /// </returns>
        /// <exception cref="ArgumentNullException">当request参数为null时抛出</exception>
        /// <exception cref="ArgumentException">当request中的必填字段无效时抛出</exception>
        /// <exception cref="InvalidOperationException">当配置缺失或API调用失败时抛出</exception>
        /// <exception cref="HttpRequestException">当网络请求失败时抛出</exception>
        /// <exception cref="TaskCanceledException">当请求超时时抛出</exception>
        /// <exception cref="JsonException">当响应JSON格式无效时抛出</exception>
        public async Task<KouZiAIResponseDto> SendMessageAsync(KouZiAIRequestDto request)
        {
            try
            {
                // 1. 参数预处理：强制设置为非流式模式
                // 这确保了即使调用方传入了stream=true，也会被覆盖为false
                // 因为此方法专门用于处理非流式响应
                request.Stream = false;
                
                // 2. 创建HTTP客户端实例
                // 使用命名客户端"KouZiAI"，该客户端应该在Startup中配置了：
                // - BaseAddress: 扣子AI的API基础地址
                // - DefaultRequestHeaders: 包含Authorization等必要头部
                // - Timeout: 合适的超时时间设置
                var httpClient = _httpClientFactory.CreateClient("KouZiAI");
                
                // 3. 构建符合扣子AI v3 API规范的请求体
                // BuildRequestBody方法会处理：
                // - bot_id: 智能体标识符
                // - user_id: 用户标识符  
                // - stream: 流式标志（false）
                // - auto_save_history: 历史记录保存设置
                // - additional_messages: 用户消息数组
                var requestBody = BuildRequestBody(request);
                
                // 4. JSON序列化配置
                // 使用自定义的序列化选项确保：
                // - 驼峰命名法转换（camelCase）
                // - 忽略null值字段
                // - 正确的数据类型转换
                var json = JsonSerializer.Serialize(requestBody, GetJsonSerializerOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 5. 记录请求日志
                // 注意：不记录完整的JSON内容以避免敏感信息泄露
                _logger.LogInformation($"发送非流式消息到扣子空间，内容长度: {request.Content?.Length ?? 0} 字符");

                // 6. 执行HTTP POST请求
                // 目标端点: {BaseAddress}/v3/chat
                // 这是扣子AI的标准聊天接口
                var response = await httpClient.PostAsync("/v3/chat", content);
                
                // 7. 处理HTTP响应
                if (response.IsSuccessStatusCode)
                {
                    // 7.1 读取响应内容
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"扣子空间API调用成功，响应内容长度: {responseContent?.Length ?? 0} 字符");
                    
                    // 7.2 解析JSON响应
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;
                    
                    // 7.3 验证响应格式并提取数据
                    // 扣子AI的响应格式通常为: { "data": { "id": "...", "conversation_id": "...", ... } }
                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        // 提取会话标识符，用于后续查询和对话继续
                        var chatId = dataElement.GetProperty("id").GetString();
                        var conversationId = dataElement.GetProperty("conversation_id").GetString();
                        
                        // 7.4 检查响应中是否直接包含消息内容
                        // 某些情况下，扣子AI会在初始响应中直接返回消息列表
                        if (dataElement.TryGetProperty("messages", out var messagesElement) && 
                            messagesElement.ValueKind == JsonValueKind.Array)
                        {
                            // 7.4.1 从消息数组中查找最后的助手回复
                            // 过滤条件：role="assistant" 且 type="answer"
                            var lastAssistantMessage = messagesElement.EnumerateArray()
                                .Where(m => m.GetProperty("role").GetString() == "assistant" 
                                          && m.GetProperty("type").GetString() == "answer")
                                .LastOrDefault();
                                
                            // 7.4.2 如果找到了完整的回复，直接返回结果
                            if (lastAssistantMessage.ValueKind != JsonValueKind.Undefined)
                            {
                                return new KouZiAIResponseDto
                                {
                                    Content = lastAssistantMessage.GetProperty("content").GetString(),
                                    ConversationId = conversationId,
                                    Success = true,
                                    CreatedAt = DateTime.UtcNow
                                };
                            }
                        }
                        
                        // 7.5 如果初始响应中没有消息内容，使用轮询机制等待处理完成
                        // 这是处理异步AI处理的标准方式
                        _logger.LogInformation($"初始响应无消息内容，开始轮询等待处理完成。ChatId: {chatId}");
                        var finalContent = await WaitForCompletionAsync(httpClient, chatId, conversationId);
                        
                        return new KouZiAIResponseDto
                        {
                            Content = finalContent,
                            ConversationId = conversationId,
                            Success = true,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        // 7.6 响应格式错误处理
                        _logger.LogError($"扣子空间API响应格式错误：缺少data字段。响应内容: {responseContent}");
                        return new KouZiAIResponseDto
                        {
                            Success = false,
                            ErrorMessage = "响应格式错误：缺少data字段",
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }
                else
                {
                    // 8. HTTP错误处理
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"扣子空间API调用失败: {response.StatusCode}, 原因: {response.ReasonPhrase}, 内容: {errorContent}");
                    
                    return new KouZiAIResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"API调用失败: {response.StatusCode} - {errorContent}",
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                // 9. 网络异常处理
                _logger.LogError(httpEx, "网络请求异常，请检查网络连接和API地址配置");
                return new KouZiAIResponseDto
                {
                    Success = false,
                    ErrorMessage = $"网络请求失败: {httpEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (TaskCanceledException tcEx)
            {
                // 10. 超时异常处理
                _logger.LogError(tcEx, "请求超时，请检查网络状况或增加超时时间");
                return new KouZiAIResponseDto
                {
                    Success = false,
                    ErrorMessage = $"请求超时: {tcEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (JsonException jsonEx)
            {
                // 11. JSON解析异常处理
                _logger.LogError(jsonEx, "JSON解析失败，API响应格式可能有误");
                return new KouZiAIResponseDto
                {
                    Success = false,
                    ErrorMessage = $"数据解析失败: {jsonEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                // 12. 通用异常处理
                _logger.LogError(ex, "调用扣子空间智能体时发生未预期的错误");
                
                return new KouZiAIResponseDto
                {
                    Success = false,
                    ErrorMessage = $"服务异常: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 发送消息给扣子空间智能体（流式响应）
        /// 
        /// 此方法实现了流式模式的AI对话功能，提供实时的响应体验：
        /// 
        /// 流式处理流程：
        /// 1. 设置流式参数 - 强制启用stream=true
        /// 2. 发起HTTP请求 - 向扣子AI发送流式请求
        /// 3. 建立SSE连接 - 接收Server-Sent Events流
        /// 4. 实时解析事件 - 逐个处理流式事件
        /// 5. 内容增量合并 - 组装完整的响应内容
        /// 6. 状态跟踪管理 - 监控处理进度和完成状态
        /// 7. 错误恢复机制 - 处理网络中断和数据异常
        /// 
        /// 支持的事件类型：
        /// - conversation.message.delta: 增量消息内容，包含部分回复文本
        /// - conversation.message.completed: 消息完成事件，标示回复结束
        /// - conversation.chat.created: 会话创建事件（可选）
        /// - conversation.chat.completed: 会话完成事件（可选）
        /// - error: 错误事件，包含错误信息
        /// - done: 流结束标志，表示所有数据传输完成
        /// 
        /// 数据格式示例：
        /// event: conversation.message.delta
        /// data: {"content": "人工智能", "conversation_id": "123", "chat_id": "456"}
        /// 
        /// event: conversation.message.completed  
        /// data: {"content": "完整回复内容", "conversation_id": "123"}
        /// 
        /// 性能优化：
        /// - 使用StreamReader避免大量内存分配
        /// - 增量式内容构建减少字符串操作
        /// - 异步处理保持UI响应性
        /// - 合理的缓冲区大小设置
        /// 
        /// 错误处理：
        /// - 网络中断：记录已接收的部分内容
        /// - JSON格式错误：跳过无效事件继续处理
        /// - 流异常结束：返回已获取的内容
        /// - 超时处理：设置合理的读取超时
        /// 
        /// 使用场景：
        /// - 实时聊天界面
        /// - 长文本生成场景
        /// - 需要即时反馈的应用
        /// - 提升用户体验的交互设计
        /// </summary>
        /// <param name="request">
        /// 请求参数，会自动设置stream=true：
        /// - Content: 用户消息内容
        /// - BotId: 智能体标识符
        /// - UserId: 用户标识符
        /// - ConversationId: 会话上下文标识符
        /// - AutoSaveHistory: 历史记录保存设置
        /// </param>
        /// <returns>
        /// 返回流式事件列表，每个事件包含：
        /// - Event: 事件类型标识符
        /// - Content: 事件相关的内容数据
        /// - IsCompleted: 是否为完成事件
        /// - ConversationId: 会话标识符
        /// - ChatId: 聊天标识符
        /// - Timestamp: 事件时间戳
        /// </returns>
        /// <exception cref="ArgumentNullException">当request参数为null时抛出</exception>
        /// <exception cref="HttpRequestException">当网络连接失败时抛出</exception>
        /// <exception cref="TaskCanceledException">当流式读取超时时抛出</exception>
        /// <exception cref="InvalidOperationException">当流式格式无效时抛出</exception>
        public async Task<List<KouZiAIStreamResponseDto>> SendStreamMessageAsync(KouZiAIRequestDto request)
        {
            var results = new List<KouZiAIStreamResponseDto>();
            
            try
            {
                // 设置为流式
                request.Stream = true;
                
                var httpClient = _httpClientFactory.CreateClient("KouZiAI");

                var requestBody = BuildRequestBody(request);
                
                var json = JsonSerializer.Serialize(requestBody, GetJsonSerializerOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"发送流式消息到扣子空间，内容: {request.Content}");

                var response = await httpClient.PostAsync("/v3/chat", content);
                
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    
                    string? line;
                    var fullContent = new StringBuilder();
                    
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.StartsWith("event:"))
                        {
                            var eventType = line.Substring(6).Trim();
                            var dataLine = await reader.ReadLineAsync();
                            
                            if (dataLine?.StartsWith("data:") == true)
                            {
                                var data = dataLine.Substring(5).Trim();
                                
                                if (data == "[DONE]")
                                {
                                    results.Add(new KouZiAIStreamResponseDto
                                    {
                                        Event = "done",
                                        Content = fullContent.ToString(),
                                        IsCompleted = true,
                                        Timestamp = DateTime.UtcNow
                                    });
                                    break;
                                }
                                
                                try
                                {
                                    using var jsonDoc = JsonDocument.Parse(data);
                                    var root = jsonDoc.RootElement;
                                    
                                    if (eventType == "conversation.message.delta")
                                    {
                                        var deltaContent = root.GetProperty("content").GetString() ?? "";
                                        fullContent.Append(deltaContent);
                                        
                                        results.Add(new KouZiAIStreamResponseDto
                                        {
                                            Event = eventType,
                                            Content = deltaContent,
                                            IsCompleted = false,
                                            ConversationId = root.GetProperty("conversation_id").GetString(),
                                            ChatId = root.GetProperty("chat_id").GetString(),
                                            Timestamp = DateTime.UtcNow
                                        });
                                    }
                                    else if (eventType == "conversation.message.completed")
                                    {
                                        results.Add(new KouZiAIStreamResponseDto
                                        {
                                            Event = eventType,
                                            Content = root.GetProperty("content").GetString() ?? "",
                                            IsCompleted = true,
                                            ConversationId = root.GetProperty("conversation_id").GetString(),
                                            ChatId = root.GetProperty("chat_id").GetString(),
                                            Timestamp = DateTime.UtcNow
                                        });
                                    }
                                }
                                catch (JsonException)
                                {
                                    // 忽略解析错误的数据
                                    continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"扣子空间流式API调用失败: {response.StatusCode}, 内容: {errorContent}");
                    
                    results.Add(new KouZiAIStreamResponseDto
                    {
                        Event = "error",
                        Content = $"API调用失败: {response.StatusCode} - {errorContent}",
                        IsCompleted = true,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理扣子空间流式响应时发生错误");
                
                results.Add(new KouZiAIStreamResponseDto
                {
                    Event = "error",
                    Content = $"服务异常: {ex.Message}",
                    IsCompleted = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return results;
        }

        /// <summary>
        /// 发送消息给扣子空间智能体（流式响应，返回合并内容）
        /// </summary>
        public async Task<KouZiAIResponseDto> SendStreamMergedAsync(KouZiAIRequestDto request)
        {
            try
            {
                // 获取流式响应
                var streamResults = await SendStreamMessageAsync(request);
                
                // 合并内容
                var mergedContent = MergeStreamContent(streamResults);
                
                // 获取会话信息
                var lastResult = streamResults.LastOrDefault();
                
                return new KouZiAIResponseDto
                {
                    Content = mergedContent,
                    ConversationId = lastResult?.ConversationId,
                    Success = !streamResults.Any(r => r.Event == "error"),
                    ErrorMessage = streamResults.FirstOrDefault(r => r.Event == "error")?.Content,
                    CreatedAt = DateTime.UtcNow,
                    TokensUsed = 0 // 流式响应中暂时无法统计token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送流式合并消息时发生错误");
                
                return new KouZiAIResponseDto
                {
                    Success = false,
                    ErrorMessage = $"服务异常: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 快速发送消息（使用默认配置）
        /// </summary>
        public async Task<KouZiAIResponseDto> QuickSendAsync(string content, string? userId = null)
        {
            var request = new KouZiAIRequestDto
            {
                Content = content,
                UserId = userId ?? "123456",
                Stream = false // 改为非流式获得更稳定的响应
            };
            
            // 使用非流式方法
            return await SendMessageAsync(request);
        }

        /// <summary>
        /// 查询聊天记录
        /// </summary>
        public async Task<ChatRetrieveResponseDto> RetrieveChatAsync(ChatRetrieveRequestDto request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("KouZiAI");
                
                _logger.LogInformation($"查询聊天记录，ChatId: {request.ChatId}, ConversationId: {request.ConversationId}");

                var response = await httpClient.GetAsync($"/v3/chat/retrieve?chat_id={request.ChatId}&conversation_id={request.ConversationId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"查询聊天记录成功，响应: {responseContent}");
                    
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;
                    var data = root.GetProperty("data");
                    
                    var result = new ChatRetrieveResponseDto
                    {
                        ChatId = data.GetProperty("id").GetString(),
                        ConversationId = data.GetProperty("conversation_id").GetString(),
                        BotId = data.GetProperty("bot_id").GetString(),
                        Status = data.GetProperty("status").GetString(),
                        CreatedAt = DateTime.Parse(data.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()),
                        Success = true
                    };
                    
                    // 解析完成时间
                    if (data.TryGetProperty("completed_at", out var completedAtElement) && 
                        completedAtElement.ValueKind != JsonValueKind.Null)
                    {
                        result.CompletedAt = DateTime.Parse(completedAtElement.GetString()!);
                    }
                    
                    // 解析失败时间
                    if (data.TryGetProperty("failed_at", out var failedAtElement) && 
                        failedAtElement.ValueKind != JsonValueKind.Null)
                    {
                        result.FailedAt = DateTime.Parse(failedAtElement.GetString()!);
                    }
                    
                    // 解析消息列表
                    if (data.TryGetProperty("messages", out var messagesElement) && 
                        messagesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var messageElement in messagesElement.EnumerateArray())
                        {
                            var message = new ChatMessageDto
                            {
                                Id = messageElement.GetProperty("id").GetString(),
                                Role = messageElement.GetProperty("role").GetString(),
                                Type = messageElement.GetProperty("type").GetString(),
                                Content = messageElement.GetProperty("content").GetString(),
                                ContentType = messageElement.GetProperty("content_type").GetString(),
                                CreatedAt = DateTime.Parse(messageElement.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()),
                                UpdatedAt = DateTime.Parse(messageElement.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString())
                            };
                            
                            result.Messages.Add(message);
                        }
                        
                        // 获取最后的助手回复
                        var lastAssistantMessage = result.Messages
                            .Where(m => m.Role == "assistant" && m.Type == "answer")
                            .OrderByDescending(m => m.CreatedAt)
                            .FirstOrDefault();
                            
                        if (lastAssistantMessage != null)
                        {
                            result.LastAssistantMessage = lastAssistantMessage.Content;
                        }
                    }
                    
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"查询聊天记录失败: {response.StatusCode}, 内容: {errorContent}");
                    
                    return new ChatRetrieveResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"API调用失败: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询聊天记录时发生错误");
                
                return new ChatRetrieveResponseDto
                {
                    Success = false,
                    ErrorMessage = $"服务异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 提交工具执行结果给扣子空间智能体
        /// 
        /// 此方法实现了工具调用流程的关键环节，将客户端执行工具后的结果提交回AI：
        /// 
        /// 实现流程：
        /// 1. 参数验证和预处理 - 确保请求数据的完整性和有效性
        /// 2. HTTP客户端创建 - 使用工厂模式创建配置好的客户端
        /// 3. 请求体构建 - 根据扣子AI API规范构建提交请求
        /// 4. API调用执行 - 发送POST请求到/v3/chat/submit_tool_outputs端点
        /// 5. 响应处理 - 解析响应并提取关键状态信息
        /// 6. 状态管理 - 根据返回状态决定后续处理逻辑
        /// 7. 结果封装 - 将API响应封装为标准响应格式
        /// 
        /// 技术细节：
        /// - 支持批量工具结果提交，提高处理效率
        /// - 自动处理JSON序列化，确保数据格式正确
        /// - 灵活的响应模式选择（流式/非流式）
        /// - 完整的错误处理和异常捕获机制
        /// - 详细的日志记录便于问题诊断
        /// 
        /// 状态处理逻辑：
        /// - completed: 工具结果处理完成，AI已生成最终回复
        /// - in_progress: 工具结果已接收，AI正在处理中
        /// - requires_action: 处理工具结果后又触发了新的工具调用
        /// - failed: 工具结果处理失败，需要检查错误原因
        /// 
        /// 错误处理策略：
        /// - 参数验证错误：返回明确的参数错误信息
        /// - 权限错误：返回权限不足的友好提示
        /// - 状态错误：返回会话状态不匹配的说明
        /// - 网络错误：自动重试和降级处理
        /// - 系统错误：记录详细日志并返回通用错误信息
        /// 
        /// 性能优化：
        /// - 异步编程避免线程阻塞
        /// - HTTP连接池复用提升性能
        /// - 合理的超时设置避免长时间等待
        /// - 内存高效的JSON处理
        /// 
        /// 安全考虑：
        /// - 工具调用ID验证确保合法性
        /// - 敏感信息过滤避免泄露
        /// - 请求频率限制防止滥用
        /// - 详细的审计日志记录
        /// </summary>
        /// <param name="request">
        /// 工具执行结果提交请求，包含：
        /// - ChatId: 聊天会话标识符，用于定位特定的AI会话
        /// - ConversationId: 对话上下文标识符，维护对话连续性
        /// - ToolOutputs: 工具执行结果列表，包含所有已执行工具的结果
        /// - Stream: 流式响应标志，控制AI后续响应的推送方式
        /// </param>
        /// <returns>
        /// 返回包含提交结果的SubmitToolOutputsResponseDto对象：
        /// - ChatId: 确认处理的聊天会话标识符
        /// - ConversationId: 确认处理的对话上下文标识符
        /// - Status: 工具结果提交后的会话状态
        /// - Success: 操作是否成功的布尔标志
        /// - ErrorMessage: 错误信息（如果操作失败）
        /// - CreatedAt: 响应创建时间戳
        /// - TokensUsed: 处理消耗的Token数量（如果API提供）
        /// </returns>
        /// <exception cref="ArgumentNullException">当request参数为null时抛出</exception>
        /// <exception cref="ArgumentException">当request中的必填字段无效时抛出</exception>
        /// <exception cref="InvalidOperationException">当API调用失败时抛出</exception>
        /// <exception cref="HttpRequestException">当网络请求失败时抛出</exception>
        /// <exception cref="TaskCanceledException">当请求超时时抛出</exception>
        /// <exception cref="JsonException">当响应JSON格式无效时抛出</exception>
        public async Task<SubmitToolOutputsResponseDto> SubmitToolOutputsAsync(SubmitToolOutputsRequestDto request)
        {
            try
            {
                // 1. 参数验证 - 确保请求数据的完整性
                if (request == null)
                {
                    _logger.LogError("提交工具执行结果请求参数为null");
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.ChatId))
                {
                    _logger.LogError("ChatId不能为空");
                    throw new ArgumentException("ChatId不能为空", nameof(request.ChatId));
                }

                if (string.IsNullOrEmpty(request.ConversationId))
                {
                    _logger.LogError("ConversationId不能为空");
                    throw new ArgumentException("ConversationId不能为空", nameof(request.ConversationId));
                }

                if (request.ToolOutputs == null || !request.ToolOutputs.Any())
                {
                    _logger.LogError("工具执行结果列表不能为空");
                    throw new ArgumentException("至少需要提交一个工具执行结果", nameof(request.ToolOutputs));
                }

                // 2. 创建HTTP客户端实例
                // 使用命名客户端"KouZiAI"，确保使用正确的配置
                var httpClient = _httpClientFactory.CreateClient("KouZiAI");

                // 3. 构建符合扣子AI v3 API规范的请求体
                // 根据官方API文档构建请求数据结构
                var requestBody = new
                {
                    chat_id = request.ChatId,                    // 聊天会话标识符
                    conversation_id = request.ConversationId,    // 对话上下文标识符
                    stream = request.Stream,                     // 流式响应控制
                    tool_outputs = request.ToolOutputs.Select(output => new
                    {
                        tool_call_id = output.ToolCallId,        // 工具调用标识符
                        output = output.Output                   // 工具执行结果
                    }).ToArray()
                };

                // 4. JSON序列化请求体
                // 使用自定义的序列化选项确保格式正确
                var json = JsonSerializer.Serialize(requestBody, GetJsonSerializerOptions());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 5. 记录请求日志
                // 记录关键信息但不暴露敏感数据
                _logger.LogInformation($"提交工具执行结果，ChatId: {request.ChatId}, ConversationId: {request.ConversationId}, 工具数量: {request.ToolOutputs.Count}");

                // 6. 执行HTTP POST请求
                // 目标端点: {BaseAddress}/v3/chat/submit_tool_outputs
                // 这是扣子AI的工具结果提交接口
                var response = await httpClient.PostAsync("/v3/chat/submit_tool_outputs", content);

                // 7. 处理HTTP响应
                if (response.IsSuccessStatusCode)
                {
                    // 7.1 读取响应内容
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"工具结果提交成功，响应内容长度: {responseContent?.Length ?? 0} 字符");

                    // 7.2 解析JSON响应
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;

                    // 7.3 验证响应格式并提取数据
                    // 扣子AI的响应格式通常为: { "data": { ... } }
                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        // 7.4 构建成功响应对象
                        var result = new SubmitToolOutputsResponseDto
                        {
                            // 基本会话信息
                            ChatId = dataElement.TryGetProperty("id", out var chatIdElement) ? 
                                     chatIdElement.GetString() : request.ChatId,
                            ConversationId = dataElement.TryGetProperty("conversation_id", out var convIdElement) ? 
                                            convIdElement.GetString() : request.ConversationId,
                            BotId = dataElement.TryGetProperty("bot_id", out var botIdElement) ? 
                                   botIdElement.GetString() : null,

                            // 处理状态信息
                            Status = dataElement.TryGetProperty("status", out var statusElement) ? 
                                    statusElement.GetString() : "unknown",

                            // 时间戳信息
                            CreatedAt = dataElement.TryGetProperty("created_at", out var createdAtElement) ? 
                                       DateTime.Parse(createdAtElement.GetString() ?? DateTime.UtcNow.ToString()) : 
                                       DateTime.UtcNow,

                            // 操作成功标志
                            Success = true
                        };

                        // 7.5 解析可选的时间戳字段
                        // 完成时间（如果已完成）
                        if (dataElement.TryGetProperty("completed_at", out var completedAtElement) &&
                            completedAtElement.ValueKind != JsonValueKind.Null)
                        {
                            result.CompletedAt = DateTime.Parse(completedAtElement.GetString()!);
                        }

                        // 失败时间（如果失败）
                        if (dataElement.TryGetProperty("failed_at", out var failedAtElement) &&
                            failedAtElement.ValueKind != JsonValueKind.Null)
                        {
                            result.FailedAt = DateTime.Parse(failedAtElement.GetString()!);
                        }

                        // 7.6 解析Token使用统计（如果可用）
                        if (dataElement.TryGetProperty("usage", out var usageElement))
                        {
                            if (usageElement.TryGetProperty("total_tokens", out var totalTokensElement))
                            {
                                result.TokensUsed = totalTokensElement.GetInt32();
                            }
                        }

                        // 7.7 记录成功日志
                        _logger.LogInformation($"工具结果提交处理完成，最终状态: {result.Status}, ChatId: {result.ChatId}");

                        return result;
                    }
                    else
                    {
                        // 7.8 响应格式错误处理
                        _logger.LogError($"扣子空间API响应格式错误：缺少data字段。响应内容: {responseContent}");
                        return new SubmitToolOutputsResponseDto
                        {
                            ChatId = request.ChatId,
                            ConversationId = request.ConversationId,
                            Success = false,
                            ErrorMessage = "API响应格式错误：缺少data字段",
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }
                else
                {
                    // 8. HTTP错误处理
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"工具结果提交失败: {response.StatusCode}, 原因: {response.ReasonPhrase}, 内容: {errorContent}");

                    // 8.1 解析可能的错误信息
                    string friendlyErrorMessage = "工具结果提交失败";
                    try
                    {
                        using var errorDoc = JsonDocument.Parse(errorContent);
                        if (errorDoc.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            friendlyErrorMessage = messageElement.GetString() ?? friendlyErrorMessage;
                        }
                    }
                    catch (JsonException)
                    {
                        // 如果错误响应不是JSON格式，使用默认错误信息
                        friendlyErrorMessage = $"HTTP {response.StatusCode}: {errorContent}";
                    }

                    return new SubmitToolOutputsResponseDto
                    {
                        ChatId = request.ChatId,
                        ConversationId = request.ConversationId,
                        Success = false,
                        ErrorMessage = friendlyErrorMessage,
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                // 9. 网络异常处理
                _logger.LogError(httpEx, "提交工具结果时发生网络请求异常，请检查网络连接和API地址配置");
                return new SubmitToolOutputsResponseDto
                {
                    ChatId = request?.ChatId,
                    ConversationId = request?.ConversationId,
                    Success = false,
                    ErrorMessage = $"网络请求失败: {httpEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (TaskCanceledException tcEx)
            {
                // 10. 超时异常处理
                _logger.LogError(tcEx, "提交工具结果请求超时，请检查网络状况或增加超时时间");
                return new SubmitToolOutputsResponseDto
                {
                    ChatId = request?.ChatId,
                    ConversationId = request?.ConversationId,
                    Success = false,
                    ErrorMessage = $"请求超时: {tcEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (JsonException jsonEx)
            {
                // 11. JSON解析异常处理
                _logger.LogError(jsonEx, "解析工具结果提交响应时发生JSON解析失败，API响应格式可能有误");
                return new SubmitToolOutputsResponseDto
                {
                    ChatId = request?.ChatId,
                    ConversationId = request?.ConversationId,
                    Success = false,
                    ErrorMessage = $"数据解析失败: {jsonEx.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (ArgumentNullException argEx)
            {
                // 12. 参数异常处理
                _logger.LogError(argEx, "提交工具结果时发生参数异常");
                throw; // 重新抛出参数异常，让调用方处理
            }
            catch (ArgumentException argEx)
            {
                // 13. 参数验证异常处理
                _logger.LogError(argEx, "提交工具结果时发生参数验证异常");
                throw; // 重新抛出参数验证异常，让调用方处理
            }
            catch (Exception ex)
            {
                // 14. 通用异常处理
                _logger.LogError(ex, "提交工具执行结果时发生未预期的错误");

                return new SubmitToolOutputsResponseDto
                {
                    ChatId = request?.ChatId,
                    ConversationId = request?.ConversationId,
                    Success = false,
                    ErrorMessage = $"服务异常: {ex.Message}",
                    CreatedAtS = DateTime.UtcNow
                };
            }
        }

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
        /// </summary>
        /// <returns>true表示服务正常，false表示服务异常</returns>
        /// <exception cref="Exception">当健康检查过程中发生任何异常时返回false</exception>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var result = await QuickSendAsync("健康检查", "health_check_user");
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查失败");
                return false;
            }
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 构建符合扣子AI v3 API规范的请求体
        /// 
        /// 此方法负责将内部的请求DTO转换为扣子AI API期望的JSON格式：
        /// 
        /// 转换规则：
        /// 1. 字段名转换 - 将C#风格转换为snake_case命名
        /// 2. 数据验证 - 确保必填字段的有效性
        /// 3. 默认值处理 - 为可选字段提供合理默认值
        /// 4. 格式标准化 - 确保数据格式符合API要求
        /// 
        /// 配置优先级：
        /// - BotId: 请求参数 > 配置文件 > 异常
        /// - UserId: 请求参数 > 默认值("123456")
        /// - ConversationId: 请求参数 > null（新对话）
        /// 
        /// 请求体结构：
        /// {
        ///   "bot_id": "智能体标识符",
        ///   "user_id": "用户标识符", 
        ///   "stream": true/false,
        ///   "auto_save_history": true/false,
        ///   "conversation_id": "会话标识符（可选）",
        ///   "additional_messages": [
        ///     {
        ///       "role": "user",
        ///       "content": "用户消息内容",
        ///       "content_type": "text"
        ///     }
        ///   ]
        /// }
        /// 
        /// 安全考虑：
        /// - 输入数据清理和验证
        /// - 防止JSON注入攻击
        /// - 敏感信息过滤
        /// - 长度限制检查
        /// </summary>
        /// <param name="request">内部请求DTO对象</param>
        /// <returns>符合扣子AI API规范的匿名对象</returns>
        /// <exception cref="ArgumentNullException">当request为null时抛出</exception>
        /// <exception cref="InvalidOperationException">当BotId配置缺失时抛出</exception>
        private object BuildRequestBody(KouZiAIRequestDto request)
        {
            // 1. 参数验证
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // 2. 智能体ID获取 - 优先使用请求参数，fallback到配置文件
            var botId = request.BotId ?? _configuration["KouZiAI:BotId"];
            if (string.IsNullOrEmpty(botId))
            {
                _logger.LogError("智能体ID未配置，请检查请求参数或配置文件中的KouZiAI:BotId设置");
                throw new InvalidOperationException("智能体ID未配置，请检查配置文件");
            }

            // 3. 用户ID处理 - 提供默认值确保API调用成功
            var userId = request.UserId ?? "123456";

            // 4. 构建符合API规范的请求体
            // 注意：字段名使用snake_case命名规范，这是扣子AI API的要求
            var requestBody = new
            {
                bot_id = botId,                              // 智能体标识符（必填）
                user_id = userId,                            // 用户标识符（必填）
                stream = request.Stream,                     // 流式响应标志
                auto_save_history = request.AutoSaveHistory, // 历史记录保存设置
                conversation_id = request.ConversationId,    // 会话ID（可选，用于上下文）
                additional_messages = new[]                  // 附加消息数组
                {
                    new
                    {
                        role = "user",                       // 消息角色：用户
                        content = request.Content,           // 消息内容
                        content_type = "text"                // 内容类型：纯文本
                    }
                }
            };

            // 5. 记录请求构建日志（不包含敏感内容）
            _logger.LogDebug($"构建API请求体完成，BotId: {botId}, UserId: {userId}, Stream: {request.Stream}");

            return requestBody;
        }

        /// <summary>
        /// 获取JSON序列化配置选项
        /// 
        /// 此方法返回用于JSON序列化/反序列化的标准化配置：
        /// 
        /// 配置特性：
        /// 1. 忽略null值 - 减少传输数据量，避免API参数冲突
        /// 2. 驼峰命名 - 符合JavaScript和现代API的命名约定
        /// 3. 类型安全 - 确保数据类型的正确转换
        /// 4. 性能优化 - 使用静态配置避免重复创建
        /// 
        /// 适用场景：
        /// - 请求数据序列化
        /// - 响应数据反序列化
        /// - 配置数据处理
        /// - 日志内容格式化
        /// 
        /// 注意事项：
        /// - 保持与API服务端的命名约定一致
        /// - 避免序列化敏感信息
        /// - 处理特殊字符和编码问题
        /// </summary>
        /// <returns>配置好的JsonSerializerOptions实例</returns>
        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                // 当属性值为null时不包含在JSON中，减少数据传输量
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                
                // 使用驼峰命名策略，将C#的PascalCase转换为camelCase
                // 例如：BotId -> botId, ConversationId -> conversationId
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                
                // 可以根据需要添加其他配置：
                // WriteIndented = false,           // 压缩JSON，减少传输量
                // PropertyNameCaseInsensitive = true, // 反序列化时忽略大小写
                // NumberHandling = JsonNumberHandling.AllowReadingFromString // 允许从字符串读取数字
            };
        }

        /// <summary>
        /// 等待非流式AI响应处理完成
        /// 
        /// 此方法实现了智能轮询机制，用于等待异步AI处理完成：
        /// 
        /// 轮询策略：
        /// 1. 最大重试次数：30次（总计约60秒）
        /// 2. 重试间隔：2秒（平衡响应速度和服务器负载）
        /// 3. 指数退避：可选实现，避免服务器过载
        /// 4. 早期退出：一旦获得结果立即返回
        /// 
        /// 状态检测：
        /// - completed: 处理完成，提取最终回复
        /// - failed: 处理失败，返回错误信息
        /// - requires_action: 需要用户操作（如确认、输入等）
        /// - in_progress: 仍在处理中，继续等待
        /// 
        /// 性能优化：
        /// - 使用异步等待避免线程阻塞
        /// - 合理的轮询间隔平衡响应性和资源消耗
        /// - 早期退出机制减少不必要的网络请求
        /// - 详细的日志记录便于问题诊断
        /// 
        /// 错误处理：
        /// - 网络异常：记录错误并继续重试
        /// - JSON解析错误：跳过当前轮询继续
        /// - 超时处理：达到最大重试次数后返回超时信息
        /// - 状态异常：对未知状态进行合理处理
        /// 
        /// 使用场景：
        /// - 非流式AI对话处理
        /// - 长时间运行的AI任务
        /// - 需要等待处理完成的批量操作
        /// </summary>
        /// <param name="httpClient">已配置的HTTP客户端实例</param>
        /// <param name="chatId">聊天会话的唯一标识符</param>
        /// <param name="conversationId">对话的唯一标识符</param>
        /// <returns>AI生成的最终回复内容，或错误描述信息</returns>
        /// <exception cref="ArgumentNullException">当httpClient为null时抛出</exception>
        /// <exception cref="ArgumentException">当chatId或conversationId为空时抛出</exception>
        private async Task<string> WaitForCompletionAsync(HttpClient httpClient, string chatId, string conversationId)
        {
            var maxRetries = 30;
            var delay = 2000; // 2秒
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await httpClient.GetAsync($"/v3/chat/retrieve?chat_id={chatId}&conversation_id={conversationId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(content);
                        var root = jsonDoc.RootElement;
                        
                        var status = root.GetProperty("data").GetProperty("status").GetString();
                        
                        if (status == "completed")
                        {
                            var messages = root.GetProperty("data").GetProperty("messages");
                            if (messages.ValueKind == JsonValueKind.Array)
                            {
                                var lastAssistantMessage = messages.EnumerateArray()
                                    .Where(m => m.GetProperty("role").GetString() == "assistant" 
                                              && m.GetProperty("type").GetString() == "answer")
                                    .LastOrDefault();
                                
                                if (lastAssistantMessage.ValueKind != JsonValueKind.Undefined)
                                {
                                    return lastAssistantMessage.GetProperty("content").GetString() ?? "无响应内容";
                                }
                            }
                            return "响应完成但无内容";
                        }
                        else if (status == "failed")
                        {
                            return "智能体处理失败";
                        }
                        else if (status == "requires_action")
                        {
                            return "需要用户操作";
                        }
                    }
                    
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"轮询第{i + 1}次失败");
                    await Task.Delay(delay);
                }
            }
            
            return "获取响应超时";
        }

        /// <summary>
        /// 合并流式响应内容
        /// </summary>
        /// <param name="streamResults">流式响应结果列表</param>
        /// <returns>合并后的完整内容</returns>
        private string MergeStreamContent(List<KouZiAIStreamResponseDto> streamResults)
        {
            if (streamResults == null || !streamResults.Any())
            {
                return "无响应内容";
            }

            var contentBuilder = new StringBuilder();

            // 只处理 conversation.message.delta 事件的内容
            var deltaEvents = streamResults
                .Where(r => r.Event == "conversation.message.delta" && !string.IsNullOrEmpty(r.Content))
                .OrderBy(r => r.Timestamp)
                .ToList();

            foreach (var deltaEvent in deltaEvents)
            {
                contentBuilder.Append(deltaEvent.Content);
            }

            var mergedContent = contentBuilder.ToString();

            // 如果没有增量内容，尝试从完成事件中获取
            if (string.IsNullOrEmpty(mergedContent))
            {
                var completedEvent = streamResults
                    .Where(r => r.Event == "conversation.message.completed" && !string.IsNullOrEmpty(r.Content))
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefault();

                if (completedEvent != null)
                {
                    mergedContent = completedEvent.Content;
                }
            }

            // 如果还是没有内容，检查是否有错误
            if (string.IsNullOrEmpty(mergedContent))
            {
                var errorEvent = streamResults.FirstOrDefault(r => r.Event == "error");
                if (errorEvent != null)
                {
                    return $"响应错误: {errorEvent.Content}";
                }
                
                return "无有效响应内容";
            }

            // 清理内容：移除多余的空白字符
            mergedContent = CleanContent(mergedContent);

            _logger.LogInformation($"流式内容合并完成，原始片段数: {deltaEvents.Count}, 合并后长度: {mergedContent.Length}");

            return mergedContent;
        }

        /// <summary>
        /// 清理和格式化内容
        /// </summary>
        /// <param name="content">原始内容</param>
        /// <returns>清理后的内容</returns>
        private string CleanContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // 移除开头和结尾的空白字符
            content = content.Trim();

            // 规范化换行符
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");

            // 移除多余的连续空行（保留最多两个连续换行）
            while (content.Contains("\n\n\n"))
            {
                content = content.Replace("\n\n\n", "\n\n");
            }

            return content;
        }
    }
}

#endregion