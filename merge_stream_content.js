// 合并流式对话内容的工具
function mergeStreamContent(eventData) {
    try {
        let events;
        
        // 如果输入是字符串，解析为JSON
        if (typeof eventData === 'string') {
            events = JSON.parse(eventData);
        } else {
            events = eventData;
        }
        
        // 确保输入是数组
        if (!Array.isArray(events)) {
            console.error('输入数据不是数组格式');
            return '';
        }
        
        let mergedContent = '';
        
        // 遍历所有事件，提取delta内容
        events.forEach(event => {
            // 只处理message.delta事件
            if (event.event === 'conversation.message.delta' && 
                event.content && 
                !event.isCompleted) {
                mergedContent += event.content;
            }
        });
        
        return mergedContent;
        
    } catch (error) {
        console.error('处理数据时发生错误:', error);
        return '';
    }
}

// 测试函数
function testMerge() {
    const sampleData = [
        {
            "event": "conversation.message.delta",
            "content": "预防",
            "isCompleted": false
        },
        {
            "event": "conversation.message.delta", 
            "content": "艾滋病，其实",
            "isCompleted": false
        },
        {
            "event": "conversation.message.delta",
            "content": "说起来也不复杂",
            "isCompleted": false
        }
    ];
    
    const result = mergeStreamContent(sampleData);
    console.log('合并结果:', result);
    return result;
}

// 导出函数
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { mergeStreamContent, testMerge };
}