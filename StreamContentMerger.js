/**
 * 流式对话内容合并工具
 * 用于将分段的对话内容合并为完整的连续语句
 */

class StreamContentMerger {
    /**
     * 合并流式对话内容
     * @param {Array|String} eventData - 事件数据数组或JSON字符串
     * @param {Object} options - 配置选项
     * @returns {Object} 合并结果
     */
    static merge(eventData, options = {}) {
        const config = {
            eventType: 'conversation.message.delta',
            contentField: 'content',
            completedField: 'isCompleted',
            includeSuggestions: false,
            ...options
        };

        try {
            let events;
            
            // 处理输入数据
            if (typeof eventData === 'string') {
                events = JSON.parse(eventData);
            } else if (Array.isArray(eventData)) {
                events = eventData;
            } else {
                throw new Error('输入数据格式不正确，需要数组或JSON字符串');
            }

            let mergedContent = '';
            let deltaCount = 0;
            let suggestions = [];
            
            // 遍历所有事件
            events.forEach((event, index) => {
                // 处理delta消息
                if (event.event === config.eventType && 
                    event[config.contentField] && 
                    !event[config.completedField]) {
                    
                    mergedContent += event[config.contentField];
                    deltaCount++;
                }
                
                // 处理建议（如果需要）
                if (config.includeSuggestions && 
                    event.event === 'conversation.message.completed' &&
                    event.content && 
                    !event.content.includes('{') && 
                    event.content.length < 100) {
                    suggestions.push(event.content);
                }
            });

            return {
                success: true,
                mergedContent: mergedContent,
                statistics: {
                    totalEvents: events.length,
                    deltaEvents: deltaCount,
                    contentLength: mergedContent.length,
                    suggestions: suggestions
                }
            };

        } catch (error) {
            return {
                success: false,
                error: error.message,
                mergedContent: '',
                statistics: null
            };
        }
    }

    /**
     * 美化输出合并结果
     * @param {Object} result - merge方法的返回结果
     */
    static prettyPrint(result) {
        if (!result.success) {
            console.error('❌ 合并失败:', result.error);
            return;
        }

        console.log('✅ 流式内容合并成功！\n');
        console.log('📝 合并后的完整内容:');
        console.log('─'.repeat(50));
        console.log(result.mergedContent);
        console.log('─'.repeat(50));
        
        console.log('\n📊 统计信息:');
        console.log(`• 总事件数: ${result.statistics.totalEvents}`);
        console.log(`• Delta事件数: ${result.statistics.deltaEvents}`);
        console.log(`• 内容长度: ${result.statistics.contentLength} 字符`);
        
        if (result.statistics.suggestions.length > 0) {
            console.log('\n💡 相关建议:');
            result.statistics.suggestions.forEach((suggestion, index) => {
                console.log(`${index + 1}. ${suggestion}`);
            });
        }
    }

    /**
     * 从你的实际数据中提取内容的便捷方法
     */
    static processYourData() {
        // 这里是从你的数据中提取的所有delta内容
        const yourData = [
            {event: 'conversation.message.delta', content: '预防', isCompleted: false},
            {event: 'conversation.message.delta', content: '艾滋病，其实', isCompleted: false},
            {event: 'conversation.message.delta', content: '说', isCompleted: false},
            {event: 'conversation.message.delta', content: '起来', isCompleted: false},
            {event: 'conversation.message.delta', content: '也不复杂，但确实', isCompleted: false},
            {event: 'conversation.message.delta', content: '需要我们从多个方面', isCompleted: false},
            // ... 添加你的所有数据
        ];

        const result = this.merge(yourData, { includeSuggestions: true });
        this.prettyPrint(result);
        
        return result.mergedContent;
    }
}

// 使用示例
if (typeof module === 'undefined') {
    // 浏览器环境示例
    console.log('🚀 流式内容合并工具已加载');
    
    // 测试示例
    const testData = [
        {event: 'conversation.message.delta', content: '你好用户，', isCompleted: false},
        {event: 'conversation.message.delta', content: '我了解了你的需求，', isCompleted: false},
        {event: 'conversation.message.delta', content: '我来为你解答', isCompleted: false}
    ];
    
    const result = StreamContentMerger.merge(testData);
    StreamContentMerger.prettyPrint(result);
    
} else {
    // Node.js环境
    module.exports = StreamContentMerger;
}